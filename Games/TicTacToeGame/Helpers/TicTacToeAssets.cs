using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.TicTacToeGame.Models;

namespace HoroshieIgry.Games.TicTacToeGame.Helpers;

public static class TicTacToeAssets
{
    public const TicTacToeMark PlayerMark = TicTacToeMark.X;
    public const TicTacToeMark AiMark = TicTacToeMark.O;

    public static string GetSymbolPath(TicTacToeMark mark)
        => mark switch
        {
            TicTacToeMark.X => KenneyPaths.InPalette(KenneyPalette.Blue, "icon_cross.svg"),
            TicTacToeMark.O => KenneyPaths.InPalette(KenneyPalette.Red, "icon_circle.svg"),
            _ => string.Empty
        };
}
