namespace HoroshieIgry.Games.LinkDotsGame.Models;

public sealed class LinkDotsLevel
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required int Rows { get; init; }
    public required int Cols { get; init; }
    public required IReadOnlyList<LinkDotsPair> Pairs { get; init; }
}
