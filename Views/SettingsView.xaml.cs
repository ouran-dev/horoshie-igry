using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.Updates;

namespace HoroshieIgry.Views;

public partial class SettingsView : UserControl, INotifyPropertyChanged
{
    private readonly INavigationContext _navigation;
    private readonly AppUpdateService _updates = AppUpdateService.Instance;
    private string _versionDisplay = AppVersion.Display;
    private string _updateStatus = "Проверка обновлений при запуске установленной версии.";
    private bool _isCheckingUpdates;
    private bool _updateReady;

    public string VersionDisplay
    {
        get => _versionDisplay;
        private set { _versionDisplay = value; OnPropertyChanged(); }
    }

    public string UpdateStatus
    {
        get => _updateStatus;
        private set { _updateStatus = value; OnPropertyChanged(); }
    }

    public bool CanCheckForUpdates =>
        UpdateSettings.SupportsAutoUpdate && _updates.CanCheckForUpdates && UpdateSettings.IsConfigured;

    public bool IsCheckingUpdates
    {
        get => _isCheckingUpdates;
        private set
        {
            _isCheckingUpdates = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanCheckForUpdates));
            OnPropertyChanged(nameof(CanRestartForUpdate));
        }
    }

    public bool CanRestartForUpdate => _updateReady && !IsCheckingUpdates;

    public event PropertyChangedEventHandler? PropertyChanged;

    public SettingsView(INavigationContext navigation)
    {
        InitializeComponent();
        _navigation = navigation;
        DataContext = this;

        RefreshVersionDisplay();
        RefreshUpdateHint();
    }

    private void RefreshVersionDisplay()
    {
        VersionDisplay = _updates.CanCheckForUpdates
            ? _updates.InstalledVersionDisplay
            : AppVersion.Display;
    }

    private void RefreshUpdateHint()
    {
        if (!UpdateSettings.SupportsAutoUpdate)
        {
            UpdateStatus = "Офлайн-версия: автообновление отключено. Для обновлений установите версию Setup.exe с GitHub.";
            return;
        }

        if (!UpdateSettings.IsConfigured)
        {
            UpdateStatus = "Обновления не настроены: укажите GitHub-репозиторий в UpdateSettings.";
            return;
        }

        if (!_updates.CanCheckForUpdates)
        {
            UpdateStatus = "Сейчас запущена версия для разработки. Обновления работают после установки через Setup.exe.";
            return;
        }

        UpdateStatus = "Нажми «Проверить обновления», чтобы скачать новую версию.";
    }

    private async void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsCheckingUpdates || !CanCheckForUpdates) return;

        IsCheckingUpdates = true;
        _updateReady = false;
        OnPropertyChanged(nameof(CanRestartForUpdate));
        UpdateStatus = "Проверяем обновления…";

        try
        {
            var result = await _updates.CheckAndDownloadAsync();
            UpdateStatus = result.Message;
            _updateReady = result.Status == UpdateCheckStatus.Downloaded;

            if (_updateReady)
            {
                var restart = await UpdateDialog.ShowAsync(
                    "Обновление готово",
                    "Новая версия скачана. Перезапустить приложение сейчас?",
                    "Перезапустить",
                    "Позже");

                if (restart)
                    _updates.ApplyDownloadedUpdateAndRestart();
            }
        }
        catch (Exception ex)
        {
            UpdateStatus = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsCheckingUpdates = false;
            OnPropertyChanged(nameof(CanRestartForUpdate));
        }
    }

    private void RestartForUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CanRestartForUpdate) return;
        _updates.ApplyDownloadedUpdateAndRestart();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _navigation.NavigateToCatalog();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
