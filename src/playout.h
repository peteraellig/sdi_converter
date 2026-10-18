// OMT -> SDI playout. The output mode is chosen from the first OMT picture (size, frame rate N/D, interlaced flag)
// among the modes the card reports; if the incoming format changes the output is restarted in the new mode.
// Included from main.cpp after capture.h (uses ModeInfo, describe, running, stopRequested, Sender and the helpers).
#pragma once

struct Rate { int n = 0, d = 1; };

// Time of frame `index` in 100 ns units / in 48 kHz samples. Exact for fractional rates (59.94: 800.8 samples per frame).
int64_t frameTime(int64_t index, const ModeInfo& m) { return index * 10000000 * m.rateD / m.rateN; }
int64_t audioPosition(int64_t index, const ModeInfo& m) { return index * 48000 * m.rateD / m.rateN; }

struct IncomingFormat {
    bool valid = false;
    int width = 0, height = 0, rateN = 0, rateD = 1;
    bool interlaced = false;
    bool operator==(const IncomingFormat& o) const {
        return width == o.width && height == o.height && static_cast<int64_t>(rateN) * o.rateD == static_cast<int64_t>(o.rateN) * rateD &&
               interlaced == o.interlaced;
    }
    std::string text() const {
        char buffer[120];
        sprintf_s(buffer, "%dx%d %.3f fps %s", width, height, rateD > 0 ? static_cast<double>(rateN) / rateD : 0.0,
                  interlaced ? "interlaced" : "progressive");
        return buffer;
    }
};

// Receives from an OMT or NDI sender. Pictures only reach the buffer once the owner has said which format it expects.
class Receiver {
    omt_receive_t* handle = nullptr;
    std::unique_ptr<ndi::Runtime> ndiRuntime;
    void* ndiHandle = nullptr;
    std::atomic<bool> active{true};
    std::thread worker;
    std::mutex mutex;
    IncomingFormat seen, expected;
    std::string error;
    bool changed = false;
    // NDI delivers an interlaced picture as two field frames (format type 2 then 3); the first is kept until its partner arrives.
    std::vector<uint8_t> firstField, woven;
    int firstFieldWidth = 0, firstFieldHeight = 0;
    int64_t firstFieldTimecode = 0;

    struct VideoIn {
        int width, height, rateN, rateD;
        bool interlaced, uyvy;
        int64_t timestamp;
        const uint8_t* data;
        int stride;
        int64_t dataLength;
    };

public:
    MediaBuffer buffer;
    std::atomic<uint64_t> videos{0}, audios{0};
    std::atomic<int64_t> lastVideo{0};

    // `address` is the OMT address ("HOST (name)") or, with ndiSource, the NDI source name ("HOST (name)").
    Receiver(const std::string& address, bool ndiSource) {
        if (ndiSource) {
            ndiRuntime = std::make_unique<ndi::Runtime>();
            // Fields must be allowed: without it the SDK deinterlaces. Two fields are woven back into one picture in pollNdi.
            ndi::RecvSettings settings{{address.c_str(), nullptr}, ndi::colorFormatUyvyBgra, ndi::bandwidthHighest, true, "SDI Converter"};
            ndiHandle = ndiRuntime->recvCreate(&settings);
            if (!ndiHandle) throw std::runtime_error("NDI receiver could not start");
        } else {
            handle = omt_receive_create(address.c_str(), static_cast<OMTFrameType>(OMTFrameType_Video | OMTFrameType_Audio),
                                        OMTPreferredVideoFormat_UYVY, OMTReceiveFlags_None);
            if (!handle) throw std::runtime_error("Cannot create OMT receiver");
        }
        worker = std::thread([this, ndiSource] {
            try {
                while (active) { if (ndiSource) pollNdi(); else pollOmt(); }
            } catch (const std::exception& e) { std::lock_guard lock(mutex); error = e.what(); }
        });
    }
    ~Receiver() {
        active = false;
        if (worker.joinable()) worker.join();
        if (handle) omt_receive_destroy(handle);
        if (ndiHandle) ndiRuntime->recvDestroy(ndiHandle);
    }

