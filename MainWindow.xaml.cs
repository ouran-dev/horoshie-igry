using System.Windows;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Core.Updates;

namespace HoroshieIgry;

/// <summary>Главное окно-оболочка: навигация между каталогом и мини-играми.</summary>
public partial class MainWindow : Window
{
    private readonly NavigationService _navigation;

    public MainWindow()
    {
        InitializeComponent();

        _navigation = new NavigationService(ContentHost, App.Catalog);
        _navigation.CurrentTitleChanged += title => HeaderTitleText.Text = title;
        _navigation.BackgroundThemeChanged += theme => AppBackground.Theme = theme;
        _navigation.HomeButtonVisibilityChanged += visible =>
            HomeButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

        _navigation.NavigateToCatalog();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
#if !DEBUG
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        WindowState = WindowState.Maximized;
#endif
        await TryCheckUpdatesOnStartupAsync();
    }

    private static async Task TryCheckUpdatesOnStartupAsync()
    {
        var updates = AppUpdateService.Instance;
        if (!updates.CanCheckForUpdates || !UpdateSettings.IsConfigured || !UpdateSettings.SupportsAutoUpdate)
            return;

        try
        {
            var result = await updates.CheckAndDownloadAsync();
            if (result.Status != UpdateCheckStatus.Downloaded)
                return;

            var restart = MessageBox.Show(
                "Доступна новая версия. Перезапустить приложение сейчас?",
                "Обновление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (restart == MessageBoxResult.Yes)
                updates.ApplyDownloadedUpdateAndRestart();
        }
        catch
        {
            // Тихо игнорируем сбои сети при автопроверке.
        }
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        _navigation.NavigateToCatalog();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        _navigation.NavigateToSettings();
    }
}
