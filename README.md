# SDI Converter

Nachfolger von SDI2OMD-CONVERTER von Peter Aellig.

Windows-Anwendung für Blackmagic SDI zu OMT / NDI und OMT zu SDI. Die Oberfläche verwendet C# / .NET 8 Windows Forms; die Engine verwendet C++20.

## Voraussetzungen

- Windows x64
- Visual Studio 2026 mit C++- und .NET-Desktopentwicklung
- CMake 3.24 oder neuer
- Blackmagic Desktop Video (Treiber und DeckLink-API)
- Für NDI-Funktionen: NDI Tools separat installieren

## Erstellen

`SdiConverter.sln` in Visual Studio öffnen und die Anwendung im Release-Modus erstellen. Das GUI-Projekt baut die Engine automatisch mit CMake.

Die Engine lässt sich auch separat erstellen:

```powershell
cmake --preset windows-x64
cmake --build --preset release
```

Build-Dateien werden unter `%LOCALAPPDATA%\sdi_converter` abgelegt.

## ClickOnce-Paket

```powershell
.\scripts\publish-clickonce.ps1
```

Das Paket wird im lokalen Ordner `publish` abgelegt. Dieser Ordner und persönliche Visual-Studio-Einstellungen sind von Git ausgeschlossen.

## Lizenz

Eigener Quellcode: [0BSD](LICENSE). Hinweise zu mitgelieferten Komponenten: [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
