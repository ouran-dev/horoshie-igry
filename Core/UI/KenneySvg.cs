using System.IO;
using System.Windows.Media;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace HoroshieIgry.Core.UI;

/// <summary>Загрузка SVG-ассетов Kenney для WPF с кэшированием.</summary>
public static class KenneySvg
{
    private static readonly Dictionary<string, ImageSource> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static ImageSource? Load(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        if (Cache.TryGetValue(relativePath, out var cached))
        {
            return cached;
        }

        var fullPath = Path.Combine(AppContext.BaseDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            return null;
        }

        try
        {
            var settings = new WpfDrawingSettings
            {
                IncludeRuntime = true,
                TextAsGeometry = false
            };

            var reader = new FileSvgReader(settings);
            var drawing = reader.Read(fullPath);
            if (drawing is null)
            {
                return null;
            }

            var image = new DrawingImage(drawing);
            image.Freeze();
            Cache[relativePath] = image;
            return image;
        }
        catch
        {
            return null;
        }
    }

    public static void ApplyTo(System.Windows.Controls.Image image, string relativePath)
    {
        image.Source = Load(relativePath);
    }
}
