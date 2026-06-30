using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.MazeGame;

public sealed class MazeGameModule : IGameModule
{
    public int CatalogOrder => 50;
    public string Id => "maze";
    public string Title => "Лабиринт";
    public string Description => "Веди персонажа пальцем по дорожке к выходу из лабиринта.";
    public string IconEmoji => "🧭";
    public bool IsAvailable => true;
    public bool IsNew => false;
    public BackgroundTheme BackgroundTheme => BackgroundTheme.Meadow;

    public UserControl CreateView(INavigationContext navigation)
        => new MazeGameView(navigation);
}
