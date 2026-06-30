using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HoroshieIgry.Core.Objects;
using HoroshieIgry.Games.SortingGame.Models;

namespace HoroshieIgry.Games.SortingGame.Controls;

public partial class SortItemControl : UserControl
{
    public static readonly DependencyProperty ItemPlanProperty =
        DependencyProperty.Register(nameof(ItemPlan), typeof(SortItemPlan), typeof(SortItemControl),
            new PropertyMetadata(null, OnItemPlanChanged));

    public SortItemPlan? ItemPlan
    {
        get => (SortItemPlan?)GetValue(ItemPlanProperty);
        set => SetValue(ItemPlanProperty, value);
    }

    public event EventHandler<SortItemDragEventArgs>? DragStarted;

    public SortItemControl()
    {
        InitializeComponent();
        PreviewMouseLeftButtonDown += OnPointerDown;
        PreviewTouchDown += OnTouchDown;
    }

    public void SetDragVisual(bool isDragging)
    {
        DragScale.ScaleX = isDragging ? 1.12 : 1;
        DragScale.ScaleY = isDragging ? 1.12 : 1;
        ItemShadow.BlurRadius = isDragging ? 14 : 0;
        ItemShadow.ShadowDepth = isDragging ? 4 : 0;
        ItemShadow.Opacity = isDragging ? 0.35 : 0;
    }

    public async Task AnimateToAsync(double x, double y, int durationMs)
    {
        var left = Canvas.GetLeft(this);
        var top = Canvas.GetTop(this);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;

        var duration = TimeSpan.FromMilliseconds(durationMs);
        var leftAnim = new DoubleAnimation(left, x, duration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var topAnim = new DoubleAnimation(top, y, duration)
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        BeginAnimation(Canvas.LeftProperty, leftAnim);
        BeginAnimation(Canvas.TopProperty, topAnim);
        await Task.Delay(durationMs + 5);
        SnapPosition(x, y);
    }

    public void SnapPosition(double x, double y)
    {
        BeginAnimation(Canvas.LeftProperty, null);
        BeginAnimation(Canvas.TopProperty, null);
        Canvas.SetLeft(this, x);
        Canvas.SetTop(this, y);
    }

    public async Task FadeOutAsync(int durationMs = 120)
    {
        var fade = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(durationMs));
        BeginAnimation(OpacityProperty, fade);
        await Task.Delay(durationMs + 5);
        Visibility = Visibility.Collapsed;
    }

    public async Task PlaySuccessStarsAsync()
    {
        StarCanvas.Visibility = Visibility.Visible;
        StarCanvas.Children.Clear();

        for (var i = 0; i < 5; i++)
        {
            var star = new TextBlock
            {
                Text = i % 2 == 0 ? "✨" : "⭐",
                FontSize = 14 + i * 2,
                Opacity = 1
            };
            Canvas.SetLeft(star, 36);
            Canvas.SetTop(star, 36);
            StarCanvas.Children.Add(star);

            var angle = i * 1.3;
            var dist = 20 + i * 4;
            var duration = TimeSpan.FromMilliseconds(180);
            star.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(36, 36 + Math.Cos(angle) * dist, duration));
            star.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(36, 36 + Math.Sin(angle) * dist, duration));
            star.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, duration));
        }

        await Task.Delay(190);
        StarCanvas.Children.Clear();
        StarCanvas.Visibility = Visibility.Collapsed;
    }

    private static void OnItemPlanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SortItemControl control && e.NewValue is SortItemPlan plan)
            control.BindPlan(plan);
    }

    private void BindPlan(SortItemPlan plan)
    {
        EmojiText.Text = plan.Emoji;

        if (plan.ImageRelativePath is not null)
        {
            var image = GameObjectImageLoader.Load(new GameObjectEntry
            {
                Id = plan.ObjectId,
                CategoryId = plan.CategoryId,
                Label = plan.Label,
                Emoji = plan.Emoji,
                ImageRelativePath = plan.ImageRelativePath
            });

            if (image is not null)
            {
                ObjectImage.Source = image;
                ObjectImage.Visibility = Visibility.Visible;
                EmojiText.Visibility = Visibility.Collapsed;
                return;
            }
        }

        ObjectImage.Visibility = Visibility.Collapsed;
        EmojiText.Visibility = Visibility.Visible;
    }

    private void OnPointerDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null) return;
        StartDrag(null);
        e.Handled = true;
    }

    private void OnTouchDown(object? sender, TouchEventArgs e)
    {
        StartDrag(e.TouchDevice);
        e.Handled = true;
    }

    private void StartDrag(TouchDevice? touchDevice)
    {
        if (ItemPlan is null || !IsHitTestVisible || Visibility != Visibility.Visible)
            return;

        BeginAnimation(Canvas.LeftProperty, null);
        BeginAnimation(Canvas.TopProperty, null);

        DragStarted?.Invoke(this, new SortItemDragEventArgs
        {
            Item = this,
            TouchDevice = touchDevice
        });
    }
}
