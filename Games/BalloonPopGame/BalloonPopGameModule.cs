using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.BalloonPopGame;

public sealed class BalloonPopGameModule : IGameModule
{
    public int CatalogOrder => 70;
    public string Id => "balloon-pop";
    public string Title => "Лопни шарики";
    public string Description => "Лопай только нужные воздушные шарики, тренируй внимание и реакцию!";
    public string IconEmoji => "🎈";
    public bool IsAvailable => true;
    public bool IsNew => true;
    public BackgroundTheme BackgroundTheme => BackgroundTheme.Clouds;

    public UserControl CreateView(INavigationContext navigation)
        => new BalloonPopGameView(navigation);
}
