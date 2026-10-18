// NDI runtime loaded at run time (not bundled; installed with NDI Tools). The structures follow the NDI SDK (NDIlib_*_t) and are
// declared here so no SDK header is needed. Included from main.cpp inside its anonymous namespace, before sinks.h.
#pragma once

namespace ndi {
struct SendSettings { const char* name; const char* groups; bool clockVideo; bool clockAudio; };
struct VideoFrame {
    int width, height;
    uint32_t fourcc;
    int rateN, rateD;
    float aspect;
    int formatType; // 1 progressive, 0 interleaved (woven fields), 2/3 single field
    int64_t timecode;
    uint8_t* data;
    int stride;
    const char* metadata;
    int64_t timestamp;
};
struct AudioFrame {
    int sampleRate, channels, samples;
    int64_t timecode;
    float* data;
    int channelStride;
    const char* metadata;
    int64_t timestamp;
};
struct Source { const char* name; const char* url; };
struct FindSettings { bool showLocal; const char* groups; const char* extraIps; };
struct RecvSettings { Source source; int colorFormat; int bandwidth; bool allowFields; const char* name; };

constexpr int colorFormatUyvyBgra = 1; // UYVY when there is no alpha
constexpr int bandwidthHighest = 100;
// NDI float audio has 1.0 at the +4 dBu reference, 20 dB below digital full scale (same convention as the FastVideo Bridge).
constexpr float audioGain = 10.0f;

class Runtime {
    HMODULE library = nullptr;
    template <class F> void bind(F& target, const char* name) {
        target = reinterpret_cast<F>(GetProcAddress(library, name));
        if (!target) throw std::runtime_error(std::string("NDI runtime lacks ") + name);
    }
    static std::wstring findPath() {
        std::vector<std::wstring> folders;
        for (const wchar_t* variable : {L"NDI_RUNTIME_DIR_V6", L"NDI_RUNTIME_DIR_V5"}) {
            wchar_t buffer[MAX_PATH]{};
            if (GetEnvironmentVariableW(variable, buffer, MAX_PATH) > 0) folders.emplace_back(buffer);
        }
        folders.emplace_back(L"C:\\Program Files\\NDI\\NDI 6 Tools\\Runtime");
        for (auto& folder : folders) {
            std::wstring path = folder + L"\\Processing.NDI.Lib.x64.dll";
            if (GetFileAttributesW(path.c_str()) != INVALID_FILE_ATTRIBUTES) return path;
        }
        throw std::runtime_error("NDI runtime not found: install NDI Tools from https://ndi.video/tools/");
    }

public:
    bool (*initialize)() = nullptr;
    void (*destroyLibrary)() = nullptr;
    void* (*sendCreate)(const SendSettings*) = nullptr;
    void (*sendDestroy)(void*) = nullptr;
    void (*sendVideo)(void*, const VideoFrame*) = nullptr;
    void (*sendAudio)(void*, const AudioFrame*) = nullptr;
    int (*sendConnections)(void*, uint32_t) = nullptr;
    const Source* (*sendSource)(void*) = nullptr;
    void* (*findCreate)(const FindSettings*) = nullptr;
    void (*findDestroy)(void*) = nullptr;
    bool (*findWait)(void*, uint32_t) = nullptr;
    const Source* (*findSources)(void*, uint32_t*) = nullptr;
    void* (*recvCreate)(const RecvSettings*) = nullptr;
    void (*recvDestroy)(void*) = nullptr;
    int (*recvCapture)(void*, VideoFrame*, AudioFrame*, void*, uint32_t) = nullptr; // 1 video, 2 audio
    void (*freeVideo)(void*, VideoFrame*) = nullptr;
    void (*freeAudio)(void*, AudioFrame*) = nullptr;

    Runtime() {
        try {
            library = LoadLibraryW(findPath().c_str());
            if (!library) throw std::runtime_error("NDI runtime could not be loaded");
            bind(initialize, "NDIlib_initialize");
            bind(destroyLibrary, "NDIlib_destroy");
            bind(sendCreate, "NDIlib_send_create");
            bind(sendDestroy, "NDIlib_send_destroy");
            bind(sendVideo, "NDIlib_send_send_video_v2");
            bind(sendAudio, "NDIlib_send_send_audio_v2");
            bind(sendConnections, "NDIlib_send_get_no_connections");
            bind(sendSource, "NDIlib_send_get_source_name");
            bind(findCreate, "NDIlib_find_create_v2");
            bind(findDestroy, "NDIlib_find_destroy");
            bind(findWait, "NDIlib_find_wait_for_sources");
            bind(findSources, "NDIlib_find_get_current_sources");
            bind(recvCreate, "NDIlib_recv_create_v3");
            bind(recvDestroy, "NDIlib_recv_destroy");
            bind(recvCapture, "NDIlib_recv_capture_v2");
            bind(freeVideo, "NDIlib_recv_free_video_v2");
            bind(freeAudio, "NDIlib_recv_free_audio_v2");
            if (!initialize()) throw std::runtime_error("NDI initialization failed (unsupported CPU?)");
        } catch (...) {
            if (library) { FreeLibrary(library); library = nullptr; } // the destructor does not run after a failed constructor
            throw;
        }
    }
    ~Runtime() { if (destroyLibrary) destroyLibrary(); if (library) FreeLibrary(library); }
    Runtime(const Runtime&) = delete;
    Runtime& operator=(const Runtime&) = delete;
};

// Names of the NDI sources currently visible (waits up to `seconds` for the first ones).
std::vector<std::string> findNdiSources(Runtime& runtime, int seconds) {
    FindSettings settings{true, nullptr, nullptr};
    void* finder = runtime.findCreate(&settings);
    if (!finder) throw std::runtime_error("NDI search could not start");
    std::vector<std::string> names;
    for (int attempt = 0; attempt < seconds; ++attempt) {
        runtime.findWait(finder, 1000);
        uint32_t count = 0;
        const Source* sources = runtime.findSources(finder, &count);
        names.clear();
        for (uint32_t i = 0; i < count; ++i) names.emplace_back(sources[i].name);
    }
    runtime.findDestroy(finder);
    return names;
}
} // namespace ndi
