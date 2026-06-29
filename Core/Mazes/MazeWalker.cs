namespace HoroshieIgry.Core.Mazes;

/// <summary>Движение персонажа по дорожкам лабиринта.</summary>
public static class MazeWalker
{
    private static readonly (int Dr, int Dc)[] Directions = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    public static bool CanGrabCharacter(
        double characterRow,
        double characterCol,
        double touchRow,
        double touchCol,
        double maxDistanceCells = 1.85)
        => IsNearCharacter(characterRow, characterCol, touchRow, touchCol, maxDistanceCells);

    public static MazeCell FindNearestWalkable(MazeDefinition maze, double row, double col)
    {
        var center = maze.ToCell(row, col);
        if (maze.IsWalkable(center))
            return center;

        var best = MazeCell.Invalid;
        var bestDist = double.MaxValue;

        for (var r = 0; r < maze.Rows; r++)
        for (var c = 0; c < maze.Cols; c++)
        {
            if (!maze.IsWalkable(r, c)) continue;
            var dist = Math.Abs(r + 0.5 - row) + Math.Abs(c + 0.5 - col);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = new MazeCell(r, c);
            }
        }

        return best;
    }

    public static (double Row, double Col) StepTowardFinger(
        MazeDefinition maze,
        double fromRow,
        double fromCol,
        double fingerRow,
        double fingerCol,
        double stepSize = 0.22)
    {
        var fromCell = maze.ToCell(fromRow, fromCol);
        if (!maze.IsWalkable(fromCell))
            return maze.StartCenter;

        var fingerCell = FindNearestWalkable(maze, fingerRow, fingerCol);
        if (!fingerCell.IsValid)
            return (fromRow, fromCol);

        if (fromCell == fingerCell)
            return (fingerCell.Row + 0.5, fingerCell.Col + 0.5);

        var path = FindPath(maze, fromCell, fingerCell);
        if (path.Count < 2)
            return ClampMove(maze, fromRow, fromCol, fingerRow, fingerCol);

        var (targetRow, targetCol) = AdvanceAlongPath(
            fromRow, fromCol, path, stepSize);

        return ClampMove(maze, fromRow, fromCol, targetRow, targetCol);
    }

    private static (double Row, double Col) AdvanceAlongPath(
        double fromRow,
        double fromCol,
        IReadOnlyList<MazeCell> path,
        double stepSize)
    {
        var row = fromRow;
        var col = fromCol;
        var remaining = stepSize;

        for (var i = 1; i < path.Count && remaining > 0.001; i++)
        {
            var waypointRow = path[i].Row + 0.5;
            var waypointCol = path[i].Col + 0.5;

            var deltaRow = waypointRow - row;
            var deltaCol = waypointCol - col;
            var distance = Math.Sqrt(deltaRow * deltaRow + deltaCol * deltaCol);

            if (distance < 0.001)
                continue;

            if (distance <= remaining)
            {
                row = waypointRow;
                col = waypointCol;
                remaining -= distance;
                continue;
            }

            row += deltaRow / distance * remaining;
            col += deltaCol / distance * remaining;
            break;
        }

        return (row, col);
    }

    public static IReadOnlyList<MazeCell> FindPath(MazeDefinition maze, MazeCell start, MazeCell goal)
    {
        if (!maze.IsWalkable(start) || !maze.IsWalkable(goal))
            return [];

        if (start == goal)
            return [start];

        var parents = new Dictionary<MazeCell, MazeCell>();
        var queue = new Queue<MazeCell>();
        queue.Enqueue(start);
        parents[start] = start;

        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            if (cell == goal)
                return Reconstruct(parents, start, goal);

            foreach (var (dr, dc) in Directions)
            {
                var next = new MazeCell(cell.Row + dr, cell.Col + dc);
                if (!maze.IsWalkable(next) || parents.ContainsKey(next)) continue;
                parents[next] = cell;
                queue.Enqueue(next);
            }
        }

        return [];
    }

    public static (double Row, double Col) ClampMove(
        MazeDefinition maze,
        double fromRow,
        double fromCol,
        double toRow,
        double toCol)
    {
        var bestRow = fromRow;
        var bestCol = fromCol;

        var steps = Math.Max(
            (int)(Math.Ceiling(Math.Abs(toRow - fromRow) * maze.Rows)) + 1,
            (int)(Math.Ceiling(Math.Abs(toCol - fromCol) * maze.Cols)) + 1);
        steps = Math.Clamp(steps, 4, 96);

        for (var i = 1; i <= steps; i++)
        {
            var t = i / (double)steps;
            var row = fromRow + (toRow - fromRow) * t;
            var col = fromCol + (toCol - fromCol) * t;
            var cell = maze.ToCell(row, col);

            if (!maze.IsWalkable(cell))
                break;

            bestRow = row;
            bestCol = col;
        }

        return (bestRow, bestCol);
    }

    public static bool IsNearCharacter(
        double characterRow,
        double characterCol,
        double row,
        double col,
        double maxDistanceCells = 2.4)
    {
        var dist = Math.Sqrt(Math.Pow(row - characterRow, 2) + Math.Pow(col - characterCol, 2));
        return dist <= maxDistanceCells;
    }

    private static List<MazeCell> Reconstruct(Dictionary<MazeCell, MazeCell> parents, MazeCell start, MazeCell goal)
    {
        var path = new List<MazeCell>();
        var current = goal;
        while (true)
        {
            path.Add(current);
            if (current == start) break;
            current = parents[current];
        }

        path.Reverse();
        return path;
    }
}
