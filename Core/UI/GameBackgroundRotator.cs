namespace HoroshieIgry.Core.UI;

/// <summary>Подбор фона для раунда — темы меняются по уровню внутри игры.</summary>
public static class GameBackgroundRotator
{
    public static BackgroundTheme ForRound(string gameId, int level)
    {
        var themes = GetThemes(gameId);
        var index = Math.Max(0, level - 1) % themes.Length;
        return themes[index];
    }

    private static BackgroundTheme[] GetThemes(string gameId) => gameId switch
    {
        "memory" => [BackgroundTheme.Meadow, BackgroundTheme.Forest, BackgroundTheme.Clouds, BackgroundTheme.Sea, BackgroundTheme.Meadow],
        "find-color" => [BackgroundTheme.Clouds, BackgroundTheme.Sea, BackgroundTheme.Space, BackgroundTheme.Mountains, BackgroundTheme.Desert],
        "find-odd" => [BackgroundTheme.Forest, BackgroundTheme.Meadow, BackgroundTheme.Desert, BackgroundTheme.Clouds, BackgroundTheme.Sea],
        "tic-tac-toe" => [BackgroundTheme.Clouds, BackgroundTheme.Meadow, BackgroundTheme.Sea, BackgroundTheme.Mountains, BackgroundTheme.Clouds],
        "maze" => [BackgroundTheme.Meadow, BackgroundTheme.Forest, BackgroundTheme.Clouds, BackgroundTheme.Desert, BackgroundTheme.Sea],
        _ => [BackgroundTheme.Catalog, BackgroundTheme.Meadow, BackgroundTheme.Forest, BackgroundTheme.Sea]
    };
}
