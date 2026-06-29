namespace HoroshieIgry.Core.Mazes;

/// <summary>Описание одного лабиринта (логика отдельно от UI).</summary>
public sealed class MazeDefinition
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required int Difficulty { get; init; }
    public required string ExitEmoji { get; init; }
    public required int Rows { get; init; }
    public required int Cols { get; init; }
    public required bool[,] Walls { get; init; }
    public required MazeCell Start { get; init; }
    public required MazeCell Exit { get; init; }

    public bool IsWalkable(int row, int col)
    {
        if (row < 0 || col < 0 || row >= Rows || col >= Cols)
            return false;

        return !Walls[row, col];
    }

    public bool IsWalkable(MazeCell cell) => IsWalkable(cell.Row, cell.Col);

    public MazeCell ToCell(double row, double col)
        => new((int)Math.Floor(row), (int)Math.Floor(col));

    public (double Row, double Col) StartCenter => (Start.Row + 0.5, Start.Col + 0.5);

    public bool IsAtExit(double row, double col)
    {
        var cell = ToCell(row, col);
        return cell.Row == Exit.Row && cell.Col == Exit.Col;
    }
}
