namespace HoroshieIgry.Core.Audio;

/// <summary>Общие звуковые эффекты для всех игр каталога.</summary>
public static class GameSoundEffects
{
    private static readonly Random Random = new();
    private static DateTime _lastPopAt = DateTime.MinValue;
    private static DateTime _lastStepAt = DateTime.MinValue;
    private static DateTime _lastDrawAt = DateTime.MinValue;

    private static readonly byte[] TickClip = ProceduralSoundPlayer.CreateTone(660, 80, 0.14);
    private static readonly byte[] GoClip = ProceduralSoundPlayer.CreateTone(880, 120, 0.2);
    private static readonly byte[] FlipClip = ProceduralSoundPlayer.CreateTone(580, 45, 0.1);
    private static readonly byte[] TapClip = ProceduralSoundPlayer.CreateTone(620, 42, 0.1);
    private static readonly byte[] GrabClip = ProceduralSoundPlayer.CreateTone(520, 55, 0.12);
    private static readonly byte[] SuccessClip = ProceduralSoundPlayer.CreateTone(740, 90, 0.18);
    private static readonly byte[] WrongClip = ProceduralSoundPlayer.CreateTone(210, 90, 0.1);
    private static readonly byte[] MismatchClip = ProceduralSoundPlayer.CreateTone(320, 100, 0.12);
    private static readonly byte[] ReturnClip = ProceduralSoundPlayer.CreateTone(280, 85, 0.1);
    private static readonly byte[] MatchClip = ProceduralSoundPlayer.CreateTone(784, 95, 0.17);
    private static readonly byte[] StepClip = ProceduralSoundPlayer.CreateTone(350, 30, 0.06);
    private static readonly byte[] DrawClip = ProceduralSoundPlayer.CreateTone(480, 38, 0.08);
    private static readonly byte[] LineCompleteClip = ProceduralSoundPlayer.CreateTone(620, 70, 0.14);
    private static readonly byte[] StarClip = ProceduralSoundPlayer.CreateTone(880, 55, 0.16);
    private static readonly byte[] CrashClip = ProceduralSoundPlayer.CreateTone(180, 140, 0.12);
    private static readonly byte[] WindClip = ProceduralSoundPlayer.CreateTone(320, 80, 0.06);
    private static readonly byte[] PlaceXClip = ProceduralSoundPlayer.CreateTone(540, 55, 0.13);
    private static readonly byte[] PlaceOClip = ProceduralSoundPlayer.CreateTone(420, 60, 0.13);
    private static readonly byte[] DefeatClip = ProceduralSoundPlayer.CreateTone(240, 120, 0.11);
    private static readonly byte[] DrawGameClip = ProceduralSoundPlayer.CreateTone(390, 110, 0.12);

    private static readonly byte[][] PopClips =
    [
        ProceduralSoundPlayer.CreateTone(420, 65, 0.2),
        ProceduralSoundPlayer.CreateTone(480, 65, 0.2),
        ProceduralSoundPlayer.CreateTone(540, 65, 0.2),
        ProceduralSoundPlayer.CreateTone(600, 65, 0.2)
    ];

    private static readonly byte[][] VictoryClips =
    [
        ProceduralSoundPlayer.CreateTone(523, 110, 0.18),
        ProceduralSoundPlayer.CreateTone(659, 110, 0.18),
        ProceduralSoundPlayer.CreateTone(784, 150, 0.2)
    ];

    public static void PlayTick() => ProceduralSoundPlayer.Play(TickClip);

    public static void PlayGo()
    {
        ProceduralSoundPlayer.Play(GoClip);
        _ = Task.Run(async () =>
        {
            await Task.Delay(90);
            ProceduralSoundPlayer.Play(ProceduralSoundPlayer.CreateTone(1047, 140, 0.18));
        });
    }

    public static void PlayFlip() => ProceduralSoundPlayer.Play(FlipClip);
    public static void PlayTap() => ProceduralSoundPlayer.Play(TapClip);
    public static void PlayGrab() => ProceduralSoundPlayer.Play(GrabClip);
    public static void PlaySuccess() => ProceduralSoundPlayer.Play(SuccessClip);
    public static void PlayWrong() => ProceduralSoundPlayer.Play(WrongClip);
    public static void PlayMismatch() => ProceduralSoundPlayer.Play(MismatchClip);
    public static void PlayReturn() => ProceduralSoundPlayer.Play(ReturnClip);
    public static void PlayLineComplete() => ProceduralSoundPlayer.Play(LineCompleteClip);
    public static void PlayStar() => ProceduralSoundPlayer.Play(StarClip);
    public static void PlayCrash() => ProceduralSoundPlayer.Play(CrashClip);
    public static void PlayWind() => ProceduralSoundPlayer.Play(WindClip);
    public static void PlayDefeat() => ProceduralSoundPlayer.Play(DefeatClip);
    public static void PlayDrawGame() => ProceduralSoundPlayer.Play(DrawGameClip);

    public static void PlayMatch()
    {
        ProceduralSoundPlayer.Play(MatchClip);
        _ = Task.Run(async () =>
        {
            await Task.Delay(70);
            ProceduralSoundPlayer.Play(ProceduralSoundPlayer.CreateTone(988, 80, 0.15));
        });
    }

    public static void PlayPlace(bool isX)
        => ProceduralSoundPlayer.Play(isX ? PlaceXClip : PlaceOClip);

    public static void PlayPop()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastPopAt).TotalMilliseconds < 45)
            return;

        _lastPopAt = now;
        ProceduralSoundPlayer.Play(PopClips[Random.Next(PopClips.Length)]);
    }

    public static void PlayStep()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastStepAt).TotalMilliseconds < 90)
            return;

        _lastStepAt = now;
        ProceduralSoundPlayer.Play(StepClip);
    }

    public static void PlayDraw()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastDrawAt).TotalMilliseconds < 55)
            return;

        _lastDrawAt = now;
        ProceduralSoundPlayer.Play(DrawClip);
    }

    public static void PlayVictory()
        => ProceduralSoundPlayer.PlaySequence(VictoryClips);
}
