using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoroshieIgry.Core.Mazes;

/// <summary>Загрузка лабиринтов из <c>Assets/Mazes/*.json</c>.</summary>
public static class MazeLibraryLoader
{
    private const string Root = "Assets/Mazes";
    private const int RequiredCount = 20;
    private static MazeLibrary? _cached;

    public static MazeLibrary Load()
    {
        if (_cached is not null) return _cached;

        var mazes = LoadFromDisk();
        if (mazes.Count < RequiredCount || ContainsLegacyMazes(mazes))
            mazes = MazeGenerator.CreateDefinitions().ToList();
        else
            mazes = mazes.OrderBy(m => m.Id).Take(RequiredCount).ToList();
        if (mazes.Count == 0)
            throw new InvalidOperationException($"Не найдено лабиринтов в папке {Root}.");

        _cached = new MazeLibrary(mazes);
        return _cached;
    }

    private static List<MazeDefinition> LoadFromDisk()
    {
        var mazes = new List<MazeDefinition>();
        var rootPath = Path.Combine(AppContext.BaseDirectory, Root);
        if (!Directory.Exists(rootPath))
            return mazes;

        foreach (var file in Directory.GetFiles(rootPath, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            var maze = TryLoadFile(file);
            if (maze is not null)
                mazes.Add(maze);
        }

        return mazes;
    }

    private static bool ContainsLegacyMazes(IReadOnlyList<MazeDefinition> mazes)
        => mazes.Any(m => string.Equals(m.Title, "Лабиринтчик", StringComparison.OrdinalIgnoreCase));

    private static MazeDefinition? TryLoadFile(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<MazeFileDto>(json, JsonOptions);
            if (dto?.Layout is null || dto.Layout.Count == 0) return null;

            return MazeLayoutParser.Parse(
                dto.Id,
                string.IsNullOrWhiteSpace(dto.Title) ? $"Уровень {dto.Id}" : dto.Title.Trim(),
                dto.Difficulty <= 0 ? 1 : dto.Difficulty,
                string.IsNullOrWhiteSpace(dto.ExitEmoji) ? "⭐" : dto.ExitEmoji.Trim(),
                dto.Layout);
        }
        catch
        {
            return null;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private sealed class MazeFileDto
    {
        public int Id { get; init; }
        public string? Title { get; init; }
        public int Difficulty { get; init; } = 1;
        public string? ExitEmoji { get; init; }
        public List<string>? Layout { get; init; }
    }
}
