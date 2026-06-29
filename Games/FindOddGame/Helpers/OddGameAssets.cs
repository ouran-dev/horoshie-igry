using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.FindColorGame.Helpers;

namespace HoroshieIgry.Games.FindOddGame.Helpers;

/// <summary>Ассеты карточек «Найди лишнее».</summary>
public static class OddGameAssets
{
    public static string GetTilePath(KenneyPalette palette)
        => ColorGameAssets.GetTilePath(palette);
}
