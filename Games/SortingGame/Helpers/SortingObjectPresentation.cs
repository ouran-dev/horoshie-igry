using HoroshieIgry.Core.Objects;

namespace HoroshieIgry.Games.SortingGame.Helpers;

/// <summary>
/// Понятные пиктограммы для сортировки: некоторые emoji из библиотеки
/// выглядят как предметы из другой категории (🧱 «стена», 💧 «капля»).
/// </summary>
public static class SortingObjectPresentation
{
    public static string GetDisplayEmoji(GameObjectEntry entry) => (entry.CategoryId, entry.Id) switch
    {
        ("Drinks", "water") => "🚰",
        _ => entry.Emoji
    };
}
