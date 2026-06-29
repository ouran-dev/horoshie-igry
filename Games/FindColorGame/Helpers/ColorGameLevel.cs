namespace HoroshieIgry.Games.FindColorGame.Helpers;

/// <summary>Параметры уровня: число фигур, время, сетка.</summary>
public static class ColorGameLevel
{
    public const int InitialItemCount = 5;
    public const int ItemsPerLevelStep = 3;
    public const int MaxItemCount = 5000;
    public const int MinTimeSeconds = 3;
    public const int MaxTimeSeconds = 120;

    public static int GetItemCount(int level)
        => Math.Min(InitialItemCount + (level - 1) * ItemsPerLevelStep, MaxItemCount);

    /// <summary>Время растёт с числом фигур (макс. 120 с).</summary>
    public static int GetTimeSeconds(int itemCount)
    {
        if (itemCount <= 4) return MinTimeSeconds;
        return Math.Min(MaxTimeSeconds, MinTimeSeconds + (itemCount - 4) / 2);
    }

    /// <summary>Пауза после генерации поля — UI успевает отрисовать фигуры.</summary>
    public static int GetRoundLoadDelayMs(int itemCount)
        => itemCount switch
        {
            <= 15 => 400,
            <= 50 => 650,
            <= 150 => 950,
            <= 500 => 1400,
            <= 1500 => 1900,
            _ => 2400
        };

    public static int GetPaletteCount(int level)
        => Math.Min(3 + (level - 1) / 2, ColorGameAssets.PlayPalettes.Length);

    public static void GetGridSize(int itemCount, double playAspectRatio, out int columns, out int rows)
    {
        if (itemCount <= 0)
        {
            columns = 1;
            rows = 1;
            return;
        }

        if (itemCount == 1)
        {
            columns = 1;
            rows = 1;
            return;
        }

        var aspect = Math.Max(0.5, playAspectRatio);
        var bestColumns = 1;
        var bestRows = itemCount;
        var bestCellScore = 0.0;

        var idealRows = Math.Max(1, (int)Math.Round(Math.Sqrt(itemCount / aspect)));
        var minRows = itemCount <= 120 ? 1 : Math.Max(1, idealRows - Math.Max(8, idealRows / 4));
        var maxRows = itemCount <= 120 ? itemCount : Math.Min(itemCount, idealRows + Math.Max(8, idealRows / 4));

        for (var candidateRows = minRows; candidateRows <= maxRows; candidateRows++)
        {
            var candidateColumns = (int)Math.Ceiling(itemCount / (double)candidateRows);
            var cellWidth = aspect / candidateColumns;
            var cellHeight = 1.0 / candidateRows;
            var cellScore = Math.Min(cellWidth, cellHeight);

            if (cellScore > bestCellScore + 0.0001)
            {
                bestCellScore = cellScore;
                bestColumns = candidateColumns;
                bestRows = candidateRows;
            }
        }

        columns = bestColumns;
        rows = bestRows;
    }

    public static double GetItemMargin(int itemCount)
        => itemCount switch
        {
            <= 8 => 6,
            <= 30 => 4,
            <= 100 => 3,
            <= 500 => 2,
            _ => 1
        };

    public static double GetMinItemSize(int itemCount)
        => itemCount switch
        {
            <= 50 => 28,
            <= 200 => 22,
            <= 800 => 16,
            <= 2000 => 12,
            _ => 8
        };

    public static double ComputeItemSize(int itemCount, int columns, int rows, double playWidth, double playHeight)
    {
        if (columns < 1 || rows < 1 || playWidth < 1 || playHeight < 1)
            return 80;

        var margin = GetItemMargin(itemCount);
        var minSize = GetMinItemSize(itemCount);

        var sizeFromW = Math.Floor((playWidth - columns * margin * 2) / columns);
        var sizeFromH = Math.Floor((playHeight - rows * margin * 2) / rows);
        var size = Math.Min(sizeFromW, sizeFromH);

        while (size >= minSize)
        {
            if (columns * (size + margin * 2) <= playWidth + 0.5
                && rows * (size + margin * 2) <= playHeight + 0.5)
                return Math.Max(minSize, size);

            size -= 1;
        }

        return minSize;
    }
}
