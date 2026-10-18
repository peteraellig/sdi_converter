namespace SdiConverter;

// The layout lives in HelpForm.Designer.cs (editable in the designer); the texts are in HelpTexts below.
public partial class HelpForm : Form
{
    public HelpForm()
    {
        InitializeComponent();
    }

    public HelpForm(string title, string text) : this()
    {
        Text = title;
        titleLabel.Text = title;
        helpBox.Text = text.ReplaceLineEndings("\r\n"); // a TextBox needs CR LF, whatever line endings this source file has
        helpBox.Select(0, 0);
    }

    private void closeButton_Click(object? sender, EventArgs e) => Close();
}

/// <summary>The help texts of the two tabs.</summary>
public static class HelpTexts
{
    public const string CaptureTitle = "Help · SDI → OMT / NDI";
    public const string PlayTitle = "Help · OMT / NDI → SDI";

    public const string Capture =
@"WHAT IT DOES
Takes the picture and sound of one Blackmagic SDI input and sends them over the network as OMT (Open Media Transport), as NDI, or as both at the same time.

CONTROLS
SDI input        The SDI ports of the Blackmagic DeckLink cards found in this PC. ""(busy)"" means another program (for example vMix) is using that port.
Refresh          Reads the list of ports again.
Sender name      The name the stream is announced with. Receivers see ""HOST (name)"".
Send as          OMT, NDI, or OMT + NDI.
OMT quality      Compression quality of OMT (Low, Normal, High). Does not affect NDI.
Audio            Which embedded SDI audio goes to the network: Stereo 1&2, 3&4, 5&6 or 7&8, a single channel Mono 1 to 8 (sent on both left and right), or Off.
If name exists   What to do when an OMT stream with the same name is already on the network (the program never opens a second one with the same name): Abort, Add a number (name 2, name 3 ...), Retry until free, or Retry for 10 minutes and then stop.
Start / Stop     Starts or stops the transmission. The settings are locked while it runs.
Status line      No SDI signal, waiting for a receiver, or sending with the number of receivers.
Format line      The format the card has detected on the SDI input.
Counters         Pictures captured, pictures or audio lost, errors, format changes, and the time OMT/NDI needs to encode and send one picture.

SOURCE AUTO-DETECT
You do not choose a video format. The card reports what arrives on the SDI input (resolution, frame rate including 23.98 / 29.97 / 59.94, progressive or interlaced) and the program follows it. If the incoming format changes while running, capture restarts in the new format by itself; the receivers get the new format with the next picture.

TECHNICAL DETAILS
- Video: 8-bit YUV 4:2:2 (UYVY) from the card to OMT and NDI, without any colour conversion in this program.
- Interlaced material is sent as full-height woven pictures at the frame rate (1080i50 = 25 pictures per second, flagged interlaced); both fields stay intact.
- Audio: the chosen embedded SDI channels are sent as stereo, 48 kHz (Mono n is sent on both channels).
- Latency: only the newest picture waits for the encoder. If the encoder is too slow the oldest picture is dropped, never the newest, and the loss is counted.
- NDI uses the NDI runtime that is installed on this PC with NDI Tools. It is not part of this program. NDI audio uses the NDI reference level (digital full scale is sent 20 dB above 1.0, as usual in NDI).
- Timestamps stay continuous over a format change.
- Not tested: 4K and SD formats.

NDI® is a registered trademark of Vizrt NDI AB. https://ndi.video/" + EngineNote;

    public const string Play =
@"WHAT IT DOES
Receives an OMT or an NDI stream and plays it out on one Blackmagic SDI output.

CONTROLS
SDI output       The SDI ports of the DeckLink cards. ""(busy)"" means another program is using the port.
Refresh          Reads the list of ports again.
Source type      OMT or NDI.
Sender           The stream to play, written as ""HOST (name)"". Type it or pick it from the list.
Find senders     Searches the network for OMT senders or NDI sources (takes 2 to 3 seconds) and fills the list.
Buffer (frames)  3 to 8 pictures kept ready for the card. More frames play smoother but add delay. 3 is the lowest delay.
SDI audio        Where the stereo sound goes on the SDI output: Stereo 1&2, 3&4, 5&6 or 7&8, Mono 1 to 8 (left and right mixed onto that one channel), or Off (silence). All other channels stay silent.
Start / Stop     Starts or stops the output. The settings are locked while it runs.
Status line      Playing, or no video from the source (the output then shows black and silence).
Format line      The SDI mode that was chosen and the format it was chosen for.
Counters         Pictures received, late or dropped pictures, repeated pictures, black frames, resyncs, queue and card buffer.

SOURCE AUTO-DETECT
You do not choose an SDI mode. The first picture of the stream tells size, exact frame rate (including 23.98 / 29.97 / 59.94) and progressive or interlaced, and the program picks the matching output mode among the modes the card supports. If the stream changes format while running, the output restarts in the matching mode. If the card has no matching mode, an error is shown; there is no scaling and no frame rate conversion.

TECHNICAL DETAILS
- Video: 8-bit YUV 4:2:2 (UYVY), no colour conversion. Interlaced pictures are full-height woven frames played at the frame rate (25 frames = 1080i50). NDI delivers interlaced material as two fields; they are woven back into one picture.
- Audio: stereo, 48 kHz, played on SDI audio channels 1 and 2, aligned to the picture by timestamp. NDI audio is brought back from the NDI reference level to digital level.
- Playback is driven by the clock of the card (scheduled playback). Missing pictures are repeated; after half a second without video the output goes black and silent.
- NDI sources with an alpha channel are not supported.
- NDI uses the NDI runtime installed with NDI Tools; it is not part of this program.

NDI® is a registered trademark of Vizrt NDI AB. https://ndi.video/" + EngineNote;

    // Shown at the very bottom of both help texts.
    private const string EngineNote =
@"

ENGINE
There is a single engine for both directions: sdi_converter_engine.exe. The command it is started with decides the direction:
- SDI → OMT/NDI: --capture (first tab)
- OMT/NDI → SDI: --play (second tab)
- In addition there are helper commands such as --list, --modes, --sources and --ndi-sources, and the test tools --test-source, --probe and --ndi-probe.

The program starts a separate engine process for each tab. Both directions can therefore run at the same time, for example port 3 as input and port 2 as output, as in the loop test. Each process uses its own port, and a crash in one direction does not affect the other.
The source code is in the folder src: capture.h for the input side, playout.h for the output side, sinks.h and ndi_runtime.h for the network outputs, and main.cpp as the common entry point.";
}
