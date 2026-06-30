using HoroshieIgry.Games.BalloonPopGame.Models;

namespace HoroshieIgry.Games.BalloonPopGame.Helpers;

public static class BalloonPopLevelGenerator
{
    public const double DesignWidth = 620;
    public const double DesignHeight = 420;

    private const double GapBetweenBalloons = 18;
    private const double AnimationPadding = 10;

    public static BalloonLevelPlan Create(int level, Random random)
    {
        var balloonCount = GetBalloonCount(level);
        var targetGroups = BuildTargetGroups(level, random);
        var colors = BuildColorList(balloonCount, targetGroups, random);
        var sizes = PickSizes(balloonCount, random);
        var positions = PlaceBalloons(sizes, random);

        var targetRemaining = targetGroups.ToDictionary(g => g.Color, g => g.Count);
        var balloons = new List<BalloonModel>(balloonCount);

        for (var i = 0; i < balloonCount; i++)
        {
            var color = colors[i];
            var isTarget = targetRemaining.TryGetValue(color, out var left) && left > 0;
            if (isTarget)
                targetRemaining[color]--;

            balloons.Add(new BalloonModel
            {
                Id = i,
                Color = color,
                X = positions[i].Center.X,
                Y = positions[i].Center.Y,
                Size = sizes[i],
                AnimPhase = random.NextDouble() * Math.PI * 2,
                IsTarget = isTarget
            });
        }

        return new BalloonLevelPlan
        {
            Level = level,
            TargetGroups = targetGroups,
            Balloons = balloons
        };
    }

    private static int GetBalloonCount(int level) => level switch
    {
        1 => 3,
        2 => 4,
        3 => 5,
        4 => 6,
        5 => 7,
        6 => 8,
        7 => 9,
        8 => 10,
        9 => 11,
        10 => 12,
        _ => Math.Min(12 + (level - 10), 18)
    };

    private static int GetTargetColorCount(int level) => level switch
    {
        <= 10 => 1,
        <= 18 => 2,
        _ => 3
    };

    private static int GetTargetsToPop(int level) => level switch
    {
        1 => 1,
        2 => 1,
        3 => 1,
        4 => 2,
        5 => 2,
        6 => 2,
        7 => 3,
        8 => 3,
        9 => 3,
        10 => 3,
        11 => 2,
        12 => 3,
        13 => 3,
        14 => 4,
        15 => 4,
        _ => Math.Min(4 + (level - 15) / 3, 8)
    };

    private static List<BalloonTargetGroup> BuildTargetGroups(int level, Random random)
    {
        var colorCount = GetTargetColorCount(level);
        var totalTargets = GetTargetsToPop(level);
        var colors = PickDistinctColors(colorCount, random);
        return DistributeTargets(colors, totalTargets);
    }

    private static List<BalloonColor> PickDistinctColors(int count, Random random)
    {
        var values = Enum.GetValues<BalloonColor>().OrderBy(_ => random.Next()).ToList();
        return values.Take(count).ToList();
    }

    private static List<BalloonTargetGroup> DistributeTargets(IReadOnlyList<BalloonColor> colors, int totalTargets)
    {
        var baseCount = totalTargets / colors.Count;
        var extra = totalTargets % colors.Count;
        var groups = new List<BalloonTargetGroup>(colors.Count);

        for (var i = 0; i < colors.Count; i++)
        {
            var count = baseCount + (i < extra ? 1 : 0);
            if (count > 0)
            {
                groups.Add(new BalloonTargetGroup
                {
                    Color = colors[i],
                    Count = count
                });
            }
        }

        return groups;
    }

    private static List<BalloonColor> BuildColorList(
        int count,
        IReadOnlyList<BalloonTargetGroup> targetGroups,
        Random random)
    {
        var list = new List<BalloonColor>(count);
        foreach (var group in targetGroups)
        {
            for (var i = 0; i < group.Count; i++)
                list.Add(group.Color);
        }

        var targetColors = targetGroups.Select(g => g.Color).ToHashSet();
        var others = Enum.GetValues<BalloonColor>().Where(c => !targetColors.Contains(c)).ToArray();
        while (list.Count < count)
            list.Add(others[random.Next(others.Length)]);

        Shuffle(list, random);
        return list;
    }

    private static List<double> PickSizes(int count, Random random)
    {
        var baseSize = count > 12 ? 80.0 : 90.0;
        var spread = count > 12 ? 18 : 28;

        var sizes = new List<double>(count);
        for (var i = 0; i < count; i++)
            sizes.Add(baseSize + random.Next(spread));

        return sizes;
    }

    private static List<Placement> PlaceBalloons(IReadOnlyList<double> sizes, Random random)
    {
        var placed = new List<Placement>(sizes.Count);

        foreach (var size in sizes)
        {
            if (TryPlaceRandom(size, placed, random, out var point)
                || TryPlaceGrid(size, placed, out point)
                || TryPlaceCell(size, placed.Count, sizes.Count, placed, random, out point))
            {
                placed.Add(new Placement(point, size));
            }
        }

        if (placed.Count < sizes.Count)
            return PlaceOnGrid(sizes, random);

        return placed;
    }

