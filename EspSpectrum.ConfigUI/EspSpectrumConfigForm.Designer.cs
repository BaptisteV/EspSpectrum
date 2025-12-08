namespace EspSpectrum.ConfigUI
{
    partial class EspSpectrumConfigForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(EspSpectrumConfigForm));
            panelHighColor = new TableLayoutPanel();
            panelMidColor = new TableLayoutPanel();
            panelLowColor = new TableLayoutPanel();
            slidersPanel = new Panel();
            amplificationLabel = new Label();
            amplificationSlider = new TrackBar();
            brightnessLabel = new Label();
            brightnessSlider = new TrackBar();
            fadedFramesLabel = new Label();
            fadedFramesSlider = new TrackBar();
            sendIntervalLabel = new Label();
            sendIntervalSlider = new TrackBar();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            serviceStatusLabel = new ToolStripStatusLabel();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            stopMenuItem = new ToolStripMenuItem();
            restartMenuItem = new ToolStripMenuItem();
            notifyIcon = new NotifyIcon(components);
            notifyIconMenuStrip = new ContextMenuStrip(components);
            notifyIconStatus = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            notifyIconRestart = new ToolStripMenuItem();
            notifyIconStop = new ToolStripMenuItem();
            slidersPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)amplificationSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)brightnessSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)fadedFramesSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sendIntervalSlider).BeginInit();
            statusStrip1.SuspendLayout();
            notifyIconMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // panelHighColor
            // 
            panelHighColor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelHighColor.AutoSize = true;
            panelHighColor.ColumnCount = 6;
            panelHighColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelHighColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelHighColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelHighColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelHighColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelHighColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelHighColor.Location = new Point(12, 12);
            panelHighColor.MinimumSize = new Size(10, 10);
            panelHighColor.Name = "panelHighColor";
            panelHighColor.RowCount = 1;
            panelHighColor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelHighColor.Size = new Size(776, 100);
            panelHighColor.TabIndex = 3;
            // 
            // panelMidColor
            // 
            panelMidColor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelMidColor.AutoSize = true;
            panelMidColor.ColumnCount = 6;
            panelMidColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelMidColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelMidColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelMidColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelMidColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelMidColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelMidColor.Location = new Point(12, 112);
            panelMidColor.MinimumSize = new Size(10, 10);
            panelMidColor.Name = "panelMidColor";
            panelMidColor.RowCount = 1;
            panelMidColor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelMidColor.Size = new Size(776, 100);
            panelMidColor.TabIndex = 4;
            // 
            // panelLowColor
            // 
            panelLowColor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelLowColor.AutoSize = true;
            panelLowColor.ColumnCount = 6;
            panelLowColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelLowColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelLowColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelLowColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelLowColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelLowColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666666F));
            panelLowColor.Location = new Point(12, 212);
            panelLowColor.MinimumSize = new Size(10, 10);
            panelLowColor.Name = "panelLowColor";
            panelLowColor.RowCount = 1;
            panelLowColor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelLowColor.Size = new Size(776, 100);
            panelLowColor.TabIndex = 5;
            // 
            // slidersPanel
            // 
            slidersPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            slidersPanel.Controls.Add(amplificationLabel);
            slidersPanel.Controls.Add(amplificationSlider);
            slidersPanel.Controls.Add(brightnessLabel);
            slidersPanel.Controls.Add(brightnessSlider);
            slidersPanel.Controls.Add(fadedFramesLabel);
            slidersPanel.Controls.Add(fadedFramesSlider);
            slidersPanel.Controls.Add(sendIntervalLabel);
            slidersPanel.Controls.Add(sendIntervalSlider);
            slidersPanel.Location = new Point(12, 318);
            slidersPanel.Name = "slidersPanel";
            slidersPanel.Size = new Size(776, 208);
            slidersPanel.TabIndex = 7;
            // 
            // amplificationLabel
            // 
            amplificationLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            amplificationLabel.Location = new Point(645, 156);
            amplificationLabel.Name = "amplificationLabel";
            amplificationLabel.Size = new Size(128, 33);
            amplificationLabel.TabIndex = 7;
            amplificationLabel.Text = "Amplification";
            amplificationLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // amplificationSlider
            // 
            amplificationSlider.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            amplificationSlider.LargeChange = 10;
            amplificationSlider.Location = new Point(3, 156);
            amplificationSlider.Maximum = 200;
            amplificationSlider.Name = "amplificationSlider";
            amplificationSlider.Size = new Size(636, 45);
            amplificationSlider.TabIndex = 6;
            amplificationSlider.TickFrequency = 5;
            amplificationSlider.Value = 1;
            amplificationSlider.ValueChanged += amplificationSlider_ValueChanged;
            // 
            // brightnessLabel
            // 
            brightnessLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            brightnessLabel.Location = new Point(645, 105);
            brightnessLabel.Name = "brightnessLabel";
            brightnessLabel.Size = new Size(128, 33);
            brightnessLabel.TabIndex = 5;
            brightnessLabel.Text = "Brightness";
            brightnessLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // brightnessSlider
            // 
            brightnessSlider.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            brightnessSlider.LargeChange = 10;
            brightnessSlider.Location = new Point(3, 105);
            brightnessSlider.Maximum = 255;
            brightnessSlider.Minimum = 1;
            brightnessSlider.Name = "brightnessSlider";
            brightnessSlider.Size = new Size(636, 45);
            brightnessSlider.TabIndex = 2;
            brightnessSlider.TickFrequency = 5;
            brightnessSlider.Value = 1;
            brightnessSlider.ValueChanged += brightnessSlider_ValueChanged;
            // 
            // fadedFramesLabel
            // 
            fadedFramesLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            fadedFramesLabel.Location = new Point(645, 54);
            fadedFramesLabel.Name = "fadedFramesLabel";
            fadedFramesLabel.Size = new Size(128, 35);
            fadedFramesLabel.TabIndex = 3;
            fadedFramesLabel.Text = "Faded frames";
            fadedFramesLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // fadedFramesSlider
            // 
            fadedFramesSlider.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            fadedFramesSlider.Location = new Point(3, 54);
            fadedFramesSlider.Maximum = 50;
            fadedFramesSlider.Name = "fadedFramesSlider";
            fadedFramesSlider.Size = new Size(636, 45);
            fadedFramesSlider.TabIndex = 1;
            fadedFramesSlider.ValueChanged += fadedFramesSlider_ValueChanged;
            // 
            // sendIntervalLabel
            // 
            sendIntervalLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sendIntervalLabel.Location = new Point(645, 3);
            sendIntervalLabel.Name = "sendIntervalLabel";
            sendIntervalLabel.Size = new Size(128, 33);
            sendIntervalLabel.TabIndex = 1;
            sendIntervalLabel.Text = "Send interval";
            sendIntervalLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // sendIntervalSlider
            // 
            sendIntervalSlider.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            sendIntervalSlider.Location = new Point(3, 3);
            sendIntervalSlider.Maximum = 50;
            sendIntervalSlider.Minimum = 2;
            sendIntervalSlider.Name = "sendIntervalSlider";
            sendIntervalSlider.Size = new Size(636, 45);
            sendIntervalSlider.TabIndex = 0;
            sendIntervalSlider.Value = 15;
            sendIntervalSlider.ValueChanged += sendIntervalSlider_ValueChanged;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, serviceStatusLabel, toolStripDropDownButton1 });
            statusStrip1.Location = new Point(0, 529);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(800, 22);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(149, 17);
            toolStripStatusLabel1.Text = "EspSpectrum service status";
            // 
            // serviceStatusLabel
            // 
            serviceStatusLabel.Name = "serviceStatusLabel";
            serviceStatusLabel.Size = new Size(14, 17);
            serviceStatusLabel.Text = "●";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.None;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { stopMenuItem, restartMenuItem });
            toolStripDropDownButton1.Image = (Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(13, 20);
            toolStripDropDownButton1.Text = "toolStripDropDownButton1";
            // 
            // stopMenuItem
            // 
            stopMenuItem.Name = "stopMenuItem";
            stopMenuItem.Size = new Size(149, 22);
            stopMenuItem.Text = "Stop service";
            stopMenuItem.Click += stopMenuItem_Click;
            // 
            // restartMenuItem
            // 
            restartMenuItem.Name = "restartMenuItem";
            restartMenuItem.Size = new Size(149, 22);
            restartMenuItem.Text = "Restart service";
            restartMenuItem.Click += restartMenuItem_Click;
            // 
            // notifyIcon
            // 
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ContextMenuStrip = notifyIconMenuStrip;
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "EspSpectrum - Config";
            notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            // 
            // notifyIconMenuStrip
            // 
            notifyIconMenuStrip.Items.AddRange(new ToolStripItem[] { notifyIconStatus, toolStripSeparator1, notifyIconRestart, notifyIconStop });
            notifyIconMenuStrip.Name = "contextMenuStrip1";
            notifyIconMenuStrip.Size = new Size(150, 76);
            // 
            // notifyIconStatus
            // 
            notifyIconStatus.Name = "notifyIconStatus";
            notifyIconStatus.Size = new Size(149, 22);
            notifyIconStatus.Text = "●";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(146, 6);
            // 
            // notifyIconRestart
            // 
            notifyIconRestart.Name = "notifyIconRestart";
            notifyIconRestart.Size = new Size(149, 22);
            notifyIconRestart.Text = "Restart service";
            notifyIconRestart.Click += notifyIconRestart_Click;
            // 
            // notifyIconStop
            // 
            notifyIconStop.Name = "notifyIconStop";
            notifyIconStop.Size = new Size(149, 22);
            notifyIconStop.Text = "Stop service";
            notifyIconStop.Click += notifyIconStop_Click;
            // 
            // EspSpectrumConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 551);
            Controls.Add(statusStrip1);
            Controls.Add(slidersPanel);
            Controls.Add(panelLowColor);
            Controls.Add(panelMidColor);
            Controls.Add(panelHighColor);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "EspSpectrumConfigForm";
            Text = "EspSpectrum - Config";
            Load += EspSpectrumConfigForm_Load;
            slidersPanel.ResumeLayout(false);
            slidersPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)amplificationSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)brightnessSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)fadedFramesSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)sendIntervalSlider).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            notifyIconMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TableLayoutPanel panelHighColor;
        private TableLayoutPanel panelMidColor;
        private TableLayoutPanel panelLowColor;
        private Panel slidersPanel;
        private Label brightnessLabel;
        private TrackBar brightnessSlider;
        private Label fadedFramesLabel;
        private TrackBar fadedFramesSlider;
        private Label sendIntervalLabel;
        private TrackBar sendIntervalSlider;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel serviceStatusLabel;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem restartMenuItem;
        private ToolStripMenuItem stopMenuItem;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip notifyIconMenuStrip;
        private ToolStripMenuItem notifyIconRestart;
        private ToolStripMenuItem notifyIconStop;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem notifyIconStatus;
        private Label amplificationLabel;
        private TrackBar amplificationSlider;
    }
}
