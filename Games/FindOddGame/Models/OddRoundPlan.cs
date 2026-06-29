namespace HoroshieIgry.Games.FindOddGame.Models;

/// <summary>Результат генерации уровня (логика отдельно от UI).</summary>
public sealed class OddRoundPlan
{
    public required string Hint { get; init; }
    public required IReadOnlyList<OddRoundItemPlan> Items { get; init; }
    public int TimeSeconds { get; init; }
}

public sealed class OddRoundItemPlan
{
    public required string ObjectId { get; init; }
    public required string CategoryId { get; init; }
    public required string Label { get; init; }
    public string Emoji { get; init; } = string.Empty;
    public string? ImageRelativePath { get; init; }
    public bool IsOdd { get; init; }
}
