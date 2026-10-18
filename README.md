# SDI Converter

**Blackmagic SDI ↔ OMT / NDI für Windows** · von Peter Aellig

SDI Converter verbindet Blackmagic-DeckLink-Anschlüsse mit Video-Streams im Netzwerk: Ein SDI-Eingang kann als **OMT**, **NDI** oder gleichzeitig als **OMT + NDI** gesendet werden. In der Gegenrichtung empfängt die Anwendung einen OMT- oder NDI-Stream und gibt Bild und Ton über einen SDI-Ausgang aus.

Das Projekt ist der Nachfolger von **SDI2OMD-CONVERTER**. Die Windows-Forms-Oberfläche steuert eine gemeinsame C++-Engine. Beide Richtungen können gleichzeitig auf getrennten, verfügbaren SDI-Anschlüssen laufen.

[**Download / Releases**](https://github.com/peteraellig/sdi_converter/releases) · [**Fehler melden**](https://github.com/peteraellig/sdi_converter/issues)

## Funktionen

| Bereich | Möglichkeiten |
| --- | --- |
| SDI → Netzwerk | OMT, NDI oder OMT + NDI gleichzeitig |
| Netzwerk → SDI | OMT- oder NDI-Quelle auswählen und auf SDI ausgeben |
| Videoformat | Automatische Erkennung von Auflösung, Bildrate und Progressive/Interlaced |
| Formatwechsel | Capture bzw. Wiedergabe startet automatisch im neuen passenden Format |
| OMT-Qualität | Low, Normal oder High; unabhängig von NDI |
| Audio | Auswahl eines Stereo-Paars, eines Mono-Kanals oder Audio aus |
| Quellensuche | OMT-Sender und NDI-Quellen im Netzwerk suchen |
| Wiedergabepuffer | 3 bis 8 Frames einstellbar |
| Diagnose | Gerätestatus, erkannter Modus, Zähler, Logs und Systemlast |
| Einstellungen | Lokale Speicherung der zuletzt gewählten Einstellungen |

## Voraussetzungen

Für den Betrieb:

- **Windows x64**.
- Eine Blackmagic-DeckLink-Karte mit geeignetem SDI-Ein- bzw. -Ausgang.
- Installierter **Blackmagic Desktop Video**-Treiber.
- Für NDI: die mit **[NDI Tools](https://ndi.video/tools/)** installierte NDI-Runtime. Diese gehört nicht zum Projektpaket.
- Eine Netzwerkverbindung zwischen Sender und Empfänger.

Das mitgelieferte OMT-SDK liegt unter `third_party/omt`. Die ClickOnce-Konfiguration ist für eine eigenständige .NET-Veröffentlichung ausgelegt und enthält eine Voraussetzung für die Visual-C++-Runtime x64. Für die Entwicklung werden zusätzlich Visual Studio und CMake benötigt; siehe unten.

## Installation

1. Unter [Releases](https://github.com/peteraellig/sdi_converter/releases) das gewünschte Installations-ZIP herunterladen.
2. Das vollständige ZIP in einen lokalen Ordner entpacken. Die Ordnerstruktur einschließlich `Application Files` beibehalten.
3. Blackmagic Desktop Video und bei Bedarf NDI Tools installieren.
4. `setup.exe` aus dem entpackten Paket starten und den Installationsschritten folgen.
5. SDI Converter starten und den gewünschten SDI-Anschluss auswählen.

**Zum bereitgestellten Release 1.0.0.2:** Das ZIP wurde aus dem vorhandenen lokalen Veröffentlichungsordner übernommen. Es wurde für den GitHub-Upload nicht neu erstellt oder mit SDI-Hardware getestet.

## Bedienung: SDI → OMT / NDI

1. Im Capture-Reiter unter **SDI input** den Eingang auswählen. **Refresh** liest die Geräteliste erneut ein.
2. Unter **Sender name** einen Namen vergeben. Im Netzwerk erscheint der Sender als `HOST (Name)`.
3. Unter **Send as** zwischen **OMT**, **NDI** und **OMT + NDI** wählen.
4. Bei OMT die gewünschte **OMT quality** einstellen.
5. Die eingebetteten SDI-Audiokanäle unter **Audio** auswählen.
6. Unter **If name exists** festlegen, wie mit einem bereits vorhandenen OMT-Sendernamen umgegangen wird.
7. Mit **Start** die Übertragung beginnen und Status, Format sowie Zähler prüfen. Mit **Stop** beenden.

Ein Videoformat muss nicht manuell gewählt werden: Die Karte meldet Auflösung, Bildrate und Halbbildverfahren. Bei einem Wechsel des Eingangssignals folgt die Anwendung dem neuen Format automatisch.

Für doppelte OMT-Sendernamen stehen Abbruch, automatische Nummerierung, Warten bis der Name frei wird sowie Warten mit einem Limit von zehn Minuten zur Verfügung.

## Bedienung: OMT / NDI → SDI

1. Im Wiedergabe-Reiter unter **SDI output** den Ausgang auswählen.
2. Unter **Source type** **OMT** oder **NDI** einstellen.
3. Mit **Find senders** nach Quellen suchen oder den Sendernamen direkt eingeben.
4. Unter **Buffer (frames)** einen Puffer zwischen **3 und 8 Frames** wählen. Ein größerer Puffer erhöht die Verzögerung und bietet mehr Reserve bei schwankender Anlieferung.
5. Unter **SDI audio** das gewünschte Ausgangskanalpaar bzw. den Mono-Kanal wählen.
6. Mit **Start** die Ausgabe starten. Mit **Stop** beenden.

Das erste empfangene Bild bestimmt den SDI-Ausgabemodus. Die Karte muss einen passenden Modus für Auflösung, exakte Bildrate und Progressive/Interlaced unterstützen. Bei einem Formatwechsel startet die Ausgabe im passenden Modus neu.

**Es gibt keine Skalierung und keine Bildratenkonvertierung.** Fehlt ein passender Kartenmodus, wird ein Fehler angezeigt. Bleibt das Videosignal ungefähr eine halbe Sekunde aus, schaltet die Ausgabe auf Schwarzbild und Stille.

## Audio-Routing

| Auswahl | SDI → Netzwerk | Netzwerk → SDI |
| --- | --- | --- |
| Stereo 1&2 / 3&4 / 5&6 / 7&8 | Gewähltes SDI-Paar als Netzwerk-Stereo senden | Netzwerk-Stereo auf das gewählte SDI-Paar ausgeben |
| Mono 1 bis 8 | Gewählten SDI-Kanal auf beide Netzwerkkanäle legen | Linken und rechten Netzwerkkanal auf den gewählten SDI-Kanal mischen |
| Off | Ohne Audio senden | Stille ausgeben |

Netzwerk-Audio arbeitet mit **Stereo, 48 kHz**. Nicht verwendete SDI-Ausgangskanäle bleiben stumm. Die NDI-Pegelreferenz wird beim Senden und Empfangen entsprechend umgesetzt.

## Video und Latenz

- Videoverarbeitung als **8-Bit YUV 4:2:2 (UYVY)**.
- Unterstützung der vom Gerät bzw. Stream gemeldeten Bildraten einschließlich 23,98 / 29,97 / 59,94.
- Interlaced-Material bleibt als Halbbildmaterial erhalten. Beispiel: 1080i50 entspricht 25 vollständigen Bildern pro Sekunde mit Interlaced-Kennzeichnung.
- NDI-Halbbilder werden für die SDI-Wiedergabe zu vollständigen Bildern zusammengesetzt.
- Beim Capture wartet nur das neueste Bild auf die Codierung. Ist der Encoder zu langsam, wird das ältere wartende Bild verworfen und gezählt.
- Die Wiedergabe wird durch den Takt der SDI-Karte gesteuert. Fehlende Bilder werden zunächst wiederholt.
- Der eingestellte Wiedergabepuffer ist nur ein Teil der gesamten Verzögerung; Karte, Encoder, Netzwerk und Empfänger tragen ebenfalls dazu bei.

## Status, Logs und Fehlersuche

| Beobachtung | Prüfen |
| --- | --- |
| Kein SDI-Gerät gefunden | Desktop-Video-Treiber, Karteninstallation und Geräteliste mit Refresh prüfen |
| Anschluss als `(busy)` angezeigt | Prüfen, ob eine andere Anwendung, etwa vMix, den Port verwendet |
| Kein SDI-Eingangssignal | Verkabelung, Signalquelle und erkannte Eingangsdaten kontrollieren |
| Keine Netzwerkquelle gefunden | Richtigen Quelltyp wählen; Quelle, Netzwerkverbindung und Firewall-Freigaben prüfen |
| NDI-Runtime fehlt | NDI Tools installieren und die Anwendung erneut starten |
| Kein passender SDI-Ausgabemodus | Quellformat mit den unterstützten Kartenmodi vergleichen; es findet keine Formatkonvertierung statt |
| Viele verlorene oder wiederholte Bilder | Logs und Systemlast prüfen; bei Wiedergabe gegebenenfalls den Puffer erhöhen |

Die Diagnosefenster zeigen die Engine-Logs. Status- und Formatanzeigen sowie Zähler helfen, Signalverlust, Formatwechsel, verworfene Bilder und Pufferzustände einzugrenzen.

Einstellungen werden pro Windows-Benutzer unter `%APPDATA%\SDI Converter\settings.json` gespeichert.

## Aus dem Quellcode erstellen

### Entwicklungsumgebung

- Visual Studio **2026**, einschließlich C++-Desktopentwicklung, MSVC x64, ATL und .NET-Desktopentwicklung.
- **.NET 8 SDK** für die Windows-Forms-Oberfläche.
- **CMake 3.24 oder neuer**.
- Blackmagic Desktop Video auf dem Build-Rechner: Die DeckLink-COM-Definitionen werden aus der installierten API erzeugt.

Das mitgelieferte CMake-Preset verwendet den Generator `Visual Studio 18 2026` und die Architektur `x64`.

### Visual Studio

`SdiConverter.sln` öffnen und das GUI-Projekt in der Konfiguration **Release** erstellen. Das GUI-Projekt konfiguriert und baut die C++-Engine automatisch mit CMake.

### Engine separat

Im Projektordner ausführen:

```powershell
cmake --preset windows-x64
cmake --build --preset release
```

Die Engine und ihre OMT-DLLs liegen danach unter:

```text
%LOCALAPPDATA%\sdi_converter\build\Release\
```

Die GUI verwendet getrennte Build-Verzeichnisse unter `%LOCALAPPDATA%\sdi_converter\gui-bin` und `gui-obj`.

### ClickOnce-Paket erstellen

```powershell
.\scripts\publish-clickonce.ps1
```

Das Skript verwendet das vollständige **Visual-Studio-MSBuild**, da die benötigten ClickOnce-Aufgaben nicht über `dotnet publish` bereitstehen. Es erstellt das Paket und kopiert es in den lokalen Ordner `publish`.

Der Ordner `publish`, Build-Zwischendateien und persönliche Visual-Studio-Einstellungen sind von Git ausgeschlossen. Fertige Installationspakete werden unter GitHub Releases bereitgestellt.

## Engine-Kommandozeile

Die Oberfläche startet `sdi_converter_engine.exe` als separaten Prozess pro Richtung. Für Diagnosezwecke kann die Engine auch direkt verwendet werden:

```powershell
# Geräte und deren Status als JSON ausgeben
.\sdi_converter_engine.exe --list

# Unterstützte Modi für Gerät mit Index 0 anzeigen
.\sdi_converter_engine.exe --modes 0

# OMT-Quellen suchen
.\sdi_converter_engine.exe --sources

# NDI-Quellen suchen
.\sdi_converter_engine.exe --ndi-sources
```

Weitere Engine-Befehle sind `--capture`, `--play`, `--test-source`, `--probe` und `--ndi-probe`. Deren Argumente sind im Einstiegspunkt `src/main.cpp` definiert.

## Projektaufbau

| Pfad | Inhalt |
| --- | --- |
| `gui/` | C#-Windows-Forms-Oberfläche, Einstellungen und Prozesssteuerung |
| `src/main.cpp` | Gemeinsamer Engine-Einstiegspunkt und Geräteabfragen |
| `src/capture.h` | SDI-Eingang und Capture-Verarbeitung |
| `src/playout.h` | Netzwerkempfang und zeitgesteuerte SDI-Ausgabe |
| `src/sinks.h`, `src/ndi_runtime.h` | Netzwerkausgabe und NDI-Anbindung |
| `third_party/omt/` | Mitgeliefertes OMT-SDK und Lizenz |
| `images/` | Logos und Symbole |
| `scripts/` | ClickOnce-Veröffentlichung |

## Grenzen und Teststand

- **4K- und SD-Formate sind laut integrierter Hilfe noch nicht getestet.**
- NDI-Quellen mit Alphakanal werden nicht unterstützt.
- Die tatsächlich nutzbaren SDI-Modi hängen von Karte, Port-Konfiguration und Treiber ab.
- Das Hochladen des Projekts und der Installationsdatei ersetzt keinen Hardware- oder Funktionstest.

Bei Fehlermeldungen bitte Programmversion, Kartenmodell, Desktop-Video-Version, Signalrichtung, Quellformat und relevante Log-Auszüge angeben.

## Lizenz und Fremdkomponenten

Der eigene Quellcode steht unter der **[0BSD-Lizenz](LICENSE)**.

Das mitgelieferte Open-Media-Transport-SDK behält seine MIT-Lizenz. Blackmagic Desktop Video und die NDI-Runtime werden separat installiert. Weitere Angaben stehen in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

NDI® ist eine eingetragene Marke von Vizrt NDI AB.
