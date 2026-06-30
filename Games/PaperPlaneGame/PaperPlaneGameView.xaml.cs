using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Games.PaperPlaneGame.Controls;
using HoroshieIgry.Games.PaperPlaneGame.Helpers;

namespace HoroshieIgry.Games.PaperPlaneGame;

public partial class PaperPlaneGameView : UserControl, INotifyPropertyChanged
{
    private const string GameId = "paper-plane";

    private readonly INavigationContext _navigation;
    private readonly DispatcherTimer _timer;

    private int _level = 1;
    private int _starsCollected;
    private int _totalStars;
    private int _distancePercent;
    private bool _isLevelActive;
    private bool _isShowingVictory;

    public string LevelDisplay => _level.ToString();
    public string StarsDisplay => $"{_starsCollected}/{_totalStars}";
    public string DistanceDisplay => $"{_distancePercent}%";
    public string HintText => "⭐ Собирай звёзды · ☁️ Облетай облака · Держи — птичка вверх, отпусти — вниз";

    public event PropertyChangedEventHandler? PropertyChanged;

    public PaperPlaneGameView(INavigationContext navigation)
    {
        _navigation = navigation;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
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
        StartLevel(resetProgress: true);
    }

    private void StartLevel(bool resetProgress = false)
    {
        if (resetProgress)
            _level = 1;

        _isShowingVictory = false;
        CrashOverlay.Hide();
        VictoryOverlay.Hide();

        var definition = PaperPlaneLevelPostProcessor.Prepare(PaperPlaneLevelLoader.Load(_level));

        _starsCollected = 0;
        _totalStars = definition.Stars.Count;
        _distancePercent = 0;

        Playfield.LoadLevel(definition);
        _navigation.SetBackgroundTheme(PaperPlaneBackgroundMapper.Map(definition.Background));

        if (definition.ShowTutorial)
            TutorialOverlay.Show();
        else
            TutorialOverlay.Hide();

        _isLevelActive = true;
        StartTimer();
        UpdateDisplays();
    }

    private void Playfield_TutorialDismissed(object? sender, EventArgs e)
        => TutorialOverlay.Hide();

    private void Playfield_ProgressChanged(object? sender, PaperPlaneProgressEventArgs e)
    {
        _starsCollected = e.StarsCollected;
        _totalStars = e.TotalStars;
        _distancePercent = e.DistancePercent;
        OnPropertyChanged(nameof(StarsDisplay));
        OnPropertyChanged(nameof(DistanceDisplay));
    }

    private async void Playfield_LevelCompleted(object? sender, EventArgs e)
    {
        if (_isShowingVictory) return;

        _isShowingVictory = true;
        _isLevelActive = false;
        StopTimer();
        Playfield.StopGame();

        PaperPlaneSounds.PlayVictory();
        await VictoryOverlay.ShowAsync(_starsCollected, _totalStars, _level, Playfield.ElapsedSeconds);
    }

    private void Playfield_LevelCrashed(object? sender, EventArgs e)
    {
        _isLevelActive = false;
        StopTimer();
        Playfield.StopGame();
        CrashOverlay.Show();
    }

    private void VictoryOverlay_NextFlightRequested(object? sender, EventArgs e)
    {
        _level++;
        StartLevel();
    }

    private void CrashOverlay_RetryRequested(object? sender, EventArgs e)
        => StartLevel();

    private void CrashOverlay_CatalogRequested(object? sender, EventArgs e)
    {
        StopTimer();
        Playfield.StopGame();
        _navigation.NavigateToCatalog();
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isShowingVictory) return;
        StartLevel();
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        StopTimer();
        Playfield.StopGame();
        _navigation.NavigateToCatalog();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_isLevelActive) return;
        // Прогресс дистанции обновляется из playfield; таймер оставлен для будущих UI-эффектов.
    }

    private void StartTimer()
    {
        _timer.Stop();
        _timer.Start();
    }

    private void StopTimer() => _timer.Stop();

    private void UpdateDisplays()
    {
        OnPropertyChanged(nameof(LevelDisplay));
        OnPropertyChanged(nameof(StarsDisplay));
        OnPropertyChanged(nameof(DistanceDisplay));
        OnPropertyChanged(nameof(HintText));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
