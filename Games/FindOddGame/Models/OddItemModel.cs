using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using HoroshieIgry.Core.Objects;
using HoroshieIgry.Games.FindOddGame.Models;

namespace HoroshieIgry.Games.FindOddGame.Models;

/// <summary>Один вариант ответа на экране.</summary>
public sealed class OddItemModel : INotifyPropertyChanged
{
    private bool _isClickable = true;
    private bool _isWrongFlash;
    private bool _isCorrectFlash;
    private bool _isShaking;

    public string ObjectId { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Emoji { get; init; } = string.Empty;
    public ImageSource? ObjectImage { get; init; }
    public bool IsOdd { get; init; }
    public string TileAssetPath { get; init; } = string.Empty;

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

    public bool IsShaking
    {
        get => _isShaking;
        set
        {
            if (_isShaking == value) return;
            _isShaking = value;
            OnPropertyChanged();
        }
    }

    public bool HasImage => ObjectImage is not null;

    public static OddItemModel FromPlan(OddRoundItemPlan plan, string tileAssetPath)
        => new()
        {
            ObjectId = plan.ObjectId,
            Label = plan.Label,
            Emoji = plan.Emoji,
            IsOdd = plan.IsOdd,
            TileAssetPath = tileAssetPath,
            ObjectImage = plan.ImageRelativePath is null
                ? null
                : GameObjectImageLoader.Load(new GameObjectEntry
                {
                    Id = plan.ObjectId,
                    CategoryId = plan.CategoryId,
                    Label = plan.Label,
                    Emoji = plan.Emoji,
                    ImageRelativePath = plan.ImageRelativePath
                })
        };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
