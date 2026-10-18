# SDI Converter

**Blackmagic SDI ↔ OMT / NDI for Windows** · by Peter Aellig

SDI Converter connects Blackmagic DeckLink ports to network video streams. An SDI input can be transmitted as **OMT**, **NDI**, or **OMT + NDI** simultaneously. In the opposite direction, the application receives an OMT or NDI stream and outputs video and audio through an SDI port.

This project is the successor to **SDI2OMD-CONVERTER**. The Windows Forms interface controls a shared C++ engine. Both directions can run simultaneously using separate, available SDI ports.

[**Download / Releases**](https://github.com/peteraellig/sdi_converter/releases) · [**Report an issue**](https://github.com/peteraellig/sdi_converter/issues)

## Features

| Area | Capabilities |
| --- | --- |
| SDI → Network | OMT, NDI, or OMT + NDI simultaneously |
| Network → SDI | Select an OMT or NDI source and output it over SDI |
| Video format | Automatic detection of resolution, frame rate, and progressive/interlaced video |
| Format changes | Capture or playback automatically restarts with the matching new format |
| OMT quality | Low, Normal, or High; independent of NDI |
| Audio | Select a stereo pair, a mono channel, or disable audio |
| Source discovery | Find OMT senders and NDI sources on the network |
| Playback buffer | Adjustable from 3 to 8 frames |
| Diagnostics | Device status, detected mode, counters, logs, and system load |
| Settings | Locally saved preferences |

## Requirements

To run the application:

- **Windows x64**.
- A Blackmagic DeckLink card with a suitable SDI input or output.
- The **Blackmagic Desktop Video** driver installed.
- For NDI: the NDI runtime installed with **[NDI Tools](https://ndi.video/tools/)**. It is not included in this project package.
- A network connection between sender and receiver.

The bundled OMT SDK is located in `third_party/omt`. The ClickOnce configuration targets a self-contained .NET deployment and includes a prerequisite for the Visual C++ x64 runtime. Development also requires Visual Studio and CMake; see below.

## Installation

1. Download the desired installation ZIP from [Releases](https://github.com/peteraellig/sdi_converter/releases).
2. Extract the entire ZIP to a local folder. Keep the directory structure, including `Application Files`, intact.
3. Install Blackmagic Desktop Video and, if needed, NDI Tools.
4. Run `setup.exe` from the extracted package and follow the installation steps.
5. Start SDI Converter and select the desired SDI port.

**About release 1.0.0.2:** The ZIP was taken from the existing local publishing folder. It was not rebuilt or tested with SDI hardware for the GitHub upload.

## Using SDI → OMT / NDI

1. In the capture tab, select the input under **SDI input**. **Refresh** reloads the device list.
2. Enter a **Sender name**. The sender appears on the network as `HOST (Name)`.
3. Under **Send as**, choose **OMT**, **NDI**, or **OMT + NDI**.
4. For OMT, select the desired **OMT quality**.
5. Select the embedded SDI audio channels under **Audio**.
6. Under **If name exists**, choose how to handle an OMT sender name that is already in use.
7. Click **Start** to begin transmission and check the status, format, and counters. Click **Stop** to finish.

You do not need to select a video format manually: the card reports the resolution, frame rate, and field order. If the input signal changes format, the application follows the new format automatically.

Options for duplicate OMT sender names include aborting, adding a number automatically, waiting until the name becomes available, or waiting for up to ten minutes.

## Using OMT / NDI → SDI

1. In the playback tab, select the output under **SDI output**.
2. Set **Source type** to **OMT** or **NDI**.
3. Click **Find senders** to discover sources, or enter the sender name directly.
4. Under **Buffer (frames)**, choose a buffer between **3 and 8 frames**. A larger buffer adds delay and provides more tolerance for irregular frame delivery.
5. Under **SDI audio**, select the desired output channel pair or mono channel.
6. Click **Start** to begin output. Click **Stop** to finish.

The first received frame determines the SDI output mode. The card must support a matching mode for the resolution, exact frame rate, and progressive/interlaced format. If the source changes format, output restarts in the matching mode.

**There is no scaling or frame rate conversion.** If no matching card mode is available, an error is displayed. If no video arrives for approximately half a second, output switches to black and silence.

## Audio routing

| Selection | SDI → Network | Network → SDI |
| --- | --- | --- |
| Stereo 1&2 / 3&4 / 5&6 / 7&8 | Send the selected SDI pair as network stereo | Output network stereo on the selected SDI pair |
| Mono 1 to 8 | Send the selected SDI channel on both network channels | Mix the left and right network channels onto the selected SDI channel |
| Off | Transmit without audio | Output silence |

Network audio uses **stereo, 48 kHz**. Unused SDI output channels remain silent. The NDI reference level is converted appropriately when sending and receiving.

## Video and latency

- Video processing uses **8-bit YUV 4:2:2 (UYVY)**.
- Frame rates reported by the device or stream are supported, including 23.98 / 29.97 / 59.94.
- Interlaced material retains both fields. For example, 1080i50 is represented as 25 full-height frames per second, flagged as interlaced.
- NDI fields are woven into complete frames for SDI playback.
- During capture, only the newest frame waits for encoding. If the encoder is too slow, the older waiting frame is discarded and counted.
- Playback follows the SDI card's clock. Missing frames are initially repeated.
- The configured playback buffer is only part of the overall delay; the card, encoder, network, and receiver also contribute.

## Status, logs, and troubleshooting

| Symptom | What to check |
| --- | --- |
| No SDI device found | Check the Desktop Video driver and card installation; reload the device list with Refresh |
| Port shown as `(busy)` | Check whether another application, such as vMix, is using the port |
| No SDI input signal | Check cabling, the signal source, and the detected input information |
| No network source found | Select the correct source type; check the source, network connection, and firewall permissions |
| NDI runtime missing | Install NDI Tools and restart the application |
| No matching SDI output mode | Compare the source format with the card's supported modes; no format conversion is performed |
| Many dropped or repeated frames | Check logs and system load; consider increasing the playback buffer |

The diagnostics windows display engine logs. Status and format indicators, together with counters, help identify signal loss, format changes, dropped frames, and buffer conditions.

Settings are saved per Windows user in `%APPDATA%\SDI Converter\settings.json`.

## Building from source

### Development environment

- Visual Studio **2026**, including Desktop development with C++, MSVC x64, ATL, and .NET desktop development.
- **.NET 8 SDK** for the Windows Forms interface.
- **CMake 3.24 or later**.
- Blackmagic Desktop Video on the build machine: DeckLink COM definitions are generated from the installed API.

The included CMake preset uses the `Visual Studio 18 2026` generator and the `x64` architecture.

### Visual Studio

Open `SdiConverter.sln` and build the GUI project in the **Release** configuration. The GUI project automatically configures and builds the C++ engine with CMake.

### Building the engine separately

Run these commands in the project folder:

```powershell
cmake --preset windows-x64
cmake --build --preset release
```

The engine and its OMT DLLs are then located in:

```text
%LOCALAPPDATA%\sdi_converter\build\Release\
```

The GUI uses separate build directories under `%LOCALAPPDATA%\sdi_converter\gui-bin` and `gui-obj`.

### Creating a ClickOnce package

```powershell
.\scripts\publish-clickonce.ps1
```

The script uses the full **Visual Studio MSBuild**, because the required ClickOnce tasks are not available through `dotnet publish`. It creates the package and copies it to the local `publish` folder.

The `publish` folder, intermediate build files, and personal Visual Studio settings are excluded from Git. Installation packages are provided through GitHub Releases.

## Engine command line

The interface starts `sdi_converter_engine.exe` as a separate process for each direction. The engine can also be used directly for diagnostics:

```powershell
# List devices and their status as JSON
.\sdi_converter_engine.exe --list

# Show supported modes for device index 0
.\sdi_converter_engine.exe --modes 0

# Discover OMT sources
.\sdi_converter_engine.exe --sources

# Discover NDI sources
.\sdi_converter_engine.exe --ndi-sources
```

Additional engine commands include `--capture`, `--play`, `--test-source`, `--probe`, and `--ndi-probe`. Their arguments are defined in the entry point in `src/main.cpp`.

## Project structure

| Path | Contents |
| --- | --- |
| `gui/` | C# Windows Forms interface, settings, and process management |
| `src/main.cpp` | Shared engine entry point and device queries |
| `src/capture.h` | SDI input and capture processing |
| `src/playout.h` | Network reception and scheduled SDI output |
| `src/sinks.h`, `src/ndi_runtime.h` | Network output and NDI integration |
| `third_party/omt/` | Bundled OMT SDK and license |
| `images/` | Logos and icons |
| `scripts/` | ClickOnce publishing |

## Limitations and testing status

- **4K and SD formats have not yet been tested, according to the built-in help.**
- NDI sources with an alpha channel are not supported.
- Available SDI modes depend on the card, port configuration, and driver.
- Uploading the project and installation package does not constitute hardware or functional testing.

When reporting an issue, please include the application version, card model, Desktop Video version, signal direction, source format, and relevant log excerpts.

## License and third-party components

The project's original source code is licensed under the **[0BSD license](LICENSE)**.

The bundled Open Media Transport SDK retains its MIT license. Blackmagic Desktop Video and the NDI runtime are installed separately. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for details.

NDI® is a registered trademark of Vizrt NDI AB.
