// SDI capture with automatic format detection -> OMT sender, plus a small OMT probe for testing.
// Included from main.cpp inside its anonymous namespace (uses check, utf8, json, devices, deviceAt);
// the standard/OMT headers are therefore included at the top of main.cpp.
#pragma once

std::atomic<bool> running{true};

BOOL WINAPI consoleHandler(DWORD event) {
    if (event == CTRL_C_EVENT || event == CTRL_BREAK_EVENT) { running = false; return TRUE; }
    return FALSE;
}

// The GUI sends "stop" on stdin; a console run stops with Ctrl+C.
bool stopRequested() {
    HANDLE handle = GetStdHandle(STD_INPUT_HANDLE);
    if (GetFileType(handle) != FILE_TYPE_PIPE) return false;
    DWORD available = 0;
    if (!PeekNamedPipe(handle, nullptr, 0, nullptr, &available, nullptr)) return true;
    if (!available) return false;
    char buffer[256];
    DWORD read = 0;
    if (!ReadFile(handle, buffer, (available < sizeof(buffer) ? available : static_cast<DWORD>(sizeof(buffer))), &read, nullptr)) return true;
    static std::string pending;
    pending.append(buffer, read);
    for (size_t end; (end = pending.find('\n')) != std::string::npos;) {
        auto command = pending.substr(0, end);
        pending.erase(0, end + 1);
        if (command.starts_with("\xef\xbb\xbf")) command.erase(0, 3);
        if (!command.empty() && command.back() == '\r') command.pop_back();
        if (command == "stop") return true;
    }
    return false;
}

// What the SDK says about one display mode. Interlaced pictures are woven full-height frames at the FRAME rate
// (1080i50 -> 25), exactly what OMT and NDI expect.
struct ModeInfo {
    BMDDisplayMode id = bmdModeUnknown;
    int width = 0, height = 0;
    int rateN = 0, rateD = 1;
    bool interlaced = false;
    std::string name;
};

ModeInfo describe(IDeckLinkDisplayMode* mode) {
    ModeInfo info;
    info.id = mode->GetDisplayMode();
    info.width = mode->GetWidth();
    info.height = mode->GetHeight();
    __int64 duration = 0, scale = 0;
    check(mode->GetFrameRate(&duration, &scale), "Read frame rate");
    info.rateN = static_cast<int>(scale);
    info.rateD = static_cast<int>(duration);
    auto fields = mode->GetFieldDominance();
    info.interlaced = fields == bmdUpperFieldFirst || fields == bmdLowerFieldFirst;
    CComBSTR name;
    if (mode->GetName(&name) == S_OK) info.name = utf8(name);
    return info;
}

// Which of the 8 embedded SDI channels go to the network, or none.
struct AudioRoute {
    int left = 0, right = 1;
    bool enabled = true;
};

AudioRoute parseAudioRoute(const std::string& text) {
    if (text == "Off") return {0, 0, false};
    if (text == "Stereo 1&2") return {0, 1, true};
    if (text == "Stereo 3&4") return {2, 3, true};
    if (text == "Stereo 5&6") return {4, 5, true};
    if (text == "Stereo 7&8") return {6, 7, true};
    if (text.size() == 6 && text.starts_with("Mono ") && text[5] >= '1' && text[5] <= '8') return {text[5] - '1', text[5] - '1', true};
    throw std::runtime_error("Audio must be Off, Stereo 1&2, 3&4, 5&6, 7&8 or Mono 1..8");
}

class Capture : public IDeckLinkInputCallback_v14_2_1 {
    std::atomic<ULONG> refs{1};
    LowLatencyQueue<Packet> queue{std::chrono::milliseconds(100)};
    VideoTiming videoTiming;
    Sink& sink;
    IDeckLinkInput_v14_2_1* input;
    const AudioRoute route;
    std::mutex modeMutex;
    ModeInfo mode;
    // Timestamps stay monotonic over a format restart (the card's stream time starts again at 0).
    int64_t timeOffset = 0, lastTimestamp = -1;
    bool rebase = false;
    std::mutex failureMutex;
    std::string failure;
    std::thread worker;
    std::atomic<int64_t> previousVideo{-1};

public:
    std::atomic<uint64_t> captured{0}, sent{0}, captureGaps{0}, noSignal{0}, noDelivery{0}, errors{0}, sendErrors{0}, formatChanges{0};
    std::atomic<bool> signalPresent{false}, failed{false};
    std::atomic<int64_t> lastVideoArrival{0}, lastSendFinished{0};
    std::atomic<int> lastEncodeMs{0};

