using System.Windows;
using System.Windows.Controls;

namespace HoroshieIgry.Games.PaperPlaneGame.Controls;

public partial class PaperPlaneCrashOverlay : UserControl
{
    public event EventHandler? RetryRequested;
    public event EventHandler? CatalogRequested;

    public PaperPlaneCrashOverlay()
    {
        InitializeComponent();
    }

    public void Show() => Visibility = Visibility.Visible;
    public void Hide() => Visibility = Visibility.Collapsed;

    private void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        RetryRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CatalogButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        CatalogRequested?.Invoke(this, EventArgs.Empty);
    }
}
