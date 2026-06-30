using HoroshieIgry.Games.LinkDotsGame.Models;

namespace HoroshieIgry.Games.LinkDotsGame.Helpers;

public sealed class LinkDotsEngine
{
    private readonly LinkDotsLevel _level;
    private readonly List<(int Row, int Col)>[] _paths;

    public LinkDotsEngine(LinkDotsLevel level)
    {
        _level = level;
        _paths = new List<(int Row, int Col)>[level.Pairs.Count];
        for (var i = 0; i < _paths.Length; i++)
            _paths[i] = [];
    }

    public LinkDotsLevel Level => _level;

    public event Action? StateChanged;

    public IReadOnlyList<(int Row, int Col)> GetPath(int colorId) => _paths[colorId];

    public int? GetColorAt(int row, int col)
    {
        foreach (var pair in _level.Pairs)
        {
            if (pair.IsEndpoint(row, col))
                return pair.ColorId;
        }

        for (var colorId = 0; colorId < _paths.Length; colorId++)
        {
            if (_paths[colorId].Contains((row, col)))
                return colorId;
        }

        return null;
    }

    public void BeginStroke(int colorId, int row, int col)
    {
        if (!BelongsToColor(colorId, row, col))
            return;

        var path = _paths[colorId];
        var index = path.IndexOf((row, col));
        if (index >= 0)
        {
            path.RemoveRange(index + 1, path.Count - index - 1);
        }
        else if (IsEndpoint(colorId, row, col))
        {
            path.Clear();
            path.Add((row, col));
        }

        StateChanged?.Invoke();
    }

    public bool TryExtend(int colorId, int row, int col)
    {
        var path = _paths[colorId];
        if (path.Count == 0)
            return false;

        var last = path[^1];
        if (last == (row, col))
            return false;

        if (!IsAdjacent(last, (row, col)))
            return false;

        if (path.Count >= 2 && path[^2] == (row, col))
        {
            path.RemoveAt(path.Count - 1);
            StateChanged?.Invoke();
            return true;
        }

        var existingIndex = path.IndexOf((row, col));
        if (existingIndex >= 0)
        {
            path.RemoveRange(existingIndex + 1, path.Count - existingIndex - 1);
            StateChanged?.Invoke();
            return true;
        }

        if (!CanEnter(colorId, row, col))
            return false;

        path.Add((row, col));
        StateChanged?.Invoke();
        return true;
    }

    public bool IsSolved()
    {
        foreach (var pair in _level.Pairs)
        {
            var path = _paths[pair.ColorId];
            if (path.Count < 2)
                return false;

            if (!path.Contains((pair.StartRow, pair.StartCol)))
                return false;

            if (!path.Contains((pair.EndRow, pair.EndCol)))
                return false;
        }

        return true;
    }

    public void Reset()
    {
        foreach (var path in _paths)
            path.Clear();

        StateChanged?.Invoke();
    }

    private bool BelongsToColor(int colorId, int row, int col)
    {
        if (IsEndpoint(colorId, row, col))
            return true;

        return _paths[colorId].Contains((row, col));
    }

    private bool IsEndpoint(int colorId, int row, int col)
    {
        var pair = _level.Pairs.First(p => p.ColorId == colorId);
        return pair.IsEndpoint(row, col);
    }

    private bool CanEnter(int colorId, int row, int col)
    {
        if (row < 0 || col < 0 || row >= _level.Rows || col >= _level.Cols)
            return false;

        foreach (var pair in _level.Pairs)
        {
            if (pair.ColorId == colorId)
                continue;

            if (pair.IsEndpoint(row, col))
                return false;
        }

        for (var otherColor = 0; otherColor < _paths.Length; otherColor++)
        {
            if (otherColor == colorId)
                continue;

            if (_paths[otherColor].Contains((row, col)))
                return false;
        }

        return true;
    }

    private static bool IsAdjacent((int Row, int Col) a, (int Row, int Col) b)
    {
        var rowDelta = Math.Abs(a.Row - b.Row);
        var colDelta = Math.Abs(a.Col - b.Col);
        return rowDelta + colDelta == 1;
    }
}
