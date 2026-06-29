using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Core.Navigation;

/// <summary>Сервис переключения экранов внутри главного окна.</summary>
public interface INavigationService : INavigationContext
{
    event Action<string>? CurrentTitleChanged;
    event Action<BackgroundTheme>? BackgroundThemeChanged;
    event Action<bool>? HomeButtonVisibilityChanged;

    new void NavigateToCatalog();
    new void NavigateToSettings();
    void NavigateToGame(IGameModule game);
}
