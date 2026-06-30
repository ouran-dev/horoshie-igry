namespace HoroshieIgry.Games.SortingGame.Models;

public sealed class SortItemPlan
{
    public required int Id { get; init; }
    public required string ObjectId { get; init; }
    public required string CategoryId { get; init; }
    public required string Label { get; init; }
    public required string Emoji { get; init; }
    public string? ImageRelativePath { get; init; }
    public double HomeX { get; set; }
    public double HomeY { get; set; }
}