    private static bool TryPlaceCell(
        double size,
        int index,
        int total,
        IReadOnlyList<Placement> placed,
        Random random,
        out Point point)
    {
        var cols = (int)Math.Ceiling(Math.Sqrt(total * 1.35));
        var rows = (int)Math.Ceiling(total / (double)cols);
        var col = index % cols;
        var row = index / cols;
        var bounds = GetBounds(size);

        var cellW = DesignWidth / cols;
        var cellH = DesignHeight / rows;
        var x = cellW * col + cellW / 2 + (random.NextDouble() - 0.5) * cellW * 0.2;
        var y = cellH * row + cellH / 2 + (random.NextDouble() - 0.5) * cellH * 0.2;

        x = Math.Clamp(x, bounds.MinX, bounds.MaxX);
        y = Math.Clamp(y, bounds.MinY, bounds.MaxY);
        var candidate = new Point(x, y);

        if (!IntersectsAny(candidate, size, placed))
        {
            point = candidate;
            return true;
        }

        point = default;
        return false;
    }

    private static bool TryPlaceRandom(double size, List<Placement> placed, Random random, out Point point)
    {
        var bounds = GetBounds(size);

        for (var attempt = 0; attempt < 1200; attempt++)
        {
            var candidate = new Point(
                bounds.MinX + random.NextDouble() * (bounds.MaxX - bounds.MinX),
                bounds.MinY + random.NextDouble() * (bounds.MaxY - bounds.MinY));

            if (!IntersectsAny(candidate, size, placed))
            {
                point = candidate;
                return true;
            }
        }

        point = default;
        return false;
    }

    private static bool TryPlaceGrid(double size, List<Placement> placed, out Point point)
    {
        var bounds = GetBounds(size);
        const int stepsX = 10;
        const int stepsY = 7;

        for (var row = 0; row < stepsY; row++)
        {
            for (var col = 0; col < stepsX; col++)
            {
                var x = bounds.MinX + col / (double)(stepsX - 1) * (bounds.MaxX - bounds.MinX);
                var y = bounds.MinY + row / (double)(stepsY - 1) * (bounds.MaxY - bounds.MinY);
                var candidate = new Point(x, y);

                if (!IntersectsAny(candidate, size, placed))
                {
                    point = candidate;
                    return true;
                }
            }
        }

        point = default;
        return false;
    }

    private static List<Placement> PlaceOnGrid(IReadOnlyList<double> sizes, Random random)
    {
        var count = sizes.Count;
        var cols = (int)Math.Ceiling(Math.Sqrt(count * 1.35));
        var rows = (int)Math.Ceiling(count / (double)cols);
        var orderedSizes = sizes.OrderByDescending(s => s).ToList();
        var result = new List<Placement>(count);

        var cellW = DesignWidth / cols;
        var cellH = DesignHeight / rows;

        for (var i = 0; i < count; i++)
        {
            var size = orderedSizes[i];
            var col = i % cols;
            var row = i / cols;
            var bounds = GetBounds(size);

            var x = cellW * col + cellW / 2 + (random.NextDouble() - 0.5) * (cellW - size) * 0.35;
            var y = cellH * row + cellH / 2 + (random.NextDouble() - 0.5) * (cellH - size) * 0.35;

            x = Math.Clamp(x, bounds.MinX, bounds.MaxX);
            y = Math.Clamp(y, bounds.MinY, bounds.MaxY);
            result.Add(new Placement(new Point(x, y), size));
        }

        return result;
    }

    private static bool IntersectsAny(Point center, double size, IReadOnlyList<Placement> placed)
    {
        var radius = BodyRadius(size);
        foreach (var other in placed)
        {
            var otherRadius = BodyRadius(other.Size);
            var minDistance = radius + otherRadius + GapBetweenBalloons;
            if (Distance(center, other.Center) < minDistance)
                return true;
        }

        return false;
    }

    private static Bounds GetBounds(double size)
    {
        var halfW = size / 2;
        var halfH = size / 2;
        var pad = AnimationPadding;

        return new Bounds(
            halfW + pad,
            DesignWidth - halfW - pad,
            halfH + pad,
            DesignHeight - halfH - pad);
    }

    private static double BodyRadius(double size)
        => size / 2;

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private readonly struct Point(double x, double y)
    {
        public double X { get; } = x;
        public double Y { get; } = y;
    }

    private readonly struct Bounds(double minX, double maxX, double minY, double maxY)
    {
        public double MinX { get; } = minX;
        public double MaxX { get; } = maxX;
        public double MinY { get; } = minY;
        public double MaxY { get; } = maxY;
    }

    private readonly struct Placement(Point center, double size)
    {
        public Point Center { get; } = center;
        public double Size { get; } = size;
    }
}
