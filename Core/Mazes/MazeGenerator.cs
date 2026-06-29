using System.IO;

namespace HoroshieIgry.Core.Mazes;

/// <summary>Генерация лабиринтов для уровней (экспорт в JSON).</summary>
public static class MazeGenerator
{
    private static readonly string[] ExitEmojis = ["⭐", "🚪", "🏁"];

    public static IReadOnlyList<(int Id, string Title, int Difficulty, string ExitEmoji, IReadOnlyList<string> Layout)> CreateCatalog()
    {
        var result = new List<(int, string, int, string, IReadOnlyList<string>)>();
        var random = new Random(2026);

        result.Add((1, "Первый шаг", 1, "⭐", SimpleCorridor(9, 3)));
        result.Add((2, "Поворот", 1, "🚪", SimpleTurn()));
        result.Add((3, "Две дорожки", 2, "⭐", SimpleFork()));
        result.Add((4, "Обход", 2, "🏁", SimpleLoop()));

        for (var id = 5; id <= 20; id++)
        {
            var difficulty = id switch
            {
                <= 7 => 2,
                <= 12 => 3,
                <= 16 => 4,
                _ => 5
            };

            var size = id switch
            {
                <= 7 => 11,
                <= 12 => 13,
                <= 16 => 15,
                _ => 17
            };

            var loops = id >= 10 ? 2 + (id - 10) / 3 : 0;
            var layout = CreatePerfectMaze(size, size, random, loops);
            result.Add((id, $"Уровень {id}", difficulty, ExitEmojis[(id - 1) % ExitEmojis.Length], layout));
        }

        return result;
    }

    public static IReadOnlyList<MazeDefinition> CreateDefinitions()
        => CreateCatalog()
            .Select(entry => MazeLayoutParser.Parse(
                entry.Id,
                entry.Title,
                entry.Difficulty,
                entry.ExitEmoji,
                entry.Layout))
            .OrderBy(m => m.Id)
            .ToList();

