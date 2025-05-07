using EspSpectrum.Core;

namespace EspSpectrum.App
{
    public partial class MainPage : ContentPage
    {
        private readonly Button[][] _buttons = new Button[BandsConfig.NBands][];

        private readonly IFftStream stream;
        private readonly IEspWebsocket ws;
        private readonly CancellationTokenSource cts = new();

        public MainPage(IFftStream stream, IEspWebsocket ws)
        {
            InitializeComponent();
            CreateButtons(BarsContainer);
            this.stream = stream;
            this.ws = ws;
        }

        public void CreateButtons(HorizontalStackLayout layout)
        {
            const int cellSize = 12;
            for (int x = 0; x < BandsConfig.NBands; x++)
            {
                _buttons[x] = new Button[BandsConfig.BandHeigth];
                var bar = new VerticalStackLayout();
                for (int y = 0; y < _buttons[x].Length; y++)
                {
                    var top = (BandsConfig.BandHeigth - 1 - y) * cellSize;
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
            LabelReadLength.Text = $"Read length: {BandsConfig.ReadLength}";
            LabelInterval.Text = $"Interval: {BandsConfig.TargetRate.TotalMilliseconds}ms";
        }

        public void UpdateUI(FftResult fft)
        {
            UpdateBars(fft);
            UpdateLabels();
        }


        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            await foreach (var fft in stream.NextFft(cts.Token))
            {
                await ws.SendAudio(fft.Bands);
                Dispatcher.Dispatch(() => UpdateUI(fft));
            }
        }

        private void ContentPage_Unloaded(object sender, EventArgs e)
        {
            cts.Cancel();
            cts.Dispose();
        }

        private static void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            BandsConfig.TargetRate = TimeSpan.FromMilliseconds(e.NewValue);
        }
    }
}
