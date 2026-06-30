using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using HoroshieIgry.Controls.Kenney;
using HoroshieIgry.Core.Mazes;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.MazeGame.Helpers;

namespace HoroshieIgry.Games.MazeGame.Controls;

public partial class MazeBoardControl : UserControl
{
    private const string CharacterEmoji = "🐻";
    private const double DesignCellSize = 40;

    private static readonly SolidColorBrush PathBrush = new(Color.FromRgb(0xFF, 0xF3, 0xC4));
    private static readonly SolidColorBrush WallBrush = new(Color.FromRgb(0x43, 0xA0, 0x47));
    private static readonly SolidColorBrush WallStrokeBrush = new(Color.FromRgb(0x2E, 0x7D, 0x32));

    private readonly Grid _characterHost;
    private readonly KenneyImage _characterTile;
    private readonly TextBlock _characterEmoji;
    private readonly Grid _exitHost;
    private readonly Border _exitGlow;
    private readonly TextBlock _exitMarker;

    private MazeDefinition? _maze;
    private double _cellSize = DesignCellSize;
    private double _logicRow;
    private double _logicCol;
    private double _grabOffsetX;
    private double _grabOffsetY;
    private bool _isCompleted;
    private bool _isDragging;
    private int _activeTouchId = -1;
    private bool _mouseDragging;
    private int _lastStepRow = -1;
    private int _lastStepCol = -1;

    public event EventHandler? MazeCompleted;