    public static void ExportToDirectory(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
        foreach (var old in Directory.GetFiles(directoryPath, "level-*.json"))
            File.Delete(old);

        foreach (var entry in CreateCatalog())
        {
            var dto = new
            {
                id = entry.Id,
                title = entry.Title,
                difficulty = entry.Difficulty,
                exitEmoji = entry.ExitEmoji,
                layout = entry.Layout
            };

            var json = System.Text.Json.JsonSerializer.Serialize(dto, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var path = Path.Combine(directoryPath, $"level-{entry.Id:D2}.json");
            File.WriteAllText(path, json);
        }
    }

    private static List<string> SimpleCorridor(int width, int pathRows)
    {
        var rows = pathRows + 2;
        var lines = new List<string> { new string('#', width) };
        for (var r = 1; r <= pathRows; r++)
        {
            var line = new char[width];
            Array.Fill(line, '#');
            line[1] = r == 1 ? 'S' : '.';
            for (var c = 2; c < width - 2; c++)
                line[c] = '.';
            line[width - 2] = r == pathRows ? 'E' : '.';
            lines.Add(new string(line));
        }

        lines.Add(new string('#', width));
        return lines;
    }

    private static List<string> SimpleTurn()
        =>
        [
            "###########",
            "#S........#",
            "#.#######.#",
            "#.......#.#",
            "#.#####.#.#",
            "#.....#...#",
            "#####.#..E#",
            "###########"
        ];

    private static List<string> SimpleFork()
        =>
        [
            "#############",
            "#S....#.....#",
            "#.###.#.###.#",
            "#.#...#...#.#",
            "#.#.#####.#.#",
            "#.#.....#.#.#",
            "#.#####.#.#.#",
            "#.....#...#E#",
            "#############"
        ];

    private static List<string> SimpleLoop()
        =>
        [
            "###############",
            "#S............#",
            "#.###########.#",
            "#.#.........#.#",
            "#.#.#######.#.#",
            "#.#.#.....#.#.#",
            "#.#.#.###.#.#.#",
            "#...#...#...#E#",
            "###############"
        ];

    private static List<string> CreatePerfectMaze(int innerSize, int innerHeight, Random random, int extraPassages)
    {
        var rows = innerHeight | 1;
        var cols = innerSize | 1;
        if (rows < 7) rows = 7;
        if (cols < 7) cols = 7;

        var grid = new bool[rows, cols];
        for (var r = 0; r < rows; r++)
        for (var c = 0; c < cols; c++)
            grid[r, c] = true;

        var start = new MazeCell(1, 1);
        Carve(grid, start.Row, start.Col, random);

        for (var i = 0; i < extraPassages; i++)
            TryBreakRandomWall(grid, random);

        var exit = FindFarthestCell(grid, start);
        return ToLayout(grid, start, exit);
    }

    private static void Carve(bool[,] walls, int row, int col, Random random)
    {
        walls[row, col] = false;
        var directions = new List<(int Dr, int Dc)> { (-2, 0), (2, 0), (0, -2), (0, 2) };
        Shuffle(directions, random);

        foreach (var (dr, dc) in directions)
        {
            var nr = row + dr;
            var nc = col + dc;
            if (nr <= 0 || nc <= 0 || nr >= walls.GetLength(0) - 1 || nc >= walls.GetLength(1) - 1)
                continue;

            if (!walls[nr, nc]) continue;

            walls[row + dr / 2, col + dc / 2] = false;
            Carve(walls, nr, nc, random);
        }
    }

    private static void TryBreakRandomWall(bool[,] walls, Random random)
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var row = random.Next(2, walls.GetLength(0) - 2);
            var col = random.Next(2, walls.GetLength(1) - 2);
            if (!walls[row, col]) continue;

            var walkableNeighbors = 0;
            if (!walls[row - 1, col]) walkableNeighbors++;
            if (!walls[row + 1, col]) walkableNeighbors++;
            if (!walls[row, col - 1]) walkableNeighbors++;
            if (!walls[row, col + 1]) walkableNeighbors++;

            if (walkableNeighbors >= 2)
            {
                walls[row, col] = false;
                return;
            }
        }
    }

    private static MazeCell FindFarthestCell(bool[,] walls, MazeCell from)
    {
        var dist = BfsDistances(walls, from);
        var best = from;
        var bestScore = 0;
        foreach (var (cell, score) in dist)
        {
            if (score > bestScore)
            {
                bestScore = score;
                best = cell;
            }
        }

        return best;
    }

    private static Dictionary<MazeCell, int> BfsDistances(bool[,] walls, MazeCell start)
    {
        var dist = new Dictionary<MazeCell, int> { [start] = 0 };
        var queue = new Queue<MazeCell>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            var nextDist = dist[cell] + 1;
            foreach (var neighbor in Neighbors(cell))
            {
                if (walls[neighbor.Row, neighbor.Col]) continue;
                if (dist.ContainsKey(neighbor)) continue;
                dist[neighbor] = nextDist;
                queue.Enqueue(neighbor);
            }
        }

        return dist;
    }

    private static IEnumerable<MazeCell> Neighbors(MazeCell cell)
    {
        yield return new MazeCell(cell.Row - 1, cell.Col);
        yield return new MazeCell(cell.Row + 1, cell.Col);
        yield return new MazeCell(cell.Row, cell.Col - 1);
        yield return new MazeCell(cell.Row, cell.Col + 1);
    }

    private static List<string> ToLayout(bool[,] walls, MazeCell start, MazeCell exit)
    {
        var rows = walls.GetLength(0);
        var cols = walls.GetLength(1);
        var lines = new List<string>(rows);

        for (var r = 0; r < rows; r++)
        {
            var chars = new char[cols];
            for (var c = 0; c < cols; c++)
            {
                if (walls[r, c])
                {
                    chars[c] = '#';
                    continue;
                }

                if (r == start.Row && c == start.Col)
                    chars[c] = 'S';
                else if (r == exit.Row && c == exit.Col)
                    chars[c] = 'E';
                else
                    chars[c] = '.';
            }

            lines.Add(new string(chars));
        }

        return lines;
    }

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
