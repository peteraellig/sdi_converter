namespace SdiConverter;

// The layout lives in DiagnosticsForm.Designer.cs. The log TextBox belongs to MainForm and is only parked here while the window is open,
// so its (bounded) content survives closing and opening the window.
public partial class DiagnosticsForm : Form
{
    private readonly TextBox? log;

    public DiagnosticsForm()
    {
        InitializeComponent();
    }

    public DiagnosticsForm(string title, TextBox logBox) : this()
    {
        Text = title;
        titleLabel.Text = title;
        log = logBox;
        log.Dock = DockStyle.Fill;
        log.Visible = true;
        logHost.Controls.Add(log);
        log.SelectionStart = log.TextLength;
        log.ScrollToCaret();
    }

    private void closeButton_Click(object? sender, EventArgs e) => Close();

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        log?.Parent?.Controls.Remove(log);
        base.OnFormClosed(e);
    }
}