    IncomingFormat seenFormat() { std::lock_guard lock(mutex); return seen; }
    void expect(const IncomingFormat& format) { std::lock_guard lock(mutex); expected = format; changed = false; }
    bool formatChanged() { std::lock_guard lock(mutex); return changed; }
    void checkError() { std::lock_guard lock(mutex); if (!error.empty()) throw std::runtime_error(error); }

private:
    void pollOmt() {
        auto* frame = omt_receive(handle, static_cast<OMTFrameType>(OMTFrameType_Video | OMTFrameType_Audio), 20);
        if (!frame) return;
        if (frame->Type == OMTFrameType_Video) {
            if (frame->Timestamp < 0) throw std::runtime_error("Source must provide valid media timestamps");
            video({frame->Width, frame->Height, frame->FrameRateN, frame->FrameRateD, (frame->Flags & OMTVideoFlags_Interlaced) != 0,
                   frame->Codec == OMTCodec_UYVY, frame->Timestamp, static_cast<const uint8_t*>(frame->Data), frame->Stride, frame->DataLength});
        } else if (frame->Type == OMTFrameType_Audio) {
            if (frame->Codec != OMTCodec_FPA1 || frame->SampleRate != 48000 || frame->Channels < 1 || frame->Channels > 32 ||
                frame->SamplesPerChannel < 1 || frame->SamplesPerChannel > 24000 || !frame->Data ||
                frame->DataLength < static_cast<int64_t>(frame->SamplesPerChannel) * frame->Channels * 4)
                throw std::runtime_error("OMT audio must be valid 48 kHz planar float audio");
            audio(frame->Timestamp, frame->SamplesPerChannel, frame->Channels, static_cast<const float*>(frame->Data), frame->SamplesPerChannel, 1.0f);
        }
    }

    void pollNdi() {
        ndi::VideoFrame v{};
        ndi::AudioFrame a{};
        const int type = ndiRuntime->recvCapture(ndiHandle, &v, &a, nullptr, 20);
        if (type == 1) {
            // The sender's timecode is the media clock shared by its video and audio.
            const bool uyvy = v.fourcc == ('U' | ('Y' << 8) | ('V' << 16) | ('Y' << 24));
            if ((v.formatType == 2 || v.formatType == 3) && uyvy && v.data && v.stride >= v.width * 2) weaveField(v);
            else video({v.width, v.height, v.rateN, v.rateD, v.formatType == 0, uyvy, v.timecode, v.data, v.stride,
                        static_cast<int64_t>(v.stride) * v.height});
            ndiRuntime->freeVideo(ndiHandle, &v);
        } else if (type == 2) {
            if (a.sampleRate == 48000 && a.channels >= 1 && a.samples >= 1 && a.samples <= 24000 && a.data && a.channelStride >= a.samples * 4)
                audio(a.timecode, a.samples, a.channels, a.data, a.channelStride / 4, 1.0f / ndi::audioGain);
            ndiRuntime->freeAudio(ndiHandle, &a);
        }
    }

    void weaveField(const ndi::VideoFrame& field) {
        const size_t rowBytes = static_cast<size_t>(field.width) * 2;
        if (field.formatType == 2) { // field 0: upper lines (0, 2, 4, ...)
            firstField.resize(rowBytes * field.height);
            for (int row = 0; row < field.height; ++row)
                std::memcpy(firstField.data() + row * rowBytes, field.data + static_cast<size_t>(row) * field.stride, rowBytes);
            firstFieldWidth = field.width;
            firstFieldHeight = field.height;
            firstFieldTimecode = field.timecode;
            return;
        }
        if (firstFieldHeight != field.height || firstFieldWidth != field.width) return; // field 1 without its partner: drop it
        woven.resize(rowBytes * field.height * 2);
        for (int row = 0; row < field.height; ++row) {
            std::memcpy(woven.data() + static_cast<size_t>(2 * row) * rowBytes, firstField.data() + row * rowBytes, rowBytes);
            std::memcpy(woven.data() + static_cast<size_t>(2 * row + 1) * rowBytes, field.data + static_cast<size_t>(row) * field.stride, rowBytes);
        }
        firstFieldHeight = 0;
        video({field.width, field.height * 2, field.rateN, field.rateD, true, true, firstFieldTimecode, woven.data(),
               static_cast<int>(rowBytes), static_cast<int64_t>(woven.size())});
    }

