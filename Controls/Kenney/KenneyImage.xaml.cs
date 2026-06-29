using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Controls.Kenney;

/// <summary>Отображение SVG-ассета Kenney.</summary>
public partial class KenneyImage : UserControl
{
    public static readonly DependencyProperty AssetPathProperty =
        DependencyProperty.Register(nameof(AssetPath), typeof(string), typeof(KenneyImage),
            new PropertyMetadata(string.Empty, OnAssetPathChanged));

    public static readonly DependencyProperty StretchProperty =
        DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(KenneyImage),
            new PropertyMetadata(Stretch.Uniform));

    public string AssetPath
    {
        get => (string)GetValue(AssetPathProperty);
        set => SetValue(AssetPathProperty, value);
    }

    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public KenneyImage()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyAsset();
    }

    private static void OnAssetPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KenneyImage image && image.IsLoaded)
        {
            image.ApplyAsset();
        }
    }

    private void ApplyAsset()
    {
        KenneySvg.ApplyTo(AssetImage, AssetPath);
    }
}
