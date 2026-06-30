using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using HoroshieIgry.Controls.Kenney;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.PaperPlaneGame.Helpers;
using HoroshieIgry.Games.PaperPlaneGame.Models;

namespace HoroshieIgry.Games.PaperPlaneGame.Controls;

public partial class PaperPlanePlayfieldControl : UserControl
{
    private enum PlayState { Tutorial, Flying, Crashing, Landing, Finished }

    private readonly DispatcherTimer _tickTimer;
    private readonly List<StarVisual> _stars = new();
    private readonly List<ObstacleVisual> _obstacles = new();
    private readonly List<WindVisual> _windGusts = new();
    private readonly List<ParallaxCloud> _parallaxFar = new();
    private readonly List<ParallaxCloud> _parallaxNear = new();
    private readonly Queue<Point> _trail = new();

    private PaperPlaneLevelDefinition? _level;
    private PlayState _state = PlayState.Tutorial;
    private double _scroll;
    private double _planeY;
    private double _planeVelY;
    private double _planeAngle;
    private double _crashSpin;
    private int _starsCollected;
    private int _totalStars;
    private bool _isPressed;
    private bool _tutorialDismissed;
    private DateTime _lastTick = DateTime.UtcNow;
    private DateTime _startedAt = DateTime.UtcNow;
    private double _elapsedSeconds;

    public event EventHandler? LevelCompleted;
    public event EventHandler? LevelCrashed;
    public event EventHandler<PaperPlaneProgressEventArgs>? ProgressChanged;
    public event EventHandler? TutorialDismissed;

    public bool IsTutorialVisible => _state == PlayState.Tutorial && !_tutorialDismissed;
    public double ElapsedSeconds => _elapsedSeconds;

    private double _viewportWidth = PaperPlanePhysics.DesignWidth;

    public PaperPlanePlayfieldControl()
    {
        InitializeComponent();
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);

        Height = PaperPlanePhysics.DesignHeight;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Center;
        ApplyCanvasLayout();

        SizeChanged += OnPlayfieldSizeChanged;

        _tickTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _tickTimer.Tick += TickTimer_Tick;

        PreviewMouseLeftButtonDown += OnPress;
        PreviewMouseLeftButtonUp += OnRelease;
        PreviewTouchDown += OnTouchPress;
        PreviewTouchUp += OnTouchRelease;
        LostMouseCapture += (_, _) => _isPressed = false;

