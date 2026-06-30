namespace HoroshieIgry.Games.PaperPlaneGame.Models;

/// <summary>Описание уровня из JSON.</summary>
public sealed class PaperPlaneLevelDefinition
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public double Length { get; init; }
    public double ScrollSpeed { get; init; }
    public string Background { get; init; } = "clouds";
    public double PlaneStartY { get; init; }
    public double FinishX { get; init; }
    public bool ShowTutorial { get; init; }
    public IReadOnlyList<PaperPlaneStarDef> Stars { get; init; } = [];
    public IReadOnlyList<PaperPlaneObstacleDef> Obstacles { get; init; } = [];
    public IReadOnlyList<PaperPlaneWindGustDef> WindGusts { get; init; } = [];
}

public sealed class PaperPlaneStarDef
{
    public double X { get; init; }
    public double Y { get; init; }
}

public sealed class PaperPlaneObstacleDef
{
    public string Type { get; init; } = "cloud";
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public bool Solid { get; init; } = true;
    public PaperPlaneMotionDef? Motion { get; init; }
}

public sealed class PaperPlaneMotionDef
{
    public string Axis { get; init; } = "y";
    public double Amplitude { get; init; }
    public double PeriodSec { get; init; } = 2.5;
}

public sealed class PaperPlaneWindGustDef
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    /// <summary>up или down.</summary>
    public string Direction { get; init; } = "up";
    public double Strength { get; init; } = 100;
}
