// NDI receive probe for tests. Included from main.cpp after capture.h (needs running, stopRequested).
#pragma once

// Receives an NDI source by (part of) its name and prints what arrives once per second - for testing only.
void probeNdi(const std::string& nameContains, int seconds) {
    ndi::Runtime runtime;
    std::string chosen;
    for (int attempt = 0; attempt < 10 && chosen.empty(); ++attempt)
        for (auto& name : ndi::findNdiSources(runtime, 1))
            if (name.find(nameContains) != std::string::npos) chosen = name;
    if (chosen.empty()) throw std::runtime_error("NDI source not found: " + nameContains);
    ndi::RecvSettings settings{{chosen.c_str(), nullptr}, 100, ndi::bandwidthHighest, true, "sdi_converter probe"};
    void* receiver = runtime.recvCreate(&settings);
    if (!receiver) throw std::runtime_error("NDI receiver could not start");
    std::cout << "Receiving " << chosen << "\n";
    auto last = std::chrono::steady_clock::now();
    const auto begin = last;
    int video = 0, audio = 0;
    double squares = 0;
    int64_t sampleCount = 0;
    ndi::VideoFrame lastVideo{};
    float peak = 0;
    char fourcc[5]{};
    while (running && !stopRequested() && (seconds == 0 || std::chrono::steady_clock::now() - begin < std::chrono::seconds(seconds))) {
        ndi::VideoFrame v{};
        ndi::AudioFrame a{};
        int type = runtime.recvCapture(receiver, &v, &a, nullptr, 100);
        if (type == 1) {
            ++video;
            lastVideo = v;
            std::memcpy(fourcc, &v.fourcc, 4);
            runtime.freeVideo(receiver, &v);
        } else if (type == 2) {
            ++audio;
            for (int i = 0; i < a.samples; ++i) {
                squares += static_cast<double>(a.data[i]) * a.data[i];
                peak = std::max(peak, std::fabs(a.data[i]));
            }
            sampleCount += a.samples;
            runtime.freeAudio(receiver, &a);
        }
        if (std::chrono::steady_clock::now() - last >= std::chrono::seconds(1)) {
            last = std::chrono::steady_clock::now();
            std::cout << "NdiProbe={\"video\":" << video << ",\"audio\":" << audio << ",\"width\":" << lastVideo.width << ",\"height\":" << lastVideo.height
                      << ",\"fourcc\":\"" << fourcc << "\",\"frameRateN\":" << lastVideo.rateN << ",\"frameRateD\":" << lastVideo.rateD
                      << ",\"formatType\":" << lastVideo.formatType << ",\"audioRms\":" << (sampleCount ? std::sqrt(squares / sampleCount) : 0.0)
                      << ",\"audioPeak\":" << peak << "}\n";
            video = audio = 0;
            squares = 0;
            sampleCount = 0;
            peak = 0;
        }
    }
    runtime.recvDestroy(receiver);
}
