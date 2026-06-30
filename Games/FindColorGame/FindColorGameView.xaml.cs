using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.FindColorGame.Controls;
using HoroshieIgry.Games.FindColorGame.Helpers;
using HoroshieIgry.Games.FindColorGame.Models;

namespace HoroshieIgry.Games.FindColorGame;

/// <summary>Экран мини-игры «Найди цвет».</summary>
public partial class FindColorGameView : UserControl, INotifyPropertyChanged
{
    private const string GameId = "find-color";
    private const int WrongFlashMs = 450;
    private const int InputDebounceMs = 320;

    private readonly INavigationContext _navigation;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

    private CancellationTokenSource? _roundCts;
    private int _level = 1;
    private int _secondsLeft;
    private bool _isInputLocked;
    private bool _isRoundActive;
    private KenneyPalette _targetPalette;
    private ColorFigureShape _targetShape;
    private KenneyPalette[] _roundPalettes = [];
    private int _nextFigureId;
    private int _gridColumns = 5;
    private int _gridRows = 1;
    private double _itemSize = 132;
    private long _lastInputTick;
    private int _lastInputDeviceId = -2;

    public ObservableCollection<ColorFigureModel> Figures { get; } = new();

    public int GridColumns
    {
        get => _gridColumns;
        private set
        {
            if (_gridColumns == value) return;
            _gridColumns = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoardWidth));
            OnPropertyChanged(nameof(BoardHeight));
            UpdateScaledLayout();
        }
    }

    public int GridRows
    {
        get => _gridRows;
        private set
        {
            if (_gridRows == value) return;
            _gridRows = value;
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

    public double ItemMargin => ColorGameLevel.GetItemMargin(Figures.Count);
    public double BoardWidth => GridColumns * (ItemSize + ItemMargin * 2);
    public double BoardHeight => GridRows * (ItemSize + ItemMargin * 2);

    public int Level => _level;
    public string LevelBadgeDisplay => _level.ToString();
    public string LevelTitle
    {
        get
        {
            var count = Figures.Count > 0 ? Figures.Count : ColorGameLevel.GetItemCount(_level);
            return $"{count} фигур · {ColorGameLevel.GetTimeSeconds(count)} сек";
        }
    }
    public string TimeDisplay => _secondsLeft.ToString();
    public string ItemCountDisplay => Figures.Count.ToString();
    public string TargetColorName => ColorGameAssets.GetColorName(_targetPalette);
    public string TargetShapePath => ColorGameAssets.GetShapePath(_targetPalette, ColorFigureShape.Circle);

    public event PropertyChangedEventHandler? PropertyChanged;

    public FindColorGameView(INavigationContext navigation)
    {
        _navigation = navigation;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        InitializeComponent();
        DataContext = this;
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);
        Loaded += OnFirstLoaded;
        Unloaded += (_, _) => StopRound();
        SizeChanged += FindColorGameView_SizeChanged;
    }

    private void FindColorGameView_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateScaledLayout();

    private void UpdateScaledLayout()
    {
        if (GridColumns < 1 || GridRows < 1 || Figures.Count < 1) return;
        if (ActualWidth < 1 || ActualHeight < 1) return;

        var (availW, availH) = GetPlayAreaSize();
        var newSize = ColorGameLevel.ComputeItemSize(Figures.Count, GridColumns, GridRows, availW, availH);

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
        await BeginRoundAsync(resetLevel: true);
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

    private async Task BeginRoundAsync(bool resetLevel)
    {
        StopRound();
        CancelPendingTasks();
        _roundCts = new CancellationTokenSource();
        var ct = _roundCts.Token;

        if (resetLevel) _level = 1;

        ApplyRoundBackground();
        _isInputLocked = true;
        try
        {
            GenerateFigures();
            _secondsLeft = ColorGameLevel.GetTimeSeconds(Figures.Count);
            UpdateDisplays();

            await Dispatcher.InvokeAsync(UpdateScaledLayout, DispatcherPriority.Loaded);
            await Task.Delay(ColorGameLevel.GetRoundLoadDelayMs(Figures.Count), ct);

            if (ct.IsCancellationRequested) return;

            _isRoundActive = true;
            _timer.Start();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            if (!ct.IsCancellationRequested)
                _isInputLocked = false;
        }
    }

    private void GenerateFigures()
    {
        Figures.Clear();

        var itemCount = ColorGameLevel.GetItemCount(_level);
        var paletteCount = ColorGameLevel.GetPaletteCount(_level);
        var palettes = ColorGameAssets.PlayPalettes.Take(paletteCount).ToArray();

        _targetPalette = palettes[_random.Next(palettes.Length)];
        _targetShape = ColorGameAssets.AllShapes[_random.Next(ColorGameAssets.AllShapes.Length)];
        _roundPalettes = palettes;
        _nextFigureId = 1;

        var (playW, playH) = GetPlayAreaSize();
        var aspect = playW / Math.Max(1, playH);
        ColorGameLevel.GetGridSize(itemCount, aspect, out var columns, out var rows);
        GridColumns = columns;
        GridRows = rows;

        var items = new List<ColorFigureModel>
        {
            new()
            {
                Id = 0,
                Palette = _targetPalette,
                Shape = _targetShape,
                IsTarget = true
            }
        };

        var wrongPalettes = palettes.Where(p => p != _targetPalette).ToArray();
        if (wrongPalettes.Length == 0)
            wrongPalettes = ColorGameAssets.PlayPalettes.Where(p => p != _targetPalette).ToArray();

        for (var i = 1; i < itemCount; i++)
        {
            var palette = wrongPalettes[_random.Next(wrongPalettes.Length)];
            var shape = ColorGameAssets.AllShapes[_random.Next(ColorGameAssets.AllShapes.Length)];
            items.Add(new ColorFigureModel
            {
                Id = i,
                Palette = palette,
                Shape = shape,
                IsTarget = false
            });
        }

        Shuffle(items);
        foreach (var item in items) Figures.Add(item);

        OnPropertyChanged(nameof(ItemCountDisplay));
        OnPropertyChanged(nameof(ItemMargin));
        OnPropertyChanged(nameof(TargetColorName));
        OnPropertyChanged(nameof(TargetShapePath));
        UpdateScaledLayout();
        Dispatcher.BeginInvoke(UpdateScaledLayout, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
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
        OnPropertyChanged(nameof(LevelTitle));
        OnPropertyChanged(nameof(TimeDisplay));
        OnPropertyChanged(nameof(ItemCountDisplay));
        OnPropertyChanged(nameof(TargetColorName));
        OnPropertyChanged(nameof(TargetShapePath));
    }

    private async void NewGameButton_Click(object sender, RoutedEventArgs e)
        => await BeginRoundAsync(resetLevel: true);

    private void BackToCatalogButton_Click(object sender, RoutedEventArgs e) => _navigation.NavigateToCatalog();

    private void PlayAreaGrid_PreviewTouchDown(object sender, TouchEventArgs e)
    {
        if (TryHandleFigureInput(e.GetTouchPoint(FiguresPanel).Position, e.TouchDevice.Id))
            e.Handled = true;
    }

    private void PlayAreaGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null) return;
        if (TryHandleFigureInput(e.GetPosition(FiguresPanel), -1))
            e.Handled = true;
    }

    private bool TryHandleFigureInput(Point positionOnBoard, int deviceId)
    {
        if (!_isRoundActive || _isInputLocked) return false;

        var tick = Environment.TickCount64;
        if (deviceId == _lastInputDeviceId && tick - _lastInputTick < InputDebounceMs)
            return true;

        if (FiguresPanel.InputHitTest(positionOnBoard) is not DependencyObject hit)
            return false;

        var control = FindParent<ColorFigureControl>(hit);
        if (control?.Figure is not { IsClickable: true } figure)
            return false;

        _lastInputDeviceId = deviceId;
        _lastInputTick = tick;
        control.PlayPressFeedback();
        _isInputLocked = true;
        _ = ProcessFigureClickAsync(figure);
        return true;
    }

    private static T? FindParent<T>(DependencyObject? node) where T : DependencyObject
    {
        while (node is not null)
        {
            if (node is T match) return match;
            node = VisualTreeHelper.GetParent(node);
        }

        return null;
    }

    private async Task ProcessFigureClickAsync(ColorFigureModel figure)
    {
        try
        {
            FindColorSounds.PlayTap();
            if (figure.IsTarget)
                await HandleCorrectPickAsync(figure);
            else
                await HandleWrongPickAsync(figure);
        }
        catch (OperationCanceledException)
        {
            if (_isRoundActive)
                _isInputLocked = false;
        }
    }

    private async Task HandleWrongPickAsync(ColorFigureModel figure)
    {
        FindColorSounds.PlayWrong();
        figure.IsWrongFlash = true;

        try
        {
            await Task.Delay(WrongFlashMs, _roundCts?.Token ?? CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            _isInputLocked = false;
            return;
        }

        figure.IsWrongFlash = false;

        if (_isRoundActive && Figures.Count < ColorGameLevel.MaxItemCount)
            AddDistractorFigure();

        _isInputLocked = false;
    }

    private void AddDistractorFigure()
    {
        var wrongPalettes = _roundPalettes.Where(p => p != _targetPalette).ToArray();
        if (wrongPalettes.Length == 0)
            wrongPalettes = ColorGameAssets.PlayPalettes.Where(p => p != _targetPalette).ToArray();

        var palette = wrongPalettes[_random.Next(wrongPalettes.Length)];
        var shape = ColorGameAssets.AllShapes[_random.Next(ColorGameAssets.AllShapes.Length)];

        Figures.Add(new ColorFigureModel
        {
            Id = _nextFigureId++,
            Palette = palette,
            Shape = shape,
            IsTarget = false
        });

        var (playW, playH) = GetPlayAreaSize();
        var aspect = playW / Math.Max(1, playH);
        ColorGameLevel.GetGridSize(Figures.Count, aspect, out var columns, out var rows);
        GridColumns = columns;
        GridRows = rows;

        OnPropertyChanged(nameof(ItemCountDisplay));
        OnPropertyChanged(nameof(ItemMargin));
        OnPropertyChanged(nameof(LevelTitle));
        UpdateScaledLayout();
        Dispatcher.BeginInvoke(UpdateScaledLayout, DispatcherPriority.Loaded);
    }

    private async Task HandleCorrectPickAsync(ColorFigureModel figure)
    {
        _timer.Stop();
        _isRoundActive = false;
        figure.IsCorrectFlash = true;
        FindColorSounds.PlaySuccess();

        try
        {
            FindColorSounds.PlayVictory();
            await PraiseOverlay.PlayAsync();
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (_roundCts?.IsCancellationRequested == true) return;

        _level++;
        await BeginRoundAsync(resetLevel: false);
    }

    private async Task HandleTimeExpiredAsync()
    {
        if (!_isRoundActive) return;

        _isInputLocked = true;
        _timer.Stop();
        _isRoundActive = false;
        FindColorSounds.PlayWrong();

        foreach (var figure in Figures)
            figure.IsClickable = false;

        var target = Figures.FirstOrDefault(f => f.IsTarget);
        if (target is not null)
            target.IsCorrectFlash = true;

        await ShowTimeExpiredDialogAsync();
    }

    private async Task ShowTimeExpiredDialogAsync()
    {
        var message = $"Время вышло!\nНужен был цвет: {TargetColorName}\nУровень {_level} · {Figures.Count} фигур";

        var confirmed = await GameDialog.ShowAsync(
            "Попробуй ещё",
            message,
            "Ещё раз",
            "Выйти");

        if (!confirmed) return;
        await BeginRoundAsync(resetLevel: false);
    }

    private void ApplyRoundBackground()
        => _navigation.SetBackgroundTheme(GameBackgroundRotator.ForRound(GameId, _level));

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
