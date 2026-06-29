using System.IO;

namespace HoroshieIgry.Core.Mazes;

internal static class MazeLayoutParser
{
    public static MazeDefinition Parse(
        int id,
        string title,
        int difficulty,
        string exitEmoji,
        IReadOnlyList<string> layout)
    {
        var rows = layout.Count;
        var cols = layout.Max(l => l.Length);
        var walls = new bool[rows, cols];
        var start = MazeCell.Invalid;
        var exit = MazeCell.Invalid;

        for (var r = 0; r < rows; r++)
        {
            var line = layout[r];
            for (var c = 0; c < cols; c++)
            {
                var ch = c < line.Length ? line[c] : '#';
                switch (ch)
                {
                    case '#':
                        walls[r, c] = true;
                        break;
                    case 'S':
                        walls[r, c] = false;
                        start = new MazeCell(r, c);
                        break;
                    case 'E':
                        walls[r, c] = false;
                        exit = new MazeCell(r, c);
                        break;
                    default:
                        walls[r, c] = false;
                        break;
                }
            }
        }

        if (!start.IsValid || !exit.IsValid)
            throw new InvalidDataException("В лабиринте должны быть клетки S и E.");

        return new MazeDefinition
        {
            Id = id,
            Title = title,
            Difficulty = difficulty,
            ExitEmoji = exitEmoji,
            Rows = rows,
            Cols = cols,
            Walls = walls,
            Start = start,
            Exit = exit
        };
    }
}
