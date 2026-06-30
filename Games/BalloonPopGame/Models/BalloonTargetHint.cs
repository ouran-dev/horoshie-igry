using System.Windows.Media;
using System.Windows;

namespace HoroshieIgry.Games.BalloonPopGame.Models;

public sealed class BalloonTargetHint
{
    public required Brush Fill { get; init; }
    public required Brush Stroke { get; init; }
    public int Count { get; init; }
    public string CountLabel => $"×{Count}";
    public Visibility CountVisibility => Count > 1 ? Visibility.Visible : Visibility.Collapsed;

    public static BalloonTargetHint Create(BalloonTargetGroup group)
    {
        var fill = new SolidColorBrush(group.Color.FillColor());
        var stroke = new SolidColorBrush(group.Color.DarkColor());
        fill.Freeze();
        stroke.Freeze();

        return new BalloonTargetHint
        {
            Fill = fill,
            Stroke = stroke,
            Count = group.Count
        };
    }
}
