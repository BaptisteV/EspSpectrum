using Dark.Net;
using EspSpectrum.Core.Display;
using Microsoft.Extensions.Logging;
namespace EspSpectrum.ConfigUI
{
    public partial class EspSpectrumConfigForm : Form
    {
        public static readonly string DefaultAppsettings = @"C:\Users\Bapt\Desktop\FFT_Publish\EspSpectrum.Worker\appsettings.json";
        private readonly IDisplayConfigManager _writer;
        private readonly IEspSpectrumServiceMonitor _serviceMonitor;
        private readonly ILogger<EspSpectrumConfigForm> _logger;

        public EspSpectrumConfigForm(
            IDisplayConfigManager writer,
            IEspSpectrumServiceMonitor serviceMonitor,
            ILogger<EspSpectrumConfigForm> logger)
        {
            _writer = writer;
            _serviceMonitor = serviceMonitor;
            _logger = logger;
            InitializeComponent();
            AdjustColors();

        }

        private void AdjustColors()
        {
            DarkNet.Instance.SetWindowThemeForms(this, Theme.Dark);
            this.BackColor = Color.FromArgb(255, 24, 32, 33);
            this.ForeColor = Color.FromArgb(255, 255 - 24, 255 - 32, 255 - 33);
            statusStrip1.BackColor = Color.FromArgb(255, 24, 32, 33);
            statusStrip1.ForeColor = Color.FromArgb(255, 255 - 24, 255 - 32, 255 - 33);
        }

        private async void ColorButtonClicked(object? sender, EventArgs e)
        {
            var button = (Button)sender!;
            var buttonColor = button.BackColor.GetInt8Hue();
            var parentPanel = (TableLayoutPanel)button.Parent!;

            if (parentPanel.Name.Contains("Low"))
            {
                await _writer.UpdateConfig(c => c.LowHue = buttonColor);
            }
            else if (parentPanel.Name.Contains("Mid"))
            {
                await _writer.UpdateConfig(c => c.MidHue = buttonColor);
            }
            else if (parentPanel.Name.Contains("High"))
            {
                await _writer.UpdateConfig(c => c.HighHue = buttonColor);
            }

            foreach (Control ctrl in parentPanel.Controls)
            {
                if (ctrl is Button btn)
                {
                    var baseColor = btn.BackColor;
                    btn.BackColor = Color.FromArgb(110, baseColor.R, baseColor.G, baseColor.B);
                    btn.Enabled = true;
                }
            }
            button.Enabled = false;
            var backColor = button.BackColor;
            button.BackColor = Color.FromArgb(255, backColor.R, backColor.G, backColor.B);
        }

        private void FillColors(TableLayoutPanel panel, int offset)
        {
            Color[] colors = [
                Color.FromArgb(110, 255, 0, 0),
                Color.FromArgb(110, 255, 255, 0),
                Color.FromArgb(110, 0, 255, 0),
                Color.FromArgb(110, 0, 255, 255),
                Color.FromArgb(110, 0, 0, 255),
                Color.FromArgb(110, 255, 0, 255),
                ];

            // Add buttons to each column
            for (int col = 0; col < 6; col++)
            {
                var color = colors[(col + offset) % 6];
                Button btn = new Button
                {
                    Dock = DockStyle.Fill,
                    BackColor = color,
                    ForeColor = color,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(2, 2, 2, 2),
                };

                btn.FlatAppearance.BorderSize = 0;

                btn.Click += ColorButtonClicked;

                panel.Controls.Add(btn, col, 0);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            FillColors(panelHighColor, 0);
            FillColors(panelMidColor, 1);
            FillColors(panelLowColor, 2);

            var initialConf = await _writer.ReadConfig();
            sendIntervalSlider.Value = (int)initialConf.SendInterval.TotalMilliseconds;
            fadedFramesSlider.Value = initialConf.HistoLength;
            brightnessSlider.Value = initialConf.Brightness;

            _ = Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
                while (await timer.WaitForNextTickAsync())
                {
                    var status = _serviceMonitor.GetStatus();
                    var statusColor = status.IsRunning ? Color.Green : Color.Red;
                    serviceStatusLabel.ForeColor = statusColor;
                    serviceStatusLabel.ToolTipText = status.ToString();
                    notifyIcon.BalloonTipText = $"EspSpectrum - Config ({status})";
                    notifyIconStatus.ForeColor = statusColor;
                }
            });
        }


        private async Task SafeUpdateConfig(Action<DisplayConfig> update)
        {
            try
            {
                await _writer.UpdateConfig(update);
            }
            // Happens when values are changed too quickly
            catch (IOException e)
            {
                _logger.LogError(e, $"Error updating configuration");
            }
        }

        private async void sendIntervalSlider_ValueChanged(object sender, EventArgs e)
        {
            var slider = (TrackBar)sender;
            sendIntervalLabel.Text = $"SendDisplayConfig interval {slider.Value}ms";
            await SafeUpdateConfig(c => c.SendInterval = TimeSpan.FromMilliseconds(slider.Value));
        }

        private async void fadedFramesSlider_ValueChanged(object sender, EventArgs e)
        {
            var slider = (TrackBar)sender;
            fadedFramesLabel.Text = $"Faded frames: {slider.Value}";
            await SafeUpdateConfig(c => c.HistoLength = slider.Value);
        }

        private async void brightnessSlider_ValueChanged(object sender, EventArgs e)
        {
            var slider = (TrackBar)sender;
            brightnessLabel.Text = $"Brightness: {slider.Value}";
            await SafeUpdateConfig(c => c.Brightness = slider.Value);
        }

        private void restartMenuItem_Click(object sender, EventArgs e)
        {
            _serviceMonitor.Restart();
        }

        private void stopMenuItem_Click(object sender, EventArgs e)
        {
            _serviceMonitor.Stop();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Activate();
        }

        private void notifyIconRestart_Click(object sender, EventArgs e)
        {
            _serviceMonitor.Restart();
        }

        private void notifyIconStop_Click(object sender, EventArgs e)
        {
            _serviceMonitor.Stop();
        }
    }
}
