using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HoroshieIgry.Core.UI;
using HoroshieIgry.Games.FindColorGame.Models;

namespace HoroshieIgry.Games.FindColorGame.Controls;

/// <summary>Отображение одной фигуры на поле (нажатия обрабатывает FindColorGameView).</summary>
public partial class ColorFigureControl : UserControl
{
    private ColorFigureModel? _figure;

    public ColorFigureControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += (_, _) => UnsubscribeFromFigure();
    }

    public ColorFigureModel? Figure => _figure;

    public void PlayPressFeedback()
    {
        PressScale.ScaleX = 0.94;
        PressScale.ScaleY = 0.94;

        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(90) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            PressScale.ScaleX = 1;
            PressScale.ScaleY = 1;
        };
        timer.Start();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UnsubscribeFromFigure();
        if (e.NewValue is ColorFigureModel figure)
        {
            _figure = figure;
            _figure.PropertyChanged += Figure_PropertyChanged;
            UpdateFeedback();
            Opacity = figure.IsClickable ? 1 : 0.55;
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e) => UpdateFeedback();

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) { }

    private void UnsubscribeFromFigure()
    {
        if (_figure is not null)
        {
            _figure.PropertyChanged -= Figure_PropertyChanged;
            _figure = null;
        }
    }

    private void Figure_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ColorFigureModel.IsClickable) && _figure is not null)
            Opacity = _figure.IsClickable ? 1 : 0.55;

        if (e.PropertyName is nameof(ColorFigureModel.IsWrongFlash) or nameof(ColorFigureModel.IsCorrectFlash))
            UpdateFeedback();
    }

    private void UpdateFeedback()
    {
        WrongFlash.Visibility = _figure?.IsWrongFlash == true ? Visibility.Visible : Visibility.Collapsed;
        CorrectFlash.Visibility = _figure?.IsCorrectFlash == true ? Visibility.Visible : Visibility.Collapsed;
    }
}
