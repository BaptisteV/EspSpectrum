using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Options;

namespace EspSpectrum.App
{
    public partial class MainPage : ContentPage
    {
        private readonly Button[][] _buttons = new Button[FftProps.NBands][];

        private readonly IFftStream _stream;

        private DisplayConfig _display;
        private readonly IOptionsMonitor<DisplayConfig> _displayMonitor;
        private readonly IWebsocketBars _wsBars;
        private readonly IWebsocketDisplay _wsDisplay;
        private readonly IDisplayConfigManager _displayWriter;
        private readonly CancellationTokenSource _cts = new();

        public MainPage(
            IFftStream stream,
            IOptionsMonitor<DisplayConfig> displayMonitor,
            IWebsocketBars wsBars,
            IWebsocketDisplay wsDisplay,
            IDisplayConfigManager displayWriter)
        {
            InitializeComponent();
            CreateButtons(BarsContainer);
            _stream = stream;
            _wsBars = wsBars;
            _wsDisplay = wsDisplay;
            _displayWriter = displayWriter;
            _displayMonitor = displayMonitor;
            _display = _displayMonitor.CurrentValue;
            _displayMonitor.OnChange(async (newConf) =>
            {
                if (newConf != _display)
                {
                    await _wsDisplay.Send(newConf);
                    _display = newConf;
                }
            });

            _ = Task.Run(async () =>
            {
                await foreach (var fft in _stream.NextFft(_cts.Token))
                {
                    await _wsBars.SendAudio(fft.Bands);
                    Dispatcher.Dispatch(() => UpdateUI(fft));
                }
            }, _cts.Token);
        }

        public void CreateButtons(HorizontalStackLayout layout)
        {
            const int cellSize = 12;
            for (int x = 0; x < FftProps.NBands; x++)
            {
                _buttons[x] = new Button[FftProps.BandHeigth];
                var bar = new VerticalStackLayout();
                for (int y = 0; y < _buttons[x].Length; y++)
                {
                    var top = (FftProps.BandHeigth - 1 - y) * cellSize;
                    _buttons[x][y] = CreateButton(cellSize, x * cellSize, top);
                    bar.Children.Insert(0, _buttons[x][y]);
                }
                layout.Children.Add(bar);
            }
        }

        private static Button CreateButton(int size, int x, int y)
        {
            return new Button
            {
                BackgroundColor = Colors.Gray,
                AnchorX = x,
                AnchorY = y,
                IsEnabled = false,
                WidthRequest = size,
                HeightRequest = size,
                IsVisible = true,
                CornerRadius = 0,
                BorderWidth = 1,
                BorderColor = Colors.LightGray,
            };
        }

        private Color GetCellColor(int barValue, int y)
        {
            if (y >= barValue)
                return Colors.Gray;

            if (y <= 4)
                return FromEspHue(_displayMonitor.CurrentValue.LowHue);

            if (y > 4 && y < 7)
                return FromEspHue(_displayMonitor.CurrentValue.MidHue);

            return FromEspHue(_displayMonitor.CurrentValue.HighHue);
        }

        public void UpdateBars(FftResult fft)
        {
            for (var x = 0; x < _buttons.Length; x++)
            {
                var barValue = fft.Bands[x];
                for (var y = 0; y < _buttons[x].Length; y++)
                {
                    _buttons[x][y].BackgroundColor = GetCellColor(barValue, y);
                }
            }
        }

        private void UpdateLabels()
        {
            LabelInterval.Text = $"Interval: {_displayMonitor.CurrentValue.SendInterval.TotalMilliseconds}ms";
            LabelHistoLength.Text = $"Histo length: {_displayMonitor.CurrentValue.HistoLength} frames";
            LabelBrightness.Text = $"Brightness: {_displayMonitor.CurrentValue.Brightness} ({_displayMonitor.CurrentValue.Brightness * 100 / 255}%)";
        }

        public void UpdateUI(FftResult fft)
        {
            UpdateBars(fft);
            UpdateLabels();
        }

        private static int ToEspHue(Color color) => (int)Math.Round(color.GetHue() * 255.0);
        private static Color FromEspHue(int hue) => Color.FromHsv(hue, 100, 100);
        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                IntervalSlider.Value = _displayMonitor.CurrentValue.SendInterval.TotalMilliseconds;
                HistoLengthSlider.Value = _displayMonitor.CurrentValue.HistoLength;
                BrightnessSlider.Value = _displayMonitor.CurrentValue.Brightness;
                UpdateLabels();
                HighColorPicker.PickedColor = FromEspHue(_displayMonitor.CurrentValue.HighHue);
                MidColorPicker.PickedColor = FromEspHue(_displayMonitor.CurrentValue.MidHue);
                LowColorPicker.PickedColor = FromEspHue(_displayMonitor.CurrentValue.LowHue);
            });
        }

        private void ContentPage_Unloaded(object sender, EventArgs e)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private async void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var newInterval = TimeSpan.FromMilliseconds(e.NewValue);
            await _displayWriter.UpdateConfig((c) => c.SendInterval = newInterval);
        }

        private async void HistoLengthSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            await _displayWriter.UpdateConfig(c => c.HistoLength = (int)Math.Round(e.NewValue));
        }

        private async void BrightnessSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            await _displayWriter.UpdateConfig(c => c.Brightness = (int)Math.Round(e.NewValue));
        }

        private async void LowColorPicker_PickedColorChanged(object sender, ColorPicker.Maui.PickedColorChangedEventArgs e)
        {
            var h = ToEspHue(e.NewPickedColorValue);
            await _displayWriter.UpdateConfig(c => c.LowHue = h);
        }

        private async void MidColorPicker_PickedColorChanged(object sender, ColorPicker.Maui.PickedColorChangedEventArgs e)
        {
            await _displayWriter.UpdateConfig(c => c.MidHue = ToEspHue(e.NewPickedColorValue));
        }

        private async void HighColorPicker_PickedColorChanged(object sender, ColorPicker.Maui.PickedColorChangedEventArgs e)
        {
            await _displayWriter.UpdateConfig(c => c.HighHue = ToEspHue(e.NewPickedColorValue));
        }
    }
}
