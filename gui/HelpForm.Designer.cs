#nullable disable
namespace SdiConverter
{
    partial class HelpForm
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
            helpBox = new TextBox();
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
            titleLabel.Size = new Size(760, 44);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Help";
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // helpBox
            //
            helpBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            helpBox.BackColor = Color.White;
            helpBox.BorderStyle = BorderStyle.None;
            helpBox.Font = new Font("Consolas", 10F);
            helpBox.Location = new Point(16, 56);
            helpBox.Multiline = true;
            helpBox.Name = "helpBox";
            helpBox.ReadOnly = true;
            helpBox.ScrollBars = ScrollBars.Vertical;
            helpBox.Size = new Size(728, 540);
            helpBox.TabIndex = 1;
            //
            // closeButton
            //
            closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            closeButton.Location = new Point(644, 610);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(100, 34);
            closeButton.TabIndex = 2;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            //
            // HelpForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeButton;
            ClientSize = new Size(760, 660);
            Controls.Add(titleLabel);
            Controls.Add(helpBox);
            Controls.Add(closeButton);
            MinimumSize = new Size(500, 400);
            Name = "HelpForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Help";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private TextBox helpBox;
        private Button closeButton;
    }
}
