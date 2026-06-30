using HoroshieIgry.Core.Audio;

namespace HoroshieIgry.Games.FindOddGame.Helpers;

public static class FindOddSounds
{
    public static void PlayTap() => GameSoundEffects.PlayTap();
    public static void PlayWrong() => GameSoundEffects.PlayWrong();
    public static void PlaySuccess() => GameSoundEffects.PlaySuccess();
    public static void PlayVictory() => GameSoundEffects.PlayVictory();
}
