namespace HoroshieIgry.Core.Games;

/// <summary>Реестр всех мини-игр приложения.</summary>
public sealed class GameCatalog
{
    private readonly List<IGameModule> _games = new();

    public IReadOnlyList<IGameModule> Games => _games;

    public void Register(IGameModule game)
    {
        if (_games.Any(g => g.Id == game.Id))
        {
            throw new InvalidOperationException($"Игра с Id «{game.Id}» уже зарегистрирована.");
        }

        _games.Add(game);
    }

    public IGameModule? FindById(string id) => _games.FirstOrDefault(g => g.Id == id);
}
