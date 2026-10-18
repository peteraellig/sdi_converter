// sdi_converter engine. First piece: what the card and its SDK report (devices, modes, detected input).
#include <Windows.h>
#include <atlbase.h>
#pragma warning(disable:4192) // The Windows RECT definition is intentionally reused.
#include "decklink_import.h"
#include "libomt.h"
#include "media_buffer.h"
#include "low_latency_queue.h"
#include "video_timing.h"
#include <array>
#include <atomic>
#include <chrono>
#include <cmath>
#include <condition_variable>
#include <cstring>
#include <limits>
#include <mutex>
#include <thread>
#include <iostream>
#include <memory>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

void check(HRESULT result, const char* action) {
    if (FAILED(result)) {
        char text[200];
        sprintf_s(text, "%s failed (0x%08lX)", action, result);
        throw std::runtime_error(text);
    }
}

struct ComScope {
    ComScope() { check(CoInitializeEx(nullptr, COINIT_MULTITHREADED), "COM initialization"); }
    ~ComScope() { CoUninitialize(); }
};

std::string utf8(BSTR text) {
    if (!text) return {};
    int size = WideCharToMultiByte(CP_UTF8, 0, text, SysStringLen(text), nullptr, 0, nullptr, nullptr);
    std::string result(size, '\0');
    WideCharToMultiByte(CP_UTF8, 0, text, SysStringLen(text), result.data(), size, nullptr, nullptr);
    return result;
}

std::string json(const std::string& value) {
    std::string result = "\"";
    const char hex[] = "0123456789abcdef";
    for (unsigned char c : value) {
        if (c == '"' || c == '\\') { result += '\\'; result += static_cast<char>(c); }
        else if (c < 32) { result += "\\u00"; result += hex[c >> 4]; result += hex[c & 15]; }
        else result += static_cast<char>(c);
    }
    return result + '"';
}

std::string fourcc(unsigned int value) {
    std::string text(4, ' ');
    for (int i = 0; i < 4; ++i) text[i] = static_cast<char>((value >> (24 - 8 * i)) & 0xFF);
    return text;
}

const char* fieldName(int dominance) {
    switch (dominance) {
    case bmdLowerFieldFirst: return "lower";
    case bmdUpperFieldFirst: return "upper";
    case bmdProgressiveFrame: return "progressive";
    case bmdProgressiveSegmentedFrame: return "psf";
    default: return "unknown";
    }
}

std::vector<CComPtr<IDeckLink>> devices() {
    CComPtr<IDeckLinkIterator> iterator;
    check(iterator.CoCreateInstance(__uuidof(CDeckLinkIterator)), "DeckLink enumeration (is Desktop Video installed?)");
    std::vector<CComPtr<IDeckLink>> result;
    for (;;) {
        CComPtr<IDeckLink> device;
        if (iterator->Next(&device) != S_OK) break;
        result.push_back(device);
    }
    return result;
}

bool capable(IDeckLink* device, int flag) {
    CComPtr<IDeckLinkProfileAttributes> attributes;
    __int64 support = 0;
    if (SUCCEEDED(device->QueryInterface(__uuidof(IDeckLinkProfileAttributes), reinterpret_cast<void**>(&attributes))) &&
        SUCCEEDED(attributes->GetInt(BMDDeckLinkVideoIOSupport, &support)))
        return (support & flag) != 0;
    return false;
}

std::string deviceName(IDeckLink* device) {
    CComBSTR name;
    check(device->GetDisplayName(&name), "Read device name");
    return utf8(name);
}

IDeckLink* deviceAt(std::vector<CComPtr<IDeckLink>>& all, int index) {
    if (index < 0 || index >= static_cast<int>(all.size())) throw std::runtime_error("Invalid device index; use --list");
    return all[index];
}

