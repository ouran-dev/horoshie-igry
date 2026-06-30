namespace HoroshieIgry.Games.SortingGame.Models;

public sealed class SortBasketPlan
{
    public required string CategoryId { get; init; }
    public required string Title { get; init; }
    public required string IconEmoji { get; init; }
    public required int SlotIndex { get; init; }
}
