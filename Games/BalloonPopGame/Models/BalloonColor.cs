using System.Windows.Media;

namespace HoroshieIgry.Games.BalloonPopGame.Models;

public enum BalloonColor
{
    Red,
    Green,
    Blue,
    Yellow,
    Orange,
    Purple
}

public static class BalloonColorExtensions
{
    public static string DisplayName(this BalloonColor color) => color switch
    {
        BalloonColor.Red => "красный",
        BalloonColor.Green => "зелёный",
        BalloonColor.Blue => "синий",
        BalloonColor.Yellow => "жёлтый",
        BalloonColor.Orange => "оранжевый",
        BalloonColor.Purple => "фиолетовый",
        _ => "цветной"
    };

    public static string DisplayNamePlural(this BalloonColor color) => color switch
    {
        BalloonColor.Red => "красные",
        BalloonColor.Green => "зелёные",
        BalloonColor.Blue => "синие",
        BalloonColor.Yellow => "жёлтые",
        BalloonColor.Orange => "оранжевые",
        BalloonColor.Purple => "фиолетовые",
        _ => "цветные"
    };

    public static Color FillColor(this BalloonColor color) => color switch
    {
        BalloonColor.Red => Color.FromRgb(0xEF, 0x53, 0x50),
        BalloonColor.Green => Color.FromRgb(0x66, 0xBB, 0x6A),
        BalloonColor.Blue => Color.FromRgb(0x42, 0xA5, 0xF5),
        BalloonColor.Yellow => Color.FromRgb(0xFF, 0xEE, 0x58),
        BalloonColor.Orange => Color.FromRgb(0xFF, 0xA7, 0x26),
        BalloonColor.Purple => Color.FromRgb(0xAB, 0x47, 0xBC),
        _ => Colors.Gray
    };

    public static Color DarkColor(this BalloonColor color)
    {
        var baseColor = color.FillColor();
        return Color.FromRgb(
            (byte)(baseColor.R * 0.72),
            (byte)(baseColor.G * 0.72),
            (byte)(baseColor.B * 0.72));
    }

    public static Color LightColor(this BalloonColor color)
    {
        var baseColor = color.FillColor();
        return Color.FromRgb(
            (byte)Math.Min(255, baseColor.R + (255 - baseColor.R) * 0.55),
            (byte)Math.Min(255, baseColor.G + (255 - baseColor.G) * 0.55),
            (byte)Math.Min(255, baseColor.B + (255 - baseColor.B) * 0.55));
    }
}
