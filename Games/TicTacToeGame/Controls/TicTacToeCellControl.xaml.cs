using System.Windows;
using System.Windows.Controls;

namespace HoroshieIgry.Games.TicTacToeGame.Controls;

public partial class TicTacToeCellControl : UserControl
{
    public TicTacToeCellControl()
    {
        InitializeComponent();
    }

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
}