    public MazeBoardControl()
    {
        InitializeComponent();
        Stylus.SetIsPressAndHoldEnabled(this, false);
        Stylus.SetIsFlicksEnabled(this, false);
        Focusable = true;
        Cursor = Cursors.Hand;

        _characterTile = new KenneyImage
        {
            AssetPath = KenneyPaths.InPalette(KenneyPalette.Blue, "button_round_depth_flat.svg"),
            Stretch = Stretch.Fill,
            IsHitTestVisible = false
        };

        _characterEmoji = new TextBlock
        {
            FontFamily = new FontFamily("Segoe UI Emoji"),
            FontSize = 28,
            Text = CharacterEmoji,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };

        _characterHost = new Grid { IsHitTestVisible = false };
        _characterHost.Children.Add(_characterTile);
        _characterHost.Children.Add(_characterEmoji);

        _exitGlow = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xD5, 0x4F)),
            CornerRadius = new CornerRadius(999),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            IsHitTestVisible = false
        };

        _exitMarker = new TextBlock
        {
            FontFamily = new FontFamily("Segoe UI Emoji"),
            FontSize = 32,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };

        _exitHost = new Grid { IsHitTestVisible = false };
        _exitHost.Children.Add(_exitGlow);
        _exitHost.Children.Add(_exitMarker);

        BoardCanvas.Children.Add(_exitHost);
        BoardCanvas.Children.Add(_characterHost);
    }

    public void LoadMaze(MazeDefinition maze)
    {
        _maze = maze;
        _isCompleted = false;
        _isDragging = false;
        _mouseDragging = false;
        _activeTouchId = -1;
        _lastStepRow = -1;
        _lastStepCol = -1;
        _exitMarker.Text = maze.ExitEmoji;

        Width = maze.Cols * DesignCellSize;
        Height = maze.Rows * DesignCellSize;
        _cellSize = DesignCellSize;

        ResetCharacter();
        RedrawBoard();
    }

    public void ResetCharacter()
    {
        if (_maze is null) return;

        (_logicRow, _logicCol) = _maze.StartCenter;
        _lastStepRow = (int)_logicRow;
        _lastStepCol = (int)_logicCol;
        UpdateCharacterVisual();
    }

    private void RedrawBoard()
    {
        BoardCanvas.Children.Clear();
        BoardCanvas.Children.Add(_exitHost);
        BoardCanvas.Children.Add(_characterHost);

        if (_maze is null) return;

        var boardWidth = _maze.Cols * _cellSize;
        var boardHeight = _maze.Rows * _cellSize;
        var gap = Math.Clamp(_cellSize * 0.1, 2, 4);

        BoardCanvas.Children.Insert(0, new Rectangle
        {
            Width = boardWidth,
            Height = boardHeight,
            RadiusX = 14,
            RadiusY = 14,
            Fill = PathBrush,
            IsHitTestVisible = false
        });

        for (var row = 0; row < _maze.Rows; row++)
        {
            for (var col = 0; col < _maze.Cols; col++)
            {
                if (!_maze.Walls[row, col]) continue;

                var wall = new Rectangle
                {
                    Width = Math.Max(6, _cellSize - gap),
                    Height = Math.Max(6, _cellSize - gap),
                    RadiusX = 8,
                    RadiusY = 8,
                    Fill = WallBrush,
                    Stroke = WallStrokeBrush,
                    StrokeThickness = 2,
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(wall, col * _cellSize + gap / 2);
                Canvas.SetTop(wall, row * _cellSize + gap / 2);
                BoardCanvas.Children.Insert(BoardCanvas.Children.Count - 2, wall);
            }
        }

        var tokenSize = _cellSize * 0.72;
        _characterHost.Width = tokenSize;
        _characterHost.Height = tokenSize;
        _characterEmoji.FontSize = Math.Clamp(_cellSize * 0.34, 22, 36);

        var exitSize = _cellSize * 0.78;
        _exitHost.Width = exitSize;
        _exitHost.Height = exitSize;
        _exitMarker.FontSize = Math.Clamp(_cellSize * 0.38, 24, 38);

        var exitCenterCol = (_maze.Exit.Col + 0.5) * _cellSize;
        var exitCenterRow = (_maze.Exit.Row + 0.5) * _cellSize;
        Canvas.SetLeft(_exitHost, exitCenterCol - exitSize / 2);
        Canvas.SetTop(_exitHost, exitCenterRow - exitSize / 2);

        UpdateCharacterVisual();
    }

    private void UpdateCharacterVisual()
    {
        if (_maze is null) return;

        var centerX = _logicCol * _cellSize;
        var centerY = _logicRow * _cellSize;

        Panel.SetZIndex(_characterHost, 10);
        Canvas.SetLeft(_characterHost, centerX - _characterHost.Width / 2);
        Canvas.SetTop(_characterHost, centerY - _characterHost.Height / 2);
    }

    private void Root_PreviewTouchDown(object sender, TouchEventArgs e)
    {
        if (_isCompleted || _maze is null || _isDragging) return;

        var pos = e.GetTouchPoint(this).Position;
        if (!CanGrabCharacter(pos)) return;

        _activeTouchId = e.TouchDevice.Id;
        _isDragging = true;
        BeginDrag(pos);
        CaptureTouch(e.TouchDevice);
        e.Handled = true;
    }

    private void Root_PreviewTouchMove(object sender, TouchEventArgs e)
    {
        if (!_isDragging || e.TouchDevice.Id != _activeTouchId || _maze is null || _isCompleted)
            return;

        ApplyPointerDrag(e.GetTouchPoint(this).Position);
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
        if (!CanGrabCharacter(pos)) return;

        _mouseDragging = true;
        Focus();
        BeginDrag(pos);
        CaptureMouse();
        e.Handled = true;
    }

    private void Root_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_mouseDragging || e.StylusDevice is not null || _maze is null || _isCompleted) return;
        if (e.LeftButton != MouseButtonState.Pressed) return;

        ApplyPointerDrag(e.GetPosition(this));
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

    private bool CanGrabCharacter(Point position)
    {
        var centerX = _logicCol * _cellSize;
        var centerY = _logicRow * _cellSize;
        var half = Math.Max(_characterHost.Width, _characterHost.Height) / 2 + 6;
        var dx = position.X - centerX;
        var dy = position.Y - centerY;
        return dx * dx + dy * dy <= half * half;
    }

    private void BeginDrag(Point position)
    {
        MazeSounds.PlayGrab();
        var centerX = _logicCol * _cellSize;
        var centerY = _logicRow * _cellSize;
        _grabOffsetX = position.X - centerX;
        _grabOffsetY = position.Y - centerY;
        ApplyPointerDrag(position);
    }

    private void ApplyPointerDrag(Point position)
    {
        if (_maze is null || _isCompleted) return;

        var wantCol = (position.X - _grabOffsetX) / _cellSize;
        var wantRow = (position.Y - _grabOffsetY) / _cellSize;
        wantCol = Math.Clamp(wantCol, 0.05, _maze.Cols - 0.05);
        wantRow = Math.Clamp(wantRow, 0.05, _maze.Rows - 0.05);

        var (row, col) = MazeFreeMovement.MoveTo(_maze, _logicRow, _logicCol, wantRow, wantCol);
        var stepRow = (int)row;
        var stepCol = (int)col;
        if (stepRow != _lastStepRow || stepCol != _lastStepCol)
        {
            _lastStepRow = stepRow;
            _lastStepCol = stepCol;
            MazeSounds.PlayStep();
        }

        _logicRow = row;
        _logicCol = col;
        UpdateCharacterVisual();

        if (MazeFreeMovement.IsNearExit(_maze, _logicRow, _logicCol))
            CompleteMaze();
    }

    private void EndDrag()
    {
        _isDragging = false;
        _mouseDragging = false;
        _activeTouchId = -1;
    }

    private void CompleteMaze()
    {
        if (_isCompleted) return;
        _isCompleted = true;
        EndDrag();
        ReleaseMouseCapture();
        MazeCompleted?.Invoke(this, EventArgs.Empty);
    }
}
