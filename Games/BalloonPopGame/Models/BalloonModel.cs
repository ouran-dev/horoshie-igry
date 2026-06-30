using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HoroshieIgry.Games.BalloonPopGame.Models;

public sealed class BalloonModel : INotifyPropertyChanged
{
    private bool _isPopped;
    private bool _isFadingOut;
    private double _opacity = 1;

    public required int Id { get; init; }
    public required BalloonColor Color { get; init; }
    public required double X { get; init; }
    public required double Y { get; init; }
    public required double Size { get; init; }
    public required double AnimPhase { get; init; }
    public required bool IsTarget { get; init; }

    public bool IsPopped
    {
        get => _isPopped;
        set
        {
            if (_isPopped == value) return;
            _isPopped = value;
            OnPropertyChanged();
        }
    }

    public bool IsFadingOut
    {
        get => _isFadingOut;
        set
        {
            if (_isFadingOut == value) return;
            _isFadingOut = value;
            OnPropertyChanged();
        }
    }

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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
