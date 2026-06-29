using System.Windows.Controls;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Views;

namespace HoroshieIgry.Core.Navigation;

/// <summary>Управляет содержимым главного окна без перезапуска приложения.</summary>
public sealed class NavigationService : INavigationService
{
    private readonly ContentControl _contentHost;
    private readonly GameCatalog _catalog;

    public NavigationService(ContentControl contentHost, GameCatalog catalog)
    {
        _contentHost = contentHost;
        _catalog = catalog;
    }

    public event Action<string>? CurrentTitleChanged;
    public event Action<BackgroundTheme>? BackgroundThemeChanged;
    public event Action<bool>? HomeButtonVisibilityChanged;

    public void NavigateToCatalog()
    {
        _contentHost.Content = new MainMenuView(this, _catalog);
        CurrentTitleChanged?.Invoke("Хорошие игры");
        BackgroundThemeChanged?.Invoke(BackgroundTheme.Catalog);
        HomeButtonVisibilityChanged?.Invoke(false);
    }

    public void NavigateToSettings()
    {
        _contentHost.Content = new SettingsView(this);
        CurrentTitleChanged?.Invoke("Настройки");
        BackgroundThemeChanged?.Invoke(BackgroundTheme.Catalog);
        HomeButtonVisibilityChanged?.Invoke(true);
    }

    public void NavigateToGame(IGameModule game)
    {
        _contentHost.Content = game.CreateView(this);
        CurrentTitleChanged?.Invoke(game.Title);
        BackgroundThemeChanged?.Invoke(game.BackgroundTheme);
        HomeButtonVisibilityChanged?.Invoke(true);
    }

    public void SetBackgroundTheme(BackgroundTheme theme)
        => BackgroundThemeChanged?.Invoke(theme);
}
