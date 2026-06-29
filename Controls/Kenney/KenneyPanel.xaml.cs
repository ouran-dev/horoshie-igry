using System.Windows;
using System.Windows.Controls;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Controls.Kenney;

/// <summary>Панель на основе ассетов Kenney (input_rectangle, input_square).</summary>
public partial class KenneyPanel : ContentControl
{
    public static readonly DependencyProperty AssetPathProperty =
        DependencyProperty.Register(nameof(AssetPath), typeof(string), typeof(KenneyPanel),
            new PropertyMetadata(KenneyPaths.PanelRectangle, OnAssetPathChanged));

    public static readonly DependencyProperty PanelPaddingProperty =
        DependencyProperty.Register(nameof(PanelPadding), typeof(Thickness), typeof(KenneyPanel),
            new PropertyMetadata(new Thickness(16)));

    private Image? _panelImage;

    public string AssetPath
    {
        get => (string)GetValue(AssetPathProperty);
        set => SetValue(AssetPathProperty, value);
    }

    public Thickness PanelPadding
    {
        get => (Thickness)GetValue(PanelPaddingProperty);
        set => SetValue(PanelPaddingProperty, value);
    }

    public KenneyPanel()
    {
        InitializeComponent();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _panelImage = GetTemplateChild("PanelImage") as Image;
        ApplyAsset();
    }

    private static void OnAssetPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KenneyPanel panel)
        {
            panel.ApplyAsset();
        }
    }

    private void ApplyAsset()
    {
        if (_panelImage is null)
        {
            return;
        }

        KenneySvg.ApplyTo(_panelImage, AssetPath);
    }
}
