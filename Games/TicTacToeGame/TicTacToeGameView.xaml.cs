using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.TicTacToeGame.Controls;
using HoroshieIgry.Games.TicTacToeGame.Helpers;
using HoroshieIgry.Games.TicTacToeGame.Models;

namespace HoroshieIgry.Games.TicTacToeGame;

public partial class TicTacToeGameView : UserControl, INotifyPropertyChanged
{
    private const string GameId = "tic-tac-toe";
    private const int InputDebounceMs = 280;
    private const int AiMoveDelayMs = 450;

    private readonly INavigationContext _navigation;
    private readonly Random _random = new();
    private CancellationTokenSource? _roundCts;

    private TicTacToeGameMode? _mode;
    private bool _isModeSelectVisible = true;
    private int _level = 1;
    private int _xWins;
    private int _oWins;
    private int _draws;
    private TicTacToeMark _currentMark = TicTacToeAssets.PlayerMark;
    private bool _isInputLocked;
    private bool _isRoundActive;
    private double _cellSize = 140;
    private string _hintText = "Твой ход — крестик.";
    private long _lastInputTick;
    private int _lastInputDeviceId = -2;

    public ObservableCollection<TicTacToeCellModel> Cells { get; } = new();

    public double CellSize
    {
        get => _cellSize;
        private set
        {
            if (Math.Abs(_cellSize - value) < 0.5) return;
            _cellSize = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoardWidth));
            OnPropertyChanged(nameof(BoardHeight));
        }
    }

    public double CellMargin => 10;
    public double BoardWidth => TicTacToeRules.BoardSize * (CellSize + CellMargin * 2);
    public double BoardHeight => TicTacToeRules.BoardSize * (CellSize + CellMargin * 2);

    public int Level => _level;
    public string LevelBadgeDisplay => _level.ToString();
    public string LevelTitle => _mode == TicTacToeGameMode.LocalTwoPlayer
        ? "Два игрока на одном экране"
        : $"Поле 3×3 · сила {_level}";
    public string ScoreDisplay => _mode == TicTacToeGameMode.LocalTwoPlayer
        ? $"Крестик {_xWins} : {_oWins} нолик · ничьи {_draws}"
        : $"Ты {_xWins} : {_oWins} компьютер · ничьи {_draws}";
    public string OpponentDisplay => _mode switch
    {
        TicTacToeGameMode.LocalTwoPlayer => "1 на 1",
        TicTacToeGameMode.VsComputer => "Компьютер",
        _ => "—"
    };
    public string HintText => _hintText;
    public string HintSymbolPath => TicTacToeAssets.GetSymbolPath(_currentMark);
    public Visibility ModeSelectVisibility => _isModeSelectVisible ? Visibility.Visible : Visibility.Collapsed;
    public Visibility LevelBadgeVisibility => _mode == TicTacToeGameMode.VsComputer && !_isModeSelectVisible
        ? Visibility.Visible
        : Visibility.Collapsed;

    public event PropertyChangedEventHandler? PropertyChanged;

    public TicTacToeGameView(INavigationContext navigation)
    {
        _navigation = navigation;
        InitializeComponent();
        DataContext = this;
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);
        Loaded += (_, _) => ShowModeSelect();
        Unloaded += (_, _) => CancelRound();
        SizeChanged += TicTacToeGameView_SizeChanged;
    }

    private void TicTacToeGameView_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateScaledLayout();

    private void UpdateScaledLayout()
    {
        if (ActualWidth < 1 || ActualHeight < 1) return;

        var (availW, availH) = GetPlayAreaSize();
        var margin = CellMargin;
        var n = TicTacToeRules.BoardSize;
        var sizeFromW = Math.Floor((availW - n * margin * 2) / n);
        var sizeFromH = Math.Floor((availH - n * margin * 2) / n);
        var newSize = Math.Clamp(Math.Min(sizeFromW, sizeFromH), 96, 280);

        if (Math.Abs(CellSize - newSize) >= 0.5)
            CellSize = newSize;

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

    private void CancelRound()
    {
        _isRoundActive = false;
        _roundCts?.Cancel();
        _roundCts?.Dispose();
        _roundCts = null;
    }

    private void ShowModeSelect()
    {
        CancelRound();
        _isModeSelectVisible = true;
        _mode = null;
        _hintText = "Выберите режим игры.";
        UpdateDisplays();
        ApplyRoundBackground();
    }

    private void StartMode(TicTacToeGameMode mode)
    {
        _mode = mode;
        _isModeSelectVisible = false;
        StartNewRound(resetLevel: true);
    }

    private void VsComputerButton_Click(object sender, RoutedEventArgs e)
        => StartMode(TicTacToeGameMode.VsComputer);

    private void TwoPlayerButton_Click(object sender, RoutedEventArgs e)
        => StartMode(TicTacToeGameMode.LocalTwoPlayer);

    private void StartNewRound(bool resetLevel)
    {
        if (_mode is null) return;

        CancelRound();
        _roundCts = new CancellationTokenSource();

        if (resetLevel)
        {
            _level = 1;
            _xWins = 0;
            _oWins = 0;
            _draws = 0;
        }

        ResetBoard();
        _currentMark = TicTacToeAssets.PlayerMark;
        _isRoundActive = true;
        _isInputLocked = false;
        _hintText = GetTurnHint(_currentMark);
        UpdateDisplays();
        ApplyRoundBackground();
        UpdateScaledLayout();
    }

    private string GetTurnHint(TicTacToeMark mark) => _mode switch
    {
        TicTacToeGameMode.LocalTwoPlayer when mark == TicTacToeMark.X => "Ход игрока 1 — крестик.",
        TicTacToeGameMode.LocalTwoPlayer => "Ход игрока 2 — нолик.",
        _ when mark == TicTacToeMark.X => "Твой ход — крестик.",
        _ => "Ход компьютера…"
    };

    private void ResetBoard()
    {
        Cells.Clear();
        for (var i = 0; i < TicTacToeRules.CellCount; i++)
            Cells.Add(new TicTacToeCellModel { Index = i });
    }

    private void ApplyRoundBackground()
        => _navigation.SetBackgroundTheme(GameBackgroundRotator.ForRound(GameId, _level));

    private void UpdateDisplays()
    {
        OnPropertyChanged(nameof(Level));
        OnPropertyChanged(nameof(LevelBadgeDisplay));
        OnPropertyChanged(nameof(LevelTitle));
        OnPropertyChanged(nameof(ScoreDisplay));
        OnPropertyChanged(nameof(OpponentDisplay));
        OnPropertyChanged(nameof(HintText));
        OnPropertyChanged(nameof(HintSymbolPath));
        OnPropertyChanged(nameof(ModeSelectVisibility));
        OnPropertyChanged(nameof(LevelBadgeVisibility));
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (_mode is null)
            ShowModeSelect();
        else
            StartNewRound(resetLevel: true);
    }

    private void BackToCatalogButton_Click(object sender, RoutedEventArgs e)
        => _navigation.NavigateToCatalog();

    private void PlayAreaGrid_PreviewTouchDown(object sender, TouchEventArgs e)
    {
        if (TryHandleCellInput(e.GetTouchPoint(PlayAreaGrid).Position, e.TouchDevice.Id))
            e.Handled = true;
    }

    private void PlayAreaGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null) return;
        if (TryHandleCellInput(e.GetPosition(PlayAreaGrid), -1))
            e.Handled = true;
    }

    private bool TryHandleCellInput(Point positionOnBoard, int deviceId)
    {
        if (_isModeSelectVisible || !_isRoundActive || _isInputLocked) return false;
        if (_mode == TicTacToeGameMode.VsComputer && _currentMark != TicTacToeAssets.PlayerMark) return false;

        var tick = Environment.TickCount64;
        if (deviceId == _lastInputDeviceId && tick - _lastInputTick < InputDebounceMs)
            return true;

        var cellIndex = ResolveCellIndex(positionOnBoard);
        if (cellIndex is null) return false;

        var cell = Cells[cellIndex.Value];
        if (cell.Mark != TicTacToeMark.Empty || !cell.IsClickable) return false;

        _lastInputDeviceId = deviceId;
        _lastInputTick = tick;
        PlayPressFeedback(cellIndex.Value);
        _ = ProcessMoveAsync(cellIndex.Value);
        return true;
    }

    private int? ResolveCellIndex(Point position)
    {
        if (BoardWidth < 1 || BoardHeight < 1) return null;

        var cellWidth = BoardWidth / TicTacToeRules.BoardSize;
        var cellHeight = BoardHeight / TicTacToeRules.BoardSize;
        var col = (int)(position.X / cellWidth);
        var row = (int)(position.Y / cellHeight);

        if (col < 0 || row < 0 || col >= TicTacToeRules.BoardSize || row >= TicTacToeRules.BoardSize)
            return null;

        var index = row * TicTacToeRules.BoardSize + col;
        return index < Cells.Count ? index : null;
    }

    private void PlayPressFeedback(int cellIndex)
    {
        if (CellsPanel.ItemContainerGenerator.ContainerFromIndex(cellIndex) is not FrameworkElement container)
            return;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(container); i++)
        {
            if (VisualTreeHelper.GetChild(container, i) is TicTacToeCellControl control)
            {
                control.PlayPressFeedback();
                return;
            }
        }
    }

    private async Task ProcessMoveAsync(int cellIndex)
    {
        _isInputLocked = true;
        var ct = _roundCts?.Token ?? CancellationToken.None;

        try
        {
            ApplyMark(cellIndex, _currentMark);

            if (await TryFinishRoundAsync(_currentMark)) return;

            if (_mode == TicTacToeGameMode.LocalTwoPlayer)
            {
                _currentMark = OppositeMark(_currentMark);
                _hintText = GetTurnHint(_currentMark);
                UpdateDisplays();
                _isInputLocked = false;
                return;
            }

            _currentMark = TicTacToeAssets.AiMark;
            _hintText = GetTurnHint(_currentMark);
            UpdateDisplays();

            await Task.Delay(AiMoveDelayMs, ct);
            if (ct.IsCancellationRequested) return;

            var board = TicTacToeRules.ReadMarks(Cells);
            var aiMove = TicTacToeAi.PickMove(board, TicTacToeAssets.AiMark, _level, _random);
            if (aiMove >= 0)
                ApplyMark(aiMove, TicTacToeAssets.AiMark);

            if (await TryFinishRoundAsync(TicTacToeAssets.AiMark)) return;

            _currentMark = TicTacToeAssets.PlayerMark;
            _hintText = GetTurnHint(_currentMark);
            UpdateDisplays();
            _isInputLocked = false;
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    private static TicTacToeMark OppositeMark(TicTacToeMark mark)
        => mark == TicTacToeMark.X ? TicTacToeMark.O : TicTacToeMark.X;

    private void ApplyMark(int index, TicTacToeMark mark)
    {
        Cells[index].Mark = mark;
        TicTacToeSounds.PlayPlace(mark == TicTacToeMark.X);
    }

    private async Task<bool> TryFinishRoundAsync(TicTacToeMark lastMark)
    {
        var board = TicTacToeRules.ReadMarks(Cells);

        if (TicTacToeRules.TryGetWinner(board, out var winner, out var line))
        {
            _isRoundActive = false;
            HighlightWinningLine(line);
            SetCellsClickable(false);

            if (winner == TicTacToeMark.X)
                _xWins++;
            else
                _oWins++;

            UpdateDisplays();

            if (_mode == TicTacToeGameMode.VsComputer)
            {
                if (winner == TicTacToeAssets.PlayerMark)
                {
                    TicTacToeSounds.PlayVictory();
                    _level++;
                    UpdateDisplays();
                    await PraiseOverlay.PlayAsync();
                    await ShowResultDialogAsync(
                        "Ты выиграл!",
                        $"Отличная партия!\nСчёт: {_xWins} : {_oWins}",
                        "Ещё раз",
                        "Выйти",
                        startNextOnYes: true);
                }
                else
                {
                    TicTacToeSounds.PlayDefeat();
                    await ShowResultDialogAsync(
                        "Компьютер выиграл",
                        "В следующий раз получится!\nПопробуй ещё раз.",
                        "Ещё раз",
                        "Выйти",
                        startNextOnYes: true);
                }
            }
            else
            {
                TicTacToeSounds.PlayVictory();
                var title = winner == TicTacToeMark.X ? "Игрок 1 выиграл!" : "Игрок 2 выиграл!";
                var message = winner == TicTacToeMark.X
                    ? "Крестик победил!\nСыграем ещё?"
                    : "Нолик победил!\nСыграем ещё?";

                await ShowResultDialogAsync(title, message, "Ещё раз", "Выйти", startNextOnYes: true);
            }

            return true;
        }

        if (!TicTacToeRules.IsDraw(board))
            return false;

        _isRoundActive = false;
        _draws++;
        SetCellsClickable(false);
        UpdateDisplays();

        TicTacToeSounds.PlayDrawGame();
        await ShowResultDialogAsync(
            "Ничья!",
            "Поле заполнено — это ничья.\nСыграем ещё?",
            "Ещё раз",
            "Выйти",
            startNextOnYes: true);

        return true;
    }

    private void HighlightWinningLine(int[]? line)
    {
        if (line is null) return;

        foreach (var index in line)
            Cells[index].IsWinning = true;
    }

    private void SetCellsClickable(bool clickable)
    {
        foreach (var cell in Cells)
            cell.IsClickable = clickable;
    }

    private async Task ShowResultDialogAsync(
        string title,
        string message,
        string yesLabel,
        string noLabel,
        bool startNextOnYes)
    {
        var confirmed = await GameDialog.ShowAsync(title, message, yesLabel, noLabel);
        if (!confirmed) return;

        if (startNextOnYes)
            StartNewRound(resetLevel: false);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
