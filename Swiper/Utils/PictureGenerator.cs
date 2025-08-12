namespace Swiper.Utils;

internal class PictureGenerator
{
    public PictureGenerator()
    {
        Uri = new Uri($"https://picsum.photos/400/400/?random&ts={DateTime.Now.Ticks}");
        var generator = new DescriptionGenerator();
        Description = generator.Generate();
    }

    public Uri Uri { get; init; }
    public string Description { get; init; }
}