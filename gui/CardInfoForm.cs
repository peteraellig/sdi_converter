using System.ComponentModel;
using System.Text.Json;

namespace SdiConverter;

// The layout lives in CardInfoForm.Designer.cs. Shows what the Blackmagic driver reports for every SDI port; refreshed every 2 seconds
// while open. Nothing is opened or switched for this check (the engine only reads driver status).
public partial class CardInfoForm : Form
{
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 2000 };
    private readonly CancellationTokenSource lifetime = new();
    private bool polling;

    public CardInfoForm()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
        timer.Tick += async (_, _) => await RefreshAsync();
        Shown += async (_, _) => { await RefreshAsync(); if (!IsDisposed && !lifetime.IsCancellationRequested) timer.Start(); };
    }

    private void closeButton_Click(object? sender, EventArgs e) => Close();

    private async Task RefreshAsync()
    {
        if (polling || IsDisposed || lifetime.IsCancellationRequested) return;
        polling = true;
        try
        {
            var text = await EngineProcess.QueryAsync("--list");
            if (IsDisposed || lifetime.IsCancellationRequested) return;
            using var doc = JsonDocument.Parse(text);
            ports.BeginUpdate();
            try
            {
                var devices = doc.RootElement.EnumerateArray().ToArray();
                while (ports.Items.Count > devices.Length) ports.Items.RemoveAt(ports.Items.Count - 1);
                for (int i = 0; i < devices.Length; i++)
                {
                    var device = devices[i];
                    var busy = device.GetProperty("busy");
                    long? busyValue = busy.ValueKind == JsonValueKind.Number ? busy.GetInt64() : null;
                    string use = busyValue is null ? "Unavailable"
                        : (busyValue.Value & 3) == 3 ? "Capture + output"
                        : (busyValue.Value & 1) != 0 ? "Capture in use"
                        : (busyValue.Value & 2) != 0 ? "Output in use"
                        : busyValue.Value != 0 ? "Other use" : "Free";
                    var modeElement = device.GetProperty("detectedMode");
                    var mode = modeElement.ValueKind == JsonValueKind.String ? modeElement.GetString() : null;
                    // An idle input reports 525i59.94 NTSC as its default: that is "no signal", not a real picture.
                    var locked = device.GetProperty("signal").GetBoolean() && mode is not null && mode != "525i59.94 NTSC";
                    string[] cells =
                    {
                        (device.GetProperty("index").GetInt32() + 1).ToString(),
                        device.GetProperty("name").GetString() ?? "?",
                        locked ? "Detected" : "Not detected",
                        locked ? mode! : "—",
                        use,
                    };
                    if (i >= ports.Items.Count) ports.Items.Add(new ListViewItem(cells));
                    else for (int column = 0; column < cells.Length; column++) ports.Items[i].SubItems[column].Text = cells[column];
                    ports.Items[i].ForeColor = locked ? Color.FromArgb(35, 125, 75) : Color.DimGray;
                }
                summaryLabel.Text = devices.Length == 0 ? "No DeckLink devices found" : "DeckLink device status · updates every 2 seconds while open";
            }
            finally { ports.EndUpdate(); }
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or JsonException or System.ComponentModel.Win32Exception)
        {
            if (!IsDisposed && !lifetime.IsCancellationRequested) { summaryLabel.Text = "DeckLink status unavailable"; ports.Items.Clear(); }
        }
        finally { polling = false; }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer.Stop();
        lifetime.Cancel();
        base.OnFormClosed(e);
    }
}
