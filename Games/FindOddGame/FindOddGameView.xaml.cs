using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.Objects;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.FindColorGame.Helpers;
using HoroshieIgry.Games.FindOddGame.Controls;
using HoroshieIgry.Games.FindOddGame.Helpers;
using HoroshieIgry.Games.FindOddGame.Models;

namespace HoroshieIgry.Games.FindOddGame;

public partial class FindOddGameView : UserControl, INotifyPropertyChanged
{
    private const string GameId = "find-odd";
    private const int WrongFlashMs = 450;
    private const int ShakeMs = 420;
    private const int TimeoutRevealMs = 1400;
    private const int InputDebounceMs = 320;

    private readonly INavigationContext _navigation;
    private readonly ObjectLibrary _objectLibrary;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

    private CancellationTokenSource? _roundCts;
    private int _level = 1;
    private int _score;
    private int _mistakes;
    private int _secondsLeft;
    private double _boardOpacity = 1;
    private DateTime _sessionStart = DateTime.UtcNow;
    private bool _isInputLocked;
    private bool _isRoundActive;
    private string _hintText = "Найди лишнее";
    private int _choiceColumns = 2;
    private int _choiceRows = 2;
    private double _itemSize = 140;
    private long _lastInputTick;
    private int _lastInputDeviceId = -2;

    public ObservableCollection<OddItemModel> Choices { get; } = new();

