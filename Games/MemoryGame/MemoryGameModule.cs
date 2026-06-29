using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.MemoryGame;

/// <summary>Модуль игры «Память» в каталоге.</summary>
public sealed class MemoryGameModule : IGameModule
{
    public int CatalogOrder => 10;
    public string Id => "memory";
    public string Title => "Память";
    public string Description => "Найди пары одинаковых карточек. Поле растёт после каждой победы.";
    public string IconEmoji => "🧠";
    public bool IsAvailable => true;
    public bool IsNew => false;
    public BackgroundTheme BackgroundTheme => BackgroundTheme.Meadow;

    public UserControl CreateView(INavigationContext navigation) => new MemoryGameView(navigation);
}
