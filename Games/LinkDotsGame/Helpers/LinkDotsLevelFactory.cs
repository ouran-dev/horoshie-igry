using HoroshieIgry.Games.LinkDotsGame.Models;

namespace HoroshieIgry.Games.LinkDotsGame.Helpers;

public static class LinkDotsLevelFactory
{
    public const int MaxLevel = 20;

    private static LinkDotsLevel[]? _cachedLevels;

    public static LinkDotsLevel CreateForLevel(int level)
    {
        var index = Math.Clamp(level, 1, MaxLevel) - 1;
        return BuildLevels()[index];
    }

    private static LinkDotsLevel[] BuildLevels()
    {
        if (_cachedLevels is not null)
            return _cachedLevels;

        var levels = new[]
        {
            // ── 3×3: знакомство ─────────────────────────────────────────────
            FromPaths(1, "Старт", 3, 3,
                (0, LinkDotsPathBuilder.RowSnake(0, 0, 3)),
                (1, LinkDotsPathBuilder.RowSnake(2, 2, 3))),

            FromPaths(2, "Столбики", 3, 3,
                (0, LinkDotsPathBuilder.ColSnake(0, 0, 3)),
                (1, LinkDotsPathBuilder.ColSnake(2, 2, 3))),

            // ── 4×4: первые повороты ───────────────────────────────────────
            FromPaths(3, "Полосы", 4, 4,
                (0, LinkDotsPathBuilder.RowSnake(0, 1, 4)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RowSnake(2, 3, 4)))),

            FromPaths(4, "Колонки", 4, 4,
                (0, LinkDotsPathBuilder.ColSnake(0, 1, 4)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.ColSnake(2, 3, 4)))),

