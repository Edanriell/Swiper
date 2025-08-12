using Swiper.Controls;

namespace Swiper;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        MainGrid.Children.Add(new SwiperControl());
    }
}