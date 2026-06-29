using System.IO;
using System.Windows;
using System.Windows.Media;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace HoroshieIgry.Core.UI;

/// <summary>Загрузка фоновых сцен Kenney с кэшированием.</summary>
public static class KenneyBackgroundLoader
{
  private static readonly Dictionary<string, ImageSource> Cache = new(StringComparer.OrdinalIgnoreCase);

  public static ImageSource? LoadScene(KenneyBackgroundScene scene)
  {
    var key = $"{scene.AssetPath}|{scene.ClipX}|{scene.ClipY}|{scene.ClipWidth}|{scene.ClipHeight}";
    if (Cache.TryGetValue(key, out var cached))
    {
      return cached;
    }

    var fullPath = Path.Combine(AppContext.BaseDirectory, scene.AssetPath.Replace('/', Path.DirectorySeparatorChar));
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

      var clip = new RectangleGeometry(new Rect(scene.ClipX, scene.ClipY, scene.ClipWidth, scene.ClipHeight));
      clip.Freeze();

      var group = new DrawingGroup { ClipGeometry = clip };
      group.Children.Add(drawing);

      var image = new DrawingImage(group);
      image.Freeze();
      Cache[key] = image;
      return image;
    }
    catch
    {
      return null;
    }
  }

  public static ImageSource? LoadTheme(BackgroundTheme theme)
    => LoadScene(KenneyBackgroundPaths.GetScene(theme));
}