    static int64_t nowMs() {
        return std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now().time_since_epoch()).count();
    }

    Capture(Sink& value, IDeckLinkInput_v14_2_1* source, const ModeInfo& initial, AudioRoute audioRoute)
        : sink(value), input(source), route(audioRoute), mode(initial) {
        // Started last, after every member exists.
        worker = std::thread([this] {
            try {
                ComScope scope;
                while (auto next = queue.wait()) {
                    Packet packet = std::move(*next);
                    packet.frame.Data = packet.frame.Type == OMTFrameType_Video ? static_cast<void*>(packet.video.data())
                                                                                : static_cast<void*>(packet.audio.data());
                    const auto begin = std::chrono::steady_clock::now();
                    const auto delivered = sink.send(packet);
                    const auto end = std::chrono::steady_clock::now();
                    const double elapsed = std::chrono::duration<double, std::milli>(end - begin).count();
                    if (packet.frame.Type == OMTFrameType_Video && delivered > 0)
                        videoTiming.add(elapsed, 1000.0 * packet.frame.FrameRateD / packet.frame.FrameRateN);
                    lastEncodeMs.store(static_cast<int>(elapsed), std::memory_order_relaxed);
                    lastSendFinished.store(nowMs(), std::memory_order_relaxed);
                    if (delivered > 0) { if (packet.frame.Type == OMTFrameType_Video) ++sent; }
                    else if (delivered == 0) ++noDelivery; // Nobody listening is not an error.
                    else { ++sendErrors; throw std::runtime_error("Send failed; transmission stopped"); }
                }
            } catch (const std::exception& e) { fail(e.what()); }
            catch (...) { fail("Unexpected encoder failure; transmission stopped"); }
        });
    }
    ~Capture() { finish(); }

    void fail(const std::string& message) {
        { std::lock_guard lock(failureMutex); failure = message; }
        failed = true;
        queue.close();
        running = false;
    }
    std::string failureMessage() { std::lock_guard lock(failureMutex); return failure; }
    void finish() { queue.close(); if (worker.joinable()) worker.join(); }
    auto queueStats() { return queue.stats(); }
    auto videoTimingStats() { return videoTiming.snapshot(); }
    ModeInfo currentMode() { std::lock_guard lock(modeMutex); return mode; }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** result) override {
        if (!result) return E_POINTER;
        *result = nullptr;
        if (iid == IID_IUnknown || iid == __uuidof(IDeckLinkInputCallback_v14_2_1)) {
            *result = static_cast<IDeckLinkInputCallback_v14_2_1*>(this);
            AddRef();
            return S_OK;
        }
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return ++refs; }
    ULONG STDMETHODCALLTYPE Release() override {
        auto value = --refs;
        if (!value) delete this;
        return value;
    }

    // The card detected another incoming format: restart the stream in it. OMT frames carry their own format.
    HRESULT STDMETHODCALLTYPE VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents, IDeckLinkDisplayMode* newMode,
                                                      _BMDDetectedVideoInputFormatFlags) override {
        try {
            if (!newMode || failed) return S_OK;
            ModeInfo next = describe(newMode);
            if (next.id == currentMode().id) return S_OK;
            input->PauseStreams();
            check(input->EnableVideoInput(next.id, bmdFormat8BitYUV, bmdVideoInputEnableFormatDetection), "Switch capture mode");
            input->FlushStreams();
            queue.clearVideo();
            previousVideo = -1;
            { std::lock_guard lock(modeMutex); mode = next; rebase = true; }
            ++formatChanges;
            check(input->StartStreams(), "Restart streams");
        } catch (const std::exception& e) { fail(e.what()); }
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE VideoInputFrameArrived(IDeckLinkVideoInputFrame_v14_2_1* video, IDeckLinkAudioInputPacket* audio) override {
        try {
            if (failed) return S_OK;
            const ModeInfo now = currentMode();
            const bool validSignal = video && !(video->GetFlags() & bmdFrameHasNoInputSource);
            if (video) signalPresent.store(validSignal, std::memory_order_relaxed);
            if (validSignal) {
                try { arrivedVideo(video, now); } catch (const std::bad_alloc&) { throw; } catch (...) { ++errors; }
            } else if (video) {
                ++noSignal;
                previousVideo = -1;
                queue.clearVideo();
            }
            if (audio && route.enabled) arrivedAudio(audio);
        } catch (const std::bad_alloc&) { ++errors; fail("Capture ran out of memory; transmission stopped"); }
        catch (...) { ++errors; }
        return S_OK;
    }

private:
    int64_t stamp(int64_t timestamp, int64_t duration) {
        std::lock_guard lock(modeMutex);
        if (rebase) { if (lastTimestamp >= 0) timeOffset = lastTimestamp + duration - timestamp; rebase = false; }
        lastTimestamp = timestamp + timeOffset;
        return lastTimestamp;
    }

    void arrivedVideo(IDeckLinkVideoInputFrame_v14_2_1* video, const ModeInfo& now) {
        void* bytes = nullptr;
        __int64 timestamp = 0, duration = 0;
        if (FAILED(video->GetBytes(&bytes)) || !bytes || FAILED(video->GetStreamTime(&timestamp, &duration, 10000000)) || duration <= 0) {
            ++errors;
            return;
        }
        Packet p;
        p.frame.Type = OMTFrameType_Video;
        p.frame.Codec = OMTCodec_UYVY;
        p.frame.Width = video->GetWidth();
        p.frame.Height = video->GetHeight();
        p.frame.Stride = video->GetRowBytes();
        if (p.frame.Width <= 0 || p.frame.Width > 4096 || p.frame.Height <= 0 || p.frame.Height > 2160 ||
            p.frame.Stride < p.frame.Width * 2 || p.frame.Stride > std::numeric_limits<int>::max() / p.frame.Height)
            throw std::runtime_error("Invalid captured video buffer");
        p.frame.Flags = now.interlaced ? OMTVideoFlags_Interlaced : OMTVideoFlags_None;
        p.frame.FrameRateN = now.rateN;
        p.frame.FrameRateD = now.rateD;
        p.frame.AspectRatio = 16.0f / 9;
        p.frame.ColorSpace = OMTColorSpace_BT709;
        p.frame.DataLength = p.frame.Stride * p.frame.Height;
        const auto raw = timestamp;
        p.frame.Timestamp = stamp(timestamp, duration);
        auto previous = previousVideo.exchange(raw, std::memory_order_relaxed);
        if (previous >= 0 && raw > previous + duration + duration / 2)
            captureGaps += static_cast<uint64_t>((raw - previous + duration / 2) / duration - 1);
        auto first = static_cast<unsigned char*>(bytes);
        p.video.assign(first, first + p.frame.DataLength);
        ++captured;
        lastVideoArrival.store(nowMs(), std::memory_order_relaxed);
        const auto stampValue = p.frame.Timestamp;
        queue.push(std::move(p), true, stampValue);
    }

    // 8 embedded channels in, stereo float planar (channels 1&2) out.
    void arrivedAudio(IDeckLinkAudioInputPacket* audio) {
        void* bytes = nullptr;
        __int64 timestamp = 0;
        auto count = audio->GetSampleFrameCount();
        if (count <= 0 || count > 48000 || FAILED(audio->GetBytes(&bytes)) || !bytes || FAILED(audio->GetPacketTime(&timestamp, 10000000))) {
            if (count > 0) ++errors;
            return;
        }
        // A large driver packet is trimmed from the front: keep the freshest 120 ms with their original time.
        int skipped = count > 5760 ? static_cast<int>(count) - 5760 : 0;
        if (skipped) {
            queue.discardAudioSamples(skipped);
            timestamp += static_cast<int64_t>(skipped) * 10000000 / 48000;
            count -= skipped;
        }
        Packet p;
        p.frame.Type = OMTFrameType_Audio;
        p.frame.Codec = OMTCodec_FPA1;
        p.frame.Timestamp = stamp(timestamp, 0);
        p.frame.SampleRate = 48000;
        p.frame.Channels = 2;
        p.frame.SamplesPerChannel = count;
        p.frame.DataLength = count * 2 * 4;
        const auto* samples = static_cast<const int32_t*>(bytes) + skipped * 8;
        p.audio.resize(static_cast<size_t>(count) * 2);
        for (int i = 0; i < count; ++i) {
            p.audio[i] = static_cast<float>(samples[i * 8 + route.left] / 2147483648.0);
            p.audio[count + i] = static_cast<float>(samples[i * 8 + route.right] / 2147483648.0);
        }
        const auto stampValue = p.frame.Timestamp;
        const int sampleCount = count;
        queue.push(std::move(p), false, stampValue, sampleCount);
    }
};

