using System.Windows;
using System.Windows.Controls;
using HoroshieIgry.Controls.Kenney;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Navigation;

namespace HoroshieIgry.Views;

public partial class MainMenuView : UserControl
{
    private readonly INavigationService _navigation;

    public MainMenuView(INavigationService navigation, GameCatalog catalog)
    {
        InitializeComponent();
        _navigation = navigation;
        GamesList.ItemsSource = catalog.Games;
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is KenneyButton { DataContext: IGameModule game } && game.IsAvailable)
        {
            _navigation.NavigateToGame(game);
        }
    }
}
