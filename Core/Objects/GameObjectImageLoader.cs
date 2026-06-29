using System.IO;
using System.Windows.Media;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace HoroshieIgry.Core.Objects;

/// <summary>Загрузка картинок объектов с кэшем.</summary>
public static class GameObjectImageLoader
{
    private static readonly Dictionary<string, ImageSource?> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static ImageSource? Load(GameObjectEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.ImageRelativePath))
            return null;

        if (Cache.TryGetValue(entry.ImageRelativePath, out var cached))
            return cached;

        var fullPath = Path.Combine(AppContext.BaseDirectory, entry.ImageRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            Cache[entry.ImageRelativePath] = null;
            return null;
        }

        try
        {
            ImageSource? image = null;
            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            if (ext is ".png" or ".jpg" or ".jpeg" or ".webp")
            {
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                image = bitmap;
            }
            else if (ext == ".svg")
            {
                var settings = new WpfDrawingSettings { IncludeRuntime = true, TextAsGeometry = false };
                var reader = new FileSvgReader(settings);
                var drawing = reader.Read(fullPath);
                if (drawing is not null)
                {
                    var drawingImage = new DrawingImage(drawing);
                    drawingImage.Freeze();
                    image = drawingImage;
                }
            }

            Cache[entry.ImageRelativePath] = image;
            return image;
        }
        catch
        {
            Cache[entry.ImageRelativePath] = null;
            return null;
        }
    }
}