        Unloaded += (_, _) => _tickTimer.Stop();
    }

    public void LoadLevel(PaperPlaneLevelDefinition level)
    {
        _level = level;
        _scroll = 0;
        _planeY = level.PlaneStartY;
        _planeVelY = 0;
        _planeAngle = 0;
        _crashSpin = 0;
        _starsCollected = 0;
        _totalStars = level.Stars.Count;
        _isPressed = false;
        _tutorialDismissed = !level.ShowTutorial;
        _state = level.ShowTutorial ? PlayState.Tutorial : PlayState.Flying;
        _startedAt = DateTime.UtcNow;
        _elapsedSeconds = 0;
        _lastTick = DateTime.UtcNow;
        _trail.Clear();

        BuildWorld();
        UpdatePlaneVisual();
        UpdateProgress();
        FinishFlag.Visibility = Visibility.Collapsed;

        _tickTimer.Stop();
        if (_state == PlayState.Flying)
            _tickTimer.Start();
    }

    public void StopGame() => _tickTimer.Stop();

    private void OnPlayfieldSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width < 1 || e.NewSize.Height < 1) return;
        _viewportWidth = e.NewSize.Width;
        Width = e.NewSize.Width;
        ApplyCanvasLayout();
    }

    private double ViewportWidth => Math.Max(320, _viewportWidth);

    private void ApplyCanvasLayout()
    {
        var w = ViewportWidth;
        var h = PaperPlanePhysics.DesignHeight;
        var sky = PaperPlanePhysics.SkyHeight;
        var groundH = h - sky;

        RootCanvas.Width = w;
        RootCanvas.Height = h;

        SkyRect.Width = w;
        SkyRect.Height = sky;
        GroundRect.Width = w;
        GroundRect.Height = groundH;
        Canvas.SetTop(GroundRect, sky);
        Canvas.SetLeft(PlaneHost, PaperPlanePhysics.PlaneScreenX);
    }

    private void BuildWorld()
    {
        WorldCanvas.Children.Clear();
        _stars.Clear();
        _obstacles.Clear();
        _windGusts.Clear();

        if (_level is null) return;

        foreach (var star in _level.Stars)
        {
            var starVisual = new KenneyImage
            {
                Width = 44,
                Height = 44,
                AssetPath = KenneyPaths.Star,
                IsHitTestVisible = false
            };
            WorldCanvas.Children.Add(starVisual);
            _stars.Add(new StarVisual(star.X, star.Y, starVisual, true));
        }

        foreach (var obs in _level.Obstacles)
        {
            UIElement visual = CreateObstacleVisual(obs.Type);
            WorldCanvas.Children.Add(visual);
            _obstacles.Add(new ObstacleVisual(obs, visual));
        }

        foreach (var gust in _level.WindGusts)
        {
            var rect = new Rectangle
            {
                RadiusX = 12,
                RadiusY = 12,
                Fill = new SolidColorBrush(Color.FromArgb(48, 120, 200, 255)),
                Stroke = new SolidColorBrush(Color.FromArgb(90, 80, 160, 230)),
                StrokeThickness = 2,
                Width = gust.Width,
                Height = gust.Height
            };
            var arrow = new TextBlock
            {
                Text = gust.Direction.Equals("up", StringComparison.OrdinalIgnoreCase) ? "⬆️" : "⬇️",
                FontSize = 28,
                Opacity = 0.65,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var host = new Canvas { Width = gust.Width, Height = gust.Height };
            host.Children.Add(rect);
            Canvas.SetLeft(arrow, gust.Width / 2 - 14);
            Canvas.SetTop(arrow, gust.Height / 2 - 16);
            host.Children.Add(arrow);
            WorldCanvas.Children.Add(host);
            _windGusts.Add(new WindVisual(gust, host));
        }

        BuildParallax();
    }

    private void BuildParallax()
    {
        ParallaxFarCanvas.Children.Clear();
        ParallaxNearCanvas.Children.Clear();
        _parallaxFar.Clear();
        _parallaxNear.Clear();

        if (_level is null) return;

        var span = _level.FinishX + 500;
        for (var x = -120.0; x < span; x += 260)
        {
            var yFar = 35 + (x % 110);
            var yNear = 95 + (x % 150);
            AddParallaxCloud(x, yFar, 0.2, 0.85, _parallaxFar, ParallaxFarCanvas);
            AddParallaxCloud(x + 130, yNear, 0.42, 1.0, _parallaxNear, ParallaxNearCanvas);
        }
    }

    private static void AddParallaxCloud(
        double worldX,
        double worldY,
        double factor,
        double scale,
        List<ParallaxCloud> list,
        Canvas canvas)
    {
        var cloud = new Ellipse
        {
            Width = 88 * scale,
            Height = 36 * scale,
            Fill = Brushes.White,
            Opacity = factor < 0.3 ? 0.14 : 0.22
        };
        canvas.Children.Add(cloud);
        list.Add(new ParallaxCloud(worldX, worldY, factor, cloud));
    }

    private static UIElement CreateObstacleVisual(string type)
    {
        var normalized = type.ToLowerInvariant();
        var emoji = normalized switch
        {
            "raincloud" => "🌧️",
            _ => "☁️"
        };

        var icon = new TextBlock
        {
            Text = emoji,
            FontFamily = new FontFamily("Segoe UI Emoji"),
            FontSize = normalized == "raincloud" ? 46 : 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        return new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(210, 255, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(140, 100, 150, 210)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(6, 4, 6, 4),
            Child = icon
        };
    }

    private void OnPress(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null) return;
        BeginPress();
        e.Handled = true;
    }

    private void OnRelease(object sender, MouseButtonEventArgs e)
    {
        _isPressed = false;
        e.Handled = true;
    }

    private void OnTouchPress(object? sender, TouchEventArgs e)
    {
        BeginPress();
        e.Handled = true;
    }

    private void OnTouchRelease(object? sender, TouchEventArgs e)
    {
        _isPressed = false;
        e.Handled = true;
    }

    private void BeginPress()
    {
        if (_state == PlayState.Tutorial && !_tutorialDismissed)
        {
            _tutorialDismissed = true;
            _state = PlayState.Flying;
            _tickTimer.Start();
            TutorialDismissed?.Invoke(this, EventArgs.Empty);
            _isPressed = true;
            return;
        }

        if (_state == PlayState.Flying)
        {
            _isPressed = true;
            PaperPlaneSounds.PlayWind();
        }
    }

    private void TickTimer_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        var dt = Math.Clamp((now - _lastTick).TotalSeconds, 0, 0.05);
        _lastTick = now;

        if (_state is PlayState.Flying or PlayState.Landing)
            _elapsedSeconds = (now - _startedAt).TotalSeconds;

        switch (_state)
        {
            case PlayState.Flying:
                StepFlying(dt);
                break;
            case PlayState.Crashing:
                StepCrashing(dt);
                break;
            case PlayState.Landing:
                StepLanding(dt);
                break;
        }

        RenderWorld(now);
        UpdatePlaneVisual();
        UpdateProgress();
    }

    private void StepFlying(double dt)
    {
        if (_level is null) return;

        _scroll += _level.ScrollSpeed * GetScrollSpeedMultiplier() * dt;

        if (_isPressed)
            _planeVelY -= PaperPlanePhysics.Lift * dt;
        else
            _planeVelY += PaperPlanePhysics.Gravity * dt;

        var targetVel = _isPressed ? -PaperPlanePhysics.MaxVelY * 0.9 : PaperPlanePhysics.MaxVelY * 0.5;
        _planeVelY += (targetVel - _planeVelY) * Math.Min(1, PaperPlanePhysics.ControlSnap * dt);

        _planeVelY = Math.Clamp(_planeVelY, -PaperPlanePhysics.MaxVelY, PaperPlanePhysics.MaxVelY);
        _planeY += _planeVelY * dt;
        _planeY = Math.Clamp(_planeY, PaperPlanePhysics.MarginY, PaperPlanePhysics.GroundY - 30);

        ApplyWind(dt);
        CollectStars();
        if (CheckCollision(_elapsedSeconds))
        {
            BeginCrash();
            return;
        }

        if (_scroll + PaperPlanePhysics.PlaneScreenX >= _level.FinishX)
            BeginLanding();
    }

    private double GetScrollSpeedMultiplier()
    {
        if (_level is null) return 1;

        var total = _level.FinishX - PaperPlanePhysics.PlaneScreenX;
        if (total <= 0) return 1;

        var t = Math.Clamp(_scroll / total, 0, 1);
        var ease = t * t;
        return 1 + ease * (0.3 + _level.Id * 0.08);
    }

    private void ApplyWind(double dt)
    {
        var planeBox = GetPlaneBounds();
        foreach (var gust in _windGusts)
        {
            var rect = gust.GetScreenRect(_scroll, _level!);
            if (!rect.IntersectsWith(planeBox)) continue;

            var sign = gust.Def.Direction.Equals("up", StringComparison.OrdinalIgnoreCase) ? -1 : 1;
            _planeVelY += sign * gust.Def.Strength * dt;
        }
    }

    private void CollectStars()
    {
        var planeCenter = GetPlaneCenter();
        const double collectRadius = 38;

        foreach (var star in _stars)
        {
            if (!star.IsActive) continue;
            var center = star.GetScreenCenter(_scroll);
            var dx = planeCenter.X - center.X;
            var dy = planeCenter.Y - center.Y;
            if (dx * dx + dy * dy > collectRadius * collectRadius) continue;

            star.IsActive = false;
            star.Element.Visibility = Visibility.Collapsed;
            _starsCollected++;
            PaperPlaneSounds.PlayStar();
        }
    }

    private bool CheckCollision(double timeSec)
    {
        var planeCenter = GetPlaneCenter();
        var planeR = PaperPlanePhysics.PlaneRadius;

        foreach (var obs in _obstacles)
        {
            if (!obs.Def.Solid) continue;

            var center = obs.GetCenter(_scroll, timeSec);
            var hitR = obs.GetHitRadius();
            var dx = planeCenter.X - center.X;
            var dy = planeCenter.Y - center.Y;
            var minDist = planeR + hitR;
            if (dx * dx + dy * dy < minDist * minDist)
                return true;
        }

        return false;
    }

    private Point GetPlaneCenter()
        => new(PaperPlanePhysics.PlaneScreenX + 26, _planeY + 26);

    private Rect GetPlaneBounds()
    {
        var c = GetPlaneCenter();
        var r = PaperPlanePhysics.PlaneRadius;
        return new Rect(c.X - r, c.Y - r, r * 2, r * 2);
    }

    private void BeginCrash()
    {
        _state = PlayState.Crashing;
        _isPressed = false;
        PaperPlaneSounds.PlayCrash();
        LevelCrashed?.Invoke(this, EventArgs.Empty);
    }

    private void StepCrashing(double dt)
    {
        _crashSpin += 420 * dt;
        _planeVelY += PaperPlanePhysics.Gravity * 1.2 * dt;
        _planeY += _planeVelY * dt;
        _planeAngle = _crashSpin;

        if (_planeY >= PaperPlanePhysics.GroundY - 10)
        {
            _planeY = PaperPlanePhysics.GroundY - 10;
            _tickTimer.Stop();
        }
    }

    private void BeginLanding()
    {
        _state = PlayState.Landing;
        _isPressed = false;
        FinishFlag.Visibility = Visibility.Visible;
    }

    private void StepLanding(double dt)
    {
        _planeVelY += PaperPlanePhysics.Gravity * 0.6 * dt;
        _planeY += _planeVelY * dt;
        _planeAngle = Math.Max(-8, Math.Min(8, _planeVelY * 0.06));

        if (_planeY >= PaperPlanePhysics.GroundY - 8)
        {
            _planeY = PaperPlanePhysics.GroundY - 8;
            _planeVelY = 0;
            _state = PlayState.Finished;
            _tickTimer.Stop();
            LevelCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void RenderWorld(DateTime now)
    {
        var time = (now - _startedAt).TotalSeconds;

        UpdateParallax(time);

        foreach (var star in _stars)
        {
            if (!star.IsActive) continue;
            var pos = star.GetScreenPos(_scroll);
            Canvas.SetLeft(star.Element, pos.X - 22);
            Canvas.SetTop(star.Element, pos.Y - 22);
            if (star.Element is FrameworkElement fe)
            {
                fe.RenderTransformOrigin = new Point(0.5, 0.5);
                fe.RenderTransform = new RotateTransform(time * 40);
            }
        }

        foreach (var obs in _obstacles)
        {
            var pos = obs.GetScreenPos(_scroll, time);
            Canvas.SetLeft(obs.Element, pos.X);
            Canvas.SetTop(obs.Element, pos.Y);
        }

        foreach (var gust in _windGusts)
        {
            var pos = gust.GetScreenPos(_scroll);
            Canvas.SetLeft(gust.Element, pos.X);
            Canvas.SetTop(gust.Element, pos.Y);
        }

        if (_level is not null)
        {
            var finishScreenX = _level.FinishX - _scroll;
            if (finishScreenX > ViewportWidth - 80)
                finishScreenX = ViewportWidth - 80;
            Canvas.SetLeft(FinishFlag, finishScreenX);
        }

        UpdateTrail();
    }

    private void UpdateParallax(double time)
    {
        UpdateParallaxLayer(_parallaxFar, time, 5);
        UpdateParallaxLayer(_parallaxNear, time, 9);
    }

    private void UpdateParallaxLayer(IReadOnlyList<ParallaxCloud> layer, double time, double driftAmp)
    {
        foreach (var cloud in layer)
        {
            var drift = Math.Sin(time * 0.55 + cloud.WorldX * 0.008) * driftAmp;
            var screenX = cloud.WorldX - _scroll * cloud.Factor;
            Canvas.SetLeft(cloud.Element, screenX);
            Canvas.SetTop(cloud.Element, cloud.WorldY + drift);
        }
    }

    private void UpdateTrail()
    {
        var center = new Point(PaperPlanePhysics.PlaneScreenX + 26, _planeY + 26);
        _trail.Enqueue(center);
        while (_trail.Count > 14)
            _trail.Dequeue();

        TrailLine.Points = new PointCollection(_trail);
    }

    private void UpdatePlaneVisual()
    {
        Canvas.SetTop(PlaneHost, _planeY);
        if (_state == PlayState.Crashing)
            PlaneTilt.Angle = _planeAngle;
        else if (_state == PlayState.Flying || _state == PlayState.Landing)
        {
            var target = Math.Clamp(-_planeVelY * 0.1, -28, 28);
            _planeAngle += (target - _planeAngle) * 0.32;
            var wobble = Math.Sin(_elapsedSeconds * 6) * 2;
            PlaneTilt.Angle = _planeAngle + wobble;
        }
    }

    private void UpdateProgress()
    {
        if (_level is null) return;
        var travel = _level.FinishX - PaperPlanePhysics.PlaneScreenX;
        var dist = travel <= 0 ? 100 : (int)Math.Clamp(_scroll / travel * 100, 0, 100);
        ProgressChanged?.Invoke(this, new PaperPlaneProgressEventArgs
        {
            StarsCollected = _starsCollected,
            TotalStars = _totalStars,
            DistancePercent = dist
        });
    }

    private sealed class ParallaxCloud(double worldX, double worldY, double factor, UIElement element)
    {
        public double WorldX { get; } = worldX;
        public double WorldY { get; } = worldY;
        public double Factor { get; } = factor;
        public UIElement Element { get; } = element;
    }

    private sealed class StarVisual(double worldX, double worldY, FrameworkElement element, bool isActive)
    {
        public FrameworkElement Element { get; } = element;
        public bool IsActive { get; set; } = isActive;

        public Point GetScreenPos(double scroll) =>
            new(worldX - scroll, worldY);

        public Point GetScreenCenter(double scroll) =>
            new(worldX - scroll, worldY);
    }

    private sealed class ObstacleVisual
    {
        public PaperPlaneObstacleDef Def { get; }
        public UIElement Element { get; }

        public ObstacleVisual(PaperPlaneObstacleDef definition, UIElement element)
        {
            Def = definition;
            Element = element;
        }

        public Point GetScreenPos(double scroll, double time)
        {
            var y = Def.Y;
            if (Def.Motion is not null && Def.Motion.Axis.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                var phase = time * Math.PI * 2 / Math.Max(0.5, Def.Motion.PeriodSec);
                y += Math.Sin(phase) * Def.Motion.Amplitude;
            }

            return new Point(Def.X - scroll, y);
        }

        public Point GetCenter(double scroll, double timeSec)
        {
            var pos = GetScreenPos(scroll, timeSec);
            var size = GetVisualSize();
            return new Point(pos.X + size / 2, pos.Y + size / 2);
        }

        public double GetHitRadius() => Def.Type.ToLowerInvariant() switch
        {
            "raincloud" => 19,
            _ => 20
        };

        private double GetVisualSize() => Def.Type.ToLowerInvariant() switch
        {
            "raincloud" => 64,
            _ => 62
        };
    }

    private sealed class WindVisual
    {
        public PaperPlaneWindGustDef Def { get; }
        public Canvas Element { get; }

        public WindVisual(PaperPlaneWindGustDef definition, Canvas element)
        {
            Def = definition;
            Element = element;
        }

        public Point GetScreenPos(double scroll) => new(Def.X - scroll, Def.Y);

        public Rect GetScreenRect(double scroll, PaperPlaneLevelDefinition _)
        {
            var pos = GetScreenPos(scroll);
            return new Rect(pos.X, pos.Y, Def.Width, Def.Height);
        }
    }
}

public sealed class PaperPlaneProgressEventArgs : EventArgs
{
    public int StarsCollected { get; init; }
    public int TotalStars { get; init; }
    public int DistancePercent { get; init; }
}