// Starting mode only: format detection switches to whatever actually arrives.
ModeInfo startingMode(IDeckLinkInput_v14_2_1* input) {
    CComPtr<IDeckLinkDisplayModeIterator> iterator;
    check(input->GetDisplayModeIterator(&iterator), "Input mode iterator");
    ModeInfo first;
    bool haveFirst = false;
    for (;;) {
        CComPtr<IDeckLinkDisplayMode> display;
        if (iterator->Next(&display) != S_OK) break;
        BMDDisplayMode actual = display->GetDisplayMode();
        long ok = 0;
        if (input->DoesSupportVideoMode(bmdVideoConnectionUnspecified, actual, bmdFormat8BitYUV, bmdNoVideoInputConversion,
                                        bmdSupportedVideoModeDefault, &actual, &ok) != S_OK || !ok) continue;
        ModeInfo info = describe(display);
        if (info.id == bmdModeHD1080p50) return info;
        if (!haveFirst) { first = info; haveFirst = true; }
    }
    if (!haveFirst) throw std::runtime_error("This input reports no supported 8-bit YUV mode");
    return first;
}

// The name part of an OMT address "HOST (name)".
std::string omtNamePart(const std::string& address) {
    const auto open = address.find(" (");
    const auto close = address.rfind(')');
    return open != std::string::npos && close != std::string::npos && close > open ? address.substr(open + 2, close - open - 2) : address;
}

