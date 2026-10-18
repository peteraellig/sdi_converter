# Third-party components

The project's original source code is licensed under [0BSD](LICENSE).

## Open Media Transport

The bundled Windows x64 SDK is Open Media Transport v1.0.0.19 (`third_party/omt/Libraries/Winx64`:
`libomt.h`, `libomt.lib`, `libomt.dll`, `libvmx.dll`). These files keep their upstream MIT license, see
[third_party/omt/LICENSE.txt](third_party/omt/LICENSE.txt). Keep this notice when redistributing them.

Upstream: https://github.com/openmediatransport/libomtnet and https://github.com/openmediatransport/libomt.

## NDI

NDI® is a registered trademark of Vizrt NDI AB (https://ndi.video/). The NDI runtime is not part of this
project: install NDI Tools from https://ndi.video/tools/ to use NDI features.

## Blackmagic Desktop Video

Installed separately by the user. Its driver and COM type library are not included; build-time COM
definitions are generated from the installed driver.

## Microsoft runtimes

The .NET runtime and the Visual C++ runtime keep their respective Microsoft terms.
