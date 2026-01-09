namespace AndroidMic;

public class StarRating : ContentView
{
    public static readonly BindableProperty RatingProperty =
        BindableProperty.Create(
            nameof(Rating),
            typeof(double),
            typeof(StarRating),
            0.0,
            propertyChanged: OnRatingChanged);

    public static readonly BindableProperty StarSizeProperty =
        BindableProperty.Create(
            nameof(StarSize),
            typeof(double),
            typeof(StarRating),
            30.0,
            propertyChanged: OnStarSizeChanged);

    public static readonly BindableProperty StarColorProperty =
        BindableProperty.Create(
            nameof(StarColor),
            typeof(Color),
            typeof(StarRating),
            Colors.Gold,
            propertyChanged: OnStarColorChanged);

    public static readonly BindableProperty EmptyStarColorProperty =
        BindableProperty.Create(
            nameof(EmptyStarColor),
            typeof(Color),
            typeof(StarRating),
            Colors.LightGray,
            propertyChanged: OnEmptyStarColorChanged);

    public double Rating
    {
        get => (double)GetValue(RatingProperty);
        set => SetValue(RatingProperty, value);
    }

    public double StarSize
    {
        get => (double)GetValue(StarSizeProperty);
        set => SetValue(StarSizeProperty, value);
    }

    public Color StarColor
    {
        get => (Color)GetValue(StarColorProperty);
        set => SetValue(StarColorProperty, value);
    }

    public Color EmptyStarColor
    {
        get => (Color)GetValue(EmptyStarColorProperty);
        set => SetValue(EmptyStarColorProperty, value);
    }

    private readonly Grid _container;
    private readonly List<Frame> _stars = [];

    public StarRating()
    {
        _container = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 4
        };

        for (int i = 0; i < 5; i++)
        {
            var starContainer = new Frame
            {
                Padding = 0,
                CornerRadius = 0,
                BorderColor = Colors.Transparent,
                BackgroundColor = Colors.Transparent,
                HasShadow = false,
                IsClippedToBounds = true
            };

            var backgroundStar = new Label
            {
                Text = "★",
                FontSize = StarSize,
                TextColor = EmptyStarColor,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            var foregroundStar = new Label
            {
                Text = "★",
                FontSize = StarSize,
                TextColor = StarColor,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = 0
            };

            var grid = new Grid();
            grid.Add(backgroundStar);
            grid.Add(foregroundStar);

            starContainer.Content = grid;
            _stars.Add(starContainer);

            _container.Add(starContainer, i, 0);
        }

        Content = _container;
        UpdateStars();
    }

    private static void OnRatingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StarRating starRating)
            starRating.UpdateStars();
    }

    private static void OnStarSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StarRating starRating)
            starRating.UpdateStarSizes();
    }

    private static void OnStarColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StarRating starRating)
            starRating.UpdateStarColors();
    }

    private static void OnEmptyStarColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StarRating starRating)
            starRating.UpdateStarColors();
    }

    private void UpdateStars()
    {
        var rating = Math.Clamp(Rating, 0, 5);

        for (int i = 0; i < 5; i++)
        {
            if (_stars[i].Content is Grid grid && grid.Children.Count >= 2)
            {
                var foregroundStar = grid.Children[1] as Label;
                if (foregroundStar != null)
                {
                    double fillAmount = Math.Clamp(rating - i, 0, 1);
                    foregroundStar.WidthRequest = fillAmount * StarSize;
                }
            }
        }
    }

    private void UpdateStarSizes()
    {
        foreach (var star in _stars)
        {
            if (star.Content is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is Label label)
                    {
                        label.FontSize = StarSize;
                    }
                }
            }
        }
        UpdateStars();
    }

    private void UpdateStarColors()
    {
        foreach (var star in _stars)
        {
            if (star.Content is Grid grid && grid.Children.Count >= 2)
            {
                if (grid.Children[0] is Label backgroundStar)
                    backgroundStar.TextColor = EmptyStarColor;

                if (grid.Children[1] is Label foregroundStar)
                    foregroundStar.TextColor = StarColor;
            }
        }
    }
}