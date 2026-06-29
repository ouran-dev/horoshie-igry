using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.FindColorGame;

public sealed class FindColorGameModule : IGameModule
{
    public int CatalogOrder => 20;
    public string Id => "find-color";
    public string Title => "Найди цвет";
    public string Description => "Выбери фигуру нужного цвета за отведённое время.";
    public string IconEmoji => "🎨";
    public bool IsAvailable => true;
    public bool IsNew => false;

    public BackgroundTheme BackgroundTheme => BackgroundTheme.Clouds;

    public UserControl CreateView(INavigationContext navigation)
        => new FindColorGameView(navigation);
}
