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
    public SpectrumGrid(Grid grid)
    {
        _grid = grid;
        if (App.Current!.Resources.TryGetValue("HighSpectrumColor", out var highSpectrumColor))
            HighSpectrumColor = (Color)highSpectrumColor;
        if (App.Current!.Resources.TryGetValue("MidSpectrumColor", out var midSpectrumColor))
            MidSpectrumColor = (Color)midSpectrumColor;
        if (App.Current!.Resources.TryGetValue("LowSpectrumColor", out var lowSpectrumColor))
            LowSpectrumColor = (Color)lowSpectrumColor;
        NoSpectrumColor = Colors.Transparent;
    }

    private Color GetCellColor(double barValue, int y)
    {
        if (y >= barValue)
            return NoSpectrumColor;

        if (y <= 4)
            return LowSpectrumColor;

        if (y > 4 && y < 7)
            return MidSpectrumColor;

        return HighSpectrumColor;
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
        for (int col = 0; col < _boxes.Length; col++)
        {
            double bandValue = bands[col];
            for (int row = 0; row < FftProps.BandHeigth; row++)
            {
                int invert = FftProps.BandHeigth - 1 - row;  // invert row index (0 = bottom)
                _boxes[col][invert].BackgroundColor = GetCellColor(bandValue - 1, row);
            }
        }
        _grid.BatchCommit();
    }
}
