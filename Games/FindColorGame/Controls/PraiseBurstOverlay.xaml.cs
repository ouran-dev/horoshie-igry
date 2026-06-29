using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HoroshieIgry.Controls.Kenney;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.FindColorGame.Controls;

/// <summary>Короткая похвала с «хлопушкой» из звёздочек.</summary>
public partial class PraiseBurstOverlay : UserControl
{
    private static readonly string[] PraisePhrases =
    {
        "Отлично!",
        "Молодец!",
        "Супер!",
        "Ура!",
        "Верно!"
    };

    private static readonly string[] ParticleEmojis = { "⭐", "✨", "🎉", "🎊", "💫" };

    private readonly Random _random = new();

    public PraiseBurstOverlay()
    {
        InitializeComponent();
    }

    public async Task PlayAsync()
    {
        Visibility = Visibility.Visible;
        ParticleCanvas.Children.Clear();
        PraiseText.Text = PraisePhrases[_random.Next(PraisePhrases.Length)];

        var centerX = ActualWidth > 0 ? ActualWidth / 2 : 400;
        var centerY = ActualHeight > 0 ? ActualHeight / 2 : 300;

        SpawnParticles(centerX, centerY);
        AnimatePraiseCard();

        await Task.Delay(680);
        Visibility = Visibility.Collapsed;
        ParticleCanvas.Children.Clear();
    }

    private void SpawnParticles(double centerX, double centerY)
    {
        const int count = 28;

        for (var i = 0; i < count; i++)
        {
            var useStar = i % 3 == 0;
            UIElement particle;

            if (useStar)
            {
                var star = new KenneyImage
                {
                    Width = 20 + _random.Next(16),
                    Height = 20 + _random.Next(16),
                    AssetPath = KenneyPaths.Star,
                    Opacity = 0.95
                };
                particle = star;
            }
            else
            {
                particle = new TextBlock
                {
                    Text = ParticleEmojis[_random.Next(ParticleEmojis.Length)],
                    FontSize = 18 + _random.Next(14),
                    Opacity = 0.95
                };
            }

            var angle = _random.NextDouble() * Math.PI * 2;
            var distance = 80 + _random.NextDouble() * 160;
            var targetX = centerX + Math.Cos(angle) * distance - 12;
            var targetY = centerY + Math.Sin(angle) * distance - 12;

            Canvas.SetLeft(particle, centerX - 12);
            Canvas.SetTop(particle, centerY - 12);
            ParticleCanvas.Children.Add(particle);

            var moveX = new DoubleAnimation(centerX - 12, targetX, TimeSpan.FromMilliseconds(480))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var moveY = new DoubleAnimation(centerY - 12, targetY, TimeSpan.FromMilliseconds(480))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(520));

            particle.BeginAnimation(Canvas.LeftProperty, moveX);
            particle.BeginAnimation(Canvas.TopProperty, moveY);
            particle.BeginAnimation(OpacityProperty, fade);
        }
    }

    private void AnimatePraiseCard()
    {
        PraiseCard.Opacity = 1;
        PraiseScale.ScaleX = 0.65;
        PraiseScale.ScaleY = 0.65;

        var easing = new BackEase { Amplitude = 0.4, EasingMode = EasingMode.EaseOut };
        var scaleAnimation = new DoubleAnimation(0.65, 1.05, TimeSpan.FromMilliseconds(240))
        {
            EasingFunction = easing
        };

        PraiseScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        PraiseScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }
}