// "Detected" is what the driver reports without opening the input: signal lock and the detected mode.
std::string detectedMode(IDeckLink* device, IDeckLinkInput_v14_2_1* input, bool& locked) {
    locked = false;
    CComPtr<IDeckLinkStatus> status;
    if (FAILED(device->QueryInterface(__uuidof(IDeckLinkStatus), reinterpret_cast<void**>(&status))) || !status) return {};
    long flag = 0;
    if (status->GetFlag(bmdDeckLinkStatusVideoInputSignalLocked, &flag) != S_OK || !flag) return {};
    locked = true;
    __int64 detected = 0;
    if (status->GetInt(bmdDeckLinkStatusDetectedVideoInputMode, &detected) != S_OK || detected == bmdModeUnknown || !input) return {};
    CComPtr<IDeckLinkDisplayMode> mode;
    CComBSTR name;
    if (input->GetDisplayMode(static_cast<BMDDisplayMode>(detected), &mode) == S_OK && mode && mode->GetName(&name) == S_OK) return utf8(name);
    return {};
}

void listDevices() {
    auto all = devices();
    std::cout << '[';
    for (size_t i = 0; i < all.size(); ++i) {
        if (i) std::cout << ',';
        CComPtr<IDeckLinkStatus> status;
        all[i]->QueryInterface(__uuidof(IDeckLinkStatus), reinterpret_cast<void**>(&status));
        __int64 busy = 0;
        bool busyKnown = status && status->GetInt(bmdDeckLinkStatusBusy, &busy) == S_OK;
        CComPtr<IDeckLinkInput_v14_2_1> input;
        all[i]->QueryInterface(__uuidof(IDeckLinkInput_v14_2_1), reinterpret_cast<void**>(&input));
        bool locked = false;
        auto mode = detectedMode(all[i], input, locked);
        std::cout << "{\"index\":" << i << ",\"name\":" << json(deviceName(all[i]))
                  << ",\"capture\":" << (capable(all[i], bmdDeviceSupportsCapture) ? "true" : "false")
                  << ",\"playback\":" << (capable(all[i], bmdDeviceSupportsPlayback) ? "true" : "false")
                  << ",\"busy\":" << (busyKnown ? std::to_string(busy) : "null")
                  << ",\"signal\":" << (locked ? "true" : "false")
                  << ",\"detectedMode\":" << (mode.empty() ? "null" : json(mode)) << '}';
    }
    std::cout << "]\n";
}

// Every mode the SDK offers for this device, with the answer of DoesSupportVideoMode for 8-bit YUV.
template <class Probe>
void printModes(IDeckLinkDisplayModeIterator* iterator, Probe supports) {
    std::cout << '[';
    bool first = true;
    for (;;) {
        CComPtr<IDeckLinkDisplayMode> mode;
        if (iterator->Next(&mode) != S_OK) break;
        CComBSTR name;
        __int64 duration = 0, scale = 0;
        mode->GetName(&name);
        mode->GetFrameRate(&duration, &scale);
        long ok = 0;
        BMDDisplayMode id = mode->GetDisplayMode();
        supports(id, ok);
        if (!first) std::cout << ',';
        first = false;
        std::cout << "{\"id\":" << json(fourcc(id)) << ",\"name\":" << json(utf8(name))
                  << ",\"width\":" << mode->GetWidth() << ",\"height\":" << mode->GetHeight()
                  << ",\"frameRateN\":" << scale << ",\"frameRateD\":" << duration
                  << ",\"fields\":\"" << fieldName(mode->GetFieldDominance()) << "\",\"supported\":" << (ok ? "true" : "false") << '}';
    }
    std::cout << ']';
}

