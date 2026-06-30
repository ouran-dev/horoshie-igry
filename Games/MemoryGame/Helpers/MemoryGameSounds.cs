using HoroshieIgry.Core.Audio;

namespace HoroshieIgry.Games.MemoryGame.Helpers;

/// <summary>Звуки фазы запоминания и игрового процесса.</summary>
public static class MemoryGameSounds
{
    public static void PlayTick() => GameSoundEffects.PlayTick();
    public static void PlayGo() => GameSoundEffects.PlayGo();
    public static void PlayFlip() => GameSoundEffects.PlayFlip();
    public static void PlayMatch() => GameSoundEffects.PlayMatch();
    public static void PlayMismatch() => GameSoundEffects.PlayMismatch();
    public static void PlayVictory() => GameSoundEffects.PlayVictory();
}
