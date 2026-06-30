using HoroshieIgry.Core.Audio;

namespace HoroshieIgry.Games.TicTacToeGame.Helpers;

public static class TicTacToeSounds
{
    public static void PlayPlace(bool isX) => GameSoundEffects.PlayPlace(isX);
    public static void PlayVictory() => GameSoundEffects.PlayVictory();
    public static void PlayDefeat() => GameSoundEffects.PlayDefeat();
    public static void PlayDrawGame() => GameSoundEffects.PlayDrawGame();
}
