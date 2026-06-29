using HoroshieIgry.Games.TicTacToeGame.Models;

namespace HoroshieIgry.Games.TicTacToeGame.Helpers;

/// <summary>ИИ для крестиков-ноликов: сначала мягкий, потом сильнее.</summary>
public static class TicTacToeAi
{
    public static int PickMove(IReadOnlyList<TicTacToeMark> board, TicTacToeMark aiMark, int level, Random random)
    {
        var empty = GetEmptyIndices(board);
        if (empty.Count == 0) return -1;

        var blunderChance = level switch
        {
            <= 1 => 0.45,
            <= 3 => 0.25,
            <= 5 => 0.12,
            _ => 0.0
        };

        if (random.NextDouble() < blunderChance)
            return empty[random.Next(empty.Count)];

        return GetBestMove(board, aiMark);
    }

    private static int GetBestMove(IReadOnlyList<TicTacToeMark> board, TicTacToeMark aiMark)
    {
        var playerMark = aiMark == TicTacToeMark.X ? TicTacToeMark.O : TicTacToeMark.X;
        var working = board.ToArray();
        var bestScore = int.MinValue;
        var bestMove = -1;

        foreach (var index in GetEmptyIndices(board))
        {
            working[index] = aiMark;
            var score = Minimax(working, false, aiMark, playerMark);
            working[index] = TicTacToeMark.Empty;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = index;
            }
        }

        return bestMove;
    }

    private static int Minimax(TicTacToeMark[] board, bool isAiTurn, TicTacToeMark aiMark, TicTacToeMark playerMark)
    {
        if (TicTacToeRules.TryGetWinner(board, out var winner, out _))
            return winner == aiMark ? 10 : winner == playerMark ? -10 : 0;

        if (TicTacToeRules.IsDraw(board))
            return 0;

        var mark = isAiTurn ? aiMark : playerMark;
        var scores = new List<int>();

        for (var i = 0; i < board.Length; i++)
        {
            if (board[i] != TicTacToeMark.Empty) continue;

            board[i] = mark;
            scores.Add(Minimax(board, !isAiTurn, aiMark, playerMark));
            board[i] = TicTacToeMark.Empty;
        }

        return isAiTurn ? scores.Max() : scores.Min();
    }

    private static List<int> GetEmptyIndices(IReadOnlyList<TicTacToeMark> board)
    {
        var result = new List<int>();
        for (var i = 0; i < board.Count; i++)
        {
            if (board[i] == TicTacToeMark.Empty)
                result.Add(i);
        }

        return result;
    }
}
