namespace AndroidMic;

public class SpectrumBoxes
{
    private BoxView[][] _boxes = [];
    private readonly HorizontalStackLayout _hContainer;
    public SpectrumBoxes(HorizontalStackLayout hContainer)
    {
        _hContainer = hContainer;
    }

    private void GenerateSliders()
    {
        _boxes = new BoxView[32][];
        for (var i = 0; i < 32; i++)
        {

            var column = new VerticalStackLayout();
            _hContainer.Children.Add(column);

            _boxes[i] = new BoxView[8];
            for (var j = 7; j >= 0; j--)
            {
                var box = new BoxView
                {
                    BackgroundColor = Colors.DarkViolet,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                };
                _boxes[i][j] = box;
                column.Children.Add(box);
            }
        }
    }

    private void UpdateBoxeSizes()
    {
        var boxViews = _hContainer.Children.OfType<VerticalStackLayout>().SelectMany(l => l.Children.OfType<BoxView>());
        var availableHeight = _hContainer.HeightRequest;
        var boxHeight = availableHeight / 8.0;
        foreach (var boxView in boxViews)
        {
            var boxWidth = DeviceDisplay.MainDisplayInfo.Width / (32.0 * DeviceDisplay.MainDisplayInfo.Density);
            boxView.WidthRequest = boxWidth;
            boxView.HeightRequest = boxHeight;
        }
    }

    public void Setup()
    {
        if (_boxes.Length > 0)
            return;
        _hContainer.Children.Clear();
        _boxes = [];
        GenerateSliders();
        UpdateBoxeSizes();
    }

    public void Update(double[] bands)
    {
        for (var i = 0; i < 32; i++)
        {
            var bandValue = bands[i];
            var column = _boxes[i];
            for (var j = 0; j < 8; j++)
            {
                var color = j < bandValue - 1 ? Colors.DarkViolet : Colors.Navy;
                column[j].BackgroundColor = color;
            }
        }
    }

    public void OnSizeChanged()
    {
        UpdateBoxeSizes();
    }
}
