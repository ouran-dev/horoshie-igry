using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using HoroshieIgry.Controls.Kenney;
using HoroshieIgry.Core.Mazes;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Games.MazeGame.Controls;

public partial class MazeBoardControl : UserControl
{
    private const string CharacterEmoji = "🐻";
    private const double TouchLogicStep = 0.28;
    private const double MouseLogicStep = 0.42;
    private const double TouchDisplaySmoothing = 0.34;
    private const double MouseDisplaySmoothing = 0.52;
    private const double WobbleAmplitude = 4.0;

    private static readonly SolidColorBrush PathBrush = new(Color.FromRgb(0xFF, 0xF3, 0xC4));
    private static readonly SolidColorBrush WallBrush = new(Color.FromRgb(0x43, 0xA0, 0x47));
    private static readonly SolidColorBrush WallStrokeBrush = new(Color.FromRgb(0x2E, 0x7D, 0x32));

    private readonly DispatcherTimer _smoothTimer;
    private readonly Grid _characterHost;
    private readonly KenneyImage _characterTile;
    private readonly TextBlock _characterEmoji;
    private readonly Border _exitGlow;
    private readonly TextBlock _exitMarker;

    private MazeDefinition? _maze;
    private double _cellSize = 44;
    private double _displayRow;
    private double _displayCol;
    private double _logicRow;
    private double _logicCol;
    private double _fingerRow;
    private double _fingerCol;
    private bool _hasFingerTarget;
    private bool _isDragging;
    private bool _isCompleted;
    private int _activeTouchId = -1;
    private bool _mouseDragging;
    private DateTime _wobbleStart = DateTime.UtcNow;

    public event EventHandler? MazeCompleted;

    public MazeBoardControl()
    {
        InitializeComponent();
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);
        Focusable = true;

        _characterTile = new KenneyImage
        {
            AssetPath = KenneyPaths.InPalette(KenneyPalette.Blue, "button_round_depth_flat.svg"),
            Stretch = Stretch.Fill,
            IsHitTestVisible = false
        };

        _characterEmoji = new TextBlock
        {
            FontFamily = new FontFamily("Segoe UI Emoji"),
            FontSize = 34,
            Text = CharacterEmoji,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };

        _characterHost = new Grid
        {
            IsHitTestVisible = false,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new TranslateTransform()
        };
        _characterHost.Children.Add(_characterTile);
        _characterHost.Children.Add(_characterEmoji);

