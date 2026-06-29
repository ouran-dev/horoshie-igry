using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Core.Navigation;

/// <summary>Контекст навигации для экранов приложения.</summary>
public interface INavigationContext
{
    void NavigateToCatalog();
    void NavigateToSettings();
    void SetBackgroundTheme(BackgroundTheme theme);
}
