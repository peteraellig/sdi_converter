// Network outputs for the capture engine: OMT and NDI. Included from main.cpp inside its anonymous namespace
// (before capture.h); the NDI runtime is loaded at run time and is not bundled.
#pragma once

struct Sender {
    omt_send_t* handle;
    explicit Sender(const std::string& name, OMTQuality quality = OMTQuality_Default) : handle(omt_send_create(name.c_str(), quality)) {
        if (!handle) throw std::runtime_error("OMT sender creation failed");
    }
    ~Sender() { omt_send_destroy(handle); }
    Sender(const Sender&) = delete;
    Sender& operator=(const Sender&) = delete;
};

OMTQuality parseQuality(const std::string& quality) {
    if (quality == "High") return OMTQuality_High;
    if (quality == "Normal") return OMTQuality_Medium;
    if (quality == "Low") return OMTQuality_Low;
    throw std::runtime_error("Unsupported quality; choose High, Normal or Low");
}

// One captured video picture (UYVY) or audio chunk (stereo float planar), ready to send.
struct Packet {
    OMTMediaFrame frame{};
    std::vector<unsigned char> video;
    std::vector<float> audio;
};

class Sink {
public:
    virtual ~Sink() = default;
    // > 0 delivered, 0 nobody received it (not an error), < 0 failed.
    virtual int send(Packet& packet) = 0;
    virtual int connections() = 0;
    virtual std::string describe() = 0;
};

class OmtSink : public Sink {
    Sender sender;
public:
    OmtSink(const std::string& name, OMTQuality quality) : sender(name, quality) {}
    int send(Packet& packet) override { return omt_send(sender.handle, &packet.frame); }
    int connections() override { return omt_send_connections(sender.handle); }
    std::string describe() override {
        char address[1024]{};
        omt_send_getaddress(sender.handle, address, sizeof(address));
        return std::string("OMT ") + address;
    }
};

class NdiSink : public Sink {
    ndi::Runtime runtime;
    void* handle = nullptr;
    std::string label;
    std::vector<float> scaled;

public:
    explicit NdiSink(const std::string& name) {
        // The pictures are paced by the SDI input, so NDI must not clock them a second time.
        ndi::SendSettings settings{name.c_str(), nullptr, false, false};
        handle = runtime.sendCreate(&settings);
        if (!handle) throw std::runtime_error("NDI sender could not start");
        if (auto* source = runtime.sendSource(handle)) label = source->name ? source->name : name;
    }
    ~NdiSink() override { if (handle) runtime.sendDestroy(handle); }

    int send(Packet& packet) override {
        if (packet.frame.Type == OMTFrameType_Video) {
            ndi::VideoFrame video{};
            video.width = packet.frame.Width;
            video.height = packet.frame.Height;
            video.fourcc = 'U' | ('Y' << 8) | ('V' << 16) | ('Y' << 24);
            video.rateN = packet.frame.FrameRateN;
            video.rateD = packet.frame.FrameRateD;
            video.aspect = packet.frame.AspectRatio;
            // A woven interlaced picture is sent as interleaved fields.
            video.formatType = (packet.frame.Flags & OMTVideoFlags_Interlaced) ? 0 : 1;
            video.timecode = packet.frame.Timestamp;
            video.data = packet.video.data();
            video.stride = packet.frame.Stride;
            runtime.sendVideo(handle, &video); // synchronous: the buffer is free again when this returns
        } else {
            const int count = packet.frame.SamplesPerChannel;
            scaled.resize(packet.audio.size());
            for (size_t i = 0; i < scaled.size(); ++i) scaled[i] = packet.audio[i] * ndi::audioGain; // the packet stays untouched for other sinks
            ndi::AudioFrame audio{};
            audio.sampleRate = 48000;
            audio.channels = 2;
            audio.samples = count;
            audio.timecode = packet.frame.Timestamp;
            audio.data = scaled.data();
            audio.channelStride = count * 4;
            runtime.sendAudio(handle, &audio);
        }
        return runtime.sendConnections(handle, 0) > 0 ? 1 : 0;
    }
    int connections() override { return handle ? runtime.sendConnections(handle, 0) : 0; }
    std::string describe() override { return "NDI " + label; }
};

// Sends every picture/chunk to all its sinks.
class SinkGroup : public Sink {
    std::vector<std::unique_ptr<Sink>> sinks;
public:
    void add(std::unique_ptr<Sink> sink) { sinks.push_back(std::move(sink)); }
    int send(Packet& packet) override {
        int best = 0;
        for (auto& sink : sinks) {
            int result = sink->send(packet);
            if (result < 0) return result;
            best = std::max(best, result);
        }
        return best;
    }
    int connections() override {
        int total = 0;
        for (auto& sink : sinks) total += sink->connections();
        return total;
    }
    std::string describe() override {
        std::string text;
        for (auto& sink : sinks) text += (text.empty() ? "" : ", ") + sink->describe();
        return text;
    }
};

