using Velopack;
using Velopack.Sources;

namespace HoroshieIgry.Core.Updates;

public enum UpdateCheckStatus
{
    NotInstalled,
    NotConfigured,
    UpToDate,
    UpdateAvailable,
    Downloaded,
    Failed
}

public sealed class UpdateCheckResult
{
    public UpdateCheckStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public VelopackAsset? TargetRelease { get; init; }
}

/// <summary>Проверка и установка обновлений через GitHub Releases (Velopack).</summary>
public sealed class AppUpdateService
{
    public static AppUpdateService Instance { get; } = new();

    private UpdateManager? _manager;
    private VelopackAsset? _downloadedRelease;

    public bool CanCheckForUpdates { get; private set; }

    public string InstalledVersionDisplay =>
        _manager?.CurrentVersion?.ToString() ?? AppVersion.Display;

    public void Initialize()
    {
        if (!UpdateSettings.IsConfigured)
        {
            CanCheckForUpdates = false;
            return;
        }

        _manager = new UpdateManager(
            new GithubSource(UpdateSettings.GitHubRepoUrl, accessToken: null, prerelease: false),
            new UpdateOptions());

        CanCheckForUpdates = _manager.IsInstalled;
    }

    public async Task<UpdateCheckResult> CheckAndDownloadAsync(
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!UpdateSettings.IsConfigured)
        {
            return new UpdateCheckResult
            {
                Status = UpdateCheckStatus.NotConfigured,
                Message = "Укажите адрес GitHub-репозитория в UpdateSettings.GitHubRepoUrl."
            };
        }

        if (_manager is null || !_manager.IsInstalled)
        {
            return new UpdateCheckResult
            {
                Status = UpdateCheckStatus.NotInstalled,
                Message = "Обновления работают в версии, установленной через установщик."
            };
        }

        try
        {
            var updateInfo = await _manager.CheckForUpdatesAsync();
            if (updateInfo is null)
            {
                return new UpdateCheckResult
                {
                    Status = UpdateCheckStatus.UpToDate,
                    Message = "У вас уже установлена последняя версия."
                };
            }

            await _manager.DownloadUpdatesAsync(
                updateInfo,
                p => progress?.Report(p),
                cancellationToken);

            _downloadedRelease = updateInfo.TargetFullRelease;

            return new UpdateCheckResult
            {
                Status = UpdateCheckStatus.Downloaded,
                Message = $"Доступна версия {updateInfo.TargetFullRelease.Version}.",
                TargetRelease = updateInfo.TargetFullRelease
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult
            {
                Status = UpdateCheckStatus.Failed,
                Message = $"Не удалось проверить обновления: {ex.Message}"
            };
        }
    }

    public void ApplyDownloadedUpdateAndRestart()
    {
        if (_manager is null)
            throw new InvalidOperationException("Служба обновлений не инициализирована.");

        _manager.ApplyUpdatesAndRestart(_downloadedRelease);
    }
}
