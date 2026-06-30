using HoroshieIgry.Core.Audio;

namespace HoroshieIgry.Games.LinkDotsGame.Helpers;

public static class LinkDotsSounds
{
    public static void PlayGrab() => GameSoundEffects.PlayGrab();
    public static void PlayDraw() => GameSoundEffects.PlayDraw();
    public static void PlayLineComplete() => GameSoundEffects.PlayLineComplete();
    public static void PlayVictory() => GameSoundEffects.PlayVictory();
}
