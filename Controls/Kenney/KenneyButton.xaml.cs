using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Controls.Kenney;

/// <summary>Кнопка на основе ассетов Kenney UI Pack.</summary>
public partial class KenneyButton : UserControl
{
    public static readonly DependencyProperty AssetPathProperty =
        DependencyProperty.Register(nameof(AssetPath), typeof(string), typeof(KenneyButton),
            new PropertyMetadata(string.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(KenneyButton),
            new PropertyMetadata(string.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty IconPathProperty =
        DependencyProperty.Register(nameof(IconPath), typeof(string), typeof(KenneyButton),
            new PropertyMetadata(string.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty ContentPaddingProperty =
        DependencyProperty.Register(nameof(ContentPadding), typeof(Thickness), typeof(KenneyButton),
            new PropertyMetadata(new Thickness(20, 8, 20, 8)));

    public event EventHandler<RoutedEventArgs>? Click;

    public string AssetPath
    {
        get => (string)GetValue(AssetPathProperty);
        set => SetValue(AssetPathProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string IconPath
    {
        get => (string)GetValue(IconPathProperty);
        set => SetValue(IconPathProperty, value);
    }

    public Thickness ContentPadding
    {
        get => (Thickness)GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    public KenneyButton()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyAssets();
        IsEnabledChanged += (_, _) => UpdateEnabledState();
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KenneyButton button && button.IsLoaded)
        {
            button.ApplyAssets();
        }
    }

    private void ApplyAssets()
    {
        KenneySvg.ApplyTo(BackgroundImage, AssetPath);

        if (!string.IsNullOrWhiteSpace(IconPath))
        {
            KenneySvg.ApplyTo(IconImage, IconPath);
            IconImage.Visibility = IconImage.Source is null ? Visibility.Collapsed : Visibility.Visible;
            IconImage.Margin = string.IsNullOrWhiteSpace(Label) ? new Thickness(0) : new Thickness(0, 0, 8, 0);
        }
        else
        {
            IconImage.Visibility = Visibility.Collapsed;
        }

        LabelText.Visibility = string.IsNullOrWhiteSpace(Label) ? Visibility.Collapsed : Visibility.Visible;

        UpdateEnabledState();
    }

    private void UpdateEnabledState()
    {
        Opacity = IsEnabled ? 1.0 : 0.55;
        Cursor = IsEnabled ? Cursors.Hand : Cursors.Arrow;
    }

    private void Root_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        e.Handled = true;
        AnimatePress();
        Click?.Invoke(this, new RoutedEventArgs());
    }

    private void Root_TouchDown(object sender, TouchEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        e.Handled = true;
        AnimatePress();
        Click?.Invoke(this, new RoutedEventArgs());
    }

    private void AnimatePress()
    {
        PressScale.ScaleX = 0.96;
        PressScale.ScaleY = 0.96;

        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(90) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            PressScale.ScaleX = 1;
            PressScale.ScaleY = 1;
        };
        timer.Start();
    }
}
