namespace HoroshieIgry.Games.FindOddGame.Helpers;

public static class OddGameLevel
{
    public static int GetCardCount(int level)
        => level switch
        {
            <= 1 => 4,
            2 => 6,
            _ => 9
        };

    public static void GetGridSize(int cardCount, out int columns, out int rows)
    {
        switch (cardCount)
        {
            case <= 4:
                columns = 2;
                rows = 2;
                return;
            case <= 6:
                columns = 3;
                rows = 2;
                return;
            default:
                columns = 3;
                rows = 3;
                return;
        }
    }

    public static int GetTimeSeconds(int cardCount)
        => cardCount switch
        {
            <= 4 => 12,
            <= 6 => 16,
            _ => 22
        };

    public static int GetRoundLoadDelayMs(int cardCount)
        => cardCount switch
        {
            <= 4 => 350,
            <= 6 => 500,
            _ => 650
        };

    public static double GetItemMargin(int cardCount)
        => cardCount <= 4 ? 10 : cardCount <= 6 ? 8 : 6;

    public static double ComputeItemSize(int cardCount, double playWidth, double playHeight)
    {
        if (cardCount < 1 || playWidth < 1 || playHeight < 1) return 140;

        GetGridSize(cardCount, out var columns, out var rows);
        var margin = GetItemMargin(cardCount);
        var sizeFromW = Math.Floor((playWidth - columns * margin * 2) / columns);
        var sizeFromH = Math.Floor((playHeight - rows * margin * 2) / rows);
        return Math.Max(72, Math.Min(sizeFromW, sizeFromH));
    }
}
