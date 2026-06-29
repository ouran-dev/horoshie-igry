using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.MemoryGame.Helpers;
using HoroshieIgry.Games.MemoryGame.Models;

namespace HoroshieIgry.Games.MemoryGame;

/// <summary>Экран мини-игры «Память».</summary>
public partial class MemoryGameView : UserControl, INotifyPropertyChanged
{
    private const string GameId = "memory";
    private const int InitialGridSize = 3;
    private const int MaxGridSize = 8;
    private const int MismatchDelayMs = 900;
    private const int MatchFadeMs = 450;
    private const int PreviewBaseMs = 2000;
    private const int PreviewExtraPerLevelMs = 500;
    private const int FlipCloseBufferMs = 350;

    private static readonly string[] AllSymbols =
    {
        "🐶", "🐱", "🐭", "🐰", "🦊", "🐻", "🐼", "🐸",
        "🐯", "🦁", "🐮", "🐷", "🐔", "🐧", "🐦", "🐢",
        "🐠", "🦋", "🐝", "🌸", "🌻", "🍎", "🍌", "🚗",
        "✈️", "🎈", "⚽", "🎸", "🌈", "⭐", "🍓", "🎁"
    };

    private readonly INavigationContext _navigation;
    private CancellationTokenSource? _roundCts;
    private int _gridSize = InitialGridSize;
    private int _totalPairs;
    private int _moves;
    private int _matchedPairs;
    private bool _isInputLocked;
    private CardModel? _firstSelected;
    private CardModel? _secondSelected;
    private double _cardSize = 128;

    public ObservableCollection<CardModel> Cards { get; } = new();

    public int GridSize
    {
        get => _gridSize;
        private set
        {
            if (_gridSize == value) return;
            _gridSize = value;
            OnPropertyChanged(nameof(GridSize));
            OnPropertyChanged(nameof(LevelTitle));
            OnPropertyChanged(nameof(BoardWidth));
            OnPropertyChanged(nameof(BoardHeight));
            UpdateScaledLayout();
        }
    }

