using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HoroshieIgry.Games.BalloonPopGame.Helpers;
using HoroshieIgry.Games.BalloonPopGame.Models;

namespace HoroshieIgry.Games.BalloonPopGame.Controls;

public partial class BalloonPlayfieldControl : UserControl
{
    private readonly Dictionary<int, BalloonControl> _controls = new();
    private readonly DispatcherTimer _idleTimer;
    private DateTime _idleStart = DateTime.UtcNow;

    private BalloonLevelPlan? _plan;
    private int _poppedTargets;

    public event EventHandler? LevelCompleted;

    public BalloonPlayfieldControl()
    {
        InitializeComponent();
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);

        _idleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _idleTimer.Tick += IdleTimer_Tick;

        Unloaded += (_, _) => _idleTimer.Stop();
    }

    public void LoadLevel(BalloonLevelPlan plan)
    {
        _plan = plan;
        _poppedTargets = 0;
        _controls.Clear();
        FieldCanvas.Children.Clear();
        _idleTimer.Stop();

        Width = BalloonPopLevelGenerator.DesignWidth;
        Height = BalloonPopLevelGenerator.DesignHeight;

        foreach (var balloon in plan.Balloons.OrderBy(b => b.IsTarget ? 1 : 0))
        {
            var control = new BalloonControl();
            control.BalloonTapped += OnBalloonTapped;
            control.DataContext = balloon;

            var halfW = balloon.Size / 2;
            var halfH = balloon.Size / 2;
            const double pad = 6.0;
            var centerX = Math.Clamp(balloon.X, halfW + pad, BalloonPopLevelGenerator.DesignWidth - halfW - pad);
            var centerY = Math.Clamp(balloon.Y, halfH + pad, BalloonPopLevelGenerator.DesignHeight - halfH - pad);

            Canvas.SetLeft(control, centerX - halfW);
            Canvas.SetTop(control, centerY - halfH);
            Panel.SetZIndex(control, balloon.IsTarget ? 20 : 0);
            FieldCanvas.Children.Add(control);
            _controls[balloon.Id] = control;
        }

        _idleStart = DateTime.UtcNow;
        _idleTimer.Start();
    }

    public async Task FadeOutRemainingAsync()
    {
        foreach (var control in _controls.Values)
        {
            if (control.Balloon is { IsPopped: false })
                control.FadeOut();
        }

        await Task.Delay(300);
    }

    private void IdleTimer_Tick(object? sender, EventArgs e)
    {
        var seconds = (DateTime.UtcNow - _idleStart).TotalSeconds;
        foreach (var control in _controls.Values)
        {
            if (control.Balloon is { IsPopped: false })
                control.UpdateIdle(seconds);
        }
    }

    private void OnBalloonTapped(object? sender, BalloonModel balloon)
    {
        if (_plan is null || balloon.IsPopped) return;

        if (!balloon.IsTarget)
        {
            if (sender is BalloonControl control)
                control.PlayWrongBounce();

            BalloonPopSounds.PlayWrong();
            return;
        }

        balloon.IsPopped = true;
        _poppedTargets++;

        if (sender is BalloonControl popControl)
        {
            BalloonPopSounds.PlayPop();
            popControl.PlayPopVisual();
        }

        if (_poppedTargets >= _plan.TargetsToPop)
        {
            _idleTimer.Stop();
            LevelCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
