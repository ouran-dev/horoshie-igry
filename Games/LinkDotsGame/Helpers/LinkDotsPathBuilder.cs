namespace HoroshieIgry.Games.LinkDotsGame.Helpers;

/// <summary>Строит непрерывные пути по сетке для гарантированно решаемых уровней.</summary>
internal static class LinkDotsPathBuilder
{
    public static (int Row, int Col)[] RowSnake(int rowFrom, int rowTo, int cols)
    {
        var cells = new List<(int Row, int Col)>();
        for (var row = rowFrom; row <= rowTo; row++)
        {
            if ((row - rowFrom) % 2 == 0)
            {
                for (var col = 0; col < cols; col++)
                    cells.Add((row, col));
            }
            else
            {
                for (var col = cols - 1; col >= 0; col--)
                    cells.Add((row, col));
            }
        }

        return cells.ToArray();
    }

    public static (int Row, int Col)[] ColSnake(int colFrom, int colTo, int rows)
    {
        var cells = new List<(int Row, int Col)>();
        for (var col = colFrom; col <= colTo; col++)
        {
            if ((col - colFrom) % 2 == 0)
            {
                for (var row = 0; row < rows; row++)
                    cells.Add((row, col));
            }
            else
            {
                for (var row = rows - 1; row >= 0; row--)
                    cells.Add((row, col));
            }
        }

        return cells.ToArray();
    }

    public static (int Row, int Col)[] RectSnake(int row, int col, int height, int width)
    {
        var cells = new List<(int Row, int Col)>();
        for (var r = row; r < row + height; r++)
        {
            var local = r - row;
            if (local % 2 == 0)
            {
                for (var c = col; c < col + width; c++)
                    cells.Add((r, c));
            }
            else
            {
                for (var c = col + width - 1; c >= col; c--)
                    cells.Add((r, c));
            }
        }

        return cells.ToArray();
    }

    public static (int Row, int Col)[] Reversed((int Row, int Col)[] path)
        => path.Reverse().ToArray();
}
