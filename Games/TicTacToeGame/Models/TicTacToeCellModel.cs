using System.ComponentModel;
using System.Runtime.CompilerServices;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.TicTacToeGame.Helpers;

namespace HoroshieIgry.Games.TicTacToeGame.Models;

public sealed class TicTacToeCellModel : INotifyPropertyChanged
{
    private TicTacToeMark _mark = TicTacToeMark.Empty;
    private bool _isWinning;
    private bool _isClickable = true;

    public int Index { get; init; }

    public TicTacToeMark Mark
    {
        get => _mark;
        set
        {
            if (_mark == value) return;
            _mark = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SymbolAssetPath));
            OnPropertyChanged(nameof(HasSymbol));
        }
    }

    public bool IsWinning
    {
        get => _isWinning;
        set
        {
            if (_isWinning == value) return;
            _isWinning = value;
            OnPropertyChanged();
        }
    }

    public bool IsClickable
    {
        get => _isClickable;
        set
        {
            if (_isClickable == value) return;
            _isClickable = value;
            OnPropertyChanged();
        }
    }

    public bool HasSymbol => _mark != TicTacToeMark.Empty;

    public string SymbolAssetPath => TicTacToeAssets.GetSymbolPath(_mark);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
