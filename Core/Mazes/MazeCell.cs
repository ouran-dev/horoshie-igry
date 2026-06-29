namespace HoroshieIgry.Core.Mazes;

public readonly record struct MazeCell(int Row, int Col)
{
    public static MazeCell Invalid => new(-1, -1);
    public bool IsValid => Row >= 0 && Col >= 0;
}
