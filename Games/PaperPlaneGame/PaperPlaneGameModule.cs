using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.PaperPlaneGame;

public sealed class PaperPlaneGameModule : IGameModule
{
    public int CatalogOrder => 75;
    public string Id => "paper-plane";
    public string Title => "Птичка";
    public string Description => "Помоги птичке пролететь сквозь облака, собирай звёзды и дойди до финиша!";
    public string IconEmoji => "🐦";
    public bool IsAvailable => true;
    public bool IsNew => true;
    public BackgroundTheme BackgroundTheme => BackgroundTheme.Clouds;

    public UserControl CreateView(INavigationContext navigation)
        => new PaperPlaneGameView(navigation);
}
