namespace HoroshieIgry.Core.Updates;

/// <summary>
/// Настройки канала обновлений Velopack + GitHub Releases.
/// Офлайн-сборка (OFFLINE_DISTRIBUTION) отключает автообновление.
/// </summary>
public static class UpdateSettings
{
    /// <summary>Должен совпадать с <c>-u</c> в команде <c>vpk pack</c>.</summary>
    public const string PackId = "HoroshieIgry";

    /// <summary>URL репозитория GitHub, куда публикуются релизы.</summary>
    public const string GitHubRepoUrl = "https://github.com/ouran-dev/horoshie-igry";

    public static bool SupportsAutoUpdate =>
#if OFFLINE_DISTRIBUTION
        false;
#else
        true;
#endif

    public static bool IsConfigured =>
        SupportsAutoUpdate && !GitHubRepoUrl.Contains("YOUR_USER", StringComparison.OrdinalIgnoreCase);
}
