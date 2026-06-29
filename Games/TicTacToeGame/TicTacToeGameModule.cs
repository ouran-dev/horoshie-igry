using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.TicTacToeGame;

public sealed class TicTacToeGameModule : IGameModule
{
    public int CatalogOrder => 40;
    public string Id => "tic-tac-toe";
    public string Title => "Крестики-нолики";
    public string Description => "Сыграй с компьютером на поле 3×3.";
    public string IconEmoji => "⭕";
    public bool IsAvailable => true;
    public bool IsNew => false;
    public BackgroundTheme BackgroundTheme => BackgroundTheme.Clouds;

    public UserControl CreateView(INavigationContext navigation)
        => new TicTacToeGameView(navigation);
}
