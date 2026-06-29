using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoroshieIgry.Core.Objects;

/// <summary>Загрузка библиотеки из папок <c>Assets/Objects/{Category}/category.json</c>.</summary>
public static class ObjectLibraryLoader
{
    private const string Root = "Assets/Objects";
    private static ObjectLibrary? _cached;

    public static ObjectLibrary Load()
    {
        if (_cached is not null) return _cached;

        var categories = new List<ObjectCategory>();
        var rootPath = Path.Combine(AppContext.BaseDirectory, Root);
        if (Directory.Exists(rootPath))
        {
            foreach (var categoryDir in Directory.GetDirectories(rootPath))
            {
                var category = TryLoadCategory(categoryDir);
                if (category is not null)
                    categories.Add(category);
            }
        }

        _cached = new ObjectLibrary(categories);
        return _cached;
    }

    private static ObjectCategory? TryLoadCategory(string categoryDir)
    {
        var manifestPath = Path.Combine(categoryDir, "category.json");
        if (!File.Exists(manifestPath)) return null;

        try
        {
            var json = File.ReadAllText(manifestPath);
            var dto = JsonSerializer.Deserialize<CategoryDto>(json, JsonOptions);
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id)) return null;

            var categoryId = dto.Id.Trim();
            var objects = new List<GameObjectEntry>();

            foreach (var item in dto.Items ?? [])
            {
                if (string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Label))
                    continue;

                string? imagePath = null;
                if (!string.IsNullOrWhiteSpace(item.File))
                {
                    var candidate = Path.Combine(categoryDir, item.File);
                    if (File.Exists(candidate))
                        imagePath = $"{Root}/{Path.GetFileName(categoryDir)}/{item.File}".Replace('\\', '/');
                }

                objects.Add(new GameObjectEntry
                {
                    Id = item.Id.Trim(),
                    CategoryId = categoryId,
                    Label = item.Label.Trim(),
                    Emoji = item.Emoji ?? string.Empty,
                    ImageRelativePath = imagePath
                });
            }

            if (objects.Count == 0) return null;

            return new ObjectCategory
            {
                Id = categoryId,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? categoryId : dto.Title.Trim(),
                Objects = objects
            };
        }
        catch
        {
            return null;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private sealed class CategoryDto
    {
        public string? Id { get; init; }
        public string? Title { get; init; }
        public List<ItemDto>? Items { get; init; }
    }

    private sealed class ItemDto
    {
        public string? Id { get; init; }
        public string? Label { get; init; }
        public string? Emoji { get; init; }
        public string? File { get; init; }
    }
}
