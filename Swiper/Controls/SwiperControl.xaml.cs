using Swiper.Utils;

namespace Swiper.Controls;

public partial class SwiperControl : ContentView
{
    private const double DeadZone = 0.4d;
    private const double DecisionThreshold = 0.4d;
    private static readonly Random random = new();
    private readonly double _initialRotation;
    private double _screenWidth = -1;

    public SwiperControl()
    {
        InitializeComponent();

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);

        _initialRotation = random.Next(-10, 10);
        photo.RotateTo(_initialRotation, 100, Easing.SinOut);

        var picture = new PictureGenerator();
        descriptionLabel.Text = picture.Description;
        image.Source = new UriImageSource { Uri = picture.Uri };

        loadingLabel.SetBinding(IsVisibleProperty, "IsLoading");
        loadingLabel.BindingContext = image;
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

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (Application.Current.MainPage == null) return;
        _screenWidth = Application.Current.MainPage.Width;
    }

    private void CalculatePanState(double panX)
    {
        var halfScreenWidth = _screenWidth / 2;
        var deadZoneEnd = DeadZone * halfScreenWidth;
        if (Math.Abs(panX) < deadZoneEnd) return;
        var passedDeadzone = panX < 0
            ? panX + deadZoneEnd
            : panX -
            deadZoneEnd;
        var decisionZoneEnd = DecisionThreshold * halfScreenWidth;
        var opacity = passedDeadzone / decisionZoneEnd;
        opacity = double.Clamp(opacity, -1, 1);
        likeStackLayout.Opacity = opacity;
        denyStackLayout.Opacity = -opacity;
    }

    private bool CheckForExitCriteria()
    {
        var halfScreenWidth = _screenWidth / 2;
        var decisionBreakpoint = DeadZone * halfScreenWidth;
        return Math.Abs(photo.TranslationX) > decisionBreakpoint;
    }

    private void Exit()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var direction = photo.TranslationX < 0 ? -1 : 1;
            await photo.TranslateTo(photo.TranslationX + _screenWidth * direction, photo.TranslationY, 200,
                Easing.CubicIn);
            var parent = Parent as Layout;
            parent?.Children.Remove(this);
        });
    }
}