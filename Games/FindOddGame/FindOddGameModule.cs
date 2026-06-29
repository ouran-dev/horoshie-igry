using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.FindOddGame;

public sealed class FindOddGameModule : IGameModule
{
    public int CatalogOrder => 30;
    public string Id => "find-odd";
    public string Title => "Найди лишнее";
    public string Description => "4–9 карточек с предметами: найди лишний за отведённое время.";
    public string IconEmoji => "🔍";
    public bool IsAvailable => true;
    public bool IsNew => false;
    public BackgroundTheme BackgroundTheme => BackgroundTheme.Forest;

    public UserControl CreateView(INavigationContext navigation)
        => new FindOddGameView(navigation);
}
