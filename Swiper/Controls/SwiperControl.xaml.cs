using Swiper.Utils;

namespace Swiper.Controls;

public partial class SwiperControl : ContentView
{
    private const double DeadZone = 0.4d;
    private const double DecisionThreshold = 0.4d;

    private static readonly Random _random = new();
    private readonly double _initialRotation;
    private double _screenWidth = -1;

    public SwiperControl()
    {
        InitializeComponent();

        var picture = new PictureGenerator();
        descriptionLabel.Text = picture.Description;
        image.Source = new UriImageSource { Uri = picture.Uri };

        // Show loading initially
        loadingOverlay.IsVisible = true;
        loadingIndicator.IsRunning = true;

        // Hide loading when image loads
        image.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Image.IsLoading))
            {
                loadingOverlay.IsVisible = image.IsLoading;
                loadingIndicator.IsRunning = image.IsLoading;
            }
        };

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);

        _initialRotation = _random.Next(-10, 10);
        photo.RotateTo(_initialRotation, 100, Easing.SinOut);
    }

    public event EventHandler OnLike;
    public event EventHandler OnDeny;

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (Application.Current.MainPage == null) return;

        _screenWidth = Application.Current.MainPage.Width;
    }

    private void CalculatePanState(double panX)
    {
        var width = _screenWidth == -1 ? 400 : _screenWidth;
        var halfScreenWidth = width / 2;
        var deadZoneEnd = DeadZone * halfScreenWidth;

        if (Math.Abs(panX) < deadZoneEnd) return;

        var passedDeadZone = panX < 0 ? panX + deadZoneEnd : panX - deadZoneEnd;
        var decisionZoneEnd = DecisionThreshold * halfScreenWidth;
        var opacity = passedDeadZone / decisionZoneEnd;

        opacity = double.Clamp(opacity, -1d, 1d);

        likeStackLayout.Opacity = opacity;
        denyStackLayout.Opacity = -opacity;
    }

    private bool CheckForExitCriteria()
    {
        var width = _screenWidth == -1 ? 400 : _screenWidth;
        var halfScreenWidth = width / 2;
        var decisionBreakpoint = DeadZone * halfScreenWidth;

        return Math.Abs(photo.TranslationX) > decisionBreakpoint;
    }

    private void Exit()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var direction = photo.TranslationX < 0 ? -1 : 1;

            if (direction > 0) OnLike?.Invoke(this, new EventArgs());

            if (direction < 0) OnDeny?.Invoke(this, new EventArgs());

            await photo.TranslateTo(photo.TranslationX + _screenWidth * direction, photo.TranslationY, 200,
                Easing.CubicIn);
            var parent = Parent as Layout;
            parent?.Children.Remove(this);
        });
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                PanStarted();
                break;

            case GestureStatus.Running:
                PanRunning(e);
                break;

            case GestureStatus.Completed:
                PanCompleted();
                break;
        }
    }

    private void PanStarted() { photo.ScaleTo(1.1, 100); }

    private void PanRunning(PanUpdatedEventArgs e)
    {
        photo.TranslationX = e.TotalX;
        photo.TranslationY = e.TotalY;
        photo.Rotation = _initialRotation + photo.TranslationX / 25;
        CalculatePanState(e.TotalX);
    }

    private void PanCompleted()
    {
        if (CheckForExitCriteria()) Exit();

        likeStackLayout.Opacity = 0;
        denyStackLayout.Opacity = 0;

        photo.TranslateTo(0, 0, 250, Easing.SpringOut);
        photo.RotateTo(_initialRotation, 250, Easing.SpringOut);
        photo.ScaleTo(1);
    }
}