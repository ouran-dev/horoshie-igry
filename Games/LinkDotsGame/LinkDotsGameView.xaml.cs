using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.LinkDotsGame.Helpers;
using HoroshieIgry.Games.LinkDotsGame.Models;

namespace HoroshieIgry.Games.LinkDotsGame;

public partial class LinkDotsGameView : UserControl, INotifyPropertyChanged
{
    private const string GameId = "link-dots";

    private readonly INavigationContext _navigation;
    private readonly DispatcherTimer _timer;
    private readonly Dictionary<int, int> _bestSecondsByLevel = new();

    private int _levelIndex;
    private int _completedCount;
    private int _elapsedSeconds;
    private bool _isLevelActive;
    private bool _isLevelCompleted;
    private bool _isAdvancing;
    private bool _allLevelsCompleted;
    private DateTime _levelStartedAt;
    private string _levelTitle = "";

    public string LevelDisplay => (_levelIndex + 1).ToString();

    public string LevelTitle => _levelTitle;

    public string CompletedDisplay => _completedCount.ToString();

    public string HintText => _allLevelsCompleted
        ? "Все уровни пройдены! Нажми «Заново», чтобы начать сначала"
        : "Нажми на цветную точку и веди пальцем по клеткам к паре того же цвета";

    public string BestTimeDisplay
    {
        get
        {
            if (_bestSecondsByLevel.TryGetValue(_levelIndex + 1, out var best))
                return FormatTime(best);

            return "—";
        }
    }

    public string ElapsedDisplay => FormatTime(_elapsedSeconds);

    public event PropertyChangedEventHandler? PropertyChanged;

    public LinkDotsGameView(INavigationContext navigation)
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
        LoadLevel(resetProgress: true);
    }

    private LinkDotsLevel GetCurrentLevel()
        => LinkDotsLevelFactory.CreateForLevel(_levelIndex + 1);

    private void LoadLevel(bool resetProgress)
    {
        if (resetProgress)
        {
            _levelIndex = 0;
            _completedCount = 0;
            _bestSecondsByLevel.Clear();
            _allLevelsCompleted = false;
        }

        _levelIndex = Math.Clamp(_levelIndex, 0, LinkDotsLevelFactory.MaxLevel - 1);
        _isLevelCompleted = false;
        _isAdvancing = false;

        var level = GetCurrentLevel();
        _levelTitle = level.Title;
        DotsBoard.LoadLevel(level);

        _elapsedSeconds = 0;
        _levelStartedAt = DateTime.UtcNow;
        _isLevelActive = true;
        StartTimer();

        ApplyRoundBackground();
        UpdateDisplays();
    }

    private void RestartLevel()
    {
        if (_isAdvancing) return;

        _allLevelsCompleted = false;
        _isLevelCompleted = false;
        _isAdvancing = false;
        DotsBoard.ResetLevel();

        _elapsedSeconds = 0;
        _levelStartedAt = DateTime.UtcNow;
        _isLevelActive = true;
        StartTimer();
        UpdateDisplays();
    }

    private async void DotsBoard_PuzzleCompleted(object? sender, EventArgs e)
    {
        if (_isLevelCompleted || _isAdvancing) return;

        _isLevelCompleted = true;
        _isAdvancing = true;
        _isLevelActive = false;
        StopTimer();

        var levelId = _levelIndex + 1;
        if (!_bestSecondsByLevel.TryGetValue(levelId, out var best) || _elapsedSeconds < best)
            _bestSecondsByLevel[levelId] = _elapsedSeconds;

        _completedCount++;
        UpdateDisplays();

        LinkDotsSounds.PlayVictory();
        await PlayPraiseSafeAsync();
        await Task.Delay(800);

        if (_levelIndex < LinkDotsLevelFactory.MaxLevel - 1)
        {
            _levelIndex++;
            LoadLevel(resetProgress: false);
            return;
        }

        _allLevelsCompleted = true;
        UpdateDisplays();
        _isAdvancing = false;
    }

    private async Task PlayPraiseSafeAsync()
    {
        try
        {
            await PraiseOverlay.PlayAsync();
        }
        catch
        {
            // ignore overlay cancellation
        }
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_allLevelsCompleted)
            LoadLevel(resetProgress: true);
        else
            RestartLevel();
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
        => _navigation.SetBackgroundTheme(GameBackgroundRotator.ForRound(GameId, _levelIndex + 1));

    private static string FormatTime(int seconds)
    {
        var minutes = seconds / 60;
        var secs = seconds % 60;
        return $"{minutes}:{secs:D2}";
    }

    private void UpdateDisplays()
    {
        OnPropertyChanged(nameof(LevelDisplay));
        OnPropertyChanged(nameof(LevelTitle));
        OnPropertyChanged(nameof(CompletedDisplay));
        OnPropertyChanged(nameof(BestTimeDisplay));
        OnPropertyChanged(nameof(ElapsedDisplay));
        OnPropertyChanged(nameof(HintText));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
