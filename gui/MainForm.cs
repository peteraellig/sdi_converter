using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace SdiConverter;

// The layout lives in MainForm.Designer.cs (editable in the Visual Studio designer); this file holds the behaviour.
public partial class MainForm : Form
{
    private readonly AppSettings settings = AppSettings.Load();
    private readonly EngineSession? capture;
    private readonly EngineSession? play;
    // The logs are owned here (not by the designer) and shown in their own window on request.
    private readonly TextBox captureLog = NewLog();
    private readonly TextBox playLog = NewLog();
    private DiagnosticsForm? captureDiagnostics, playDiagnostics;
    private CardInfoForm? cardInfo;
    private readonly SystemLoad systemLoad = new();
    private readonly System.Windows.Forms.Timer loadTimer = new() { Interval = 1000 };

    private static TextBox NewLog() => new()
    {
        Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9F),
        BackColor = SystemColors.Window, Visible = false,
    };

    public MainForm()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return; // the designer only needs the layout

        var version = typeof(MainForm).Assembly.GetName().Version;
        Text = $"SDI Converter  |  {version}  |  Peter Aellig";
        int link = footer.Text.IndexOf("https://", StringComparison.Ordinal);
        footer.LinkArea = new LinkArea(link, "https://ndi.video/".Length);

        captureOutputBox.SelectedItem = captureOutputBox.Items.Contains(settings.Output) ? settings.Output : "OMT";
        captureQualityBox.SelectedItem = captureQualityBox.Items.Contains(settings.Quality) ? settings.Quality : "Normal";
        captureNameBox.Text = settings.SenderName;
        playKindBox.SelectedItem = playKindBox.Items.Contains(settings.PlayKind) ? settings.PlayKind : "OMT";
        playSourceBox.Text = settings.OmtSource ?? "";
        playBufferBox.Value = Math.Clamp(settings.BufferFrames, 3, 8);
        captureAudioBox.SelectedItem = captureAudioBox.Items.Contains(settings.AudioIn) ? settings.AudioIn : "Stereo 1&2";
        captureDuplicateBox.SelectedItem = captureDuplicateBox.Items.Contains(settings.Duplicate) ? settings.Duplicate : "Abort";
        playAudioBox.SelectedItem = playAudioBox.Items.Contains(settings.AudioOut) ? settings.AudioOut : "Stereo 1&2";

        capture = new EngineSession(this, captureStartButton, captureStatus, captureFormat, captureDetail, captureLog,
            BuildCaptureArgs, CaptureHealth, null,
            enabled => captureDeviceBox.Enabled = captureRefreshButton.Enabled = captureNameBox.Enabled = captureOutputBox.Enabled = captureQualityBox.Enabled =
                captureAudioBox.Enabled = captureDuplicateBox.Enabled = enabled);
        play = new EngineSession(this, playStartButton, playStatus, playFormat, playDetail, playLog,
            BuildPlayArgs, PlayHealth, PlayLine,
            enabled => playDeviceBox.Enabled = playRefreshButton.Enabled = playKindBox.Enabled = playSourceBox.Enabled = playFindButton.Enabled = playBufferBox.Enabled = playAudioBox.Enabled = enabled);

        playKindBox.SelectedIndexChanged += (_, _) => { playSourceBox.Items.Clear(); playSourceBox.Text = ""; };
        loadTimer.Tick += (_, _) => UpdateLoad();
        loadTimer.Start();
        Shown += async (_, _) => { await RefreshDevicesAsync(); };
        FormClosing += MainForm_FormClosing;
    }

    private record Device(int Index, string Name, bool Busy)
    {
        public override string ToString() => Busy ? Name + "  (busy)" : Name;
    }

    // One exit path for the EXIT button and the X of the window: always ask, and stop running transmissions first.
    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (capture is null || play is null) return;
        if (e.CloseReason is CloseReason.WindowsShutDown or CloseReason.TaskManagerClosing or CloseReason.ApplicationExitCall) { StopAll(); return; }
        var running = capture.Running || play.Running;
        var question = running ? "A transmission is running. Stop it and exit SDI Converter?" : "Do you really want to exit SDI Converter?";
        if (MessageBox.Show(this, question, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        {
            e.Cancel = true;
            return;
        }
        StopAll();
    }

    private void UpdateLoad()
    {
        var running = capture!.Running ? capture : play!.Running ? play : null;
        var sample = systemLoad.Read(running?.Engine);
        SetCell(cpuLabel, sample.Cpu);
        SetCell(ramLabel, sample.Ram);
        SetCell(freeRamLabel, sample.FreeRam);
        SetCell(engineCpuLabel, sample.EngineCpu);
        SetCell(engineRamLabel, sample.EngineRam);
    }

    // The designer text of a cell is "title<newline>-"; the title stays in Tag.
    private static void SetCell(Label cell, string value) => cell.Text = cell.Tag + "\n" + value;

    private void OpenDiagnostics(ref DiagnosticsForm? form, string title, TextBox log)
    {
        if (form is { IsDisposed: false, Visible: true }) { form.Activate(); return; }
        var created = new DiagnosticsForm(title, log);
        form = created;
        created.Show(this);
    }

    private void btn_log_capture_Click(object? sender, EventArgs e) => OpenDiagnostics(ref captureDiagnostics, "Log · SDI → OMT / NDI", captureLog);

    private void btn_log_play_Click(object? sender, EventArgs e) => OpenDiagnostics(ref playDiagnostics, "Log · OMT / NDI → SDI", playLog);

    private void OpenCardInfo()
    {
        if (cardInfo is { IsDisposed: false }) { cardInfo.Activate(); return; }
        cardInfo = new CardInfoForm();
        cardInfo.Show(this);
    }

    private void btn_card_capture_Click(object? sender, EventArgs e) => OpenCardInfo();

    private void btn_card_play_Click(object? sender, EventArgs e) => OpenCardInfo();

    private void StopAll()
    {
        loadTimer.Stop();
        cardInfo?.Close();
        captureDiagnostics?.Close();
        playDiagnostics?.Close();
        Task.WaitAll(new[] { capture!.StopAsync(), play!.StopAsync() }, 10000);
        SaveSettings();
    }

    private void btn_exit_Click(object? sender, EventArgs e) => Close();

    private void btn_sdi2omt_Click(object? sender, EventArgs e) => new HelpForm(HelpTexts.CaptureTitle, HelpTexts.Capture).ShowDialog(this);

    private void btn_help_omt2sdi_Click(object? sender, EventArgs e) => new HelpForm(HelpTexts.PlayTitle, HelpTexts.Play).ShowDialog(this);

    private void SaveSettings()
    {
        if (captureDeviceBox.SelectedItem is Device c) settings.CaptureDevice = c.Name;
        if (playDeviceBox.SelectedItem is Device p) settings.PlayDevice = p.Name;
        settings.SenderName = captureNameBox.Text;
        settings.Output = captureOutputBox.SelectedItem?.ToString() ?? "OMT";
        settings.Quality = captureQualityBox.SelectedItem?.ToString() ?? "Normal";
        settings.PlayKind = playKindBox.SelectedItem?.ToString() ?? "OMT";
        settings.OmtSource = playSourceBox.Text.Trim();
        settings.BufferFrames = (int)playBufferBox.Value;
        settings.AudioIn = captureAudioBox.SelectedItem?.ToString() ?? "Stereo 1&2";
        settings.Duplicate = captureDuplicateBox.SelectedItem?.ToString() ?? "Abort";
        settings.AudioOut = playAudioBox.SelectedItem?.ToString() ?? "Stereo 1&2";
        settings.Save();
    }

    /// <summary>Shows an error whose text contains a web address: the address gets its own line and can be opened.</summary>
    public static void ShowErrorWithLink(IWin32Window owner, string message)
    {
        var at = message.IndexOf("https://", StringComparison.Ordinal);
        if (at < 0) { MessageBox.Show(owner, message, "SDI Converter", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var text = message[..at].TrimEnd(' ', ':', '-');
        var url = message[at..].Trim().TrimEnd('.', ' ');
        var answer = MessageBox.Show(owner, text + "\n\n" + url + "\n\nOpen this page now?", "SDI Converter", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (answer == DialogResult.Yes) Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private void footer_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e) =>
        Process.Start(new ProcessStartInfo("https://ndi.video/") { UseShellExecute = true });

    // ---- device lists -------------------------------------------------------------------------------------------

    private async Task RefreshDevicesAsync()
    {
        if (capture is null || play is null) return;
        try
        {
            using var doc = JsonDocument.Parse(await EngineProcess.QueryAsync("--list"));
            Fill(captureDeviceBox, doc.RootElement, "capture", settings.CaptureDevice);
            Fill(playDeviceBox, doc.RootElement, "playback", settings.PlayDevice);
            capture.Say($"{captureDeviceBox.Items.Count} SDI ports found");
            play.Say($"{playDeviceBox.Items.Count} SDI ports found");
        }
        catch (Exception ex)
        {
            capture.Say("Device detection failed: " + ex.Message);
            play.Say("Device detection failed: " + ex.Message);
        }
    }

    private static void Fill(ComboBox box, JsonElement devices, string capability, string? preferred)
    {
        box.Items.Clear();
        foreach (var d in devices.EnumerateArray())
            if (d.GetProperty(capability).GetBoolean())
            {
                var busy = d.GetProperty("busy");
                box.Items.Add(new Device(d.GetProperty("index").GetInt32(), d.GetProperty("name").GetString() ?? "?",
                    busy.ValueKind == JsonValueKind.Number && busy.GetInt64() != 0));
            }
        box.SelectedItem = box.Items.Cast<Device>().FirstOrDefault(d => d.Name == preferred) ?? (box.Items.Count > 0 ? box.Items[0] : null);
    }

    private async void captureRefreshButton_Click(object? sender, EventArgs e) => await RefreshDevicesAsync();
    private async void playRefreshButton_Click(object? sender, EventArgs e) => await RefreshDevicesAsync();

    private async void playFindButton_Click(object? sender, EventArgs e)
    {
        playFindButton.Enabled = false;
        var ndi = playKindBox.SelectedItem?.ToString() == "NDI";
        play!.Say(ndi ? "Searching for NDI sources …" : "Searching for OMT senders …");
        try
        {
            using var doc = JsonDocument.Parse(await EngineProcess.QueryAsync(ndi ? "--ndi-sources" : "--sources"));
            var current = playSourceBox.Text;
            playSourceBox.Items.Clear();
            foreach (var s in doc.RootElement.EnumerateArray()) playSourceBox.Items.Add(s.GetString() ?? "");
            playSourceBox.Text = current;
            if (string.IsNullOrWhiteSpace(current) && playSourceBox.Items.Count > 0) playSourceBox.SelectedIndex = 0;
            play.Say($"{playSourceBox.Items.Count} {(ndi ? "NDI sources" : "OMT senders")} found");
        }
        catch (Exception ex) { play.Say("Search failed: " + ex.Message.Split("https://")[0].Trim()); ShowErrorWithLink(this, ex.Message); }
        finally { playFindButton.Enabled = true; }
    }

    // ---- SDI -> OMT / NDI ---------------------------------------------------------------------------------------

    private async void captureStartButton_Click(object? sender, EventArgs e) => await capture!.ToggleAsync();
    private async void playStartButton_Click(object? sender, EventArgs e) => await play!.ToggleAsync();

    private string[]? BuildCaptureArgs()
    {
        if (captureDeviceBox.SelectedItem is not Device device) { capture!.Say("Select an SDI input first"); return null; }
        if (string.IsNullOrWhiteSpace(captureNameBox.Text)) { capture!.Say("Enter a sender name"); return null; }
        var kind = captureOutputBox.SelectedItem?.ToString() switch { "NDI" => "ndi", "OMT + NDI" => "omt+ndi", _ => "omt" };
        SaveSettings();
        var duplicate = captureDuplicateBox.SelectedItem?.ToString() switch
        {
            "Add a number" => "increment",
            "Retry until free" => "retry",
            "Retry 10 minutes, then stop" => "retry10",
            _ => "abort",
        };
        return new[] { "--capture", device.Index.ToString(), captureNameBox.Text.Trim(), "0", captureQualityBox.SelectedItem?.ToString() ?? "Normal", kind,
            captureAudioBox.SelectedItem?.ToString() ?? "Stereo 1&2", duplicate };
    }

    private static string Fps(int n, int d) => d > 0 ? (n / (double)d).ToString("0.###", CultureInfo.InvariantCulture) : "?";

    private void CaptureHealth(JsonElement h)
    {
        var signal = h.GetProperty("signal").GetBoolean();
        var connections = h.GetProperty("connections").GetInt32();
        var videoAge = h.GetProperty("videoAgeMs").GetInt64();
        if (!signal || videoAge < 0 || videoAge > 500) capture!.SetStatus("No SDI signal");
        else if (connections == 0) capture!.SetStatus("SDI present · waiting for a receiver");
        else capture!.SetStatus($"Sending · {connections} receiver(s)");
        captureFormat.Text = signal
            ? $"{h.GetProperty("mode").GetString()}  ·  {h.GetProperty("width").GetInt32()}×{h.GetProperty("height").GetInt32()}  ·  " +
              $"{Fps(h.GetProperty("frameRateN").GetInt32(), h.GetProperty("frameRateD").GetInt32())} fps" +
              (h.GetProperty("interlaced").GetBoolean() ? " interlaced" : " progressive") + "  (detected)"
            : "";
        captureDetail.Text =
            $"Pictures {h.GetProperty("captured").GetUInt64()}  ·  lost {h.GetProperty("videoLost").GetUInt64()}  ·  audio lost {h.GetProperty("audioLostSamples").GetUInt64()} samples  ·  " +
            $"errors {h.GetProperty("errors").GetUInt64()}  ·  format changes {h.GetProperty("formatChanges").GetUInt64()}\n" +
            $"Encode {h.GetProperty("encodeMs").GetInt32()} ms (avg {h.GetProperty("videoAverageMs").GetDouble():0.0}, " +
            $"P95 {h.GetProperty("videoP95Ms").GetDouble():0.0}, max {h.GetProperty("videoMaximumMs").GetDouble():0.0})";
    }

    // ---- OMT -> SDI ---------------------------------------------------------------------------------------------

    private string[]? BuildPlayArgs()
    {
        if (playDeviceBox.SelectedItem is not Device device) { play!.Say("Select an SDI output first"); return null; }
        if (string.IsNullOrWhiteSpace(playSourceBox.Text)) { play!.Say("Choose or type the sender (\"HOST (name)\")"); return null; }
        SaveSettings();
        var kind = playKindBox.SelectedItem?.ToString() == "NDI" ? "ndi" : "omt";
        return new[] { "--play", device.Index.ToString(), playSourceBox.Text.Trim(), "0", ((int)playBufferBox.Value).ToString(), kind,
            playAudioBox.SelectedItem?.ToString() ?? "Stereo 1&2" };
    }

    private void PlayLine(string line)
    {
        const string prefix = "Output mode: ";
        if (line.StartsWith(prefix, StringComparison.Ordinal)) playFormat.Text = line[prefix.Length..].Replace(" for incoming ", "   ←   ");
    }

    private void PlayHealth(JsonElement h)
    {
        var age = h.GetProperty("sourceAgeMs").GetInt64();
        var mode = h.GetProperty("mode").GetString();
        if (age < 0 || age > 500) play!.SetStatus("No video from the source · output shows black");
        else if (h.GetProperty("late").GetUInt64() > 0 || h.GetProperty("dropped").GetUInt64() > 0) play!.SetStatus($"Playing {mode} · late/dropped frames");
        else play!.SetStatus($"Playing {mode}");
        playDetail.Text =
            $"Received {h.GetProperty("receivedVideo").GetUInt64()} pictures  ·  late {h.GetProperty("late").GetUInt64()}  ·  dropped {h.GetProperty("dropped").GetUInt64()}  ·  " +
            $"repeated {h.GetProperty("repeated").GetUInt64()}  ·  black {h.GetProperty("black").GetUInt64()}  ·  resyncs {h.GetProperty("resyncs").GetUInt64()}\n" +
            $"Queue {h.GetProperty("queueFrames").GetInt32()}  ·  card buffer {h.GetProperty("buffered").GetInt32()}  ·  silent audio samples {h.GetProperty("silentSamples").GetUInt64()}";
    }

    private void MainForm_Load(object sender, EventArgs e)
    {

    }

    private void label3_Click(object sender, EventArgs e)
    {

    }
}

/// <summary>Runs one engine for one tab and keeps that tab's button, status and log in step. Holds no layout.</summary>
public sealed class EngineSession
{
    private readonly Control owner;
    private readonly Button button;
    private readonly Label status, format, detail;
    private readonly TextBox log;
    private readonly Func<string[]?> buildArgs;
    private readonly Action<JsonElement> onHealth;
    private readonly Action<string>? onLine;
    private readonly Action<bool> setInputsEnabled;
    private EngineProcess? engine;
    private bool stopping;
    private string lastError = "";

    public EngineSession(Control owner, Button button, Label status, Label format, Label detail, TextBox log,
        Func<string[]?> buildArgs, Action<JsonElement> onHealth, Action<string>? onLine, Action<bool> setInputsEnabled)
    {
        this.owner = owner; this.button = button; this.status = status; this.format = format; this.detail = detail; this.log = log;
        this.buildArgs = buildArgs; this.onHealth = onHealth; this.onLine = onLine; this.setInputsEnabled = setInputsEnabled;
    }

    public bool Running => engine is { IsRunning: true };
    public System.Diagnostics.Process? Engine => engine?.Process;

    // Only the text is set here; fonts and colours of the labels belong to the designer.
    public void SetStatus(string text) => status.Text = text;
    public void Say(string text) => SetStatus(text);

    private void Idle(string text)
    {
        button.Text = "Start";
        button.BackColor = Color.FromArgb(35, 125, 75);
        button.Enabled = true;
        setInputsEnabled(true);
        Say(text);
    }

    public Task ToggleAsync() => Running ? StopAsync() : StartAsync();

    private Task StartAsync()
    {
        var args = buildArgs();
        if (args is null) return Task.CompletedTask;
        log.Clear();
        format.Text = detail.Text = "";
        lastError = "";
        stopping = false;
        try
        {
            engine = new EngineProcess(args);
            engine.Line += line => owner.BeginInvoke(() => HandleLine(line));
            engine.Exited += code => owner.BeginInvoke(() => HandleExit(code));
            engine.Start();
        }
        catch (Exception ex) { engine = null; Idle("Could not start: " + ex.Message); return Task.CompletedTask; }
        button.Text = "Stop";
        button.BackColor = Color.FromArgb(190, 65, 55);
        setInputsEnabled(false);
        Say("Starting …");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (engine is null) return;
        stopping = true;
        button.Enabled = false;
        Say("Stopping …");
        await engine.StopAsync();
    }

    private void HandleLine(string line)
    {
        if (line.StartsWith("Health=", StringComparison.Ordinal))
        {
            try { using var doc = JsonDocument.Parse(line[7..]); onHealth(doc.RootElement); }
            catch (JsonException) { }
            return;
        }
        if (line.StartsWith("Error:", StringComparison.Ordinal)) lastError = line[6..].Trim();
        if (line.StartsWith("Notice:", StringComparison.Ordinal)) Say(line[7..].Trim());
        if (log.TextLength > 40000) log.Text = log.Text[^20000..];
        log.AppendText(line + Environment.NewLine);
        onLine?.Invoke(line);
    }

    private void HandleExit(int code)
    {
        engine?.Dispose();
        engine = null;
        if (stopping || code == 0) Idle("Stopped");
        else
        {
            // A web address in the message is shown on its own line in a dialog instead of being cut off in the status line.
            Idle("Stopped: " + (lastError.Length > 0 ? lastError.Split("https://")[0].Trim() : "engine ended with code " + code));
            if (lastError.Contains("https://", StringComparison.Ordinal)) MainForm.ShowErrorWithLink(owner, lastError);
        }
        stopping = false;
    }
}