void listModes(int index) {
    auto all = devices();
    IDeckLink* device = deviceAt(all, index);
    std::cout << "{\"name\":" << json(deviceName(device));
    CComPtr<IDeckLinkInput_v14_2_1> input;
    device->QueryInterface(__uuidof(IDeckLinkInput_v14_2_1), reinterpret_cast<void**>(&input));
    std::cout << ",\"input\":";
    if (input && capable(device, bmdDeviceSupportsCapture)) {
        CComPtr<IDeckLinkDisplayModeIterator> iterator;
        check(input->GetDisplayModeIterator(&iterator), "Input mode iterator");
        printModes(iterator, [&](BMDDisplayMode id, long& ok) {
            BMDDisplayMode actual = id;
            if (input->DoesSupportVideoMode(bmdVideoConnectionUnspecified, id, bmdFormat8BitYUV, bmdNoVideoInputConversion,
                                            bmdSupportedVideoModeDefault, &actual, &ok) != S_OK) ok = 0;
        });
    } else std::cout << "null";
    CComPtr<IDeckLinkOutput_v14_2_1> output;
    device->QueryInterface(__uuidof(IDeckLinkOutput_v14_2_1), reinterpret_cast<void**>(&output));
    std::cout << ",\"output\":";
    if (output && capable(device, bmdDeviceSupportsPlayback)) {
        CComPtr<IDeckLinkDisplayModeIterator> iterator;
        check(output->GetDisplayModeIterator(&iterator), "Output mode iterator");
        printModes(iterator, [&](BMDDisplayMode id, long& ok) {
            BMDDisplayMode actual = id;
            if (output->DoesSupportVideoMode(bmdVideoConnectionSDI, id, bmdFormat8BitYUV, bmdNoVideoOutputConversion,
                                             bmdSupportedVideoModeDefault, &actual, &ok) != S_OK) ok = 0;
        });
    } else std::cout << "null";
    std::cout << "}\n";
}

#include "ndi_runtime.h"
#include "sinks.h"
#include "capture.h"
#include "ndi_probe.h"
#include "playout.h"

} // namespace

int main(int argc, char** argv) {
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCtrlHandler(consoleHandler, TRUE);
    std::cout.setf(std::ios::unitbuf);
    int result = 0;
    try {
        ComScope scope;
        const std::string command = argc > 1 ? argv[1] : "";
        if (argc == 2 && command == "--list") listDevices();
        else if (argc == 3 && command == "--modes") listModes(std::stoi(argv[2]));
        else if (argc >= 3 && command == "--capture") // INDEX [NAME] [SECONDS] [QUALITY]
            runCapture(std::stoi(argv[2]), argc >= 4 ? argv[3] : "SDI Converter", argc >= 5 ? std::stoi(argv[4]) : 0, argc >= 6 ? argv[5] : "Normal", argc >= 7 ? argv[6] : "omt",
                       argc >= 8 ? argv[7] : "Stereo 1&2", argc >= 9 ? argv[8] : "abort");
        else if (argc >= 4 && command == "--play") // INDEX ADDRESS [SECONDS] [BUFFER] [omt|ndi]
            runPlay(std::stoi(argv[2]), argv[3], argc >= 5 ? std::stoi(argv[4]) : 0, argc >= 6 ? std::stoi(argv[5]) : 3,
                    argc >= 7 && std::string(argv[6]) == "ndi", argc >= 8 ? argv[7] : "Stereo 1&2");
        else if (argc == 2 && command == "--ndi-sources") listNdiSources();
        else if (argc >= 8 && command == "--test-source") // NAME WIDTH HEIGHT RATE_N RATE_D progressive|interlaced [SECONDS]
            runTestSource(argv[2], std::stoi(argv[3]), std::stoi(argv[4]), std::stoi(argv[5]), std::stoi(argv[6]),
                          std::string(argv[7]) == "interlaced", argc >= 9 ? std::stoi(argv[8]) : 0, argc >= 10 && std::string(argv[9]) == "ndi");
        else if (argc == 2 && command == "--sources") listSources();
        else if (argc >= 3 && command == "--ndi-probe") // NAME_PART [SECONDS]
            probeNdi(argv[2], argc >= 4 ? std::stoi(argv[3]) : 0);
        else if (argc >= 3 && command == "--probe") // ADDRESS [SECONDS]
            probeOmt(argv[2], argc >= 4 ? std::stoi(argv[3]) : 0);
        else {
            std::cout << "sdi_converter engine\n"
                         "  --list           devices, capabilities, busy state, detected input signal (JSON)\n"
                         "  --modes INDEX    modes the SDK reports for device INDEX, input and output (JSON)\n";
            if (argc > 1) result = 1;
        }
    } catch (const std::exception& e) {
        std::cerr << e.what() << "\n";
        result = 1;
    }
    return result;
}
