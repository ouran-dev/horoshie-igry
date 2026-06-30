using HoroshieIgry.Core.Audio;

namespace HoroshieIgry.Games.SortingGame.Helpers;

public static class SortingSounds
{
    public static void PlayGrab() => GameSoundEffects.PlayGrab();
    public static void PlaySuccess() => GameSoundEffects.PlaySuccess();
    public static void PlayReturn() => GameSoundEffects.PlayReturn();
    public static void PlayVictory() => GameSoundEffects.PlayVictory();
}