std::string lower(std::string text) {
    for (auto& c : text) c = static_cast<char>(std::tolower(static_cast<unsigned char>(c)));
    return text;
}

// Names of the OMT senders currently announced on the network (the first call needs a moment to hear them).
std::vector<std::string> announcedOmtNames(int waitMs) {
    int count = 0;
    omt_discovery_getaddresses(&count); // starts the discovery
    std::this_thread::sleep_for(std::chrono::milliseconds(waitMs));
    count = 0;
    char** addresses = omt_discovery_getaddresses(&count);
    std::vector<std::string> names;
    for (int i = 0; i < count; ++i) names.push_back(lower(omtNamePart(addresses[i])));
    return names;
}

// Never opens an OMT stream whose name is already in use. policy: abort | increment | retry | retry10.
// Returns the name to use, or an empty string when the user stopped while waiting.
std::string freeOmtName(const std::string& wanted, const std::string& policy) {
    if (policy != "abort" && policy != "increment" && policy != "retry" && policy != "retry10")
        throw std::runtime_error("Duplicate-name handling must be abort, increment, retry or retry10");
    const auto started = std::chrono::steady_clock::now();
    auto lastNotice = started - std::chrono::seconds(60);
    bool first = true;
    for (;;) {
        const auto names = announcedOmtNames(first ? 2500 : 2000);
        first = false;
        auto used = [&](const std::string& candidate) { return std::find(names.begin(), names.end(), lower(candidate)) != names.end(); };
        if (!used(wanted)) return wanted;
        if (policy == "abort")
            throw std::runtime_error("An OMT stream named \"" + wanted + "\" already exists on the network. Choose another name.");
        if (policy == "increment") {
            for (int n = 2; n < 1000; ++n) {
                const std::string candidate = wanted + " " + std::to_string(n);
                if (!used(candidate)) { std::cout << "Notice: the name \"" << wanted << "\" is in use, sending as \"" << candidate << "\"\n"; return candidate; }
            }
            throw std::runtime_error("No free numbered name found for \"" + wanted + "\"");
        }
        const auto now = std::chrono::steady_clock::now();
        if (policy == "retry10" && now - started > std::chrono::minutes(10))
            throw std::runtime_error("The OMT name \"" + wanted + "\" stayed in use for 10 minutes; stopped.");
        if (now - lastNotice >= std::chrono::seconds(10)) {
            std::cout << "Notice: waiting, the OMT name \"" << wanted << "\" is in use\n";
            lastNotice = now;
        }
        for (int tick = 0; tick < 20; ++tick) { // about 2 s, staying responsive to Stop
            if (stopRequested() || !running) return {};
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
        }
    }
}

