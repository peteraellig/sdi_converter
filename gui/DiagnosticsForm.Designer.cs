#nullable disable
namespace SdiConverter
{
    partial class DiagnosticsForm
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
            logHost = new Panel();
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
            titleLabel.Size = new Size(850, 44);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Log";
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // logHost
            //
            logHost.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logHost.Location = new Point(12, 56);
            logHost.Name = "logHost";
            logHost.Size = new Size(826, 288);
            logHost.TabIndex = 1;
            //
            // closeButton
            //
            closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            closeButton.Location = new Point(738, 354);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(100, 34);
            closeButton.TabIndex = 2;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            //
            // DiagnosticsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeButton;
            ClientSize = new Size(850, 400);
            Controls.Add(titleLabel);
            Controls.Add(logHost);
            Controls.Add(closeButton);
            MinimumSize = new Size(650, 280);
            Name = "DiagnosticsForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Log";
            ResumeLayout(false);
        }

        #endregion

        private Label titleLabel;
        private Panel logHost;
        private Button closeButton;
    }
}
