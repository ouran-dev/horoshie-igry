using HoroshieIgry.Core.Audio;

namespace HoroshieIgry.Games.BalloonPopGame.Helpers;

/// <summary>Мягкие звуки для «Лопни шарик».</summary>
public static class BalloonPopSounds
{
    public static void PlayPop() => GameSoundEffects.PlayPop();
    public static void PlayWrong() => GameSoundEffects.PlayWrong();
    public static void PlayVictory() => GameSoundEffects.PlayVictory();
}
