namespace HoroshieIgry.Core.Objects;

/// <summary>Один предмет в библиотеке объектов.</summary>
public sealed class GameObjectEntry
{
    public required string Id { get; init; }
    public required string CategoryId { get; init; }
    public required string Label { get; init; }
    public string Emoji { get; init; } = string.Empty;
    public string? ImageRelativePath { get; init; }
}
