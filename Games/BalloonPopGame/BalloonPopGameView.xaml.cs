using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.BalloonPopGame.Helpers;
using HoroshieIgry.Games.BalloonPopGame.Models;

namespace HoroshieIgry.Games.BalloonPopGame;

public partial class BalloonPopGameView : UserControl, INotifyPropertyChanged
{
    private readonly INavigationContext _navigation;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

    private int _level = 1;
    private int _elapsedSeconds;
    private bool _isLevelActive;
    private bool _isAdvancing;
    private DateTime _levelStartedAt;
    private string _taskPrefix = "";
    private IReadOnlyList<BalloonTargetHint> _targetHints = [];

    public string LevelDisplay => _level.ToString();
    public string ElapsedDisplay => FormatTime(_elapsedSeconds);
    public string TaskPrefix => _taskPrefix;
    public IReadOnlyList<BalloonTargetHint> TargetHints => _targetHints;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BalloonPopGameView(INavigationContext navigation)
    {
        _navigation = navigation;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        InitializeComponent();
        DataContext = this;
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);

        Loaded += OnFirstLoaded;
        Unloaded += (_, _) => StopTimer();
    }

    private void OnFirstLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnFirstLoaded;
        _ = StartLevelAsync(resetProgress: true);
    }

    private async Task StartLevelAsync(bool resetProgress = false)
    {
        if (resetProgress)
            _level = 1;

        _isAdvancing = false;
        var plan = BalloonPopLevelGenerator.Create(_level, _random);
        _taskPrefix = plan.TaskPrefix;
        _targetHints = plan.TargetGroups.Select(BalloonTargetHint.Create).ToList();

        Playfield.LoadLevel(plan);
        ApplyRoundBackground();

        _elapsedSeconds = 0;
        _levelStartedAt = DateTime.UtcNow;
        _isLevelActive = true;
        StartTimer();
        UpdateDisplays();

        await Task.CompletedTask;
    }

    private async void Playfield_LevelCompleted(object? sender, EventArgs e)
    {
        if (_isAdvancing) return;

        _isAdvancing = true;
        _isLevelActive = false;
        StopTimer();

        await Playfield.FadeOutRemainingAsync();
        BalloonPopSounds.PlayVictory();
        await VictoryOverlay.PlayVictoryAsync();

        _level++;
        await StartLevelAsync();
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isAdvancing) return;
        _ = StartLevelAsync(resetProgress: false);
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        StopTimer();
        _navigation.NavigateToCatalog();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_isLevelActive) return;

        _elapsedSeconds = Math.Max(0, (int)(DateTime.UtcNow - _levelStartedAt).TotalSeconds);
        OnPropertyChanged(nameof(ElapsedDisplay));
    }

    private void StartTimer()
    {
        _timer.Stop();
        _timer.Start();
    }

    private void StopTimer() => _timer.Stop();

    private void ApplyRoundBackground()
        => _navigation.SetBackgroundTheme(BackgroundTheme.Clouds);

    private static string FormatTime(int seconds)
    {
        var minutes = seconds / 60;
        var secs = seconds % 60;
        return $"{minutes:00}:{secs:D2}";
    }

    private void UpdateDisplays()
    {
        OnPropertyChanged(nameof(LevelDisplay));
        OnPropertyChanged(nameof(ElapsedDisplay));
        OnPropertyChanged(nameof(TaskPrefix));
        OnPropertyChanged(nameof(TargetHints));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
