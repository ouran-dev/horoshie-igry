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

    private int _level = 1;
    private int _playerWins;
    private int _aiWins;
    private int _draws;
    private bool _isPlayerTurn = true;
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
    public string LevelTitle => $"Поле 3×3 · сила {_level}";
    public string ScoreDisplay => $"Ты {_playerWins} : {_aiWins} компьютер · ничьи {_draws}";
    public string HintText => _hintText;
    public string HintSymbolPath => TicTacToeAssets.GetSymbolPath(
        _isPlayerTurn ? TicTacToeAssets.PlayerMark : TicTacToeAssets.AiMark);

    public event PropertyChangedEventHandler? PropertyChanged;

    public TicTacToeGameView(INavigationContext navigation)
    {
        _navigation = navigation;
        InitializeComponent();
        DataContext = this;
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);
        Loaded += (_, _) => StartNewRound(resetLevel: true);
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

    private void StartNewRound(bool resetLevel)
    {
        CancelRound();
        _roundCts = new CancellationTokenSource();

        if (resetLevel)
        {
            _level = 1;
            _playerWins = 0;
            _aiWins = 0;
            _draws = 0;
        }

        ResetBoard();
        _isPlayerTurn = true;
        _isRoundActive = true;
        _isInputLocked = false;
        _hintText = "Твой ход — крестик.";
        UpdateDisplays();
        ApplyRoundBackground();
        UpdateScaledLayout();
    }

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
        OnPropertyChanged(nameof(HintText));
        OnPropertyChanged(nameof(HintSymbolPath));
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e)
        => StartNewRound(resetLevel: true);

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
        if (!_isRoundActive || _isInputLocked || !_isPlayerTurn) return false;

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
        _ = ProcessPlayerMoveAsync(cellIndex.Value);
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

    private async Task ProcessPlayerMoveAsync(int cellIndex)
    {
        _isInputLocked = true;
        var ct = _roundCts?.Token ?? CancellationToken.None;

        try
        {
            ApplyMark(cellIndex, TicTacToeAssets.PlayerMark);

            if (await TryFinishRoundAsync(TicTacToeAssets.PlayerMark)) return;

            _isPlayerTurn = false;
            _hintText = "Ход компьютера…";
            UpdateDisplays();

            await Task.Delay(AiMoveDelayMs, ct);
            if (ct.IsCancellationRequested) return;

            var board = TicTacToeRules.ReadMarks(Cells);
            var aiMove = TicTacToeAi.PickMove(board, TicTacToeAssets.AiMark, _level, _random);
            if (aiMove >= 0)
                ApplyMark(aiMove, TicTacToeAssets.AiMark);

            if (await TryFinishRoundAsync(TicTacToeAssets.AiMark)) return;

            _isPlayerTurn = true;
            _hintText = "Твой ход — крестик.";
            UpdateDisplays();
            _isInputLocked = false;
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    private void ApplyMark(int index, TicTacToeMark mark)
        => Cells[index].Mark = mark;

    private async Task<bool> TryFinishRoundAsync(TicTacToeMark lastMark)
    {
        var board = TicTacToeRules.ReadMarks(Cells);

        if (TicTacToeRules.TryGetWinner(board, out var winner, out var line))
        {
            _isRoundActive = false;
            HighlightWinningLine(line);
            SetCellsClickable(false);

            if (winner == TicTacToeAssets.PlayerMark)
            {
                _playerWins++;
                _level++;
                UpdateDisplays();
                await PraiseOverlay.PlayAsync();
                await ShowResultDialogAsync(
                    "Ты выиграл!",
                    $"Отличная партия!\nСчёт: {_playerWins} : {_aiWins}",
                    "Ещё раз",
                    "Выйти",
                    startNextOnYes: true);
            }
            else
            {
                _aiWins++;
                UpdateDisplays();
                await ShowResultDialogAsync(
                    "Компьютер выиграл",
                    "В следующий раз получится!\nПопробуй ещё раз.",
                    "Ещё раз",
                    "Выйти",
                    startNextOnYes: true);
            }

            return true;
        }

        if (!TicTacToeRules.IsDraw(board))
            return false;

        _isRoundActive = false;
        _draws++;
        SetCellsClickable(false);
        UpdateDisplays();

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
