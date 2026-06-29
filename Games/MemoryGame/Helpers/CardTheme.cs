namespace HoroshieIgry.Games.MemoryGame.Helpers;

/// <summary>Пастельные цвета лицевой стороны карточек.</summary>
public static class CardTheme
{
    private static readonly Dictionary<string, string> Colors = new()
    {
        ["🐶"] = "#FFE8CC", ["🐱"] = "#FFE0F0", ["🐭"] = "#F0E8FF", ["🐰"] = "#FFF0E0",
        ["🦊"] = "#FFF0C2", ["🐻"] = "#E8DCC8", ["🐼"] = "#E5F5D8", ["🐸"] = "#DDF0E6",
        ["🐯"] = "#FFE8B8", ["🦁"] = "#FFECC8", ["🐮"] = "#E8F4FF", ["🐷"] = "#FFE4EC",
        ["🐔"] = "#FFF5D8", ["🐧"] = "#E0F0FF", ["🐦"] = "#E8F8FF", ["🐢"] = "#D8F0E0",
        ["🐠"] = "#D8EEFF", ["🦋"] = "#F0E0FF", ["🐝"] = "#FFF8D0", ["🌸"] = "#FFE8F0",
        ["🌻"] = "#FFF5C8", ["🍎"] = "#FFE0E0", ["🍌"] = "#FFF8D0", ["🚗"] = "#E0ECFF",
        ["✈️"] = "#E0F4FF", ["🎈"] = "#FFE0E8", ["⚽"] = "#E8F8E8", ["🎸"] = "#FFE8E0",
        ["🌈"] = "#F0E8FF", ["⭐"] = "#FFF8D8", ["🍓"] = "#FFE8EC", ["🎁"] = "#FFE8F0"
    };

    public static string GetFrontColor(string symbol)
        => Colors.TryGetValue(symbol, out var color) ? color : "#FFF8E7";
}
