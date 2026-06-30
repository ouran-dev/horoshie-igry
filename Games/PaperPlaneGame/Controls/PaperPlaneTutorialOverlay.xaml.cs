using System.Windows.Controls;

namespace HoroshieIgry.Games.PaperPlaneGame.Controls;

public partial class PaperPlaneTutorialOverlay : UserControl
{
    public PaperPlaneTutorialOverlay()
    {
        InitializeComponent();
    }

    public void Show() => Visibility = System.Windows.Visibility.Visible;
    public void Hide() => Visibility = System.Windows.Visibility.Collapsed;
}
