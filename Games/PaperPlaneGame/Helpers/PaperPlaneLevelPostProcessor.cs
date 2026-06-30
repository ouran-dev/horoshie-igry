using HoroshieIgry.Games.PaperPlaneGame.Models;

namespace HoroshieIgry.Games.PaperPlaneGame.Helpers;

/// <summary>
/// Подготавливает уровень: только облака, чистый финиш, нарастающая плотность к середине/концу.
/// </summary>
public static class PaperPlaneLevelPostProcessor
{
    private const double FinishClearRatio = 0.24;

    private static readonly HashSet<string> AllowedObstacleTypes =
        new(StringComparer.OrdinalIgnoreCase) { "cloud", "raincloud" };

    public static PaperPlaneLevelDefinition Prepare(PaperPlaneLevelDefinition level)
    {
        var clearFromX = level.FinishX * (1 - FinishClearRatio);

        var obstacles = level.Obstacles
            .Where(o => AllowedObstacleTypes.Contains(o.Type))
            .Where(o => o.X < clearFromX)
            .ToList();

        obstacles.AddRange(GenerateLateChallenges(level, clearFromX));

        var stars = level.Stars
            .Where(s => s.X < level.FinishX - 120)
            .ToList();

        return new PaperPlaneLevelDefinition
        {
            Id = level.Id,
            Title = level.Title,
            Length = level.Length,
            ScrollSpeed = level.ScrollSpeed + (level.Id - 1) * 4,
            Background = level.Background,
            PlaneStartY = level.PlaneStartY,
            FinishX = level.FinishX,
            ShowTutorial = level.ShowTutorial,
            Stars = stars,
            Obstacles = obstacles.OrderBy(o => o.X).ToList(),
            WindGusts = level.WindGusts
        };
    }

    private static IEnumerable<PaperPlaneObstacleDef> GenerateLateChallenges(
        PaperPlaneLevelDefinition level,
        double clearFromX)
    {
        var extras = new List<PaperPlaneObstacleDef>();
        var startBand = level.FinishX * 0.38;
        var endBand = clearFromX - 80;
        if (endBand <= startBand) return extras;

        var bands = 1 + Math.Min(level.Id, 4);
        var step = (endBand - startBand) / Math.Max(1, bands - 1);

        for (var i = 0; i < bands; i++)
        {
            var x = startBand + step * i;
            var density = (double)i / Math.Max(1, bands - 1);
            var topY = Lerp(70, 55, density);
            var bottomY = Lerp(290, 310, density);

            if (i % 2 == 0)
            {
                extras.Add(Cloud(x, topY));
                if (level.Id >= 2 && density > 0.35)
                    extras.Add(Cloud(x + 90, bottomY));
            }
            else
            {
                extras.Add(Cloud(x, bottomY));
                if (level.Id >= 3 && density > 0.45)
                    extras.Add(RainCloud(x + 60, Lerp(150, 120, density)));
            }

            if (level.Id >= 4 && density > 0.55 && i % 3 == 0)
                extras.Add(RainCloud(x + 40, Lerp(200, 170, density)));
        }

        return extras;
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private static PaperPlaneObstacleDef Cloud(double x, double y) => new()
    {
        Type = "cloud",
        X = x,
        Y = y,
        Width = 130,
        Height = 72
    };

    private static PaperPlaneObstacleDef RainCloud(double x, double y) => new()
    {
        Type = "raincloud",
        X = x,
        Y = y,
        Width = 125,
        Height = 86
    };
}
