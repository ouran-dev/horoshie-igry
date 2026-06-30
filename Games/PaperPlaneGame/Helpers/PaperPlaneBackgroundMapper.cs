using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.PaperPlaneGame.Helpers;

public static class PaperPlaneBackgroundMapper
{
    public static BackgroundTheme Map(string? backgroundId) => backgroundId?.ToLowerInvariant() switch
    {
        "meadow" => BackgroundTheme.Meadow,
        "sea" => BackgroundTheme.Sea,
        "forest" => BackgroundTheme.Forest,
        "mountains" => BackgroundTheme.Mountains,
        _ => BackgroundTheme.Clouds
    };
}
