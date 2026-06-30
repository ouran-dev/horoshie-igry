using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.LinkDotsGame;

public sealed class LinkDotsGameModule : IGameModule
{
    public int CatalogOrder => 60;
    public string Id => "link-dots";
    public string Title => "Соедини точки";
    public string Description => "Проведи линии между точками одного цвета. Линии разных цветов не должны пересекаться.";
    public string IconEmoji => "🎨";
    public bool IsAvailable => true;
    public bool IsNew => false;
    public BackgroundTheme BackgroundTheme => BackgroundTheme.Clouds;

    public UserControl CreateView(INavigationContext navigation)
        => new LinkDotsGameView(navigation);
}
