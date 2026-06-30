using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HoroshieIgry.Games.SortingGame.Helpers;
using HoroshieIgry.Games.SortingGame.Models;

namespace HoroshieIgry.Games.SortingGame.Controls;

public partial class SortingPlayfieldControl : UserControl
{
    public static readonly GridLength BasketRowHeightValue =
        new(SortingLevelGenerator.BasketRowHeight);

    private readonly Dictionary<int, SortItemControl> _items = new();
    private readonly List<SortBasketControl> _baskets = new();

    private SortLevelPlan? _plan;
    private SortItemControl? _dragItem;
    private TouchDevice? _activeTouch;
    private Point _dragPointerOffset;
    private int _sortedCount;
    private bool _isFinishingDrag;

    public event EventHandler? LevelCompleted;

    public SortingPlayfieldControl()
    {
        InitializeComponent();
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);

        PreviewMouseMove += OnPointerMove;
        PreviewMouseLeftButtonUp += OnPointerUp;
        PreviewTouchMove += OnTouchMove;
        PreviewTouchUp += OnTouchUp;
        LostMouseCapture += OnLostMouseCapture;

        Unloaded += (_, _) => ReleaseCapture();
    }

    private void OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        if (_dragItem is null || _isFinishingDrag || _activeTouch is not null)
            return;

        _ = FinishDragAsync();
    }

    public void LoadLevel(SortLevelPlan plan)
    {
        _plan = plan;
        _sortedCount = 0;
        _isFinishingDrag = false;
        _dragItem = null;
        _activeTouch = null;
        _items.Clear();
        _baskets.Clear();
        BasketsPanel.Children.Clear();
        ItemsCanvas.Children.Clear();
        DragCanvas.Children.Clear();
        BasketsPanel.Columns = plan.Baskets.Count;

        Width = SortingLevelGenerator.DesignWidth;
        Height = SortingLevelGenerator.DesignHeight;

        foreach (var basketPlan in plan.Baskets.OrderBy(b => b.SlotIndex))
        {
            var basket = new SortBasketControl
            {
                BasketPlan = basketPlan,
                Margin = new Thickness(6, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Stretch
            };
            _baskets.Add(basket);
            BasketsPanel.Children.Add(basket);
        }

        foreach (var itemPlan in plan.Items)
        {
            var item = new SortItemControl
            {
                ItemPlan = itemPlan,
                Width = SortingLevelGenerator.ItemSize,
                Height = SortingLevelGenerator.ItemSize
            };
            item.DragStarted += OnItemDragStarted;
            Canvas.SetLeft(item, itemPlan.HomeX);
            Canvas.SetTop(item, itemPlan.HomeY);
            ItemsCanvas.Children.Add(item);
            _items[itemPlan.Id] = item;
        }
    }

    private void OnItemDragStarted(object? sender, SortItemDragEventArgs e)
    {
        var item = e.Item;
        if (_isFinishingDrag || _dragItem is not null || item.ItemPlan is null) return;
        if (!_items.ContainsKey(item.ItemPlan.Id)) return;

        _dragItem = item;
        _activeTouch = e.TouchDevice;
        EnsureItemOnDragLayer(item);

        var pointer = GetPointerOnDragCanvas();
        var left = Canvas.GetLeft(item);
        var top = Canvas.GetTop(item);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;

        _dragPointerOffset = new Point(pointer.X - left, pointer.Y - top);
        item.SetDragVisual(true);
        SortingSounds.PlayGrab();
        CapturePointer();
        MoveDragItem(pointer);
    }

    private Point GetPointerOnDragCanvas()
    {
        if (_activeTouch is not null)
            return _activeTouch.GetTouchPoint(DragCanvas).Position;

        return Mouse.GetPosition(DragCanvas);
    }

    private void CapturePointer()
    {
        if (_activeTouch is not null)
            _activeTouch.Capture(this);
        else
            Mouse.Capture(this);
    }

    private void EnsureItemOnDragLayer(SortItemControl item)
    {
        if (DragCanvas.Children.Contains(item))
            return;

        MoveItemToDragLayer(item);
    }

    private void MoveItemToDragLayer(SortItemControl item)
    {
        var left = Canvas.GetLeft(item);
        var top = Canvas.GetTop(item);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;

        ItemsCanvas.Children.Remove(item);
        DragCanvas.Children.Add(item);
        item.SnapPosition(left, top);
    }

    private void MoveItemToFieldLayer(SortItemControl item, double x, double y)
    {
        if (DragCanvas.Children.Contains(item))
            DragCanvas.Children.Remove(item);

        if (!ItemsCanvas.Children.Contains(item))
            ItemsCanvas.Children.Add(item);

        item.SnapPosition(x, y);
    }

    private void OnPointerMove(object sender, MouseEventArgs e)
    {
        if (_dragItem is null || _isFinishingDrag) return;

        if (_activeTouch is not null)
        {
            if (_activeTouch.Captured == this)
                return;

            _activeTouch = null;
            if (Mouse.Captured != this)
                Mouse.Capture(this);
        }

        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        MoveDragItem(e.GetPosition(DragCanvas));
    }

    private void OnTouchMove(object? sender, TouchEventArgs e)
    {
        if (_dragItem is null || _isFinishingDrag) return;
        MoveDragItem(e.GetTouchPoint(DragCanvas).Position);
        e.Handled = true;
    }

    private void MoveDragItem(Point pointerOnDragCanvas)
    {
        if (_dragItem is null) return;

        var (minX, minY, maxX, maxY) = GetItemDragBounds();
        var x = Math.Clamp(pointerOnDragCanvas.X - _dragPointerOffset.X, minX, maxX);
        var y = Math.Clamp(pointerOnDragCanvas.Y - _dragPointerOffset.Y, minY, maxY);

        _dragItem.SnapPosition(x, y);
    }

    private (double MinX, double MinY, double MaxX, double MaxY) GetItemDragBounds()
    {
        var itemSize = SortingLevelGenerator.ItemSize;
        var maxX = Math.Max(0, DragCanvas.ActualWidth - itemSize);
        var maxY = Math.Max(0, DragCanvas.ActualHeight - itemSize);
        var minY = -SortingLevelGenerator.BasketRowHeight;
        return (0, minY, maxX, maxY);
    }

    private async void OnPointerUp(object sender, MouseButtonEventArgs e)
    {
        if (_dragItem is null || _isFinishingDrag) return;

        if (_activeTouch is not null)
        {
            if (_activeTouch.Captured == this)
                return;

            _activeTouch = null;
        }

        await FinishDragAsync();
        e.Handled = true;
    }

    private async void OnTouchUp(object? sender, TouchEventArgs e)
    {
        if (_dragItem is null || _isFinishingDrag) return;
        if (_activeTouch is not null && e.TouchDevice != _activeTouch) return;
        await FinishDragAsync();
        e.Handled = true;
    }

    private async Task FinishDragAsync()
    {
        var item = _dragItem;
        if (item?.ItemPlan is null || _isFinishingDrag) return;

        _isFinishingDrag = true;
        _dragItem = null;
        ReleaseCapture();
        item.SetDragVisual(false);
        EnsureItemOnDragLayer(item);

        try
        {
            var basket = HitTestBasket(GetItemCenterOnRoot(item));
            if (basket is not null && basket.CategoryId == item.ItemPlan.CategoryId)
            {
                SortingSounds.PlaySuccess();
                item.SetDragVisual(false);
                item.IsHitTestVisible = false;

                _items.Remove(item.ItemPlan.Id);
                _sortedCount++;

                _ = PlaySuccessResolutionAsync(item, basket);
                _ = basket.PlaySuccessGlowAsync();

                if (_sortedCount >= (_plan?.Items.Count ?? 0))
                    LevelCompleted?.Invoke(this, EventArgs.Empty);

                return;
            }

            if (basket is not null)
            {
                basket.PlayWrongWiggle();
                SortingSounds.PlayReturn();
            }
            else
            {
                SortingSounds.PlayReturn();
            }

            await item.AnimateToAsync(item.ItemPlan.HomeX, item.ItemPlan.HomeY, 180);
            MoveItemToFieldLayer(item, item.ItemPlan.HomeX, item.ItemPlan.HomeY);
        }
        finally
        {
            _isFinishingDrag = false;
        }
    }

    private async Task PlaySuccessResolutionAsync(SortItemControl item, SortBasketControl basket)
    {
        try
        {
            _ = item.PlaySuccessStarsAsync();

            var basketCenter = basket.TranslatePoint(basket.GetDropCenter(), DragCanvas);
            var itemSize = SortingLevelGenerator.ItemSize;
            await item.AnimateToAsync(
                basketCenter.X - itemSize / 2,
                basketCenter.Y - itemSize / 2,
                120);
            await item.FadeOutAsync(100);

            if (DragCanvas.Children.Contains(item))
                DragCanvas.Children.Remove(item);
            else if (ItemsCanvas.Children.Contains(item))
                ItemsCanvas.Children.Remove(item);
        }
        catch
        {
            // Фоновая анимация — не мешаем следующему ходу.
        }
    }

    private Point GetItemCenterOnRoot(SortItemControl item)
    {
        var size = SortingLevelGenerator.ItemSize;
        return item.TranslatePoint(new Point(size / 2, size / 2), RootGrid);
    }

    private SortBasketControl? HitTestBasket(Point pointOnRoot)
    {
        foreach (var basket in _baskets)
        {
            var center = basket.TranslatePoint(basket.GetDropCenter(), RootGrid);
            var dx = pointOnRoot.X - center.X;
            var dy = pointOnRoot.Y - center.Y;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist <= basket.DropRadius)
                return basket;
        }

        return null;
    }

    private void ReleaseCapture()
    {
        if (_activeTouch is not null)
        {
            _activeTouch.Capture(null);
            _activeTouch = null;
        }

        if (Mouse.Captured == this)
            Mouse.Capture(null);
    }
}