            FromPaths(5, "Три дорожки", 4, 4,
                (0, LinkDotsPathBuilder.RowSnake(0, 0, 4)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RowSnake(1, 2, 4))),
                (2, LinkDotsPathBuilder.RowSnake(3, 3, 4))),

            FromPaths(6, "Четыре уголка", 4, 4,
                (0, LinkDotsPathBuilder.RectSnake(0, 0, 2, 2)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(0, 2, 2, 2))),
                (2, LinkDotsPathBuilder.RectSnake(2, 0, 2, 2)),
                (3, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(2, 2, 2, 2)))),

            // ── 5×5: длиннее и больше цветов ───────────────────────────────
            FromPaths(7, "Длинные нитки", 5, 5,
                (0, LinkDotsPathBuilder.RowSnake(0, 2, 5)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RowSnake(3, 4, 5)))),

            FromPaths(8, "Три реки", 5, 5,
                (0, LinkDotsPathBuilder.RowSnake(0, 1, 5)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RowSnake(2, 3, 5))),
                (2, LinkDotsPathBuilder.RowSnake(4, 4, 5))),

            FromPaths(9, "Четыре змейки", 5, 5,
                (0, LinkDotsPathBuilder.RectSnake(0, 0, 3, 3)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(0, 3, 3, 2))),
                (2, LinkDotsPathBuilder.RectSnake(3, 0, 2, 3)),
                (3, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(3, 3, 2, 2)))),

            FromPaths(10, "Пять дорожек", 5, 5,
                (0, LinkDotsPathBuilder.RowSnake(0, 0, 5)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RowSnake(1, 1, 5))),
                (2, LinkDotsPathBuilder.RowSnake(2, 2, 5)),
                (3, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RowSnake(3, 3, 5))),
                (4, LinkDotsPathBuilder.RowSnake(4, 4, 5))),

            // ── 6×6: широкое поле ─────────────────────────────────────────
            FromPaths(11, "Большие повороты", 6, 6,
                (0, LinkDotsPathBuilder.RowSnake(0, 2, 6)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RowSnake(3, 5, 6)))),

            FromPaths(12, "Три колонны", 6, 6,
                (0, LinkDotsPathBuilder.ColSnake(0, 1, 6)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.ColSnake(2, 3, 6))),
                (2, LinkDotsPathBuilder.ColSnake(4, 5, 6))),

            FromPaths(13, "Четыре сектора", 6, 6,
                (0, LinkDotsPathBuilder.RectSnake(0, 0, 3, 3)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(0, 3, 3, 3))),
                (2, LinkDotsPathBuilder.RectSnake(3, 0, 3, 3)),
                (3, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(3, 3, 3, 3)))),

            FromPaths(14, "Лабиринт", 6, 6,
                (0, [(0, 0), (0, 1), (0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2), (5, 3), (5, 4), (5, 5)]),
                (1, LinkDotsPathBuilder.Reversed([(5, 0), (5, 1), (4, 1), (3, 1), (2, 1), (1, 1), (1, 0), (2, 0), (3, 0), (4, 0)])),
                (2, [(0, 3), (0, 4), (0, 5), (1, 5), (1, 4), (1, 3), (2, 3), (2, 4), (2, 5), (3, 5), (3, 4), (3, 3), (4, 3), (4, 4), (4, 5)])),

            FromPaths(15, "Пять нитей", 6, 6,
                (0, LinkDotsPathBuilder.RectSnake(0, 0, 2, 6)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(2, 0, 2, 6))),
                (2, LinkDotsPathBuilder.RectSnake(4, 0, 1, 3)),
                (3, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(4, 3, 1, 3))),
                (4, LinkDotsPathBuilder.RectSnake(5, 0, 1, 6))),

            // ── 6×6: сложные раскладки ─────────────────────────────────────
            FromPaths(16, "Узлы", 6, 6,
                (0, [(0, 0), (0, 1), (1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (5, 0)]),
                (1, LinkDotsPathBuilder.Reversed([(0, 5), (0, 4), (1, 4), (2, 4), (3, 4), (4, 4), (5, 4), (5, 5)])),
                (2, [(1, 0), (2, 0), (3, 0), (4, 0)]),
                (3, LinkDotsPathBuilder.Reversed([(1, 5), (2, 5), (3, 5), (4, 5)])),
                (4, LinkDotsPathBuilder.RectSnake(2, 2, 2, 2))),

            FromPaths(17, "Кольцо", 6, 6,
                (0, [(0, 0), (0, 1), (0, 2), (0, 3), (0, 4), (0, 5), (1, 5), (2, 5), (3, 5), (4, 5), (5, 5), (5, 4), (5, 3), (5, 2), (5, 1), (5, 0)]),
                (1, LinkDotsPathBuilder.Reversed([(1, 0), (2, 0), (3, 0), (4, 0), (4, 1), (4, 2), (4, 3), (4, 4), (3, 4), (2, 4), (1, 4), (1, 3), (1, 2), (1, 1)])),
                (2, LinkDotsPathBuilder.RectSnake(2, 1, 2, 2))),

            FromPaths(18, "Шесть блоков", 6, 6,
                (0, LinkDotsPathBuilder.RectSnake(0, 0, 3, 2)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(0, 2, 3, 2))),
                (2, LinkDotsPathBuilder.RectSnake(0, 4, 3, 2)),
                (3, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(3, 0, 3, 2))),
                (4, LinkDotsPathBuilder.RectSnake(3, 2, 3, 2)),
                (5, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(3, 4, 3, 2)))),

            FromPaths(19, "Переплетение", 6, 6,
                (0, LinkDotsPathBuilder.RectSnake(0, 0, 2, 3)),
                (1, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(0, 3, 2, 3))),
                (2, LinkDotsPathBuilder.RectSnake(2, 0, 2, 3)),
                (3, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(2, 3, 2, 3))),
                (4, LinkDotsPathBuilder.RectSnake(4, 0, 2, 3)),
                (5, LinkDotsPathBuilder.Reversed(LinkDotsPathBuilder.RectSnake(4, 3, 2, 3)))),

            FromPaths(20, "Финал", 6, 6,
                (0, [(0, 0), (0, 1), (0, 2), (0, 3), (0, 4), (0, 5), (1, 5), (2, 5), (3, 5), (4, 5), (5, 5), (5, 4), (5, 3), (5, 2), (5, 1), (5, 0)]),
                (1, LinkDotsPathBuilder.Reversed([(1, 0), (2, 0), (3, 0), (4, 0), (4, 1), (4, 2), (4, 3), (4, 4), (3, 4), (2, 4), (1, 4), (1, 3), (1, 2), (1, 1)])),
                (2, [(2, 1), (2, 2), (2, 3), (3, 3), (3, 2), (3, 1)])),
        };

        ValidateLevels(levels);

        _cachedLevels = levels;
        return levels;
    }

    private static LinkDotsLevel FromPaths(
        int id,
        string title,
        int rows,
        int cols,
        params (int colorId, (int Row, int Col)[] cells)[] paths)
    {
        var pairs = new List<LinkDotsPair>();
        var occupied = new HashSet<(int Row, int Col)>();

        foreach (var (colorId, cells) in paths)
        {
            if (cells.Length < 2)
                throw new InvalidOperationException($"Уровень {id}: путь цвета {colorId} слишком короткий.");

            foreach (var cell in cells)
            {
                if (cell.Row < 0 || cell.Col < 0 || cell.Row >= rows || cell.Col >= cols)
                    throw new InvalidOperationException($"Уровень {id}: клетка ({cell.Row},{cell.Col}) вне поля.");

                if (!occupied.Add(cell))
                    throw new InvalidOperationException($"Уровень {id}: клетка ({cell.Row},{cell.Col}) пересекается.");
            }

            for (var i = 0; i < cells.Length - 1; i++)
            {
                var delta = Math.Abs(cells[i].Row - cells[i + 1].Row) + Math.Abs(cells[i].Col - cells[i + 1].Col);
                if (delta != 1)
                    throw new InvalidOperationException($"Уровень {id}: разрыв в пути цвета {colorId}.");
            }

            pairs.Add(new LinkDotsPair
            {
                ColorId = colorId,
                StartRow = cells[0].Row,
                StartCol = cells[0].Col,
                EndRow = cells[^1].Row,
                EndCol = cells[^1].Col,
                PathColor = LinkDotsPalette.PathColor(colorId),
                DotColor = LinkDotsPalette.DotColor(colorId)
            });
        }

        return new LinkDotsLevel
        {
            Id = id,
            Title = title,
            Rows = rows,
            Cols = cols,
            Pairs = pairs
        };
    }

    private static void ValidateLevels(LinkDotsLevel[] levels)
    {
#if DEBUG
        foreach (var level in levels)
        {
            if (!LinkDotsSolver.IsSolvable(level))
                throw new InvalidOperationException($"Уровень {level.Id} «{level.Title}» нерешаем.");
        }
#endif
    }
}
