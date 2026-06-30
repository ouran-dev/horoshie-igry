namespace HoroshieIgry.Core.Mazes;

/// <summary>Плавное движение по лабиринту без привязки к центрам клеток.</summary>
public static class MazeFreeMovement
{
    public const double CharacterRadius = 0.18;

    private const double MaxMoveSegment = 0.08;
    private const double RaySampleSpacing = 0.04;

    public static bool IsWalkableAt(MazeDefinition maze, double row, double col, double radius = CharacterRadius)
    {
        if (!IsPointInsideWalkableArea(maze, row, col))
            return false;

        const int samples = 8;
        for (var i = 0; i < samples; i++)
        {
            var angle = i * Math.PI * 2 / samples;
            var sampleRow = row + Math.Sin(angle) * radius;
            var sampleCol = col + Math.Cos(angle) * radius;
            if (!IsPointInsideWalkableArea(maze, sampleRow, sampleCol))
                return false;
        }

        return true;
    }

    public static (double Row, double Col) MoveTo(
        MazeDefinition maze,
        double fromRow,
        double fromCol,
        double toRow,
        double toCol)
    {
        var deltaRow = toRow - fromRow;
        var deltaCol = toCol - fromCol;
        var distance = Math.Sqrt(deltaRow * deltaRow + deltaCol * deltaCol);
        if (distance < 0.0001)
            return (fromRow, fromCol);

        var segmentCount = Math.Max(1, (int)Math.Ceiling(distance / MaxMoveSegment));
        var row = fromRow;
        var col = fromCol;

        for (var i = 1; i <= segmentCount; i++)
        {
            var t = i / (double)segmentCount;
            var segmentRow = fromRow + deltaRow * t;
            var segmentCol = fromCol + deltaCol * t;
            (row, col) = AdvanceAlongRay(maze, row, col, segmentRow, segmentCol);
        }

        return (row, col);
    }

    public static bool IsNearExit(MazeDefinition maze, double row, double col, double threshold = 0.38)
    {
        var exitRow = maze.Exit.Row + 0.5;
        var exitCol = maze.Exit.Col + 0.5;
        var dist = Math.Sqrt(Math.Pow(row - exitRow, 2) + Math.Pow(col - exitCol, 2));
        return dist <= threshold;
    }

    private static (double Row, double Col) AdvanceAlongRay(
        MazeDefinition maze,
        double fromRow,
        double fromCol,
        double toRow,
        double toCol)
    {
        var deltaRow = toRow - fromRow;
        var deltaCol = toCol - fromCol;
        var distance = Math.Sqrt(deltaRow * deltaRow + deltaCol * deltaCol);
        if (distance < 0.0001)
            return (fromRow, fromCol);

        if (!SegmentCrossesBlockedCell(maze, fromRow, fromCol, toRow, toCol))
            return IsWalkableAt(maze, toRow, toCol) ? (toRow, toCol) : (fromRow, fromCol);

        var bestRow = fromRow;
        var bestCol = fromCol;
        var stepCount = Math.Max(4, (int)Math.Ceiling(distance / RaySampleSpacing));

        for (var i = 1; i <= stepCount; i++)
        {
            var t = i / (double)stepCount;
            var row = fromRow + deltaRow * t;
            var col = fromCol + deltaCol * t;
            if (!IsWalkableAt(maze, row, col))
                break;

            bestRow = row;
            bestCol = col;
        }

        return (bestRow, bestCol);
    }

    private static bool SegmentCrossesBlockedCell(
        MazeDefinition maze,
        double fromRow,
        double fromCol,
        double toRow,
        double toCol)
    {
        var cells = CollectCellsAlongSegment(fromRow, fromCol, toRow, toCol);
        foreach (var (row, col) in cells)
        {
            if (!maze.IsWalkable(row, col))
                return true;
        }

        return false;
    }

    private static List<(int Row, int Col)> CollectCellsAlongSegment(
        double fromRow,
        double fromCol,
        double toRow,
        double toCol)
    {
        var startRow = (int)Math.Floor(fromRow);
        var startCol = (int)Math.Floor(fromCol);
        var endRow = (int)Math.Floor(toRow);
        var endCol = (int)Math.Floor(toCol);

        var cells = new List<(int Row, int Col)>();
        var row = startRow;
        var col = startCol;
        var deltaRow = Math.Abs(endRow - startRow);
        var deltaCol = Math.Abs(endCol - startCol);
        var stepRow = startRow < endRow ? 1 : startRow > endRow ? -1 : 0;
        var stepCol = startCol < endCol ? 1 : startCol > endCol ? -1 : 0;
        var error = deltaCol - deltaRow;

        while (true)
        {
            cells.Add((row, col));

            if (row == endRow && col == endCol)
                break;

            var error2 = error * 2;
            if (error2 > -deltaRow)
            {
                error -= deltaRow;
                col += stepCol;
            }

            if (error2 < deltaCol)
            {
                error += deltaCol;
                row += stepRow;
            }
        }

        return cells;
    }

    private static bool IsPointInsideWalkableArea(MazeDefinition maze, double row, double col)
    {
        var cellRow = (int)Math.Floor(row);
        var cellCol = (int)Math.Floor(col);

        if (maze.IsWalkable(cellRow, cellCol))
            return true;

        foreach (var (checkRow, checkCol) in NeighborCells(cellRow, cellCol))
        {
            if (!maze.IsWalkable(checkRow, checkCol))
                continue;

            if (IsInsideCellBounds(row, col, checkRow, checkCol))
                return true;
        }

        return false;
    }

    private static IEnumerable<(int Row, int Col)> NeighborCells(int row, int col)
    {
        yield return (row, col);
        yield return (row - 1, col);
        yield return (row + 1, col);
        yield return (row, col - 1);
        yield return (row, col + 1);
    }

    private static bool IsInsideCellBounds(double row, double col, int cellRow, int cellCol)
    {
        return row >= cellRow && row < cellRow + 1
            && col >= cellCol && col < cellCol + 1;
    }
}
