namespace HoroshieIgry.Core.Objects;

/// <summary>Библиотека категорий и объектов для логических игр.</summary>
public sealed class ObjectLibrary
{
    private readonly Dictionary<string, ObjectCategory> _categoriesById;
    private readonly Dictionary<string, GameObjectEntry> _objectsById;

    public ObjectLibrary(IEnumerable<ObjectCategory> categories)
    {
        var list = categories.ToList();
        Categories = list;
        _categoriesById = list.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
        _objectsById = list.SelectMany(c => c.Objects)
            .GroupBy(o => o.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ObjectCategory> Categories { get; }

    public ObjectCategory? GetCategory(string categoryId)
        => _categoriesById.TryGetValue(categoryId, out var category) ? category : null;

    public GameObjectEntry? GetObject(string objectId)
        => _objectsById.TryGetValue(objectId, out var entry) ? entry : null;

    public IReadOnlyList<GameObjectEntry> GetObjects(params string[] categoryIds)
    {
        var result = new List<GameObjectEntry>();
        foreach (var categoryId in categoryIds)
        {
            if (_categoriesById.TryGetValue(categoryId, out var category))
                result.AddRange(category.Objects);
        }

        return result;
    }
}