    void video(const VideoIn& in) {
        if (in.rateD <= 0 || in.rateN <= 0 || in.timestamp < 0) return;
        IncomingFormat now{true, in.width, in.height, in.rateN, in.rateD, in.interlaced};
        {
            std::lock_guard lock(mutex);
            seen = now;
            if (!expected.valid) return;
            if (!(now == expected)) { changed = true; return; }
        }
        if (!in.uyvy || !in.data || in.stride < in.width * 2 || in.stride > 65536 || in.dataLength < static_cast<int64_t>(in.stride) * in.height)
            throw std::runtime_error("The source delivers no usable UYVY picture (NDI with alpha is not supported)");
        VideoPicture picture;
        picture.timestamp = in.timestamp;
        picture.pixels.resize(static_cast<size_t>(in.width) * in.height * 2);
        for (int row = 0; row < in.height; ++row)
            std::memcpy(picture.pixels.data() + static_cast<size_t>(row) * in.width * 2, in.data + static_cast<size_t>(row) * in.stride,
                        static_cast<size_t>(in.width) * 2);
        buffer.push(std::move(picture));
        ++videos;
        lastVideo = Capture::nowMs();
    }

    // Planar float in (first two channels, `planeStride` floats per channel), stereo planar float out, scaled by `gain`.
    void audio(int64_t timestamp, int samples, int channels, const float* data, int planeStride, float gain) {
        Sound sound;
        sound.timestamp = timestamp;
        sound.samples = samples;
        sound.planar.resize(static_cast<size_t>(samples) * 2);
        const float* right = data + (channels > 1 ? planeStride : 0);
        for (int i = 0; i < samples; ++i) {
            sound.planar[i] = data[i] * gain;
            sound.planar[samples + i] = right[i] * gain;
        }
        buffer.push(std::move(sound));
        ++audios;
    }
};

// The output mode for an incoming picture: same size, same frame rate (exact N/D), same scan type, and DoesSupportVideoMode agrees.
ModeInfo pickOutputMode(IDeckLinkOutput_v14_2_1* output, const IncomingFormat& in) {
    CComPtr<IDeckLinkDisplayModeIterator> iterator;
    check(output->GetDisplayModeIterator(&iterator), "Output mode iterator");
    for (;;) {
        CComPtr<IDeckLinkDisplayMode> display;
        if (iterator->Next(&display) != S_OK) break;
        ModeInfo mode = describe(display);
        if (mode.width != in.width || mode.height != in.height || mode.interlaced != in.interlaced ||
            static_cast<int64_t>(mode.rateN) * in.rateD != static_cast<int64_t>(in.rateN) * mode.rateD)
            continue;
        BMDDisplayMode actual = mode.id;
        long ok = 0;
        if (output->DoesSupportVideoMode(bmdVideoConnectionSDI, mode.id, bmdFormat8BitYUV, bmdNoVideoOutputConversion,
                                         bmdSupportedVideoModeDefault, &actual, &ok) == S_OK && ok)
            return mode;
    }
    throw std::runtime_error("No SDI output mode for the incoming picture: " + in.text());
}

struct AudioOut {
    int firstChannel = 0; // 0-based SDI channel
    bool mono = false, off = false;
};

class Player : public IDeckLinkVideoOutputCallback_v14_2_1 {
    std::atomic<ULONG> refs{1};
    struct Slot {
        CComPtr<IDeckLinkMutableVideoFrame_v14_2_1> frame;
        std::atomic<bool> busy{false};
        std::vector<int32_t> audio;
    };
    std::array<Slot, 12> slots;
    CComPtr<IDeckLinkOutput_v14_2_1> output;
    ModeInfo format;
    int bufferFrames;
    bool videoEnabled = false, audioEnabled = false, started = false, closed = false;
    std::atomic<bool> stopped{false};
    std::mutex wakeMutex;
    std::condition_variable wake;
    VideoPicture previous;
    bool havePrevious = false;
    const AudioOut audioOut;
    const int audioChannels;
    int64_t origin = 0, index = 0;
    uint64_t generation = 0;

public:
    std::atomic<uint64_t> completed{0}, late{0}, dropped{0};
    uint64_t repeated = 0, black = 0, silentSamples = 0, resyncs = 0, scheduled = 0;