void runCapture(int index, const std::string& name, int seconds, const std::string& qualityName, const std::string& outputs,
                const std::string& audioText, const std::string& duplicate) {
    const AudioRoute audioRoute = parseAudioRoute(audioText);
    auto quality = parseQuality(qualityName);
    auto all = devices();
    IDeckLink* device = deviceAt(all, index);
    CComPtr<IDeckLinkStatus> status;
    device->QueryInterface(__uuidof(IDeckLinkStatus), reinterpret_cast<void**>(&status));
    __int64 busy = 0;
    if (status && status->GetInt(bmdDeckLinkStatusBusy, &busy) == S_OK && busy) throw std::runtime_error("Port is busy (used by another program)");
    CComPtr<IDeckLinkInput_v14_2_1> input;
    check(device->QueryInterface(__uuidof(IDeckLinkInput_v14_2_1), reinterpret_cast<void**>(&input)), "Capture interface");
    const ModeInfo initial = startingMode(input);
    if (outputs != "omt" && outputs != "ndi" && outputs != "omt+ndi") throw std::runtime_error("Output must be omt, ndi or omt+ndi");
    std::string streamName = name;
    if (outputs != "ndi") {
        streamName = freeOmtName(name, duplicate);
        if (streamName.empty()) return; // stopped while waiting for the name
    }
    SinkGroup sinks;
    if (outputs == "omt" || outputs == "omt+ndi") sinks.add(std::make_unique<OmtSink>(streamName, quality));
    if (outputs == "ndi" || outputs == "omt+ndi") sinks.add(std::make_unique<NdiSink>(streamName));
    CComPtr<Capture> callback;
    callback.Attach(new Capture(sinks, input, initial, audioRoute));
    struct Stop {
        IDeckLinkInput_v14_2_1* input;
        ~Stop() { input->StopStreams(); input->SetCallback(nullptr); input->DisableAudioInput(); input->DisableVideoInput(); }
    } stop{input};
    check(input->EnableVideoInput(initial.id, bmdFormat8BitYUV, bmdVideoInputEnableFormatDetection), "Enable capture (port may be occupied)");
    check(input->EnableAudioInput(bmdAudioSampleRate48kHz, bmdAudioSampleType32bitInteger, 8), "Enable 8-channel SDI audio input");
    check(input->SetCallback(callback), "Set callback");
    check(input->StartStreams(), "Start streams");
    std::cout << "Sending as " << sinks.describe() << " (starting mode " << initial.name << ", follows the incoming format). Ctrl+C to stop.\n";
    const auto started = Capture::nowMs();
    for (int elapsed = 0; running && (seconds == 0 || elapsed < seconds); ++elapsed) {
        for (int tick = 0; tick < 10 && running; ++tick) {
            if (stopRequested()) { running = false; break; }
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
        }
        const auto state = callback->queueStats();
        const auto now = Capture::nowMs();
        const auto arrival = callback->lastVideoArrival.load();
        const auto finished = callback->lastSendFinished.load();
        const auto timing = callback->videoTimingStats();
        const ModeInfo mode = callback->currentMode();
        // One compact sample per second.
        std::cout << "Health={\"uptimeMs\":" << now - started << ",\"mode\":" << json(mode.name)
                  << ",\"width\":" << mode.width << ",\"height\":" << mode.height << ",\"frameRateN\":" << mode.rateN
                  << ",\"frameRateD\":" << mode.rateD << ",\"interlaced\":" << (mode.interlaced ? "true" : "false")
                  << ",\"formatChanges\":" << callback->formatChanges << ",\"captured\":" << callback->captured
                  << ",\"sent\":" << callback->sent << ",\"videoLost\":" << state.videoDropped + callback->captureGaps.load()
                  << ",\"audioLostSamples\":" << state.audioSamplesDropped
                  << ",\"errors\":" << callback->errors.load() + callback->sendErrors.load()
                  << ",\"signal\":" << (callback->signalPresent ? "true" : "false")
                  << ",\"videoAgeMs\":" << (arrival ? now - arrival : -1) << ",\"sendAgeMs\":" << (finished ? now - finished : -1)
                  << ",\"encodeMs\":" << callback->lastEncodeMs << ",\"connections\":" << sinks.connections()
                  << ",\"videoAverageMs\":" << timing.average << ",\"videoP95Ms\":" << timing.p95
                  << ",\"videoMaximumMs\":" << timing.maximum << "}\n";
    }
    input->StopStreams();
    input->SetCallback(nullptr);
    callback->finish();
    if (callback->failed) throw std::runtime_error(callback->failureMessage());
}

