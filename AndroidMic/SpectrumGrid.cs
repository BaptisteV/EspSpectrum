namespace AndroidMic;

public class SpectrumGrid
{
    private BoxView[][] _boxes = [];
    private readonly Grid _grid;
    public SpectrumGrid(Grid grid)
    {
        _grid = grid;
    }

    private void FillBoxes()
    {
        _grid.BatchBegin();
        _boxes = new BoxView[32][];
        for (var i = 0; i < 32; i++)
        {
            _boxes[i] = new BoxView[8];
            for (var j = 0; j < 8; j++)
            {
                var box = new BoxView
                {
                    BackgroundColor = Colors.DarkViolet,
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
        for (int col = 0; col < 32; col++)
        {
            double bandValue = bands[col];

            for (int row = 0; row < 8; row++)
            {
                int invert = 7 - row;  // invert row index (0 = bottom)
                bool active = row < bandValue - 1;

                _boxes[col][invert].BackgroundColor = active
                    ? Colors.DarkViolet
                    : Colors.Navy;
            }
        }
        _grid.BatchCommit();
    }
}
