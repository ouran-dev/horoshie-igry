using HoroshieIgry.Core.Objects;
using HoroshieIgry.Games.SortingGame.Models;

namespace HoroshieIgry.Games.SortingGame.Helpers;

public static class SortingLevelGenerator
{
    public const double DesignWidth = 700;
    public const double DesignHeight = 500;
    public const double BasketRowHeight = 140;
    public const double ItemSize = 92;
    private const double ItemGap = 16;
    private const double CanvasPad = 20;

  // Высота нижней строки поля (совпадает с ItemsCanvas / DragCanvas).
    public static double ItemsCanvasHeight => DesignHeight - BasketRowHeight - 14;

    private static readonly (int Level, string[] Categories)[] ScriptedStarts =
    [
        (1, ["Fruits", "Transport"]),
        (2, ["Fruits", "Animals", "Transport"])
    ];

    public static SortLevelPlan Create(int level, ObjectLibrary library, Random random)
    {
        var basketCount = GetBasketCount(level);
        var itemsPerBasket = GetItemsPerBasket(level);
        var categories = PickCategories(level, library, basketCount, itemsPerBasket, random);

        var baskets = categories
            .Select((category, index) => new SortBasketPlan
            {
                CategoryId = category.Id,
                Title = category.Title,
                IconEmoji = SortingCategoryPresentation.GetIconEmoji(category.Id),
                SlotIndex = index
            })
            .ToList();

        var items = new List<SortItemPlan>();
        var id = 0;

        foreach (var category in categories)
        {
            var picks = PickObjects(category.Objects, itemsPerBasket, random);
            foreach (var entry in picks)
            {
                if (!string.Equals(entry.CategoryId, category.Id, StringComparison.Ordinal))
                    continue;

                items.Add(new SortItemPlan
                {
                    Id = id++,
                    ObjectId = entry.Id,
                    CategoryId = category.Id,
                    Label = entry.Label,
                    Emoji = SortingObjectPresentation.GetDisplayEmoji(entry),
                    ImageRelativePath = entry.ImageRelativePath
                });
            }
        }

        PlaceItems(items, random);

        return new SortLevelPlan
        {
            Level = level,
            Baskets = baskets,
            Items = items
        };
    }

    private static int GetBasketCount(int level) => level switch
    {
        <= 2 => 2,
        <= 5 => 3,
        <= 12 => 4,
        _ => Math.Min(5 + (level - 13) / 4, 6)
    };

    private static int GetItemsPerBasket(int level) => level switch
    {
        1 => 2,
        2 => 3,
        3 => 3,
        _ => Math.Min(3 + level / 3, 5)
    };

    private static List<ObjectCategory> PickCategories(
        int level,
        ObjectLibrary library,
        int basketCount,
        int itemsPerBasket,
        Random random)
    {
        var scripted = ScriptedStarts.FirstOrDefault(s => s.Level == level);
        if (scripted.Categories is { Length: > 0 })
        {
            var picked = scripted.Categories
                .Select(id => library.GetCategory(id))
                .Where(c => c is not null && c.Objects.Count >= itemsPerBasket)
                .Cast<ObjectCategory>()
                .ToList();

            if (picked.Count == basketCount)
                return picked;
        }

        return library.Categories
            .Where(c => c.Id != "Colors" && c.Objects.Count >= itemsPerBasket)
            .OrderBy(_ => random.Next())
            .Take(basketCount)
            .ToList();
    }

    private static List<GameObjectEntry> PickObjects(
        IReadOnlyList<GameObjectEntry> pool,
        int count,
        Random random)
    {
        return pool.OrderBy(_ => random.Next()).Take(Math.Min(count, pool.Count)).ToList();
    }

    private static void PlaceItems(List<SortItemPlan> items, Random random)
    {
        var half = ItemSize / 2;
        var bounds = new Bounds(
            CanvasPad + half,
            DesignWidth - CanvasPad - half,
            CanvasPad + half,
            ItemsCanvasHeight - CanvasPad - half);

        var placed = new List<Point>();

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (TryPlaceRandom(bounds, placed, random, out var point)
                || TryPlaceGrid(bounds, placed, i, items.Count, out point))
            {
                placed.Add(point);
                item.HomeX = point.X - ItemSize / 2;
                item.HomeY = point.Y - ItemSize / 2;
            }
        }
    }

    private static bool TryPlaceRandom(Bounds bounds, List<Point> placed, Random random, out Point point)
    {
        var radius = ItemSize / 2 + ItemGap / 2;

        for (var attempt = 0; attempt < 500; attempt++)
        {
            var candidate = new Point(
                bounds.MinX + random.NextDouble() * (bounds.MaxX - bounds.MinX),
                bounds.MinY + random.NextDouble() * (bounds.MaxY - bounds.MinY));

            if (!Intersects(candidate, radius, placed))
            {
                point = candidate;
                return true;
            }
        }

        point = default;
        return false;
    }

    private static bool TryPlaceGrid(Bounds bounds, List<Point> placed, int index, int total, out Point point)
    {
        var cols = (int)Math.Ceiling(Math.Sqrt(total * 1.2));
        var rows = (int)Math.Ceiling(total / (double)cols);
        var col = index % cols;
        var row = index / cols;
        var radius = ItemSize / 2 + ItemGap / 2;

        var cellW = (bounds.MaxX - bounds.MinX) / Math.Max(1, cols - 1);
        var cellH = (bounds.MaxY - bounds.MinY) / Math.Max(1, rows - 1);

        var x = cols == 1 ? (bounds.MinX + bounds.MaxX) / 2 : bounds.MinX + col * cellW;
        var y = rows == 1 ? (bounds.MinY + bounds.MaxY) / 2 : bounds.MinY + row * cellH;
        var candidate = new Point(x, y);

        if (!Intersects(candidate, radius, placed))
        {
            point = candidate;
            return true;
        }

        point = candidate;
        return true;
    }

    private static bool Intersects(Point center, double radius, List<Point> placed)
    {
        foreach (var other in placed)
        {
            var dx = center.X - other.X;
            var dy = center.Y - other.Y;
            if (Math.Sqrt(dx * dx + dy * dy) < radius * 2)
                return true;
        }

        return false;
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
}
