using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HoroshieIgry.Core.Mazes;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.MazeGame;

public partial class MazeGameView : UserControl, INotifyPropertyChanged
{
    private const string GameId = "maze";

    private readonly INavigationContext _navigation;
    private readonly MazeLibrary _library;
    private readonly DispatcherTimer _timer;
    private readonly Dictionary<int, int> _bestSecondsByLevel = new();

    private int _levelIndex;
    private int _completedCount;
    private int _elapsedSeconds;
    private bool _isLevelActive;
    private bool _isLevelCompleted;
    private DateTime _levelStartedAt;

    public string LevelDisplay => (_levelIndex + 1).ToString();

    public string LevelTitle
    {
        get
        {
            if (_levelIndex < 0 || _levelIndex >= _library.Mazes.Count)
                return "—";

            return _library.GetByIndex(_levelIndex).Title;
        }
    }

    public string CompletedDisplay => _completedCount.ToString();
    public string HintText => "Коснись медвежонка и веди пальцем по дорожке к выходу";

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

    public MazeGameView(INavigationContext navigation)
    {
        _navigation = navigation;
        _library = MazeLibraryLoader.Load();
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

    private void LoadLevel(bool resetProgress)
    {
        if (resetProgress)
        {
            _levelIndex = 0;
            _completedCount = 0;
            _bestSecondsByLevel.Clear();
        }

        _levelIndex = Math.Clamp(_levelIndex, 0, _library.Mazes.Count - 1);
        _isLevelCompleted = false;
        SetNextLevelEnabled(false);

        var maze = _library.GetByIndex(_levelIndex);
        MazeBoard.LoadMaze(maze);

        _elapsedSeconds = 0;
        _levelStartedAt = DateTime.UtcNow;
        _isLevelActive = true;
        StartTimer();

        ApplyRoundBackground();
        UpdateDisplays();
    }

    private void RestartLevel()
    {
        _isLevelCompleted = false;
        SetNextLevelEnabled(false);
        MazeBoard.ResetCharacter();

        _elapsedSeconds = 0;
        _levelStartedAt = DateTime.UtcNow;
        _isLevelActive = true;
        StartTimer();
        UpdateDisplays();
    }

    private async void MazeBoard_MazeCompleted(object? sender, EventArgs e)
    {
        if (_isLevelCompleted) return;

        _isLevelCompleted = true;
        _isLevelActive = false;
        StopTimer();

        var levelId = _levelIndex + 1;
        if (!_bestSecondsByLevel.TryGetValue(levelId, out var best) || _elapsedSeconds < best)
            _bestSecondsByLevel[levelId] = _elapsedSeconds;

        _completedCount++;
        SetNextLevelEnabled(_levelIndex < _library.Mazes.Count - 1);
        UpdateDisplays();

        try
        {
            await PraiseOverlay.PlayAsync();
        }
        catch
        {
            // ignore overlay cancellation
        }
    }

    private void NextLevelButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isLevelCompleted || _levelIndex >= _library.Mazes.Count - 1) return;

        _levelIndex++;
        LoadLevel(resetProgress: false);
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e) => RestartLevel();

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

    private void SetNextLevelEnabled(bool enabled)
    {
        NextLevelButton.IsEnabled = enabled;
        NextLevelButton.AssetPath = enabled
            ? KenneyPaths.ButtonRectangleGreen
            : KenneyPaths.ButtonRectangleGrey;
    }

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
