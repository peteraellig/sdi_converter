#nullable disable
namespace SdiConverter
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            captureDeviceLabel = new Label();
            captureDeviceBox = new ComboBox();
            captureRefreshButton = new Button();
            captureNameLabel = new Label();
            captureNameBox = new TextBox();
            captureOutputLabel = new Label();
            captureOutputBox = new ComboBox();
            captureAudioLabel = new Label();
            captureAudioBox = new ComboBox();
            captureDuplicateLabel = new Label();
            captureDuplicateBox = new ComboBox();
            captureQualityLabel = new Label();
            captureQualityBox = new ComboBox();
            captureStartButton = new Button();
            captureStatus = new Label();
            captureFormat = new Label();
            captureDetail = new Label();
            playDeviceLabel = new Label();
            playDeviceBox = new ComboBox();
            playRefreshButton = new Button();
            playKindLabel = new Label();
            playKindBox = new ComboBox();
            playSourceLabel = new Label();
            playSourceBox = new ComboBox();
            playFindButton = new Button();
            playAudioLabel = new Label();
            playAudioBox = new ComboBox();
            playBufferLabel = new Label();
            playBufferBox = new NumericUpDown();
            playStartButton = new Button();
            playStatus = new Label();
            playFormat = new Label();
            playDetail = new Label();
            tabs = new TabControl();
            captureTab = new TabPage();
            btn_sdi2omt = new Button();
            btn_log_capture = new Button();
            btn_card_capture = new Button();
            label2 = new Label();
            label1 = new Label();
            pictureBox2 = new PictureBox();
            pictureBox1 = new PictureBox();
            playTab = new TabPage();
            btn_help_omt2sdi = new Button();
            btn_log_play = new Button();
            btn_card_play = new Button();
            pictureBox4 = new PictureBox();
            pictureBox3 = new PictureBox();
            btn_exit = new Button();
            footer = new LinkLabel();
            cpuLabel = new Label();
            ramLabel = new Label();
            freeRamLabel = new Label();
            engineCpuLabel = new Label();
            engineRamLabel = new Label();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)playBufferBox).BeginInit();
            tabs.SuspendLayout();
            captureTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            playTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // captureDeviceLabel
            // 
            captureDeviceLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureDeviceLabel.Location = new Point(32, 169);
            captureDeviceLabel.Name = "captureDeviceLabel";
            captureDeviceLabel.Size = new Size(120, 20);
            captureDeviceLabel.TabIndex = 0;
            captureDeviceLabel.Text = "SDI input";
            captureDeviceLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // captureDeviceBox
            // 
            captureDeviceBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureDeviceBox.DropDownStyle = ComboBoxStyle.DropDownList;
            captureDeviceBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureDeviceBox.FormattingEnabled = true;
            captureDeviceBox.Location = new Point(156, 165);
            captureDeviceBox.Name = "captureDeviceBox";
            captureDeviceBox.Size = new Size(566, 29);
            captureDeviceBox.TabIndex = 0;
            // 
            // captureRefreshButton
            // 
            captureRefreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            captureRefreshButton.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            captureRefreshButton.Location = new Point(728, 164);
            captureRefreshButton.Name = "captureRefreshButton";
            captureRefreshButton.Size = new Size(104, 30);
            captureRefreshButton.TabIndex = 1;
            captureRefreshButton.Text = "Refresh";
            captureRefreshButton.UseVisualStyleBackColor = true;
            captureRefreshButton.Click += captureRefreshButton_Click;
            // 
            // captureNameLabel
            // 
            captureNameLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureNameLabel.Location = new Point(32, 209);
            captureNameLabel.Name = "captureNameLabel";
            captureNameLabel.Size = new Size(120, 20);
            captureNameLabel.TabIndex = 0;
            captureNameLabel.Text = "Sender name";
            captureNameLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // captureNameBox
            // 
            captureNameBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureNameBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureNameBox.Location = new Point(158, 206);
            captureNameBox.Name = "captureNameBox";
            captureNameBox.Size = new Size(566, 29);
            captureNameBox.TabIndex = 2;
            captureNameBox.Text = "SDI Converter";
            // 
            // captureOutputLabel
            // 
            captureOutputLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureOutputLabel.Location = new Point(32, 249);
            captureOutputLabel.Name = "captureOutputLabel";
            captureOutputLabel.Size = new Size(120, 20);
            captureOutputLabel.TabIndex = 0;
            captureOutputLabel.Text = "Send as";
            captureOutputLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // captureOutputBox
            // 
            captureOutputBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureOutputBox.DropDownStyle = ComboBoxStyle.DropDownList;
            captureOutputBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureOutputBox.FormattingEnabled = true;
            captureOutputBox.Items.AddRange(new object[] { "OMT", "NDI", "OMT + NDI" });
            captureOutputBox.Location = new Point(158, 246);
            captureOutputBox.Name = "captureOutputBox";
            captureOutputBox.Size = new Size(566, 29);
            captureOutputBox.TabIndex = 3;
            // 
            // captureAudioLabel
            // 
            captureAudioLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureAudioLabel.Location = new Point(32, 334);
            captureAudioLabel.Name = "captureAudioLabel";
            captureAudioLabel.Size = new Size(120, 20);
            captureAudioLabel.TabIndex = 31;
            captureAudioLabel.Text = "Audio";
            captureAudioLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // captureAudioBox
            // 
            captureAudioBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureAudioBox.DropDownStyle = ComboBoxStyle.DropDownList;
            captureAudioBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureAudioBox.FormattingEnabled = true;
            captureAudioBox.Items.AddRange(new object[] { "Stereo 1&2", "Stereo 3&4", "Stereo 5&6", "Stereo 7&8", "Mono 1", "Mono 2", "Mono 3", "Mono 4", "Mono 5", "Mono 6", "Mono 7", "Mono 8", "Off" });
            captureAudioBox.Location = new Point(158, 326);
            captureAudioBox.Name = "captureAudioBox";
            captureAudioBox.Size = new Size(566, 29);
            captureAudioBox.TabIndex = 20;
            // 
            // captureDuplicateLabel
            // 
            captureDuplicateLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureDuplicateLabel.Location = new Point(32, 369);
            captureDuplicateLabel.Name = "captureDuplicateLabel";
            captureDuplicateLabel.Size = new Size(120, 20);
            captureDuplicateLabel.TabIndex = 32;
            captureDuplicateLabel.Text = "If name exists";
            captureDuplicateLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // captureDuplicateBox
            // 
            captureDuplicateBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureDuplicateBox.DropDownStyle = ComboBoxStyle.DropDownList;
            captureDuplicateBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureDuplicateBox.FormattingEnabled = true;
            captureDuplicateBox.Items.AddRange(new object[] { "Abort", "Add a number", "Retry until free", "Retry 10 minutes, then stop" });
            captureDuplicateBox.Location = new Point(158, 366);
            captureDuplicateBox.Name = "captureDuplicateBox";
            captureDuplicateBox.Size = new Size(566, 29);
            captureDuplicateBox.TabIndex = 21;
            // 
            // captureQualityLabel
            // 
            captureQualityLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureQualityLabel.Location = new Point(32, 289);
            captureQualityLabel.Name = "captureQualityLabel";
            captureQualityLabel.Size = new Size(120, 20);
            captureQualityLabel.TabIndex = 0;
            captureQualityLabel.Text = "OMT quality";
            captureQualityLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // captureQualityBox
            // 
            captureQualityBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureQualityBox.DropDownStyle = ComboBoxStyle.DropDownList;
            captureQualityBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            captureQualityBox.FormattingEnabled = true;
            captureQualityBox.Items.AddRange(new object[] { "Low", "Normal", "High" });
            captureQualityBox.Location = new Point(158, 286);
            captureQualityBox.Name = "captureQualityBox";
            captureQualityBox.Size = new Size(566, 29);
            captureQualityBox.TabIndex = 4;
            // 
            // captureStartButton
            // 
            captureStartButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureStartButton.BackColor = Color.FromArgb(35, 125, 75);
            captureStartButton.FlatAppearance.BorderSize = 0;
            captureStartButton.FlatStyle = FlatStyle.Flat;
            captureStartButton.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            captureStartButton.ForeColor = Color.White;
            captureStartButton.Location = new Point(24, 419);
            captureStartButton.Name = "captureStartButton";
            captureStartButton.Size = new Size(698, 48);
            captureStartButton.TabIndex = 10;
            captureStartButton.Text = "Start";
            captureStartButton.UseVisualStyleBackColor = false;
            captureStartButton.Click += captureStartButton_Click;
            // 
            // captureStatus
            // 
            captureStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureStatus.BackColor = Color.Black;
            captureStatus.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            captureStatus.ForeColor = Color.White;
            captureStatus.Location = new Point(275, 5);
            captureStatus.Name = "captureStatus";
            captureStatus.Size = new Size(668, 38);
            captureStatus.TabIndex = 0;
            captureStatus.Text = "Not started";
            captureStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // captureFormat
            // 
            captureFormat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureFormat.BackColor = Color.Black;
            captureFormat.Font = new Font("Segoe UI", 11F);
            captureFormat.ForeColor = Color.White;
            captureFormat.Location = new Point(275, 55);
            captureFormat.Name = "captureFormat";
            captureFormat.Size = new Size(659, 28);
            captureFormat.TabIndex = 0;
            captureFormat.Text = "capture format";
            captureFormat.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // captureDetail
            // 
            captureDetail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            captureDetail.BackColor = Color.Black;
            captureDetail.ForeColor = Color.White;
            captureDetail.Location = new Point(275, 97);
            captureDetail.Name = "captureDetail";
            captureDetail.Size = new Size(659, 44);
            captureDetail.TabIndex = 0;
            captureDetail.Text = "capture detail";
            // 
            // playDeviceLabel
            // 
            playDeviceLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playDeviceLabel.ImageAlign = ContentAlignment.MiddleRight;
            playDeviceLabel.Location = new Point(32, 249);
            playDeviceLabel.Name = "playDeviceLabel";
            playDeviceLabel.Size = new Size(120, 20);
            playDeviceLabel.TabIndex = 0;
            playDeviceLabel.Text = "SDI output";
            playDeviceLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // playDeviceBox
            // 
            playDeviceBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            playDeviceBox.DropDownStyle = ComboBoxStyle.DropDownList;
            playDeviceBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playDeviceBox.FormattingEnabled = true;
            playDeviceBox.Location = new Point(156, 245);
            playDeviceBox.Name = "playDeviceBox";
            playDeviceBox.Size = new Size(565, 29);
            playDeviceBox.TabIndex = 0;
            // 
            // playRefreshButton
            // 
            playRefreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            playRefreshButton.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playRefreshButton.Location = new Point(727, 243);
            playRefreshButton.Name = "playRefreshButton";
            playRefreshButton.Size = new Size(104, 30);
            playRefreshButton.TabIndex = 1;
            playRefreshButton.Text = "Refresh";
            playRefreshButton.UseVisualStyleBackColor = true;
            playRefreshButton.Click += playRefreshButton_Click;
            // 
            // playKindLabel
            // 
            playKindLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playKindLabel.ImageAlign = ContentAlignment.MiddleRight;
            playKindLabel.Location = new Point(32, 169);
            playKindLabel.Name = "playKindLabel";
            playKindLabel.Size = new Size(120, 20);
            playKindLabel.TabIndex = 0;
            playKindLabel.Text = "Source type";
            playKindLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // playKindBox
            // 
            playKindBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            playKindBox.DropDownStyle = ComboBoxStyle.DropDownList;
            playKindBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playKindBox.FormattingEnabled = true;
            playKindBox.Items.AddRange(new object[] { "OMT", "NDI" });
            playKindBox.Location = new Point(156, 165);
            playKindBox.Name = "playKindBox";
            playKindBox.Size = new Size(565, 29);
            playKindBox.TabIndex = 2;
            // 
            // playSourceLabel
            // 
            playSourceLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playSourceLabel.ImageAlign = ContentAlignment.MiddleRight;
            playSourceLabel.Location = new Point(32, 209);
            playSourceLabel.Name = "playSourceLabel";
            playSourceLabel.Size = new Size(120, 20);
            playSourceLabel.TabIndex = 0;
            playSourceLabel.Text = "Sender";
            playSourceLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // playSourceBox
            // 
            playSourceBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            playSourceBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playSourceBox.FormattingEnabled = true;
            playSourceBox.Location = new Point(156, 205);
            playSourceBox.Name = "playSourceBox";
            playSourceBox.Size = new Size(565, 29);
            playSourceBox.TabIndex = 3;
            // 
            // playFindButton
            // 
            playFindButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            playFindButton.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playFindButton.Location = new Point(727, 203);
            playFindButton.Name = "playFindButton";
            playFindButton.Size = new Size(104, 30);
            playFindButton.TabIndex = 4;
            playFindButton.Text = "Find senders";
            playFindButton.UseVisualStyleBackColor = true;
            playFindButton.Click += playFindButton_Click;
            // 
            // playAudioLabel
            // 
            playAudioLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            playAudioLabel.Location = new Point(32, 334);
            playAudioLabel.Name = "playAudioLabel";
            playAudioLabel.Size = new Size(120, 20);
            playAudioLabel.TabIndex = 31;
            playAudioLabel.Text = "Audio";
            playAudioLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // playAudioBox
            // 
            playAudioBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            playAudioBox.DropDownStyle = ComboBoxStyle.DropDownList;
            playAudioBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            playAudioBox.FormattingEnabled = true;
            playAudioBox.Items.AddRange(new object[] { "Stereo 1&2", "Stereo 3&4", "Stereo 5&6", "Stereo 7&8", "Mono 1", "Mono 2", "Mono 3", "Mono 4", "Mono 5", "Mono 6", "Mono 7", "Mono 8", "Off" });
            playAudioBox.Location = new Point(158, 326);
            playAudioBox.Name = "playAudioBox";
            playAudioBox.Size = new Size(565, 29);
            playAudioBox.TabIndex = 20;
            // 
            // playBufferLabel
            // 
            playBufferLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playBufferLabel.ImageAlign = ContentAlignment.MiddleRight;
            playBufferLabel.Location = new Point(32, 289);
            playBufferLabel.Name = "playBufferLabel";
            playBufferLabel.Size = new Size(120, 20);
            playBufferLabel.TabIndex = 0;
            playBufferLabel.Text = "Buffer (frames)";
            playBufferLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // playBufferBox
            // 
            playBufferBox.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            playBufferBox.Location = new Point(158, 285);
            playBufferBox.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            playBufferBox.Minimum = new decimal(new int[] { 3, 0, 0, 0 });
            playBufferBox.Name = "playBufferBox";
            playBufferBox.Size = new Size(120, 29);
            playBufferBox.TabIndex = 5;
            playBufferBox.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // playStartButton
            // 
            playStartButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            playStartButton.BackColor = Color.FromArgb(35, 125, 75);
            playStartButton.FlatAppearance.BorderSize = 0;
            playStartButton.FlatStyle = FlatStyle.Flat;
            playStartButton.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            playStartButton.ForeColor = Color.White;
            playStartButton.Location = new Point(24, 419);
            playStartButton.Name = "playStartButton";
            playStartButton.Size = new Size(697, 48);
            playStartButton.TabIndex = 10;
            playStartButton.Text = "Start";
            playStartButton.UseVisualStyleBackColor = false;
            playStartButton.Click += playStartButton_Click;
            // 
            // playStatus
            // 
            playStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            playStatus.BackColor = Color.Black;
            playStatus.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            playStatus.ForeColor = Color.White;
            playStatus.Location = new Point(275, 5);
            playStatus.Name = "playStatus";
            playStatus.Size = new Size(676, 38);
            playStatus.TabIndex = 0;
            playStatus.Text = "Not started";
            playStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // playFormat
            // 
            playFormat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            playFormat.BackColor = Color.Black;
            playFormat.Font = new Font("Segoe UI", 11F);
            playFormat.ForeColor = Color.White;
            playFormat.Location = new Point(275, 55);
            playFormat.Name = "playFormat";
            playFormat.Size = new Size(676, 28);
            playFormat.TabIndex = 0;
            playFormat.Text = "play format";
            playFormat.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // playDetail
            // 
            playDetail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            playDetail.BackColor = Color.Black;
            playDetail.ForeColor = Color.White;
            playDetail.Location = new Point(275, 97);
            playDetail.Name = "playDetail";
            playDetail.Size = new Size(671, 44);
            playDetail.TabIndex = 0;
            playDetail.Text = "play detail";
            // 
            // tabs
            // 
            tabs.Appearance = TabAppearance.FlatButtons;
            tabs.Controls.Add(captureTab);
            tabs.Controls.Add(playTab);
            tabs.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabs.Location = new Point(0, 0);
            tabs.Name = "tabs";
            tabs.Padding = new Point(18, 6);
            tabs.SelectedIndex = 0;
            tabs.Size = new Size(967, 522);
            tabs.TabIndex = 0;
            // 
            // captureTab
            // 
            captureTab.BackColor = Color.Gainsboro;
            captureTab.Controls.Add(btn_sdi2omt);
            captureTab.Controls.Add(btn_log_capture);
            captureTab.Controls.Add(btn_card_capture);
            captureTab.Controls.Add(captureDeviceLabel);
            captureTab.Controls.Add(label2);
            captureTab.Controls.Add(label1);
            captureTab.Controls.Add(captureStatus);
            captureTab.Controls.Add(pictureBox2);
            captureTab.Controls.Add(captureDeviceBox);
            captureTab.Controls.Add(captureRefreshButton);
            captureTab.Controls.Add(captureNameLabel);
            captureTab.Controls.Add(captureNameBox);
            captureTab.Controls.Add(captureOutputBox);
            captureTab.Controls.Add(captureQualityLabel);
            captureTab.Controls.Add(captureQualityBox);
            captureTab.Controls.Add(captureAudioLabel);
            captureTab.Controls.Add(captureAudioBox);
            captureTab.Controls.Add(captureDuplicateLabel);
            captureTab.Controls.Add(captureDuplicateBox);
            captureTab.Controls.Add(captureStartButton);
            captureTab.Controls.Add(captureFormat);
            captureTab.Controls.Add(captureDetail);
            captureTab.Controls.Add(pictureBox1);
            captureTab.Controls.Add(captureOutputLabel);
            captureTab.Location = new Point(4, 33);
            captureTab.Name = "captureTab";
            captureTab.Size = new Size(959, 485);
            captureTab.TabIndex = 0;
            captureTab.Text = "SDI  →  OMT / NDI";
            // 
            // btn_sdi2omt
            // 
            btn_sdi2omt.BackColor = Color.Silver;
            btn_sdi2omt.FlatStyle = FlatStyle.Flat;
            btn_sdi2omt.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_sdi2omt.ForeColor = Color.Black;
            btn_sdi2omt.Location = new Point(788, 419);
            btn_sdi2omt.Name = "btn_sdi2omt";
            btn_sdi2omt.Size = new Size(155, 48);
            btn_sdi2omt.TabIndex = 15;
            btn_sdi2omt.Text = "HELP";
            btn_sdi2omt.UseVisualStyleBackColor = false;
            btn_sdi2omt.Click += btn_sdi2omt_Click;
            // 
            // btn_log_capture
            // 
            btn_log_capture.BackColor = Color.Silver;
            btn_log_capture.FlatStyle = FlatStyle.Flat;
            btn_log_capture.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_log_capture.ForeColor = Color.Black;
            btn_log_capture.Location = new Point(788, 311);
            btn_log_capture.Name = "btn_log_capture";
            btn_log_capture.Size = new Size(155, 44);
            btn_log_capture.TabIndex = 30;
            btn_log_capture.Text = "LOG";
            btn_log_capture.UseVisualStyleBackColor = false;
            btn_log_capture.Click += btn_log_capture_Click;
            // 
            // btn_card_capture
            // 
            btn_card_capture.BackColor = Color.Silver;
            btn_card_capture.FlatStyle = FlatStyle.Flat;
            btn_card_capture.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_card_capture.ForeColor = Color.Black;
            btn_card_capture.Location = new Point(788, 366);
            btn_card_capture.Name = "btn_card_capture";
            btn_card_capture.Size = new Size(155, 44);
            btn_card_capture.TabIndex = 30;
            btn_card_capture.Text = "BM CARD INFO";
            btn_card_capture.UseVisualStyleBackColor = false;
            btn_card_capture.Click += btn_card_capture_Click;
            // 
            // label2
            // 
            label2.Font = new Font("Segoe UI", 6.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(32, 189);
            label2.Name = "label2";
            label2.Size = new Size(120, 20);
            label2.TabIndex = 13;
            label2.Text = "auto detect format";
            label2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.BackColor = Color.Black;
            label1.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(8, 79);
            label1.Name = "label1";
            label1.Size = new Size(198, 62);
            label1.TabIndex = 12;
            label1.Text = "SDI ➜ OMT";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Black;
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(6, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(200, 89);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 2;
            pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Black;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(956, 146);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // playTab
            // 
            playTab.BackColor = Color.Gainsboro;
            playTab.Controls.Add(label4);
            playTab.Controls.Add(btn_help_omt2sdi);
            playTab.Controls.Add(btn_log_play);
            playTab.Controls.Add(btn_card_play);
            playTab.Controls.Add(playStatus);
            playTab.Controls.Add(pictureBox4);
            playTab.Controls.Add(playDeviceLabel);
            playTab.Controls.Add(playDeviceBox);
            playTab.Controls.Add(playRefreshButton);
            playTab.Controls.Add(playKindLabel);
            playTab.Controls.Add(playKindBox);
            playTab.Controls.Add(playSourceLabel);
            playTab.Controls.Add(playSourceBox);
            playTab.Controls.Add(playFindButton);
            playTab.Controls.Add(playBufferLabel);
            playTab.Controls.Add(playBufferBox);
            playTab.Controls.Add(playAudioLabel);
            playTab.Controls.Add(playAudioBox);
            playTab.Controls.Add(playStartButton);
            playTab.Controls.Add(playFormat);
            playTab.Controls.Add(playDetail);
            playTab.Controls.Add(pictureBox3);
            playTab.Location = new Point(4, 33);
            playTab.Name = "playTab";
            playTab.Size = new Size(959, 485);
            playTab.TabIndex = 1;
            playTab.Text = "OMT / NDI  →  SDI";
            // 
            // btn_help_omt2sdi
            // 
            btn_help_omt2sdi.BackColor = Color.Silver;
            btn_help_omt2sdi.FlatStyle = FlatStyle.Flat;
            btn_help_omt2sdi.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_help_omt2sdi.ForeColor = Color.Black;
            btn_help_omt2sdi.Location = new Point(788, 419);
            btn_help_omt2sdi.Name = "btn_help_omt2sdi";
            btn_help_omt2sdi.Size = new Size(155, 48);
            btn_help_omt2sdi.TabIndex = 16;
            btn_help_omt2sdi.Text = "HELP";
            btn_help_omt2sdi.UseVisualStyleBackColor = false;
            btn_help_omt2sdi.Click += btn_help_omt2sdi_Click;
            // 
            // btn_log_play
            // 
            btn_log_play.BackColor = Color.Silver;
            btn_log_play.FlatStyle = FlatStyle.Flat;
            btn_log_play.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_log_play.ForeColor = Color.Black;
            btn_log_play.Location = new Point(788, 311);
            btn_log_play.Name = "btn_log_play";
            btn_log_play.Size = new Size(155, 44);
            btn_log_play.TabIndex = 30;
            btn_log_play.Text = "LOG";
            btn_log_play.UseVisualStyleBackColor = false;
            btn_log_play.Click += btn_log_play_Click;
            // 
            // btn_card_play
            // 
            btn_card_play.BackColor = Color.Silver;
            btn_card_play.FlatStyle = FlatStyle.Flat;
            btn_card_play.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_card_play.ForeColor = Color.Black;
            btn_card_play.Location = new Point(788, 366);
            btn_card_play.Name = "btn_card_play";
            btn_card_play.Size = new Size(155, 44);
            btn_card_play.TabIndex = 30;
            btn_card_play.Text = "BM CARD INFO";
            btn_card_play.UseVisualStyleBackColor = false;
            btn_card_play.Click += btn_card_play_Click;
            // 
            // pictureBox4
            // 
            pictureBox4.BackColor = Color.Black;
            pictureBox4.BackgroundImage = (Image)resources.GetObject("pictureBox4.BackgroundImage");
            pictureBox4.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox4.Location = new Point(8, 3);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(238, 80);
            pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox4.TabIndex = 13;
            pictureBox4.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.Black;
            pictureBox3.Location = new Point(2, 0);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(956, 146);
            pictureBox3.TabIndex = 12;
            pictureBox3.TabStop = false;
            // 
            // btn_exit
            // 
            btn_exit.BackColor = Color.IndianRed;
            btn_exit.FlatStyle = FlatStyle.Flat;
            btn_exit.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_exit.ForeColor = Color.White;
            btn_exit.Location = new Point(836, 533);
            btn_exit.Name = "btn_exit";
            btn_exit.Size = new Size(119, 70);
            btn_exit.TabIndex = 14;
            btn_exit.Text = "EXIT";
            btn_exit.UseVisualStyleBackColor = false;
            btn_exit.Click += btn_exit_Click;
            // 
            // footer
            // 
            footer.LinkArea = new LinkArea(50, 18);
            footer.Location = new Point(5, 584);
            footer.Name = "footer";
            footer.Size = new Size(820, 22);
            footer.TabIndex = 1;
            footer.TabStop = true;
            footer.Text = "NDI® is a registered trademark of Vizrt NDI AB. https://ndi.video/  ·  OMT (MIT) bundled; the NDI runtime is not bundled.";
            footer.TextAlign = ContentAlignment.MiddleLeft;
            footer.UseCompatibleTextRendering = true;
            footer.LinkClicked += footer_LinkClicked;
            // 
            // cpuLabel
            // 
            cpuLabel.BorderStyle = BorderStyle.FixedSingle;
            cpuLabel.Location = new Point(5, 538);
            cpuLabel.Name = "cpuLabel";
            cpuLabel.Size = new Size(160, 38);
            cpuLabel.TabIndex = 40;
            cpuLabel.Tag = "CPU · system";
            cpuLabel.Text = "CPU · system\n—";
            cpuLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ramLabel
            // 
            ramLabel.BorderStyle = BorderStyle.FixedSingle;
            ramLabel.Location = new Point(169, 538);
            ramLabel.Name = "ramLabel";
            ramLabel.Size = new Size(160, 38);
            ramLabel.TabIndex = 40;
            ramLabel.Tag = "RAM · system";
            ramLabel.Text = "RAM · system\n—";
            ramLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // freeRamLabel
            // 
            freeRamLabel.BorderStyle = BorderStyle.FixedSingle;
            freeRamLabel.Location = new Point(333, 538);
            freeRamLabel.Name = "freeRamLabel";
            freeRamLabel.Size = new Size(160, 38);
            freeRamLabel.TabIndex = 40;
            freeRamLabel.Tag = "RAM · free";
            freeRamLabel.Text = "RAM · free\n—";
            freeRamLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // engineCpuLabel
            // 
            engineCpuLabel.BorderStyle = BorderStyle.FixedSingle;
            engineCpuLabel.Location = new Point(497, 538);
            engineCpuLabel.Name = "engineCpuLabel";
            engineCpuLabel.Size = new Size(160, 38);
            engineCpuLabel.TabIndex = 40;
            engineCpuLabel.Tag = "CPU · engine";
            engineCpuLabel.Text = "CPU · engine\n—";
            engineCpuLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // engineRamLabel
            // 
            engineRamLabel.BorderStyle = BorderStyle.FixedSingle;
            engineRamLabel.Location = new Point(661, 538);
            engineRamLabel.Name = "engineRamLabel";
            engineRamLabel.Size = new Size(160, 38);
            engineRamLabel.TabIndex = 40;
            engineRamLabel.Tag = "RAM · engine";
            engineRamLabel.Text = "RAM · engine\n—";
            engineRamLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label4.BackColor = Color.Black;
            label4.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.White;
            label4.Location = new Point(8, 79);
            label4.Name = "label4";
            label4.Size = new Size(198, 62);
            label4.TabIndex = 32;
            label4.Text = "OMT ➜ SDI";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(976, 612);
            Controls.Add(btn_exit);
            Controls.Add(tabs);
            Controls.Add(footer);
            Controls.Add(cpuLabel);
            Controls.Add(ramLabel);
            Controls.Add(freeRamLabel);
            Controls.Add(engineCpuLabel);
            Controls.Add(engineRamLabel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MaximumSize = new Size(992, 651);
            MinimumSize = new Size(992, 651);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SDI Converter";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)playBufferBox).EndInit();
            tabs.ResumeLayout(false);
            captureTab.ResumeLayout(false);
            captureTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            playTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabs;
        private TabPage captureTab;
        private TabPage playTab;
        private Label captureDeviceLabel;
        private ComboBox captureDeviceBox;
        private Button captureRefreshButton;
        private Label captureNameLabel;
        private TextBox captureNameBox;
        private Label captureOutputLabel;
        private ComboBox captureOutputBox;
        private Label captureAudioLabel;
        private ComboBox captureAudioBox;
        private Label captureDuplicateLabel;
        private ComboBox captureDuplicateBox;
        private Label captureQualityLabel;
        private ComboBox captureQualityBox;
        private Button captureStartButton;
        private Label captureStatus;
        private Label captureFormat;
        private Label captureDetail;
        private Label playDeviceLabel;
        private ComboBox playDeviceBox;
        private Button playRefreshButton;
        private Label playKindLabel;
        private ComboBox playKindBox;
        private Label playSourceLabel;
        private ComboBox playSourceBox;
        private Button playFindButton;
        private Label playAudioLabel;
        private ComboBox playAudioBox;
        private Label playBufferLabel;
        private NumericUpDown playBufferBox;
        private Button playStartButton;
        private Label playStatus;
        private Label playFormat;
        private Label playDetail;
        private LinkLabel footer;
        private Button btn_log_capture;
        private Button btn_card_capture;
        private Button btn_log_play;
        private Button btn_card_play;
        private Label cpuLabel;
        private Label ramLabel;
        private Label freeRamLabel;
        private Label engineCpuLabel;
        private Label engineRamLabel;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Label label1;
        private Label label2;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;
        private Button btn_exit;
        private Button btn_sdi2omt;
        private Button btn_help_omt2sdi;
        private Label label4;
    }
}
