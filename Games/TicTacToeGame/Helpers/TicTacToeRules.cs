using HoroshieIgry.Games.TicTacToeGame.Models;

namespace HoroshieIgry.Games.TicTacToeGame.Helpers;

public static class TicTacToeRules
{
    public const int BoardSize = 3;
    public const int CellCount = BoardSize * BoardSize;

    private static readonly int[][] WinLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];

    public static bool TryGetWinner(IReadOnlyList<TicTacToeMark> board, out TicTacToeMark winner, out int[]? line)
    {
        foreach (var candidate in WinLines)
        {
            var a = board[candidate[0]];
            if (a == TicTacToeMark.Empty) continue;
            if (a == board[candidate[1]] && a == board[candidate[2]])
            {
                winner = a;
                line = candidate;
                return true;
            }
        }

        winner = TicTacToeMark.Empty;
        line = null;
        return false;
    }

    public static bool IsDraw(IReadOnlyList<TicTacToeMark> board)
        => board.All(m => m != TicTacToeMark.Empty);

    public static IReadOnlyList<TicTacToeMark> ReadMarks(IReadOnlyList<TicTacToeCellModel> cells)
        => cells.Select(c => c.Mark).ToArray();
}
