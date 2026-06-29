namespace HoroshieIgry.Core.UI;

/// <summary>
/// Сцены из спрайтшита <c>vector_backgrounds.svg</c> (Kenney Background Elements).
/// Каждая ячейка — 512×512. Перед добавлением новой темы проверяйте этот файл.
/// </summary>
public static class KenneyBackgroundPaths
{
  private const string Root = "Assets/Background Elements/Kenney";
  private const double CellSize = 512;

  public const string VectorBackgrounds = $"{Root}/Vector/vector_backgrounds.svg";

  public static KenneyBackgroundScene GetScene(BackgroundTheme theme) => theme switch
  {
    BackgroundTheme.Catalog => Scene(40, 520),
    BackgroundTheme.Meadow => Scene(40, 1042),
    BackgroundTheme.Sea => Scene(562, 1042),
    BackgroundTheme.Desert => Scene(1084, 1042),
    BackgroundTheme.Forest => Scene(1084, 520),
    BackgroundTheme.Mountains => Scene(40, 0),
    BackgroundTheme.Clouds => Scene(1084, 0),
    BackgroundTheme.Space => Scene(1606, 520),
    _ => Scene(40, 520)
  };

  private static KenneyBackgroundScene Scene(double x, double y)
    => new(VectorBackgrounds, x, y, CellSize, CellSize);
}

/// <summary>Фрагмент спрайтшита Kenney для отображения как фон.</summary>
public readonly record struct KenneyBackgroundScene(
  string AssetPath,
  double ClipX,
  double ClipY,
  double ClipWidth,
  double ClipHeight);
