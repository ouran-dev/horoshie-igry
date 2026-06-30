namespace HoroshieIgry.Games.SortingGame.Models;

public sealed class SortLevelPlan
{
    public required int Level { get; init; }
    public required IReadOnlyList<SortBasketPlan> Baskets { get; init; }
    public required IReadOnlyList<SortItemPlan> Items { get; init; }
}
