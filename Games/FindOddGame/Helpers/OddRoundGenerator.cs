using HoroshieIgry.Core.Objects;
using HoroshieIgry.Games.FindOddGame.Models;

namespace HoroshieIgry.Games.FindOddGame.Helpers;

/// <summary>Генерация заданий «Найди лишнее» из библиотеки объектов.</summary>
public static class OddRoundGenerator
{
    private static readonly (string Main, string Odd)[] SimilarPairs =
    [
        ("Fruits", "Food"),
        ("Food", "Fruits"),
        ("Animals", "Nature"),
        ("Transport", "Toys")
    ];

    public static OddRoundPlan Generate(int level, ObjectLibrary library, Random random)
    {
        var cardCount = OddGameLevel.GetCardCount(level);
        var normalCount = cardCount - 1;
        var (normals, odd, hint) = BuildItems(level, library, normalCount, random);

        var items = normals.Select(o => ToPlan(o, false)).ToList();
        items.Add(ToPlan(odd, true));
        Shuffle(items, random);

        return new OddRoundPlan
        {
            Hint = hint,
            Items = items,
            TimeSeconds = OddGameLevel.GetTimeSeconds(cardCount)
        };
    }

    private static (List<GameObjectEntry> Normals, GameObjectEntry Odd, string Hint) BuildItems(
        int level, ObjectLibrary library, int normalCount, Random random)
    {
        var categories = library.Categories;
        if (categories.Count < 2)
            throw new InvalidOperationException("В библиотеке объектов нужно минимум две категории.");

        if (level >= 5)
            return BuildHintedRound(library, normalCount, random);

        if (level >= 4)
        {
            var pair = SimilarPairs[random.Next(SimilarPairs.Length)];
            var main = library.GetCategory(pair.Main) ?? categories[random.Next(categories.Count)];
            var oddCategory = library.GetCategory(pair.Odd) ?? categories.First(c => c.Id != main.Id);
            return (PickDistinct(main.Objects, normalCount, random), oddCategory.Objects[random.Next(oddCategory.Objects.Count)], "Найди лишнее");
        }

        var primary = categories[random.Next(categories.Count)];
        var other = categories.Where(c => c.Id != primary.Id).OrderBy(_ => random.Next()).First();
        return (PickDistinct(primary.Objects, normalCount, random), other.Objects[random.Next(other.Objects.Count)], "Найди лишнее");
    }

    private static (List<GameObjectEntry> Normals, GameObjectEntry Odd, string Hint) BuildHintedRound(
        ObjectLibrary library, int normalCount, Random random)
    {
        var transport = library.GetCategory("Transport");
        if (transport is not null)
        {
            var air = transport.Objects.Where(o => o.Id is "plane" or "helicopter").ToList();
            var groundWater = transport.Objects.Where(o => o.Id is "car" or "train" or "boat" or "bus").ToList();
            if (air.Count > 0 && groundWater.Count >= Math.Min(normalCount, 1))
            {
                return (
                    PickDistinct(groundWater, normalCount, random),
                    air[random.Next(air.Count)],
                    "Найди воздушный транспорт");
            }
        }

        var categories = library.Categories;
        var main = categories[random.Next(categories.Count)];
        var oddCategory = categories.First(c => c.Id != main.Id);
        return (
            PickDistinct(main.Objects, normalCount, random),
            oddCategory.Objects[random.Next(oddCategory.Objects.Count)],
            "Что здесь лишнее?");
    }

    private static List<GameObjectEntry> PickDistinct(
        IReadOnlyList<GameObjectEntry> pool, int count, Random random)
    {
        if (count <= 0) return [];
        var result = pool.OrderBy(_ => random.Next()).Take(Math.Min(count, pool.Count)).ToList();
        while (result.Count < count && pool.Count > 0)
            result.Add(pool[random.Next(pool.Count)]);
        return result;
    }

    private static OddRoundItemPlan ToPlan(GameObjectEntry entry, bool isOdd)
        => new()
        {
            ObjectId = entry.Id,
            CategoryId = entry.CategoryId,
            Label = entry.Label,
            Emoji = entry.Emoji,
            ImageRelativePath = entry.ImageRelativePath,
            IsOdd = isOdd
        };

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
