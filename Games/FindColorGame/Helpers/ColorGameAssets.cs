using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.FindColorGame.Models;

namespace HoroshieIgry.Games.FindColorGame.Helpers;

/// <summary>Пути к Kenney-ассетам и подписи цветов для игры «Найди цвет».</summary>
public static class ColorGameAssets
{
    public static readonly KenneyPalette[] PlayPalettes =
    {
        KenneyPalette.Blue,
        KenneyPalette.Green,
        KenneyPalette.Red,
        KenneyPalette.Yellow,
        KenneyPalette.Grey
    };

    public static readonly ColorFigureShape[] AllShapes =
    {
        ColorFigureShape.Circle,
        ColorFigureShape.Square,
        ColorFigureShape.Star,
        ColorFigureShape.Cross
    };

    public static string GetShapePath(KenneyPalette palette, ColorFigureShape shape)
        => shape switch
        {
            ColorFigureShape.Circle => KenneyPaths.InPalette(palette, "icon_circle.svg"),
            ColorFigureShape.Square => KenneyPaths.InPalette(palette, "icon_square.svg"),
            ColorFigureShape.Star => KenneyPaths.InPalette(palette, "star.svg"),
            ColorFigureShape.Cross => KenneyPaths.InPalette(palette, "icon_cross.svg"),
            _ => KenneyPaths.InPalette(palette, "icon_circle.svg")
        };

    public static string GetTilePath(KenneyPalette palette)
        => KenneyPaths.InPalette(palette, "button_square_depth_flat.svg");

    public static string GetColorName(KenneyPalette palette)
        => palette switch
        {
            KenneyPalette.Blue => "Синий",
            KenneyPalette.Green => "Зелёный",
            KenneyPalette.Red => "Красный",
            KenneyPalette.Yellow => "Жёлтый",
            KenneyPalette.Grey => "Серый",
            _ => "Цвет"
        };
}