        _exitGlow = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x66, 0xFF, 0xD5, 0x4F)),
            CornerRadius = new CornerRadius(18),
            IsHitTestVisible = false
        };

        _exitMarker = new TextBlock
        {
            FontFamily = new FontFamily("Segoe UI Emoji"),
            FontSize = 40,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };

        BoardCanvas.Children.Add(_exitGlow);
        BoardCanvas.Children.Add(_exitMarker);
        BoardCanvas.Children.Add(_characterHost);

        _smoothTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _smoothTimer.Tick += SmoothTimer_Tick;
        _smoothTimer.Start();
    }

    public void LoadMaze(MazeDefinition maze)
    {
        _maze = maze;
        _isCompleted = false;
        _isDragging = false;
        _mouseDragging = false;
        _hasFingerTarget = false;
        _exitMarker.Text = maze.ExitEmoji;
        ResetCharacter();
        RedrawBoard();
    }

    public void ResetCharacter()
    {
        if (_maze is null) return;

        (_logicRow, _logicCol) = _maze.StartCenter;
        _displayRow = _logicRow;
        _displayCol = _logicCol;
        _fingerRow = _logicRow;
        _fingerCol = _logicCol;
        _hasFingerTarget = false;
        UpdateCharacterVisual();
    }

    private void Root_SizeChanged(object sender, SizeChangedEventArgs e) => RedrawBoard();

    private void RedrawBoard()
    {
        BoardCanvas.Children.Clear();
        BoardCanvas.Children.Add(_exitGlow);
        BoardCanvas.Children.Add(_exitMarker);
        BoardCanvas.Children.Add(_characterHost);

        if (_maze is null || ActualWidth < 1 || ActualHeight < 1) return;

        _cellSize = Math.Floor(Math.Min(ActualWidth / _maze.Cols, ActualHeight / _maze.Rows));
        _cellSize = Math.Max(36, _cellSize);

        var boardWidth = _cellSize * _maze.Cols;
        var boardHeight = _cellSize * _maze.Rows;
        var offsetX = (ActualWidth - boardWidth) / 2;
        var offsetY = (ActualHeight - boardHeight) / 2;
        var gap = Math.Clamp(_cellSize * 0.08, 2, 6);

        BoardCanvas.Children.Insert(0, new Rectangle
        {
            Width = boardWidth,
            Height = boardHeight,
            RadiusX = 16,
            RadiusY = 16,
            Fill = PathBrush,
            IsHitTestVisible = false
        });
        Canvas.SetLeft(BoardCanvas.Children[0], offsetX);
        Canvas.SetTop(BoardCanvas.Children[0], offsetY);

        for (var row = 0; row < _maze.Rows; row++)
        {
            for (var col = 0; col < _maze.Cols; col++)
            {
                if (!_maze.Walls[row, col]) continue;

                var rect = new Rectangle
                {
                    Width = Math.Max(8, _cellSize - gap),
                    Height = Math.Max(8, _cellSize - gap),
                    RadiusX = 10,
                    RadiusY = 10,
                    Fill = WallBrush,
                    Stroke = WallStrokeBrush,
                    StrokeThickness = 2,
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(rect, offsetX + col * _cellSize + gap / 2);
                Canvas.SetTop(rect, offsetY + row * _cellSize + gap / 2);
                BoardCanvas.Children.Insert(BoardCanvas.Children.Count - 3, rect);
            }
        }

        var exitSize = _cellSize * 0.92;
        _exitGlow.Width = exitSize;
        _exitGlow.Height = exitSize;
        Canvas.SetLeft(_exitGlow, offsetX + _maze.Exit.Col * _cellSize + (_cellSize - exitSize) / 2);
        Canvas.SetTop(_exitGlow, offsetY + _maze.Exit.Row * _cellSize + (_cellSize - exitSize) / 2);

        Canvas.SetLeft(_exitMarker, offsetX + _maze.Exit.Col * _cellSize + _cellSize * 0.22);
        Canvas.SetTop(_exitMarker, offsetY + _maze.Exit.Row * _cellSize + _cellSize * 0.16);

        var hostSize = _cellSize * 0.88;
        _characterHost.Width = hostSize;
        _characterHost.Height = hostSize;
        _characterEmoji.FontSize = Math.Clamp(_cellSize * 0.42, 28, 52);

        UpdateCharacterVisual();
    }

    private void SmoothTimer_Tick(object? sender, EventArgs e)
    {
        if (_maze is null) return;

        if ((_isDragging || _mouseDragging) && _hasFingerTarget && !_isCompleted)
        {
            var step = _mouseDragging ? MouseLogicStep : TouchLogicStep;
            var (nextRow, nextCol) = MazeWalker.StepTowardFinger(
                _maze, _logicRow, _logicCol, _fingerRow, _fingerCol, step);
            _logicRow = nextRow;
            _logicCol = nextCol;

            if (_maze.IsAtExit(_logicRow, _logicCol))
                CompleteMaze();
        }

        var smoothing = _mouseDragging ? MouseDisplaySmoothing : TouchDisplaySmoothing;
        _displayRow += (_logicRow - _displayRow) * smoothing;
        _displayCol += (_logicCol - _displayCol) * smoothing;

        if (Math.Abs(_logicRow - _displayRow) < 0.003)
            _displayRow = _logicRow;
        if (Math.Abs(_logicCol - _displayCol) < 0.003)
            _displayCol = _logicCol;

        UpdateCharacterVisual();
    }

    private void UpdateCharacterVisual()
    {
        if (_maze is null) return;

        var (offsetX, offsetY) = GetBoardOffset();

        var wobble = 0.0;
        if (_isDragging || _mouseDragging)
        {
            var phase = (DateTime.UtcNow - _wobbleStart).TotalMilliseconds / 110.0;
            wobble = Math.Sin(phase) * WobbleAmplitude;
        }

        Canvas.SetLeft(_characterHost, offsetX + _displayCol * _cellSize + (_cellSize - _characterHost.Width) / 2);
        Canvas.SetTop(_characterHost, offsetY + _displayRow * _cellSize + (_cellSize - _characterHost.Height) / 2 + wobble);

        if (_characterHost.RenderTransform is TranslateTransform transform)
            transform.X = wobble * 0.15;
    }

    private void Root_PreviewTouchDown(object sender, TouchEventArgs e)
    {
        if (_isCompleted || _maze is null || _isDragging) return;

        var pos = e.GetTouchPoint(this).Position;
        if (!TryBeginDrag(pos, forMouse: false)) return;

        _activeTouchId = e.TouchDevice.Id;
        _isDragging = true;
        _wobbleStart = DateTime.UtcNow;
        ApplyFingerPosition(pos);
        CaptureTouch(e.TouchDevice);
        e.Handled = true;
    }

    private void Root_PreviewTouchMove(object sender, TouchEventArgs e)
    {
        if (!_isDragging || e.TouchDevice.Id != _activeTouchId || _maze is null || _isCompleted)
            return;

        ApplyFingerPosition(e.GetTouchPoint(this).Position);
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
        if (e.StylusDevice is not null || _isCompleted || _maze is null || _mouseDragging) return;

        var pos = e.GetPosition(this);
        if (!TryBeginDrag(pos, forMouse: true)) return;

        _mouseDragging = true;
        _wobbleStart = DateTime.UtcNow;
        Focus();
        ApplyFingerPosition(pos);
        CaptureMouse();
        e.Handled = true;
    }

    private void Root_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_mouseDragging || e.StylusDevice is not null || _maze is null || _isCompleted) return;
        if (e.LeftButton != MouseButtonState.Pressed) return;

        ApplyFingerPosition(e.GetPosition(this));
        e.Handled = true;
    }

    private void Root_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_mouseDragging) return;
        EndDrag();
        ReleaseMouseCapture();
        e.Handled = true;
    }

    private void Root_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (_mouseDragging)
            EndDrag();
    }

    private bool TryBeginDrag(Point position, bool forMouse)
    {
        if (_maze is null) return false;

        if (forMouse && IsNearCharacterOnScreen(position))
            return true;

        if (!TryPointToMaze(position, out var row, out var col, allowOutside: forMouse))
            return false;

        return MazeWalker.CanGrabCharacter(_logicRow, _logicCol, row, col);
    }

    private bool IsNearCharacterOnScreen(Point position)
    {
        var center = GetCharacterScreenCenter();
        var grabRadius = Math.Max(_characterHost.Width, _characterHost.Height) * 0.75;
        var dx = position.X - center.X;
        var dy = position.Y - center.Y;
        return dx * dx + dy * dy <= grabRadius * grabRadius;
    }

    private Point GetCharacterScreenCenter()
    {
        var (offsetX, offsetY) = GetBoardOffset();
        return new Point(
            offsetX + _displayCol * _cellSize + _cellSize / 2,
            offsetY + _displayRow * _cellSize + _cellSize / 2);
    }

    private (double OffsetX, double OffsetY) GetBoardOffset()
    {
        if (_maze is null)
            return (0, 0);

        var boardWidth = _cellSize * _maze.Cols;
        var boardHeight = _cellSize * _maze.Rows;
        return ((ActualWidth - boardWidth) / 2, (ActualHeight - boardHeight) / 2);
    }

    private void ApplyFingerPosition(Point position)
    {
        if (_maze is null) return;

        if (!TryPointToMaze(position, out var row, out var col, allowOutside: true))
            return;

        var nearest = MazeWalker.FindNearestWalkable(_maze, row, col);
        if (!nearest.IsValid)
            return;

        _fingerRow = nearest.Row + 0.5;
        _fingerCol = nearest.Col + 0.5;
        _hasFingerTarget = true;
    }

    private void EndDrag()
    {
        _isDragging = false;
        _mouseDragging = false;
        _activeTouchId = -1;
        _hasFingerTarget = false;
    }

    private void CompleteMaze()
    {
        if (_isCompleted) return;
        _isCompleted = true;
        EndDrag();
        MazeCompleted?.Invoke(this, EventArgs.Empty);
    }

    private bool TryPointToMaze(Point position, out double row, out double col, bool allowOutside)
    {
        row = 0;
        col = 0;
        if (_maze is null || _cellSize < 1) return false;

        var (offsetX, offsetY) = GetBoardOffset();
        var boardWidth = _cellSize * _maze.Cols;
        var boardHeight = _cellSize * _maze.Rows;

        var localX = position.X - offsetX;
        var localY = position.Y - offsetY;

        if (localX < 0 || localY < 0 || localX > boardWidth || localY > boardHeight)
        {
            if (!allowOutside) return false;

            col = localX / _cellSize;
            row = localY / _cellSize;
            return true;
        }

        col = localX / _cellSize;
        row = localY / _cellSize;
        return true;
    }
}
