using EspSpectrum.Core;

namespace EspSpectrum.App
{
    public partial class MainPage : ContentPage
    {
        private readonly Button[][] _buttons = new Button[FftProps.NBands][];

        private readonly IFftStream _stream;
        private readonly EspSpectrumConfig _config;
        private readonly IEspWebsocket _ws;
        private readonly CancellationTokenSource _cts = new();

        public MainPage(IFftStream stream, EspSpectrumConfig config, IEspWebsocket ws)
        {
            InitializeComponent();
            CreateButtons(BarsContainer);
            _stream = stream;
            _config = config;
            _ws = ws;
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

        private static Color GetCellColor(int barValue, int y)
        {
            if (y >= barValue)
                return Colors.Gray;

            if (y <= 4)
                return Colors.Chartreuse;

            if (y > 4 && y < 7)
                return Colors.Orange;

            return Colors.Red;
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
            LabelReadLength.Text = $"Read length: {FftProps.ReadLength}";
            LabelInterval.Text = $"Interval: {_config.SendInterval.TotalMilliseconds}ms";
        }

        public void UpdateUI(FftResult fft)
        {
            UpdateBars(fft);
            UpdateLabels();
        }


        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            await foreach (var fft in _stream.NextFft(_cts.Token))
            {
                await _ws.SendAudio(fft.Bands);
                Dispatcher.Dispatch(() => UpdateUI(fft));
            }
        }

        private void ContentPage_Unloaded(object sender, EventArgs e)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            _config.SendInterval = TimeSpan.FromMilliseconds(e.NewValue);
        }
    }
}
