using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using HoroshieIgry.Games.LinkDotsGame.Helpers;
using HoroshieIgry.Games.LinkDotsGame.Models;

namespace HoroshieIgry.Games.LinkDotsGame.Controls;

public partial class LinkDotsBoardControl : UserControl
{
    private const double DesignCellSize = 56;

    private static readonly SolidColorBrush BoardBrush = new(Color.FromRgb(0xFF, 0xF8, 0xE1));
    private static readonly SolidColorBrush GridBrush = new(Color.FromArgb(0x55, 0x8D, 0x6E, 0x63));

    private LinkDotsEngine? _engine;
    private double _cellSize = DesignCellSize;
    private int _activeColor = -1;
    private (int Row, int Col)? _lastCell;
    private bool _isDragging;
    private int _activeTouchId = -1;
    private bool _mouseDragging;
    private bool _isCompleted;
    private bool[] _pairCompleteNotified = [];

    public event EventHandler? PuzzleCompleted;

    public LinkDotsBoardControl()
    {
        InitializeComponent();
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);
        Focusable = true;
        Cursor = Cursors.Hand;
    }

    public void LoadLevel(LinkDotsLevel level)
    {
        if (_engine is not null)
            _engine.StateChanged -= Engine_StateChanged;

        ReleasePointerCapture();
        _isCompleted = false;
        _activeColor = -1;
        _lastCell = null;
        _isDragging = false;
        _mouseDragging = false;
        _activeTouchId = -1;

        _engine = new LinkDotsEngine(level);
        _engine.StateChanged += Engine_StateChanged;
        _pairCompleteNotified = new bool[level.Pairs.Count];

        _cellSize = DesignCellSize;
        Width = level.Cols * _cellSize;
        Height = level.Rows * _cellSize;

        Redraw();
    }

    public void ResetLevel()
    {
        if (_engine is null) return;

        ReleasePointerCapture();
        _isCompleted = false;
        _activeColor = -1;
        _lastCell = null;
        _engine.Reset();
        if (_engine is not null)
            _pairCompleteNotified = new bool[_engine.Level.Pairs.Count];
        Redraw();
    }

    private void Engine_StateChanged()
    {
        Redraw();
        CheckPairCompletions();

        if (_isCompleted || _engine is null || !_engine.IsSolved())
            return;

        _isCompleted = true;
        EndDrag();
        ReleasePointerCapture();
        PuzzleCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void CheckPairCompletions()
    {
        if (_engine is null) return;

        for (var i = 0; i < _engine.Level.Pairs.Count; i++)
        {
            var pair = _engine.Level.Pairs[i];
            var path = _engine.GetPath(pair.ColorId);
            var complete = path.Count >= 2
                           && path.Contains((pair.StartRow, pair.StartCol))
                           && path.Contains((pair.EndRow, pair.EndCol));

            if (complete && !_pairCompleteNotified[i])
            {
                _pairCompleteNotified[i] = true;
                LinkDotsSounds.PlayLineComplete();
            }
            else if (!complete)
            {
                _pairCompleteNotified[i] = false;
            }
        }
    }

    private void Redraw()
    {
        BoardCanvas.Children.Clear();
        if (_engine is null) return;

        var level = _engine.Level;
        var boardWidth = level.Cols * _cellSize;
        var boardHeight = level.Rows * _cellSize;
        var pipeInset = _cellSize * 0.18;

        BoardCanvas.Children.Add(new Rectangle
        {
            Width = boardWidth,
            Height = boardHeight,
            RadiusX = 16,
            RadiusY = 16,
            Fill = BoardBrush,
            Stroke = new SolidColorBrush(Color.FromRgb(0x8D, 0x6E, 0x63)),
            StrokeThickness = 3,
            IsHitTestVisible = false
        });

        DrawGrid(level);

        for (var colorId = 0; colorId < level.Pairs.Count; colorId++)
        {
            var pair = level.Pairs[colorId];
            var path = _engine.GetPath(colorId);
            var brush = CreateBrush(pair.PathColor, 0.82);

            foreach (var (row, col) in path)
            {
                var cell = CreatePipeCell(row, col, brush);
                BoardCanvas.Children.Add(cell);
            }

            for (var i = 0; i < path.Count - 1; i++)
            {
                var connector = CreateConnector(path[i], path[i + 1], brush);
                if (connector is not null)
                    BoardCanvas.Children.Add(connector);
            }
        }

        foreach (var pair in level.Pairs)
        {
            BoardCanvas.Children.Add(CreateDot(pair.StartRow, pair.StartCol, pair.DotColor));
            BoardCanvas.Children.Add(CreateDot(pair.EndRow, pair.EndCol, pair.DotColor));
        }

        return;

        Brush CreateBrush(Color color, double opacity)
        {
            var brush = new SolidColorBrush(color) { Opacity = opacity };
            brush.Freeze();
            return brush;
        }

        Rectangle CreatePipeCell(int row, int col, Brush brush)
        {
            var size = _cellSize - pipeInset * 2;
            var rect = new Rectangle
            {
                Width = size,
                Height = size,
                RadiusX = size / 2,
                RadiusY = size / 2,
                Fill = brush,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(rect, col * _cellSize + pipeInset);
            Canvas.SetTop(rect, row * _cellSize + pipeInset);
            return rect;
        }

        Shape? CreateConnector((int Row, int Col) from, (int Row, int Col) to, Brush brush)
        {
            var thickness = _cellSize - pipeInset * 2;
            var fromX = from.Col * _cellSize + _cellSize / 2;
            var fromY = from.Row * _cellSize + _cellSize / 2;
            var toX = to.Col * _cellSize + _cellSize / 2;
            var toY = to.Row * _cellSize + _cellSize / 2;

            if (from.Row == to.Row)
            {
                var left = Math.Min(fromX, toX);
                var width = Math.Abs(toX - fromX);
                if (width < 0.5) return null;

                var rect = new Rectangle
                {
                    Width = width,
                    Height = thickness,
                    Fill = brush,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(rect, left);
                Canvas.SetTop(rect, fromY - thickness / 2);
                return rect;
            }

            if (from.Col == to.Col)
            {
                var top = Math.Min(fromY, toY);
                var height = Math.Abs(toY - fromY);
                if (height < 0.5) return null;

                var rect = new Rectangle
                {
                    Width = thickness,
                    Height = height,
                    Fill = brush,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(rect, fromX - thickness / 2);
                Canvas.SetTop(rect, top);
                return rect;
            }

            return null;
        }

        Ellipse CreateDot(int row, int col, Color color)
        {
            var size = _cellSize * 0.46;
            var dot = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(color),
                Stroke = Brushes.White,
                StrokeThickness = 3,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(dot, col * _cellSize + (_cellSize - size) / 2);
            Canvas.SetTop(dot, row * _cellSize + (_cellSize - size) / 2);
            return dot;
        }
    }

    private void DrawGrid(LinkDotsLevel level)
    {
        for (var col = 1; col < level.Cols; col++)
        {
            var line = new Line
            {
                X1 = col * _cellSize,
                Y1 = 8,
                X2 = col * _cellSize,
                Y2 = level.Rows * _cellSize - 8,
                Stroke = GridBrush,
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            BoardCanvas.Children.Add(line);
        }

        for (var row = 1; row < level.Rows; row++)
        {
            var line = new Line
            {
                X1 = 8,
                Y1 = row * _cellSize,
                X2 = level.Cols * _cellSize - 8,
                Y2 = row * _cellSize,
                Stroke = GridBrush,
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            BoardCanvas.Children.Add(line);
        }
    }

    private void Root_PreviewTouchDown(object sender, TouchEventArgs e)
    {
        if (_isCompleted || _engine is null || _isDragging) return;

        if (!TryBeginAt(e.GetTouchPoint(this).Position))
            return;

        _activeTouchId = e.TouchDevice.Id;
        _isDragging = true;
        CaptureTouch(e.TouchDevice);
        e.Handled = true;
    }

    private void Root_PreviewTouchMove(object sender, TouchEventArgs e)
    {
        if (!_isDragging || e.TouchDevice.Id != _activeTouchId || _engine is null || _isCompleted)
            return;

        ApplyPointer(e.GetTouchPoint(this).Position);
        e.Handled = true;
    }

    private void Root_PreviewTouchUp(object sender, TouchEventArgs e)
    {
        if (e.TouchDevice.Id != _activeTouchId) return;
        EndDrag();
        e.Handled = true;
    }

    private void Root_LostTouchCapture(object sender, TouchEventArgs e)
    {
        if (e.TouchDevice.Id == _activeTouchId)
            EndDrag();
    }

    private void Root_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null || _isCompleted || _engine is null || _mouseDragging) return;

        if (!TryBeginAt(e.GetPosition(this)))
            return;

        _mouseDragging = true;
        Focus();
        CaptureMouse();
        e.Handled = true;
    }

    private void Root_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_mouseDragging || e.StylusDevice is not null || _engine is null || _isCompleted) return;
        if (e.LeftButton != MouseButtonState.Pressed) return;

        ApplyPointer(e.GetPosition(this));
        e.Handled = true;
    }

    private void Root_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_mouseDragging) return;
        EndDrag();
        e.Handled = true;
    }

    private void Root_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (_mouseDragging)
            EndDrag();
    }

    private bool TryBeginAt(Point position)
    {
        if (_engine is null) return false;

        var cell = PointToCell(position);
        if (cell is null) return false;

        var color = _engine.GetColorAt(cell.Value.Row, cell.Value.Col);
        if (color is null) return false;

        _activeColor = color.Value;
        _engine.BeginStroke(_activeColor, cell.Value.Row, cell.Value.Col);
        _lastCell = cell;
        LinkDotsSounds.PlayGrab();
        return true;
    }

    private void ApplyPointer(Point position)
    {
        if (_engine is null || _activeColor < 0) return;

        var cell = PointToCell(position);
        if (cell is null) return;

        if (_lastCell == cell) return;

        if (_lastCell is { } last)
        {
            foreach (var step in CellsAlongLine(last, cell.Value))
            {
                if (_engine.TryExtend(_activeColor, step.Row, step.Col))
                    LinkDotsSounds.PlayDraw();
            }
        }
        else if (_engine.TryExtend(_activeColor, cell.Value.Row, cell.Value.Col))
        {
            LinkDotsSounds.PlayDraw();
        }

        _lastCell = cell;
    }

    private (int Row, int Col)? PointToCell(Point position)
    {
        if (_engine is null) return null;

        var col = (int)(position.X / _cellSize);
        var row = (int)(position.Y / _cellSize);
        if (row < 0 || col < 0 || row >= _engine.Level.Rows || col >= _engine.Level.Cols)
            return null;

        return (row, col);
    }

    private static IEnumerable<(int Row, int Col)> CellsAlongLine((int Row, int Col) from, (int Row, int Col) to)
    {
        var row = from.Row;
        var col = from.Col;

        while (row != to.Row || col != to.Col)
        {
            if (row != to.Row)
                row += row < to.Row ? 1 : -1;
            else
                col += col < to.Col ? 1 : -1;

            yield return (row, col);
        }
    }

    private void EndDrag()
    {
        _isDragging = false;
        _mouseDragging = false;
        _activeTouchId = -1;
        _activeColor = -1;
        _lastCell = null;
        ReleasePointerCapture();
    }

    private void ReleasePointerCapture()
    {
        if (IsMouseCaptured)
            ReleaseMouseCapture();
    }
}
