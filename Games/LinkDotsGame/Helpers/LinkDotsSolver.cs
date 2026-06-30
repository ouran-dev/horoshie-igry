using HoroshieIgry.Games.LinkDotsGame.Models;

namespace HoroshieIgry.Games.LinkDotsGame.Helpers;

/// <summary>Проверяет, можно ли соединить все пары без пересечений.</summary>
public static class LinkDotsSolver
{
    public static bool IsSolvable(LinkDotsLevel level)
    {
        var used = new HashSet<(int Row, int Col)>();
        return SolvePair(level, 0, used);
    }

    private static bool SolvePair(LinkDotsLevel level, int pairIndex, HashSet<(int Row, int Col)> used)
    {
        if (pairIndex >= level.Pairs.Count)
            return true;

        var pair = level.Pairs[pairIndex];
        var path = new List<(int Row, int Col)>();
        var start = (pair.StartRow, pair.StartCol);
        var end = (pair.EndRow, pair.EndCol);

        path.Add(start);
        return FindPath(level, pair, start, end, used, path, pairIndex);
    }

    private static bool FindPath(
        LinkDotsLevel level,
        LinkDotsPair pair,
        (int Row, int Col) current,
        (int Row, int Col) end,
        HashSet<(int Row, int Col)> used,
        List<(int Row, int Col)> path,
        int pairIndex)
    {
        if (current == end)
        {
            foreach (var cell in path)
                used.Add(cell);

            if (SolvePair(level, pairIndex + 1, used))
                return true;

            foreach (var cell in path)
                used.Remove(cell);

            return false;
        }

        foreach (var next in AdjacentCells(current))
        {
            if (!IsInside(level, next))
                continue;

            if (path.Contains(next))
                continue;

            if (used.Contains(next))
                continue;

            if (IsForeignEndpoint(level, pair, next))
                continue;

            path.Add(next);

            if (FindPath(level, pair, next, end, used, path, pairIndex))
                return true;

            path.RemoveAt(path.Count - 1);
        }

        return false;
    }

    private static bool IsForeignEndpoint(LinkDotsLevel level, LinkDotsPair pair, (int Row, int Col) cell)
    {
        foreach (var other in level.Pairs)
        {
            if (other.ColorId == pair.ColorId)
                continue;

            if (other.IsEndpoint(cell.Row, cell.Col))
                return true;
        }

        return false;
    }

    private static IEnumerable<(int Row, int Col)> AdjacentCells((int Row, int Col) cell)
    {
        yield return (cell.Row - 1, cell.Col);
        yield return (cell.Row + 1, cell.Col);
        yield return (cell.Row, cell.Col - 1);
        yield return (cell.Row, cell.Col + 1);
    }

    private static bool IsInside(LinkDotsLevel level, (int Row, int Col) cell)
        => cell.Row >= 0 && cell.Col >= 0 && cell.Row < level.Rows && cell.Col < level.Cols;
}
