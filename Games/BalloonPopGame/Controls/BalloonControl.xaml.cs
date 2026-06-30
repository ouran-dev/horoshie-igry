using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using HoroshieIgry.Games.BalloonPopGame.Models;

namespace HoroshieIgry.Games.BalloonPopGame.Controls;

public partial class BalloonControl : UserControl
{
    private BalloonModel? _balloon;

    public event EventHandler<BalloonModel>? BalloonTapped;

    public BalloonControl()
    {
        InitializeComponent();
        DataContextChanged += (_, e) => BindModel(e.NewValue as BalloonModel);

        PreviewMouseLeftButtonDown += (_, _) => HandleTap();
        PreviewTouchDown += (_, e) =>
        {
            HandleTap();
            e.Handled = true;
        };
    }

    public BalloonModel? Balloon => _balloon;

    public void UpdateIdle(double seconds)
    {
        if (_balloon is null || _balloon.IsPopped) return;

        var phase = _balloon.AnimPhase;
        BobTransform.X = Math.Sin(seconds * 0.9 + phase) * 2;
        BobTransform.Y = Math.Sin(seconds * 1.3 + phase * 1.4) * 2;

        var scale = 1 + Math.Sin(seconds * 1.1 + phase) * 0.02;
        ScaleTransform.ScaleX = scale;
        ScaleTransform.ScaleY = scale;
    }

    public void PlayPopVisual()
    {
        if (_balloon is null) return;

        IsHitTestVisible = false;
        BalloonBody.Visibility = Visibility.Collapsed;

        var center = Width / 2;
        const int pieces = 4;

        PopCanvas.Visibility = Visibility.Visible;
        PopCanvas.Children.Clear();

        for (var i = 0; i < pieces; i++)
        {
            var size = 5 + i % 2 * 2;
            var piece = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = Brushes.White,
                Opacity = 0.85,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(piece, center);
            Canvas.SetTop(piece, center);
            PopCanvas.Children.Add(piece);

            var angle = i * Math.PI * 2 / pieces;
            var dist = 14 + i * 2;
            var duration = TimeSpan.FromMilliseconds(140);

            piece.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(center, center + Math.Cos(angle) * dist, duration));
            piece.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(center, center + Math.Sin(angle) * dist, duration));

            var fade = new DoubleAnimation(0.85, 0, duration);
            if (i == pieces - 1)
                fade.Completed += (_, _) => FinishPopVisual();

            piece.BeginAnimation(OpacityProperty, fade);
        }
    }

    private void FinishPopVisual()
    {
        Visibility = Visibility.Collapsed;
        PopCanvas.Children.Clear();
        PopCanvas.Visibility = Visibility.Collapsed;
    }

    public void PlayWrongBounce()
    {
        var bounce = new DoubleAnimationUsingKeyFrames();
        bounce.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        bounce.KeyFrames.Add(new LinearDoubleKeyFrame(-6, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(70))));
        bounce.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(170))));
        BobTransform.BeginAnimation(TranslateTransform.YProperty, bounce);
    }

    public void FadeOut()
    {
        if (_balloon is null) return;

        var fade = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(280));
        fade.Completed += (_, _) => Visibility = Visibility.Collapsed;
        BeginAnimation(OpacityProperty, fade);
    }

    private void BindModel(BalloonModel? model)
    {
        _balloon = model;
        DataContext = model;

        if (model is null) return;

        var size = model.Size;
        Width = size;
        Height = size;

        BubbleDome.Width = size;
        BubbleDome.Height = size;

        BubbleHighlight.Width = size * 0.38;
        BubbleHighlight.Height = size * 0.30;
        BubbleHighlight.Margin = new Thickness(size * 0.14, size * 0.12, 0, 0);

        var fill = model.Color.FillColor();
        var light = model.Color.LightColor();
        var dark = model.Color.DarkColor();
        LightColorStop.Color = light;
        MainColorStop.Color = fill;
        DarkColorStop.Color = dark;
        StrokeBrush.Color = dark;

        Visibility = model.IsPopped ? Visibility.Collapsed : Visibility.Visible;
        IsHitTestVisible = !model.IsPopped;
        Opacity = model.Opacity;
        BalloonBody.Opacity = model.Opacity;
    }

    private void HandleTap()
    {
        if (_balloon is null || _balloon.IsPopped) return;
        BalloonTapped?.Invoke(this, _balloon);
    }
}
