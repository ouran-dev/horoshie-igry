using HoroshieIgry.Core.Audio;

namespace HoroshieIgry.Games.PaperPlaneGame.Helpers;

/// <summary>Звуки для «Птичка».</summary>
public static class PaperPlaneSounds
{
    public static void PlayStar() => GameSoundEffects.PlayStar();
    public static void PlayCrash() => GameSoundEffects.PlayCrash();
    public static void PlayWind() => GameSoundEffects.PlayWind();
    public static void PlayVictory() => GameSoundEffects.PlayVictory();
}
