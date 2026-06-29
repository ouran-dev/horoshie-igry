using System.Windows;
using System.Windows.Controls;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Controls;

/// <summary>Фоновый слой из библиотеки Kenney Background Elements.</summary>
public partial class GameBackground : UserControl
{
  public static readonly DependencyProperty ThemeProperty =
    DependencyProperty.Register(nameof(Theme), typeof(BackgroundTheme), typeof(GameBackground),
      new PropertyMetadata(BackgroundTheme.Catalog, OnThemeChanged));

  public BackgroundTheme Theme
  {
    get => (BackgroundTheme)GetValue(ThemeProperty);
    set => SetValue(ThemeProperty, value);
  }

  public GameBackground()
  {
    InitializeComponent();
    Loaded += (_, _) => ApplyTheme();
  }

  private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is GameBackground background && background.IsLoaded)
    {
      background.ApplyTheme();
    }
  }

  private void ApplyTheme()
  {
    BackgroundImage.Source = KenneyBackgroundLoader.LoadTheme(Theme);
  }
}