// Receives from an OMT sender and prints the format of what arrives, once per second. barEven/barOdd are the x position of the
// first bright pixel on lines 100/101 (the --test-source bar), audioRms the level of the left channel.
void probeOmt(const std::string& address, int seconds) {
    omt_receive_t* receiver = omt_receive_create(address.c_str(), static_cast<OMTFrameType>(OMTFrameType_Video | OMTFrameType_Audio),
                                                 OMTPreferredVideoFormat_UYVY, OMTReceiveFlags_None);
    if (!receiver) throw std::runtime_error("OMT receiver creation failed");
    struct Cleanup { omt_receive_t* handle; ~Cleanup() { omt_receive_destroy(handle); } } cleanup{receiver};
    auto last = std::chrono::steady_clock::now();
    const auto begin = last;
    int video = 0, audio = 0, barEven = -1, barOdd = -1;
    double squares = 0;
    int64_t sampleCount = 0;
    OMTMediaFrame format{};
    auto bar = [](const OMTMediaFrame& f, int row) {
        if (row >= f.Height) return -1;
        const auto* line = static_cast<const unsigned char*>(f.Data) + static_cast<size_t>(row) * f.Stride;
        for (int x = 0; x < f.Width; ++x) if (line[x * 2 + 1] > 150) return x;
        return -1;
    };
    while (running && !stopRequested() && (seconds == 0 || std::chrono::steady_clock::now() - begin < std::chrono::seconds(seconds))) {
        auto* frame = omt_receive(receiver, static_cast<OMTFrameType>(OMTFrameType_Video | OMTFrameType_Audio), 100);
        if (frame && frame->Type == OMTFrameType_Video) {
            ++video;
            format = *frame;
            if (frame->Data && frame->Codec == OMTCodec_UYVY) { barEven = bar(*frame, 100); barOdd = bar(*frame, 101); }
        } else if (frame && frame->Type == OMTFrameType_Audio && frame->Data) {
            ++audio;
            const auto* left = static_cast<const float*>(frame->Data);
            for (int i = 0; i < frame->SamplesPerChannel; ++i) squares += static_cast<double>(left[i]) * left[i];
            sampleCount += frame->SamplesPerChannel;
        }
        if (std::chrono::steady_clock::now() - last >= std::chrono::seconds(1)) {
            last = std::chrono::steady_clock::now();
            std::cout << "Probe={\"video\":" << video << ",\"audio\":" << audio << ",\"width\":" << format.Width << ",\"height\":" << format.Height
                      << ",\"frameRateN\":" << format.FrameRateN << ",\"frameRateD\":" << format.FrameRateD
                      << ",\"interlaced\":" << ((format.Flags & OMTVideoFlags_Interlaced) ? "true" : "false")
                      << ",\"barEven\":" << barEven << ",\"barOdd\":" << barOdd
                      << ",\"audioRms\":" << (sampleCount ? std::sqrt(squares / sampleCount) : 0.0) << "}\n";
            video = audio = 0;
            squares = 0;
            sampleCount = 0;
        }
    }
}