    public double CardSize
    {
        get => _cardSize;
        private set
        {
            if (Math.Abs(_cardSize - value) < 0.5) return;
            _cardSize = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoardWidth));
            OnPropertyChanged(nameof(BoardHeight));
        }
    }

    public double BoardWidth => GridSize * (CardSize + CardMargin * 2);
    public double BoardHeight => GridSize * (CardSize + CardMargin * 2);

    private double BaseCardSize => _gridSize switch
    {
        3 => 148, 4 => 128, 5 => 108, 6 => 92, 7 => 80, _ => 68
    };

    public double CardMargin => _gridSize <= 4 ? 10 : 7;
    public string LevelTitle => $"Поле {GridSize}×{GridSize}";
    public string MovesDisplay => _moves.ToString();
    public string PairsDisplay => $"{_matchedPairs} / {_totalPairs}";
    public string PreviewHint => _isInputLocked ? "Запоминай карточки…" : string.Empty;
    public Visibility PreviewHintVisibility => _isInputLocked ? Visibility.Visible : Visibility.Collapsed;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MemoryGameView(INavigationContext navigation)
    {
        _navigation = navigation;
        InitializeComponent();
        DataContext = this;
        Loaded += OnFirstLoaded;
        Unloaded += (_, _) => CancelRound();
        SizeChanged += MemoryGameView_SizeChanged;
    }

    private void MemoryGameView_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateScaledLayout();

    private void UpdateScaledLayout()
    {
        if (ActualWidth < 1 || ActualHeight < 1) return;

        var margin = CardMargin;
        var n = GridSize;
        var availW = Math.Max(0, LayoutRoot.ActualWidth > 0 ? LayoutRoot.ActualWidth - 16 : ActualWidth - 16);
        var availH = LayoutRoot.RowDefinitions.Count > 2 && LayoutRoot.RowDefinitions[2].ActualHeight > 40
            ? LayoutRoot.RowDefinitions[2].ActualHeight - 8
            : Math.Max(0, ActualHeight - 200);

        var maxFromW = (availW - n * margin * 2) / n;
        var maxFromH = (availH - n * margin * 2) / n;
        var fitted = Math.Min(maxFromW, maxFromH);

        CardSize = Math.Max(44, fitted);
    }

    private async void OnFirstLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnFirstLoaded;
        await BeginRoundAsync(resetLevel: true);
    }

    private void CancelRound()
    {
        _roundCts?.Cancel();
        _roundCts?.Dispose();
        _roundCts = null;
    }

    private async Task BeginRoundAsync(bool resetLevel)
    {
        CancelRound();
        _roundCts = new CancellationTokenSource();
        var ct = _roundCts.Token;

        if (resetLevel) GridSize = InitialGridSize;

        ApplyRoundBackground();
        _moves = 0;
        _matchedPairs = 0;
        _firstSelected = null;
        _secondSelected = null;
        _isInputLocked = true;
        UpdatePreviewHint();

        UpdateCounters();
        GenerateAndShuffleCards();

        try
        {
            await Task.Delay(GetPreviewDurationMs(), ct);

            foreach (var card in Cards)
            {
                if (!card.IsEmpty)
                    card.IsOpen = false;
            }

            await Task.Delay(FlipCloseBufferMs, ct);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (ct.IsCancellationRequested) return;

        _isInputLocked = false;
        UpdatePreviewHint();
    }

    private int GetPreviewDurationMs()
        => PreviewBaseMs + (_gridSize - InitialGridSize) * PreviewExtraPerLevelMs;

    private void UpdatePreviewHint()
    {
        OnPropertyChanged(nameof(PreviewHint));
        OnPropertyChanged(nameof(PreviewHintVisibility));
    }

    private static int GetPairCount(int gridSize)
    {
        var totalCells = gridSize * gridSize;
        return totalCells % 2 == 0 ? totalCells / 2 : (totalCells - 1) / 2;
    }

    private static bool HasEmptyCell(int gridSize) => (gridSize * gridSize) % 2 != 0;

    private void GenerateAndShuffleCards()
    {
        Cards.Clear();
        _totalPairs = GetPairCount(_gridSize);
        var symbolsForLevel = AllSymbols.Take(_totalPairs).ToArray();

        var cardList = new List<CardModel>();
        var id = 0;

        foreach (var symbol in symbolsForLevel)
        {
            var frontColor = CardTheme.GetFrontColor(symbol);
            cardList.Add(new CardModel { Id = id++, Symbol = symbol, FrontColor = frontColor, IsOpen = true });
            cardList.Add(new CardModel { Id = id++, Symbol = symbol, FrontColor = frontColor, IsOpen = true });
        }

        if (HasEmptyCell(_gridSize))
            cardList.Add(new CardModel { Id = id++, IsEmpty = true });

        var random = new Random();
        for (var i = cardList.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (cardList[i], cardList[j]) = (cardList[j], cardList[i]);
        }

        foreach (var card in cardList) Cards.Add(card);
    }

    private void UpdateCounters()
    {
        OnPropertyChanged(nameof(MovesDisplay));
        OnPropertyChanged(nameof(PairsDisplay));
    }

    private async void NewGameButton_Click(object sender, RoutedEventArgs e)
        => await BeginRoundAsync(resetLevel: true);

    private void BackToCatalogButton_Click(object sender, RoutedEventArgs e) => _navigation.NavigateToCatalog();

    private async void FlipMemoryCard_CardClicked(object sender, CardModel card)
    {
        if (_isInputLocked || card.IsEmpty) return;
        if (!card.IsClickable || card.IsOpen || card.IsMatched) return;

        card.IsOpen = true;

        if (_firstSelected is null) { _firstSelected = card; return; }
        if (_firstSelected == card) return;

        _secondSelected = card;
        _moves++;
        UpdateCounters();
        _isInputLocked = true;

        if (_firstSelected.Symbol == _secondSelected.Symbol)
            await HandleMatchAsync(_firstSelected, _secondSelected);
        else
            await HandleMismatchAsync(_firstSelected, _secondSelected);

        _firstSelected = null;
        _secondSelected = null;
        _isInputLocked = false;

        if (_matchedPairs == _totalPairs) await ShowWinMessageAsync();
    }

    private async Task HandleMatchAsync(CardModel first, CardModel second)
    {
        _matchedPairs++;
        UpdateCounters();
        await Task.WhenAll(FadeOutCardAsync(first), FadeOutCardAsync(second));
        first.IsMatched = true;
        second.IsMatched = true;
        first.IsOpen = false;
        second.IsOpen = false;
        first.Opacity = 1;
        second.Opacity = 1;
    }

    private Task FadeOutCardAsync(CardModel card)
    {
        var tcs = new TaskCompletionSource<bool>();
        var startTime = DateTime.UtcNow;
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        timer.Tick += (_, _) =>
        {
            var progress = Math.Min((DateTime.UtcNow - startTime).TotalMilliseconds / MatchFadeMs, 1.0);
            card.Opacity = 1.0 - progress;
            if (progress >= 1.0) { timer.Stop(); tcs.TrySetResult(true); }
        };
        timer.Start();
        return tcs.Task;
    }

    private async Task HandleMismatchAsync(CardModel first, CardModel second)
    {
        await Task.Delay(MismatchDelayMs);
        first.IsOpen = false;
        second.IsOpen = false;
    }

    private async Task ShowWinMessageAsync()
    {
        var nextSize = GridSize + 1;
        var canLevelUp = nextSize <= MaxGridSize;

        var title = canLevelUp ? "Поздравляем!" : "Все уровни пройдены!";
        var message = canLevelUp
            ? $"Все пары найдены!\nПоле {GridSize}×{GridSize} · ходов {_moves}"
            : $"Отличная игра!\nПоле {GridSize}×{GridSize} · ходов {_moves}\n\nНачать сначала с поля 3×3?";

        var yesLabel = canLevelUp ? "Следующий уровень" : "Сначала 3×3";
        var noLabel = "Выйти";

        var confirmed = await WinDialog.ShowAsync(title, message, yesLabel, noLabel);
        if (!confirmed) return;

        if (canLevelUp) { GridSize = nextSize; await BeginRoundAsync(resetLevel: false); }
        else await BeginRoundAsync(resetLevel: true);
    }

    private void ApplyRoundBackground()
        => _navigation.SetBackgroundTheme(GameBackgroundRotator.ForRound(GameId, _gridSize));

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