    Player(CComPtr<IDeckLinkOutput_v14_2_1> value, const ModeInfo& mode, int depth, const AudioOut& target)
        : output(value), format(mode), bufferFrames(depth), audioOut(target), audioChannels(target.firstChannel > 1 || target.mono ? 8 : 2) {}

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** result) override {
        if (!result) return E_POINTER;
        *result = nullptr;
        if (iid == IID_IUnknown || iid == __uuidof(IDeckLinkVideoOutputCallback_v14_2_1)) {
            *result = static_cast<IDeckLinkVideoOutputCallback_v14_2_1*>(this);
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
    HRESULT STDMETHODCALLTYPE ScheduledFrameCompleted(IDeckLinkVideoFrame_v14_2_1* frame, _BMDOutputFrameCompletionResult result) override {
        if (result == bmdOutputFrameDisplayedLate) ++late;
        else if (result == bmdOutputFrameDropped) ++dropped;
        if (result != bmdOutputFrameFlushed) ++completed;
        for (auto& slot : slots)
            if (slot.frame.p == frame) { slot.busy.store(false, std::memory_order_release); break; }
        wake.notify_one();
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE ScheduledPlaybackHasStopped() override { stopped = true; wake.notify_one(); return S_OK; }

    void initialize() {
        check(output->EnableVideoOutput(format.id, bmdVideoOutputFlagDefault), "Enable SDI video (port may be occupied)");
        videoEnabled = true;
        check(output->EnableAudioOutput(bmdAudioSampleRate48kHz, bmdAudioSampleType32bitInteger, audioChannels, bmdAudioOutputStreamTimestamped),
              "Enable embedded SDI audio");
        audioEnabled = true;
        for (auto& slot : slots)
            check(output->CreateVideoFrame(format.width, format.height, format.width * 2, bmdFormat8BitYUV, bmdFrameFlagDefault, &slot.frame),
                  "Allocate SDI frame");
        check(output->SetScheduledFrameCompletionCallback(this), "Install playback callback");
        check(output->BeginAudioPreroll(), "Begin audio preroll");
    }

    void fill(Receiver& receiver) {
        unsigned int pending = 0;
        check(output->GetBufferedVideoFrameCount(&pending), "Read output buffer");
        const int64_t period = frameTime(1, format);
        while (pending < static_cast<unsigned int>(bufferFrames)) {
            auto state = receiver.buffer.state();
            int64_t target = origin + frameTime(index, format);
            const bool signal = receiver.lastVideo > 0 && Capture::nowMs() - receiver.lastVideo < 500;
            if (state.generation != generation || (started && signal && (state.last > target + period * 8 || state.last < target - period * 5))) {
                origin = state.last - period * 2 - frameTime(index, format);
                target = origin + frameTime(index, format);
                generation = state.generation;
                ++resyncs;
                havePrevious = false;
            }
            Slot* free = nullptr;
            for (auto& slot : slots) {
                bool expected = false;
                if (slot.busy.compare_exchange_strong(expected, true)) { free = &slot; break; }
            }
            if (!free) throw std::runtime_error("SDI frame pool exhausted");
            const bool fresh = receiver.buffer.picture(target, period / 3, previous);
            if (fresh) havePrevious = true;
            void* data = nullptr;
            check(free->frame->GetBytes(&data), "Access SDI frame");
            if (havePrevious && signal) {
                std::memcpy(data, previous.pixels.data(), previous.pixels.size());
                if (!fresh) ++repeated;
            } else {
                auto pixels = static_cast<uint8_t*>(data);
                for (int i = 0; i < format.width * format.height * 2; i += 4) { pixels[i] = 128; pixels[i + 1] = 16; pixels[i + 2] = 128; pixels[i + 3] = 16; }
                ++black;
            }
            const int samples = static_cast<int>(audioPosition(index + 1, format) - audioPosition(index, format));
            free->audio = receiver.buffer.sound(target, samples, silentSamples);
            if (!signal || audioOut.off) std::fill(free->audio.begin(), free->audio.end(), 0);
            if (audioChannels > 2 || audioOut.mono) { // route to the chosen SDI channels, all others stay silent
                std::vector<int32_t> wide(static_cast<size_t>(samples) * audioChannels, 0);
                for (int i = 0; i < samples; ++i) {
                    const int64_t left = free->audio[static_cast<size_t>(i) * 2], right = free->audio[static_cast<size_t>(i) * 2 + 1];
                    int32_t* frame = &wide[static_cast<size_t>(i) * audioChannels];
                    if (audioOut.mono) frame[audioOut.firstChannel] = static_cast<int32_t>((left + right) / 2); // mono mix on one channel
                    else { frame[audioOut.firstChannel] = static_cast<int32_t>(left); frame[audioOut.firstChannel + 1] = static_cast<int32_t>(right); }
                }
                free->audio = std::move(wide);
            }
            unsigned int written = 0;
            check(output->ScheduleAudioSamples(free->audio.data(), samples, static_cast<__int64>(audioPosition(index, format)), 48000, &written),
                  "Schedule SDI audio");
            if (written != static_cast<unsigned int>(samples)) throw std::runtime_error("SDI audio buffer accepted only a partial packet");
            check(output->ScheduleVideoFrame(free->frame, index * format.rateD, format.rateD, format.rateN), "Schedule SDI video");
            ++index;
            ++scheduled;
            ++pending;
        }
    }
    void start(Receiver& receiver) {
        auto state = receiver.buffer.state();
        origin = state.last - frameTime(bufferFrames - 1, format);
        generation = state.generation;
        fill(receiver);
        check(output->EndAudioPreroll(), "End audio preroll");
        check(output->StartScheduledPlayback(0, format.rateN, 1.0), "Start SDI playback");
        started = true;
    }
    void tick(Receiver& receiver) {
        __int64 streamTime = 0;
        double speed = 0;
        check(output->GetScheduledStreamTime(format.rateN, &streamTime, &speed), "Read SDI clock");
        const int64_t frame = streamTime / format.rateD;
        if (frame >= index) { // The card ran past what was scheduled: skip ahead and re-anchor.
            index = frame + bufferFrames;
            auto state = receiver.buffer.state();
            origin = state.last - 2 * frameTime(1, format) - frameTime(index, format);
            ++resyncs;
        }
        fill(receiver);
        std::unique_lock lock(wakeMutex);
        wake.wait_for(lock, std::chrono::milliseconds(2));
    }
    unsigned int buffered() {
        unsigned int value = 0;
        check(output->GetBufferedVideoFrameCount(&value), "Read buffered frames");
        return value;
    }
    void close() noexcept {
        if (closed) return;
        closed = true;
        if (started) {
            __int64 actual = 0;
            output->StopScheduledPlayback(0, &actual, format.rateN);
            std::unique_lock lock(wakeMutex);
            wake.wait_for(lock, std::chrono::seconds(2), [this] { return stopped.load(); });
        }
        if (audioEnabled) output->DisableAudioOutput();
        if (videoEnabled) output->DisableVideoOutput();
        output->SetScheduledFrameCompletionCallback(nullptr);
    }
    ~Player() { close(); }
};

// One playout session in one format. Returns true when the incoming format changed (the caller starts a new session).
bool playSession(Receiver& receiver, IDeckLink* device, const IncomingFormat& incoming, int seconds, int depth, const AudioOut& audioOut, int64_t& startedAt) {
    CComPtr<IDeckLinkOutput_v14_2_1> output;
    check(device->QueryInterface(__uuidof(IDeckLinkOutput_v14_2_1), reinterpret_cast<void**>(&output)), "DeckLink output interface");
    const ModeInfo mode = pickOutputMode(output, incoming);
    std::cout << "Output mode: " << mode.name << " for incoming " << incoming.text() << "\n";
    receiver.expect(incoming);
    auto begin = Capture::nowMs();
    while (running && receiver.buffer.state().count < static_cast<size_t>(depth)) {
        receiver.checkError();
        if (stopRequested()) { running = false; return false; }
        if (receiver.formatChanged()) return true;
        if (Capture::nowMs() - begin > 15000) throw std::runtime_error("Not enough OMT video received to start the SDI output");
        std::this_thread::sleep_for(std::chrono::milliseconds(10));
    }
    if (!running) return false;
    CComPtr<Player> player;
    player.Attach(new Player(output, mode, depth, audioOut));
    try { player->initialize(); player->start(receiver); } catch (...) { player->close(); throw; }
    std::cout << "Started scheduled SDI output\n";
    if (!startedAt) startedAt = Capture::nowMs();
    auto lastReport = Capture::nowMs();
    bool changed = false;
    try {
        while (running && (seconds == 0 || Capture::nowMs() - startedAt < static_cast<int64_t>(seconds) * 1000)) {
            receiver.checkError();
            if (stopRequested()) { running = false; break; }
            if (receiver.formatChanged()) { changed = true; break; }
            player->tick(receiver);
            auto now = Capture::nowMs();
            if (now - lastReport >= 1000) {
                lastReport = now;
                auto state = receiver.buffer.state();
                std::cout << "Health={\"mode\":" << json(mode.name) << ",\"receivedVideo\":" << receiver.videos << ",\"receivedAudio\":" << receiver.audios
                          << ",\"sourceAgeMs\":" << (receiver.lastVideo ? now - receiver.lastVideo.load() : -1) << ",\"queueFrames\":" << state.count
                          << ",\"queueDrops\":" << state.discarded << ",\"completed\":" << player->completed << ",\"late\":" << player->late
                          << ",\"dropped\":" << player->dropped << ",\"repeated\":" << player->repeated << ",\"black\":" << player->black
                          << ",\"silentSamples\":" << player->silentSamples << ",\"resyncs\":" << player->resyncs
                          << ",\"buffered\":" << player->buffered() << "}\n";
            }
        }
    } catch (...) { player->close(); throw; }
    player->close();
    return changed;
}

// Where the audio goes on the SDI output: a stereo pair (firstChannel, firstChannel + 1) or, for "Mono n", the mix of both
// channels on channel n only. "Off" plays silence.
AudioOut parseOutputAudio(const std::string& text) {
    AudioOut result;
    if (text == "Off") { result.off = true; return result; }
    for (int pair = 0; pair < 4; ++pair)
        if (text == "Stereo " + std::to_string(pair * 2 + 1) + "&" + std::to_string(pair * 2 + 2)) { result.firstChannel = pair * 2; return result; }
    if (text.size() == 6 && text.starts_with("Mono ") && text[5] >= '1' && text[5] <= '8') {
        result.mono = true;
        result.firstChannel = text[5] - '1';
        return result;
    }
    throw std::runtime_error("SDI audio must be Off, Stereo 1&2, 3&4, 5&6, 7&8 or Mono 1..8");
}

void runPlay(int outputIndex, const std::string& address, int seconds, int depth, bool ndiSource, const std::string& audioText) {
    const AudioOut audioOut = parseOutputAudio(audioText);
    auto all = devices();
    IDeckLink* device = deviceAt(all, outputIndex);
    CComPtr<IDeckLinkStatus> status;
    device->QueryInterface(__uuidof(IDeckLinkStatus), reinterpret_cast<void**>(&status));
    __int64 busy = 0;
    if (status && status->GetInt(bmdDeckLinkStatusBusy, &busy) == S_OK && busy) throw std::runtime_error("Port is busy (used by another program)");
    if (!capable(device, bmdDeviceSupportsPlayback)) throw std::runtime_error("Selected device is not configured for playback; check Desktop Video Setup");
    Receiver receiver(address, ndiSource);
    int64_t startedAt = 0;
    do {
        const auto begin = Capture::nowMs();
        IncomingFormat incoming;
        while (running && !(incoming = receiver.seenFormat()).valid) {
            receiver.checkError();
            if (stopRequested()) return;
            if (Capture::nowMs() - begin > 15000) throw std::runtime_error(std::string("No ") + (ndiSource ? "NDI" : "OMT") + " video received within 15 seconds; no SDI output started");
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        }
        if (!running) break;
        // Let the sender settle: take the latest seen format after a short moment.
        std::this_thread::sleep_for(std::chrono::milliseconds(300));
        incoming = receiver.seenFormat();
        if (!playSession(receiver, device, incoming, seconds, depth, audioOut, startedAt)) break;
        std::cout << "Incoming format changed; restarting the output\n";
    } while (running);
    std::cout << "Stopped\n";
}

// Synthetic OMT sender for loop tests: a moving bar, shifted by 200 px on odd lines when interlaced (so both fields differ),
// and a 1 kHz tone at -20 dBFS.
void runTestSource(const std::string& name, int width, int height, int rateN, int rateD, bool interlaced, int seconds, bool ndiOutput) {
    std::unique_ptr<Sink> sink;
    if (ndiOutput) sink = std::make_unique<NdiSink>(name);
    else sink = std::make_unique<OmtSink>(name, OMTQuality_Default);
    std::cout << "Test source " << sink->describe() << ": " << width << "x" << height << " " << rateN << "/" << rateD << (interlaced ? " interlaced" : " progressive") << "\n";
    ModeInfo timing;
    timing.rateN = rateN;
    timing.rateD = rateD;
    std::vector<unsigned char> pixels(static_cast<size_t>(width) * height * 2);
    const auto begin = std::chrono::steady_clock::now();
    int64_t audioDone = 0;
    for (int64_t frameIndex = 0; running && !stopRequested(); ++frameIndex) {
        if (seconds && frameTime(frameIndex, timing) > static_cast<int64_t>(seconds) * 10000000) break;
        for (int y = 0; y < height; ++y) {
            const int shift = interlaced && (y & 1) ? 200 : 0;
            const int bar = static_cast<int>((frameIndex * 16 + shift) % width);
            unsigned char* row = pixels.data() + static_cast<size_t>(y) * width * 2;
            for (int x = 0; x < width; ++x) {
                const bool white = ((x - bar + width) % width) < 64;
                row[x * 2] = 128;
                row[x * 2 + 1] = white ? 235 : 64;
            }
        }
        Packet videoPacket;
        videoPacket.video = pixels; // the sinks read the picture from the packet
        OMTMediaFrame& v = videoPacket.frame;
        v.Type = OMTFrameType_Video;
        v.Codec = OMTCodec_UYVY;
        v.Width = width;
        v.Height = height;
        v.Stride = width * 2;
        v.Flags = interlaced ? OMTVideoFlags_Interlaced : OMTVideoFlags_None;
        v.FrameRateN = rateN;
        v.FrameRateD = rateD;
        v.AspectRatio = 16.0f / 9;
        v.ColorSpace = OMTColorSpace_BT709;
        v.Timestamp = frameTime(frameIndex, timing);
        v.Data = videoPacket.video.data();
        v.DataLength = static_cast<int>(pixels.size());
        sink->send(videoPacket);
        const int64_t endSample = audioPosition(frameIndex + 1, timing);
        const int count = static_cast<int>(endSample - audioDone);
        std::vector<float> tone(static_cast<size_t>(count) * 2);
        for (int i = 0; i < count; ++i) {
            const float s = 0.1f * static_cast<float>(std::sin(2.0 * 3.14159265358979 * 1000.0 * (audioDone + i) / 48000.0));
            tone[i] = tone[count + i] = s;
        }
        Packet audioPacket;
        audioPacket.audio = std::move(tone);
        OMTMediaFrame& a = audioPacket.frame;
        a.Type = OMTFrameType_Audio;
        a.Codec = OMTCodec_FPA1;
        a.SampleRate = 48000;
        a.Channels = 2;
        a.SamplesPerChannel = count;
        a.Timestamp = audioDone * 10000000 / 48000;
        a.Data = audioPacket.audio.data();
        a.DataLength = count * 2 * 4;
        sink->send(audioPacket);
        audioDone = endSample;
        std::this_thread::sleep_until(begin + std::chrono::nanoseconds(frameTime(frameIndex + 1, timing) * 100));
    }
}

void listSources() {
    std::this_thread::sleep_for(std::chrono::seconds(2));
    int count = 0;
    char** addresses = omt_discovery_getaddresses(&count);
    std::cout << '[';
    for (int i = 0; i < count; ++i) std::cout << (i ? "," : "") << json(addresses[i]);
    std::cout << "]\n";
}

void listNdiSources() {
    ndi::Runtime runtime;
    std::cout << '[';
    bool first = true;
    for (auto& name : ndi::findNdiSources(runtime, 3)) {
        std::cout << (first ? "" : ",") << json(name);
        first = false;
    }
    std::cout << "]\n";
}
