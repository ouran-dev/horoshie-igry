using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HoroshieIgry.Controls.Kenney;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.PaperPlaneGame.Controls;

public partial class PaperPlaneVictoryOverlay : UserControl
{
    private static readonly string[] ParticleEmojis = ["🎉", "🎊", "⭐", "✨", "💫", "✈️"];
    private readonly Random _random = new();

    public event EventHandler? NextFlightRequested;

    public PaperPlaneVictoryOverlay()
    {
        InitializeComponent();
    }

    public async Task ShowAsync(int starsCollected, int totalStars, int level, double elapsedSeconds)
    {
        StarsResultText.Text = $"Звёзды: {starsCollected} из {totalStars}";
        TimeResultText.Text = $"Время: {FormatTime(elapsedSeconds)}";
        LevelResultText.Text = $"Уровень {level}";

        Visibility = Visibility.Visible;
        ParticleCanvas.Children.Clear();

        var width = ActualWidth > 0 ? ActualWidth : 800;
        var height = ActualHeight > 0 ? ActualHeight : 600;
        SpawnConfetti(width / 2, height / 2, width, height);
        AnimateCard();

        await Task.Delay(400);
    }

    public void Hide()
    {
        Visibility = Visibility.Collapsed;
        ParticleCanvas.Children.Clear();
        VictoryCard.Opacity = 0;
    }

    private void NextFlightButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        NextFlightRequested?.Invoke(this, EventArgs.Empty);
    }

    private void SpawnConfetti(double centerX, double centerY, double width, double height)
    {
        for (var i = 0; i < 44; i++)
        {
            UIElement particle = i % 4 == 0
                ? new KenneyImage
                {
                    Width = 20 + _random.Next(16),
                    Height = 20 + _random.Next(16),
                    AssetPath = KenneyPaths.Star,
                    Opacity = 0.95
                }
                : new TextBlock
                {
                    Text = ParticleEmojis[_random.Next(ParticleEmojis.Length)],
                    FontSize = 18 + _random.Next(14),
                    Opacity = 0.95
                };

            var startX = centerX + (_random.NextDouble() - 0.5) * width * 0.3;
            var startY = centerY + (_random.NextDouble() - 0.5) * height * 0.15;
            var angle = _random.NextDouble() * Math.PI * 2;
            var distance = 100 + _random.NextDouble() * Math.Max(width, height) * 0.4;

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            ParticleCanvas.Children.Add(particle);

            var duration = TimeSpan.FromMilliseconds(650 + _random.Next(220));
            particle.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(startX, startX + Math.Cos(angle) * distance, duration));
            particle.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(startY, startY + Math.Sin(angle) * distance, duration));
            particle.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(800)));
        }
    }

    private void AnimateCard()
    {
        VictoryCard.Opacity = 1;
        VictoryScale.ScaleX = 0.65;
        VictoryScale.ScaleY = 0.65;
        var easing = new BackEase { Amplitude = 0.4, EasingMode = EasingMode.EaseOut };
        var anim = new DoubleAnimation(0.65, 1.05, TimeSpan.FromMilliseconds(300)) { EasingFunction = easing };
        VictoryScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        VictoryScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
    }

    private static string FormatTime(double seconds)
    {
        var total = Math.Max(0, (int)seconds);
        return $"{total / 60:00}:{total % 60:00}";
    }
}
