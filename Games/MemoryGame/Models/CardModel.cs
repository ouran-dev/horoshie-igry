namespace HoroshieIgry.Games.MemoryGame.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>Модель одной карточки в игре «Память».</summary>
public class CardModel : INotifyPropertyChanged
{
    private bool _isOpen;
    private bool _isMatched;
    private double _opacity = 1.0;

    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string FrontColor { get; set; } = "#FFF8E7";
    public bool IsEmpty { get; set; }

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value) return;
            _isOpen = value;
            NotifyDisplayChanged();
        }
    }

    public bool IsMatched
    {
        get => _isMatched;
        set
        {
            if (_isMatched == value) return;
            _isMatched = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsClickable));
            OnPropertyChanged(nameof(IsSlotHidden));
        }
    }

    public bool IsSlotHidden => IsEmpty || IsMatched;

    public double Opacity
    {
        get => _opacity;
        set
        {
            if (Math.Abs(_opacity - value) < 0.001) return;
            _opacity = value;
            OnPropertyChanged();
        }
    }

    public string DisplayText => IsOpen ? Symbol : "?";
    public bool IsClickable => !IsEmpty && !IsOpen && !IsMatched;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyDisplayChanged()
    {
        OnPropertyChanged(nameof(IsOpen));
        OnPropertyChanged(nameof(DisplayText));
        OnPropertyChanged(nameof(IsClickable));
    }
}
