using EspSpectrum.Core.Fft;

namespace AndroidMic;

public class SpectrumGrid
{
    private BoxView[][] _boxes = [];
    private readonly Grid _grid;

    private readonly Color HighSpectrumColor;
    private readonly Color MidSpectrumColor;
    private readonly Color LowSpectrumColor;
    private readonly Color NoSpectrumColor;

    private static Color ColorInResource(string colorName)
    {
        if (App.Current!.Resources.TryGetValue(colorName, out var color))
            return (Color)color;
        throw new InvalidOperationException($"{colorName} not found in resources");
    }

    public SpectrumGrid(Grid grid)
    {
        _grid = grid;
        HighSpectrumColor = ColorInResource("HighSpectrumColor");
        MidSpectrumColor = ColorInResource("MidSpectrumColor");
        LowSpectrumColor = ColorInResource("LowSpectrumColor");
        NoSpectrumColor = Colors.Transparent;
    }

    private Color GetCellColor(int barValue, int y)
    {
        if (barValue <= y)
            return NoSpectrumColor;

        if (y <= 4)
            return LowSpectrumColor;
        if (y < 7)
            return MidSpectrumColor;
        if (y >= 8)
            return HighSpectrumColor;
        return NoSpectrumColor;
    }

    private void FillBoxes()
    {
        _grid.BatchBegin();
        _boxes = new BoxView[FftProps.NBands][];
        for (var i = 0; i < _boxes.Length; i++)
        {
            _boxes[i] = new BoxView[FftProps.BandHeigth];
            for (var j = 0; j < _boxes[i].Length; j++)
            {
                var box = new BoxView
                {
                    BackgroundColor = NoSpectrumColor,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Opacity = 0.8,
                    Shadow = new Shadow()
                };
                _boxes[i][j] = box;

                // Position inside the Grid
                _grid.SetColumn(box, i);
                _grid.SetRow(box, j);

                _grid.Children.Add(box);
            }
        }
        _grid.BatchCommit();
    }

    public void Setup()
    {
        if (_boxes.Length > 0)
            return;

        FillBoxes();
    }

    public void Update(double[] bands)
    {
        _grid.BatchBegin();
        for (int x = 0; x < _boxes.Length; x++)
        {
            var bandValue = (int)Math.Round(bands[x]);
            for (int y = 0; y < FftProps.BandHeigth; y++)
            {
                int invert = FftProps.BandHeigth - y;  // invert row index (0 = bottom)
                _boxes[x][invert - 1].BackgroundColor = GetCellColor(bandValue, y);
            }
        }
        _grid.BatchCommit();
    }
}
