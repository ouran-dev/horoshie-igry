using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.SortingGame;

public sealed class SortingGameModule : IGameModule
{
    public int CatalogOrder => 65;
    public string Id => "sorting";
    public string Title => "Наведи порядок";
    public string Description => "Разложи предметы по своим местам и помоги навести порядок!";
    public string IconEmoji => "🧺";
    public bool IsAvailable => true;
    public bool IsNew => true;
    public BackgroundTheme BackgroundTheme => BackgroundTheme.Meadow;

    public UserControl CreateView(INavigationContext navigation)
        => new SortingGameView(navigation);
}
