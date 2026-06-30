using System.Windows.Media;

namespace HoroshieIgry.Games.LinkDotsGame.Helpers;

public static class LinkDotsPalette
{
    public static readonly Color[] Colors =
    [
        Color.FromRgb(0xE5, 0x39, 0x35),
        Color.FromRgb(0x1E, 0x88, 0xE5),
        Color.FromRgb(0x43, 0xA0, 0x47),
        Color.FromRgb(0xFB, 0x8C, 0x00),
        Color.FromRgb(0x8E, 0x24, 0xAA),
        Color.FromRgb(0x00, 0xAC, 0xC1),
        Color.FromRgb(0xF4, 0x43, 0x36),
        Color.FromRgb(0x5E, 0x35, 0xB1)
    ];

    public static Color PathColor(int colorId) => Colors[colorId % Colors.Length];

    public static Color DotColor(int colorId) => Colors[colorId % Colors.Length];
}
