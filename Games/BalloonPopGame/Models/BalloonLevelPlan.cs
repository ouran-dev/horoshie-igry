namespace HoroshieIgry.Games.BalloonPopGame.Models;

public sealed class BalloonLevelPlan
{
    public required int Level { get; init; }
    public required IReadOnlyList<BalloonTargetGroup> TargetGroups { get; init; }
    public required IReadOnlyList<BalloonModel> Balloons { get; init; }

    public int TargetsToPop => TargetGroups.Sum(g => g.Count);

    public string TaskPrefix => TargetGroups.Count == 1
        ? "Лопни шарики такого цвета:"
        : "Лопни шарики этих цветов:";
}
