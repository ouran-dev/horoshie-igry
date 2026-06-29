using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using HoroshieIgry.Games.FindOddGame.Models;

namespace HoroshieIgry.Games.FindOddGame.Controls;

public partial class OddChoiceControl : UserControl
{
    private OddItemModel? _item;

    public static readonly DependencyProperty IconSlotSizeProperty =
        DependencyProperty.Register(nameof(IconSlotSize), typeof(double), typeof(OddChoiceControl),
            new PropertyMetadata(76.0));

    public static readonly DependencyProperty LabelFontSizeProperty =
        DependencyProperty.Register(nameof(LabelFontSize), typeof(double), typeof(OddChoiceControl),
            new PropertyMetadata(15.0));

    public static readonly DependencyProperty LabelMaxWidthProperty =
        DependencyProperty.Register(nameof(LabelMaxWidth), typeof(double), typeof(OddChoiceControl),
            new PropertyMetadata(120.0));

    public double IconSlotSize
    {
        get => (double)GetValue(IconSlotSizeProperty);
        set => SetValue(IconSlotSizeProperty, value);
    }

    public double LabelFontSize
    {
        get => (double)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public double LabelMaxWidth
    {
        get => (double)GetValue(LabelMaxWidthProperty);
        set => SetValue(LabelMaxWidthProperty, value);
    }

    public OddChoiceControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += (_, _) => UnsubscribeFromItem();
    }

    public OddItemModel? Item => _item;

    public void PlayPressFeedback()
    {
        PressScale.ScaleX = 0.94;
        PressScale.ScaleY = 0.94;

        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(90) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            PressScale.ScaleX = 1;
            PressScale.ScaleY = 1;
        };
        timer.Start();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UnsubscribeFromItem();
        if (e.NewValue is OddItemModel item)
        {
            _item = item;
            _item.PropertyChanged += Item_PropertyChanged;
            UpdateFeedback();
            Opacity = item.IsClickable ? 1 : 0.55;
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e) => UpdateSizes();

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateSizes();

    private void UpdateSizes()
    {
        if (ActualWidth < 1) return;

        IconSlotSize = Math.Clamp(ActualWidth * 0.68, 56, 120);
        LabelFontSize = Math.Clamp(ActualWidth * 0.11, 12, 18);
        LabelMaxWidth = Math.Max(80, ActualWidth - 16);
    }

    private void UnsubscribeFromItem()
    {
        if (_item is not null)
        {
            _item.PropertyChanged -= Item_PropertyChanged;
            _item = null;
        }
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OddItemModel.IsClickable) && _item is not null)
            Opacity = _item.IsClickable ? 1 : 0.55;

        if (e.PropertyName is nameof(OddItemModel.IsWrongFlash) or nameof(OddItemModel.IsCorrectFlash))
            UpdateFeedback();

        if (e.PropertyName == nameof(OddItemModel.IsShaking) && _item?.IsShaking == true)
            PlayShake();
    }

    private void PlayShake()
    {
        var animation = new DoubleAnimationUsingKeyFrames
        {
            Duration = TimeSpan.FromMilliseconds(420)
        };
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(-8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(70))));
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(140))));
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(210))));
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(280))));
        animation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(420))));

        ShakeTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, animation);
    }

    private void UpdateFeedback()
    {
        WrongFlash.Visibility = _item?.IsWrongFlash == true ? Visibility.Visible : Visibility.Collapsed;
        CorrectFlash.Visibility = _item?.IsCorrectFlash == true ? Visibility.Visible : Visibility.Collapsed;
    }
}
