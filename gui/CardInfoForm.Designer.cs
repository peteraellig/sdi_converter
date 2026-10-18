#nullable disable
namespace SdiConverter
{
    partial class CardInfoForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            titleLabel = new Label();
            summaryLabel = new Label();
            ports = new ListView();
            columnNumber = new ColumnHeader();
            columnDevice = new ColumnHeader();
            columnSignal = new ColumnHeader();
            columnFormat = new ColumnHeader();
            columnUse = new ColumnHeader();
            noteLabel = new Label();
            closeButton = new Button();
            SuspendLayout();
            //
            // titleLabel
            //
            titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            titleLabel.BackColor = Color.Black;
            titleLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Location = new Point(0, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Padding = new Padding(12, 0, 0, 0);
            titleLabel.Size = new Size(720, 44);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "BM card info";
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // summaryLabel
            //
            summaryLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            summaryLabel.Location = new Point(12, 54);
            summaryLabel.Name = "summaryLabel";
            summaryLabel.Size = new Size(696, 20);
            summaryLabel.TabIndex = 1;
            summaryLabel.Text = "Reading DeckLink status …";
            //
            // ports
            //
            ports.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ports.Columns.AddRange(new ColumnHeader[] { columnNumber, columnDevice, columnSignal, columnFormat, columnUse });
            ports.FullRowSelect = true;
            ports.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            ports.Location = new Point(12, 80);
            ports.MultiSelect = false;
            ports.Name = "ports";
            ports.Size = new Size(696, 190);
            ports.TabIndex = 2;
            ports.UseCompatibleStateImageBehavior = false;
            ports.View = View.Details;
            //
            // columnNumber
            //
            columnNumber.Text = "#";
            columnNumber.Width = 32;
            //
            // columnDevice
            //
            columnDevice.Text = "Device";
            columnDevice.Width = 220;
            //
            // columnSignal
            //
            columnSignal.Text = "Signal";
            columnSignal.Width = 110;
            //
            // columnFormat
            //
            columnFormat.Text = "Format";
            columnFormat.Width = 140;
            //
            // columnUse
            //
            columnUse.Text = "Use";
            columnUse.Width = 170;
            //
            // noteLabel
            //
            noteLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            noteLabel.Location = new Point(12, 272);
            noteLabel.Name = "noteLabel";
            noteLabel.Size = new Size(590, 54);
            noteLabel.TabIndex = 3;
            noteLabel.Text = "Numbers follow the DeckLink port order. Physical connector mapping: Desktop Video Setup. Not detected reflects the driver's input status (an idle input reports a default NTSC mode); no input is opened for this check.";
            //
            // closeButton
            //
            closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            closeButton.Location = new Point(608, 280);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(100, 34);
            closeButton.TabIndex = 4;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            //
            // CardInfoForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeButton;
            ClientSize = new Size(720, 330);
            Controls.Add(titleLabel);
            Controls.Add(summaryLabel);
            Controls.Add(ports);
            Controls.Add(noteLabel);
            Controls.Add(closeButton);
            MinimumSize = new Size(640, 300);
            Name = "CardInfoForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "BM card info";
            ResumeLayout(false);
        }

        #endregion

        private Label titleLabel;
        private Label summaryLabel;
        private ListView ports;
        private ColumnHeader columnNumber;
        private ColumnHeader columnDevice;
        private ColumnHeader columnSignal;
        private ColumnHeader columnFormat;
        private ColumnHeader columnUse;
        private Label noteLabel;
        private Button closeButton;
    }
}