    public int ChoiceColumns
    {
        get => _choiceColumns;
        private set
        {
            if (_choiceColumns == value) return;
            _choiceColumns = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoardWidth));
            OnPropertyChanged(nameof(BoardHeight));
            UpdateScaledLayout();
        }
    }

    public int ChoiceRows
    {
        get => _choiceRows;
        private set
        {
            if (_choiceRows == value) return;
            _choiceRows = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoardWidth));
            OnPropertyChanged(nameof(BoardHeight));
            UpdateScaledLayout();
        }
    }

    public double ItemSize
    {
        get => _itemSize;
        private set
        {
            if (Math.Abs(_itemSize - value) < 0.5) return;
            _itemSize = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoardWidth));
            OnPropertyChanged(nameof(BoardHeight));
        }
    }

    public double ItemMargin => OddGameLevel.GetItemMargin(Choices.Count);
    public double BoardWidth => ChoiceColumns * (ItemSize + ItemMargin * 2);
    public double BoardHeight => ChoiceRows * (ItemSize + ItemMargin * 2);
    public double BoardOpacity
    {
        get => _boardOpacity;
        private set
        {
            if (Math.Abs(_boardOpacity - value) < 0.01) return;
            _boardOpacity = value;
            OnPropertyChanged();
        }
    }

    public int Level => _level;
    public string LevelBadgeDisplay => _level.ToString();
    public string ScoreDisplay => _score.ToString();
    public string HintText
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_loadError))
                return _loadError;

            return string.IsNullOrWhiteSpace(_hintText) ? "Найди лишнее" : _hintText;
        }
    }

    public string LevelTitle
    {
        get
        {
            var count = Choices.Count > 0 ? Choices.Count : OddGameLevel.GetCardCount(_level);
            return $"{count} карточки · {OddGameLevel.GetTimeSeconds(count)} сек";
        }
    }

    public string TimeDisplay => _secondsLeft.ToString();

    public event PropertyChangedEventHandler? PropertyChanged;

    public FindOddGameView(INavigationContext navigation)
    {
        _navigation = navigation;
        _objectLibrary = ObjectLibraryLoader.Load();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        InitializeComponent();
        DataContext = this;
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);
        Loaded += OnFirstLoaded;
        Unloaded += (_, _) => StopRound();
        SizeChanged += FindOddGameView_SizeChanged;

        try
        {
            GenerateRound();
            UpdateDisplays();
        }
        catch (Exception ex)
        {
            _hintText = "Не удалось загрузить задание";
            _loadError = ex.Message;
            UpdateDisplays();
        }
    }

    private string? _loadError;

    private void FindOddGameView_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateScaledLayout();

    private void UpdateScaledLayout()
    {
        if (ChoiceColumns < 1 || ChoiceRows < 1 || Choices.Count < 1) return;
        if (ActualWidth < 1 || ActualHeight < 1) return;

        var (availW, availH) = GetPlayAreaSize();
        var newSize = OddGameLevel.ComputeItemSize(Choices.Count, availW, availH);

        if (Math.Abs(ItemSize - newSize) >= 0.5)
            ItemSize = newSize;

        OnPropertyChanged(nameof(BoardWidth));
        OnPropertyChanged(nameof(BoardHeight));
    }

    private (double Width, double Height) GetPlayAreaSize()
    {
        var availW = Math.Max(200, LayoutRoot.ActualWidth > 0 ? LayoutRoot.ActualWidth - 16 : ActualWidth - 16);

        if (LayoutRoot.RowDefinitions.Count > 2 && LayoutRoot.RowDefinitions[2].ActualHeight > 40)
            return (availW, LayoutRoot.RowDefinitions[2].ActualHeight - 8);

        return (availW, Math.Max(160, ActualHeight - 220));
    }

    private async void OnFirstLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnFirstLoaded;

        if (!string.IsNullOrWhiteSpace(_loadError))
        {
            await GameDialog.ShowAsync(
                "Найди лишнее",
                $"Не удалось подготовить игру.\n{_loadError}",
                "Ок",
                "В каталог");
            return;
        }

        BoardOpacity = 1;
        ApplyRoundBackground();
        UpdateScaledLayout();
        _isRoundActive = true;
        _timer.Start();
        _isInputLocked = false;
    }

    private void StopRound()
    {
        _timer.Stop();
        _isRoundActive = false;
        CancelPendingTasks();
    }

    private void CancelPendingTasks()
    {
        _roundCts?.Cancel();
        _roundCts?.Dispose();
        _roundCts = null;
    }

    private async Task BeginRoundAsync(bool resetSession)
    {
        StopRound();
        CancelPendingTasks();
        _roundCts = new CancellationTokenSource();
        var ct = _roundCts.Token;

        if (resetSession)
        {
            _level = 1;
            _score = 0;
            _mistakes = 0;
            _sessionStart = DateTime.UtcNow;
        }

        ApplyRoundBackground();
        _isInputLocked = true;

        try
        {
            if (!resetSession && Choices.Count > 0)
                await FadeBoardAsync(1, 0, 220, ct);

            GenerateRound();
            _loadError = null;
            BoardOpacity = resetSession ? 1 : 0;
            UpdateDisplays();
            RefreshChoicesPanel();

            await Dispatcher.InvokeAsync(UpdateScaledLayout, DispatcherPriority.Loaded);

            if (!resetSession)
            {
                var loadDelay = OddGameLevel.GetRoundLoadDelayMs(Choices.Count);
                if (loadDelay > 0)
                    await Task.Delay(loadDelay, ct);
                if (ct.IsCancellationRequested) return;
                await FadeBoardAsync(0, 1, 260, ct);
            }

            if (ct.IsCancellationRequested) return;

            _isRoundActive = true;
            _timer.Start();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            _loadError = ex.Message;
            _hintText = "Не удалось загрузить задание";
            UpdateDisplays();
            await GameDialog.ShowAsync(
                "Найди лишнее",
                $"Не удалось подготовить уровень.\n{ex.Message}",
                "Ок",
                "Закрыть");
        }
        finally
        {
            if (!ct.IsCancellationRequested)
                _isInputLocked = false;
        }
    }

    private void GenerateRound()
    {
        Choices.Clear();

        var plan = OddRoundGenerator.Generate(_level, _objectLibrary, _random);
        _hintText = plan.Hint;
        _secondsLeft = plan.TimeSeconds;

        OddGameLevel.GetGridSize(plan.Items.Count, out var columns, out var rows);
        ChoiceColumns = columns;
        ChoiceRows = rows;

        var palettes = ColorGameAssets.PlayPalettes;
        for (var i = 0; i < plan.Items.Count; i++)
        {
            var tile = OddGameAssets.GetTilePath(palettes[i % palettes.Length]);
            Choices.Add(OddItemModel.FromPlan(plan.Items[i], tile));
        }

        OnPropertyChanged(nameof(ItemMargin));
        RefreshChoicesPanel();
        UpdateScaledLayout();
        Dispatcher.BeginInvoke(UpdateScaledLayout, DispatcherPriority.Loaded);
    }

    private void RefreshChoicesPanel()
    {
        var factory = new FrameworkElementFactory(typeof(UniformGrid));
        factory.SetValue(UniformGrid.RowsProperty, ChoiceRows);
        factory.SetValue(UniformGrid.ColumnsProperty, ChoiceColumns);
        ChoicesPanel.ItemsPanel = new ItemsPanelTemplate { VisualTree = factory };
    }

    private async Task FadeBoardAsync(double from, double to, int durationMs, CancellationToken ct)
    {
        const int steps = 10;
        var stepDelay = Math.Max(1, durationMs / steps);

        for (var i = 1; i <= steps; i++)
        {
            BoardOpacity = from + (to - from) * i / steps;
            await Task.Delay(stepDelay, ct);
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!_isRoundActive || _isInputLocked) return;

        _secondsLeft--;
        OnPropertyChanged(nameof(TimeDisplay));

        if (_secondsLeft <= 0)
            _ = HandleTimeExpiredAsync();
    }

    private void UpdateDisplays()
    {
        OnPropertyChanged(nameof(Level));
        OnPropertyChanged(nameof(LevelBadgeDisplay));
        OnPropertyChanged(nameof(ScoreDisplay));
        OnPropertyChanged(nameof(LevelTitle));
        OnPropertyChanged(nameof(TimeDisplay));
        OnPropertyChanged(nameof(HintText));
    }

    private async void RestartButton_Click(object sender, RoutedEventArgs e)
        => await BeginRoundAsync(resetSession: true);

    private async void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        StopRound();
        if (_score > 0 || _mistakes > 0 || _level > 1)
            await ShowSessionSummaryAsync();

        _navigation.NavigateToCatalog();
    }

    private async Task ShowSessionSummaryAsync()
    {
        var elapsed = DateTime.UtcNow - _sessionStart;
        var minutes = (int)elapsed.TotalMinutes;
        var seconds = elapsed.Seconds;
        var message =
            $"Правильных ответов: {_score}\n" +
            $"Ошибок: {_mistakes}\n" +
            $"Время: {minutes}:{seconds:D2}\n" +
            $"Итог: {_score} очков";

        await GameDialog.ShowAsync("Итог игры", message, "Ок", "Закрыть");
    }

    private void PlayAreaGrid_PreviewTouchDown(object sender, TouchEventArgs e)
    {
        if (TryHandleChoiceInput(e.GetTouchPoint(PlayAreaGrid).Position, e.TouchDevice.Id))
            e.Handled = true;
    }

    private void PlayAreaGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null) return;
        if (TryHandleChoiceInput(e.GetPosition(PlayAreaGrid), -1))
            e.Handled = true;
    }

    private bool TryHandleChoiceInput(Point positionOnBoard, int deviceId)
    {
        if (!_isRoundActive || _isInputLocked) return false;

        var tick = Environment.TickCount64;
        if (deviceId == _lastInputDeviceId && tick - _lastInputTick < InputDebounceMs)
            return true;

        var index = ResolveChoiceIndex(positionOnBoard);
        if (index is null) return false;

        var item = Choices[index.Value];
        if (!item.IsClickable) return false;

        _lastInputDeviceId = deviceId;
        _lastInputTick = tick;
        PlayPressFeedback(index.Value);
        _isInputLocked = true;
        _ = ProcessChoiceClickAsync(item);
        return true;
    }

    private int? ResolveChoiceIndex(Point position)
    {
        if (BoardWidth < 1 || BoardHeight < 1 || ChoiceColumns < 1 || ChoiceRows < 1)
            return null;

        var cellWidth = BoardWidth / ChoiceColumns;
        var cellHeight = BoardHeight / ChoiceRows;
        var col = (int)(position.X / cellWidth);
        var row = (int)(position.Y / cellHeight);

        if (col < 0 || row < 0 || col >= ChoiceColumns || row >= ChoiceRows)
            return null;

        var index = row * ChoiceColumns + col;
        return index < Choices.Count ? index : null;
    }

    private void PlayPressFeedback(int choiceIndex)
    {
        if (ChoicesPanel.ItemContainerGenerator.ContainerFromIndex(choiceIndex) is not FrameworkElement container)
            return;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(container); i++)
        {
            if (VisualTreeHelper.GetChild(container, i) is OddChoiceControl control)
            {
                control.PlayPressFeedback();
                return;
            }
        }
    }

    private async Task ProcessChoiceClickAsync(OddItemModel item)
    {
        try
        {
            if (item.IsOdd)
                await HandleCorrectPickAsync(item);
            else
                await HandleWrongPickAsync(item);
        }
        catch (OperationCanceledException)
        {
            if (_isRoundActive)
                _isInputLocked = false;
        }
    }

    private async Task HandleWrongPickAsync(OddItemModel item)
    {
        _mistakes++;
        UpdateDisplays();
        item.IsWrongFlash = true;
        item.IsShaking = true;

        try
        {
            await Task.Delay(Math.Max(WrongFlashMs, ShakeMs), _roundCts?.Token ?? CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            _isInputLocked = false;
            return;
        }

        item.IsWrongFlash = false;
        item.IsShaking = false;

        if (_isRoundActive)
            _isInputLocked = false;
    }

    private async Task HandleCorrectPickAsync(OddItemModel item)
    {
        _timer.Stop();
        _isRoundActive = false;
        _score++;
        UpdateDisplays();
        item.IsCorrectFlash = true;

        try
        {
            await PraiseOverlay.PlayAsync();
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (_roundCts?.IsCancellationRequested == true) return;

        _level++;
        await BeginRoundAsync(resetSession: false);
    }

    private async Task HandleTimeExpiredAsync()
    {
        if (!_isRoundActive) return;

        _isInputLocked = true;
        _timer.Stop();
        _isRoundActive = false;
        _mistakes++;
        UpdateDisplays();

        foreach (var choice in Choices)
            choice.IsClickable = false;

        var odd = Choices.FirstOrDefault(c => c.IsOdd);
        if (odd is not null)
            odd.IsCorrectFlash = true;

        try
        {
            await Task.Delay(TimeoutRevealMs, _roundCts?.Token ?? CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (_roundCts?.IsCancellationRequested == true) return;

        _level++;
        await BeginRoundAsync(resetSession: false);
    }

    private void ApplyRoundBackground()
        => _navigation.SetBackgroundTheme(GameBackgroundRotator.ForRound(GameId, _level));

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
