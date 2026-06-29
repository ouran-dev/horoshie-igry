using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.MemoryGame.Models;

namespace HoroshieIgry.Games.MemoryGame.Controls;

public partial class FlipMemoryCard : UserControl
{
    private const int FlipHalfDurationMs = 140;

    private CardModel? _card;
    private bool _isAnimating;
    private bool _isPressed;

    public event EventHandler<CardModel>? CardClicked;

    public FlipMemoryCard()
    {
        InitializeComponent();
        Background = Brushes.Transparent;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UnsubscribeFromCard();
        if (e.NewValue is CardModel card)
        {
            _card = card;
            _card.PropertyChanged += Card_PropertyChanged;
            ApplyCardState();
            UpdateFace(showFront: card.IsOpen, animate: false);
        }
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateFontSizes();

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        KenneySvg.ApplyTo(BackImage, KenneyPaths.MemoryCardBack);
        KenneySvg.ApplyTo(FrontImage, KenneyPaths.MemoryCardFront);
        KenneySvg.ApplyTo(ShadowImage, KenneyPaths.MemoryCardBack);

        if (_card is not null)
        {
            ApplyCardState();
            UpdateFace(showFront: _card.IsOpen, animate: false);
        }
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e) => UnsubscribeFromCard();

    private void UnsubscribeFromCard()
    {
        if (_card is not null)
        {
            _card.PropertyChanged -= Card_PropertyChanged;
            _card = null;
        }
    }

    private async void Card_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not CardModel card) return;

        if (e.PropertyName is nameof(CardModel.IsMatched) or nameof(CardModel.IsEmpty))
        {
            ApplyCardState();
            return;
        }

        if (e.PropertyName != nameof(CardModel.IsOpen)) return;
        if (card.IsMatched) { ApplyCardState(); return; }

        await RunFlipAsync(card.IsOpen);
    }

    private void CardButton_Click(object sender, RoutedEventArgs e)
    {
        if (_card is not null && _card.IsClickable && !_isAnimating)
            CardClicked?.Invoke(this, _card);
    }

    private void CardShell_MouseEnter(object sender, MouseEventArgs e) => ApplyHoverScale(true);

    private void CardShell_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!_isPressed) ApplyHoverScale(false);
    }

    private void CardButton_MouseEnter(object sender, MouseEventArgs e) => ApplyHoverScale(true);

    private void CardButton_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!_isPressed) ApplyHoverScale(false);
    }

    private void ApplyHoverScale(bool isHovered)
    {
        if (_card?.IsClickable != true || _isAnimating) return;
        SetPressScale(isHovered ? 1.03 : 1);
    }

    private void CardButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (CardButton.IsEnabled) { _isPressed = true; SetPressScale(0.94); }
    }

    private void CardButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isPressed = false; SetPressScale(1);
    }

    private void CardButton_PreviewTouchDown(object sender, TouchEventArgs e)
    {
        if (CardButton.IsEnabled) { _isPressed = true; SetPressScale(0.94); }
    }

    private void CardButton_PreviewTouchUp(object sender, TouchEventArgs e)
    {
        _isPressed = false; SetPressScale(1);
    }

    private void ApplyCardState()
    {
        if (_card?.IsSlotHidden == true)
        {
            CardShell.Visibility = Visibility.Collapsed;
            CardButton.Visibility = Visibility.Collapsed;
            return;
        }

        CardShell.Visibility = Visibility.Visible;
        CardButton.Visibility = Visibility.Visible;
        UpdateFontSizes();
    }

    private void UpdateFontSizes()
    {
        if (_card?.IsSlotHidden == true || ActualWidth < 24)
        {
            return;
        }

        FrontSymbol.FontSize = Math.Clamp(ActualWidth * 0.46, 12, 72);
        BackSymbol.FontSize = Math.Clamp(ActualWidth * 0.38, 10, 60);
    }

    private void SetPressScale(double scale)
    {
        PressScale.ScaleX = scale;
        PressScale.ScaleY = scale;
    }

    private void UpdateFace(bool showFront, bool animate)
    {
        ResetFlipTransform();
        BackFace.Visibility = showFront ? Visibility.Collapsed : Visibility.Visible;
        FrontFace.Visibility = showFront ? Visibility.Visible : Visibility.Collapsed;
        if (_card is not null) FrontSymbol.Text = _card.Symbol;
        if (animate) _ = RunFlipAsync(showFront);
    }

    private void ResetFlipTransform()
    {
        FlipScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        FlipScale.ScaleX = 1;
        FlipScale.ScaleY = 1;
    }

    private async Task RunFlipAsync(bool showFront)
    {
        if (_isAnimating || _card?.IsSlotHidden == true) return;

        _isAnimating = true;
        CardButton.IsEnabled = false;

        await AnimateScaleAsync(FlipScale, 1, 0);
        BackFace.Visibility = showFront ? Visibility.Collapsed : Visibility.Visible;
        FrontFace.Visibility = showFront ? Visibility.Visible : Visibility.Collapsed;
        if (_card is not null) FrontSymbol.Text = _card.Symbol;
        await AnimateScaleAsync(FlipScale, 0, 1);
        ResetFlipTransform();

        _isAnimating = false;
        if (_card is not null) CardButton.IsEnabled = _card.IsClickable;
    }

    private static Task AnimateScaleAsync(ScaleTransform transform, double from, double to)
    {
        var tcs = new TaskCompletionSource<bool>();
        var animation = new DoubleAnimation
        {
            From = from, To = to,
            Duration = TimeSpan.FromMilliseconds(FlipHalfDurationMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        animation.Completed += (_, _) =>
        {
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            transform.ScaleX = to;
            tcs.TrySetResult(true);
        };
        transform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        return tcs.Task;
    }
}
