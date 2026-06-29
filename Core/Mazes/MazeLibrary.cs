namespace HoroshieIgry.Core.Mazes;

public sealed class MazeLibrary
{
    public MazeLibrary(IReadOnlyList<MazeDefinition> mazes)
    {
        Mazes = mazes;
        _byId = mazes.ToDictionary(m => m.Id);
    }

    public IReadOnlyList<MazeDefinition> Mazes { get; }

    private readonly Dictionary<int, MazeDefinition> _byId;

    public MazeDefinition GetByIndex(int index)
    {
        if (index < 0 || index >= Mazes.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return Mazes[index];
    }

    public MazeDefinition? TryGetById(int id)
        => _byId.TryGetValue(id, out var maze) ? maze : null;
}
