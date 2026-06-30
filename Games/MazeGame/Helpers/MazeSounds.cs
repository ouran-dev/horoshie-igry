using HoroshieIgry.Core.Audio;

namespace HoroshieIgry.Games.MazeGame.Helpers;

public static class MazeSounds
{
    public static void PlayGrab() => GameSoundEffects.PlayGrab();
    public static void PlayStep() => GameSoundEffects.PlayStep();
    public static void PlayVictory() => GameSoundEffects.PlayVictory();
}
