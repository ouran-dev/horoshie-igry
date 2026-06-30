using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using HoroshieIgry.Games.PaperPlaneGame.Models;

namespace HoroshieIgry.Games.PaperPlaneGame.Helpers;

/// <summary>Загрузка уровней из <c>Games/PaperPlaneGame/Levels/*.json</c>.</summary>
public static class PaperPlaneLevelLoader
{
    private const string LevelsRoot = "Games/PaperPlaneGame/Levels";
    private static IReadOnlyList<PaperPlaneLevelDefinition>? _cached;

    public static int MaxLevel { get; private set; }

    public static PaperPlaneLevelDefinition Load(int level)
    {
        EnsureCache();
        var index = (Math.Max(1, level) - 1) % MaxLevel;
        return _cached![index];
    }

    public static void Warmup() => EnsureCache();

    private static void EnsureCache()
    {
        if (_cached is not null) return;

        var levels = new List<PaperPlaneLevelDefinition>();
        var rootPath = Path.Combine(AppContext.BaseDirectory, LevelsRoot);
        if (Directory.Exists(rootPath))
        {
            foreach (var file in Directory.GetFiles(rootPath, "level_*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var level = TryLoadFile(file);
                if (level is not null)
                    levels.Add(level);
            }
        }

        if (levels.Count == 0)
            levels.Add(CreateFallbackLevel());

        _cached = levels;
        MaxLevel = Math.Max(1, levels.Count);
    }

    private static PaperPlaneLevelDefinition? TryLoadFile(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<LevelDto>(json, JsonOptions);
            if (dto is null) return null;

            return new PaperPlaneLevelDefinition
            {
                Id = dto.Id,
                Title = dto.Title ?? $"Уровень {dto.Id}",
                Length = dto.Length,
                ScrollSpeed = dto.ScrollSpeed,
                Background = dto.Background ?? "clouds",
                PlaneStartY = dto.PlaneStart?.Y ?? 210,
                FinishX = dto.Finish?.X ?? dto.Length,
                ShowTutorial = dto.ShowTutorial,
                Stars = (dto.Stars ?? []).Select(s => new PaperPlaneStarDef { X = s.X, Y = s.Y }).ToList(),
                Obstacles = (dto.Obstacles ?? []).Select(MapObstacle).ToList(),
                WindGusts = (dto.WindGusts ?? []).Select(w => new PaperPlaneWindGustDef
                {
                    X = w.X,
                    Y = w.Y,
                    Width = w.Width,
                    Height = w.Height,
                    Direction = w.Direction ?? "up",
                    Strength = w.Strength
                }).ToList()
            };
        }
        catch
        {
            return null;
        }
    }

    private static PaperPlaneObstacleDef MapObstacle(ObstacleDto dto) => new()
    {
        Type = dto.Type ?? "cloud",
        X = dto.X,
        Y = dto.Y,
        Width = dto.Width,
        Height = dto.Height,
        Solid = dto.Solid ?? true,
        Motion = dto.Motion is null
            ? null
            : new PaperPlaneMotionDef
            {
                Axis = dto.Motion.Axis ?? "y",
                Amplitude = dto.Motion.Amplitude,
                PeriodSec = dto.Motion.PeriodSec
            }
    };

    private static PaperPlaneLevelDefinition CreateFallbackLevel() => new()
    {
        Id = 1,
        Title = "Первый полёт",
        Length = 3000,
        ScrollSpeed = 70,
        PlaneStartY = 210,
        FinishX = 3000,
        ShowTutorial = true,
        Stars =
        [
            new PaperPlaneStarDef { X = 450, Y = 180 },
            new PaperPlaneStarDef { X = 900, Y = 250 },
            new PaperPlaneStarDef { X = 1400, Y = 160 },
            new PaperPlaneStarDef { X = 1900, Y = 220 },
            new PaperPlaneStarDef { X = 2400, Y = 190 }
        ],
        Obstacles =
        [
            new PaperPlaneObstacleDef { Type = "cloud", X = 700, Y = 120, Width = 120, Height = 70 },
            new PaperPlaneObstacleDef { Type = "cloud", X = 1600, Y = 280, Width = 130, Height = 75 }
        ]
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private sealed class LevelDto
    {
        public int Id { get; init; }
        public string? Title { get; init; }
        public double Length { get; init; }
        public double ScrollSpeed { get; init; }
        public string? Background { get; init; }
        public bool ShowTutorial { get; init; }
        public PointDto? PlaneStart { get; init; }
        public FinishDto? Finish { get; init; }
        public List<StarDto>? Stars { get; init; }
        public List<ObstacleDto>? Obstacles { get; init; }
        public List<WindDto>? WindGusts { get; init; }
    }

    private sealed class PointDto
    {
        public double X { get; init; }
        public double Y { get; init; }
    }

    private sealed class FinishDto
    {
        public double X { get; init; }
    }

    private sealed class StarDto
    {
        public double X { get; init; }
        public double Y { get; init; }
    }

    private sealed class ObstacleDto
    {
        public string? Type { get; init; }
        public double X { get; init; }
        public double Y { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public bool? Solid { get; init; }
        public MotionDto? Motion { get; init; }
    }

    private sealed class MotionDto
    {
        public string? Axis { get; init; }
        public double Amplitude { get; init; }
        public double PeriodSec { get; init; } = 2.5;
    }

    private sealed class WindDto
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public string? Direction { get; init; }
        public double Strength { get; init; } = 100;
    }
}
