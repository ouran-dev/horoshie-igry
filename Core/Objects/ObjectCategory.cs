namespace HoroshieIgry.Core.Objects;

/// <summary>Категория игровых объектов из <c>Assets/Objects/</c>.</summary>
public sealed class ObjectCategory
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required IReadOnlyList<GameObjectEntry> Objects { get; init; }
}
