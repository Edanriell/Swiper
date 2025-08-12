using Swiper.Utils;

namespace Swiper.Controls;

public partial class SwiperControl : ContentView
{
    public SwiperControl()
    {
        InitializeComponent();
        var picture = new PictureGenerator();
        descriptionLabel.Text = picture.Description;
        image.Source = new UriImageSource { Uri = picture.Uri };
    }
}