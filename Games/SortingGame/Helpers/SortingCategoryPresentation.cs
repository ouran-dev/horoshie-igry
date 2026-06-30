namespace HoroshieIgry.Games.SortingGame.Helpers;

/// <summary>Иконка категории для корзины — по id из Assets/Objects.</summary>
public static class SortingCategoryPresentation
{
    public static string GetIconEmoji(string categoryId) => categoryId switch
    {
        "Fruits" => "🍎",
        "Food" => "🍞",
        "Transport" => "🚗",
        "Animals" => "🐶",
        "School" => "📚",
        "Drinks" => "🥤",
        "Toys" => "🧸",
        "Nature" => "🌳",
        "Sports" => "⚽",
        "Clothes" => "👕",
        "Furniture" => "🪑",
        "MusicalInstruments" => "🎸",
        "Colors" => "🎨",
        _ => "📦"
    };
}
