using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HoroshieIgry.Controls.Kenney;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.SortingGame.Controls;

public partial class SortingVictoryOverlay : UserControl
{
    private static readonly string[] ParticleEmojis = ["🎉", "🎊", "⭐", "✨", "💫", "🎈"];

    private readonly Random _random = new();

    public SortingVictoryOverlay()
    {
        InitializeComponent();
    }

    public async Task PlayVictoryAsync()
    {
        Visibility = Visibility.Visible;
        ParticleCanvas.Children.Clear();

        var width = ActualWidth > 0 ? ActualWidth : 800;
        var height = ActualHeight > 0 ? ActualHeight : 600;
        var centerX = width / 2;
        var centerY = height / 2;

        SpawnConfetti(centerX, centerY, width, height);
        AnimateVictoryCard();

        await Task.Delay(1100);
        Visibility = Visibility.Collapsed;
        ParticleCanvas.Children.Clear();
    }

    private void SpawnConfetti(double centerX, double centerY, double width, double height)
    {
        const int count = 48;

        for (var i = 0; i < count; i++)
        {
            UIElement particle;
            if (i % 4 == 0)
            {
                particle = new KenneyImage
                {
                    Width = 22 + _random.Next(18),
                    Height = 22 + _random.Next(18),
                    AssetPath = KenneyPaths.Star,
                    Opacity = 0.95
                };
            }
            else
            {
                particle = new TextBlock
                {
                    Text = ParticleEmojis[_random.Next(ParticleEmojis.Length)],
                    FontSize = 20 + _random.Next(16),
                    Opacity = 0.95
                };
            }

            var startX = centerX + (_random.NextDouble() - 0.5) * width * 0.35;
            var startY = centerY + (_random.NextDouble() - 0.5) * height * 0.2;
            var angle = _random.NextDouble() * Math.PI * 2;
            var distance = 120 + _random.NextDouble() * Math.Max(width, height) * 0.45;
            var targetX = startX + Math.Cos(angle) * distance;
            var targetY = startY + Math.Sin(angle) * distance;

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            ParticleCanvas.Children.Add(particle);

            var duration = TimeSpan.FromMilliseconds(700 + _random.Next(250));
            particle.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(startX, targetX, duration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
            particle.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(startY, targetY, duration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
            particle.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(850)));
        }
    }

    private void AnimateVictoryCard()
    {
        VictoryCard.Opacity = 1;
        VictoryScale.ScaleX = 0.55;
        VictoryScale.ScaleY = 0.55;

        var easing = new BackEase { Amplitude = 0.45, EasingMode = EasingMode.EaseOut };
        var scaleAnimation = new DoubleAnimation(0.55, 1.08, TimeSpan.FromMilliseconds(320))
        {
            EasingFunction = easing
        };

        VictoryScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        VictoryScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }
}
