using System.ComponentModel;
using System.Runtime.CompilerServices;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.FindColorGame.Helpers;

namespace HoroshieIgry.Games.FindColorGame.Models;

/// <summary>Фигура на игровом поле.</summary>
public sealed class ColorFigureModel : INotifyPropertyChanged
{
    private bool _isClickable = true;
    private bool _isWrongFlash;
    private bool _isCorrectFlash;

    public int Id { get; init; }
    public KenneyPalette Palette { get; init; }
    public ColorFigureShape Shape { get; init; }
    public bool IsTarget { get; init; }

    public string ShapeAssetPath => ColorGameAssets.GetShapePath(Palette, Shape);
    public string TileAssetPath => ColorGameAssets.GetTilePath(Palette);

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

    public bool IsWrongFlash
    {
        get => _isWrongFlash;
        set
        {
            if (_isWrongFlash == value) return;
            _isWrongFlash = value;
            OnPropertyChanged();
        }
    }

    public bool IsCorrectFlash
    {
        get => _isCorrectFlash;
        set
        {
            if (_isCorrectFlash == value) return;
            _isCorrectFlash = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
