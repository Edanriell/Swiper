using Swiper.Controls;

namespace Swiper;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        AddInitialPhotos();
        MainGrid.Children.Add(new SwiperControl());
    }

    private void AddInitialPhotos()
    {
        for (var i = 0; i < 10; i++) InsertPhoto()
    }

    private void InsertPhoto()
    {
        var photo = new SwiperControl();
        MainGrid.Children.Insert(0, photo);
    }
}