using System.Windows;
using System.Windows.Controls;
using HoroshieIgry.Core.Navigation;

namespace HoroshieIgry.Views;

/// <summary>Заглушка для игр, которые ещё в разработке.</summary>
public partial class ComingSoonGameView : UserControl
{
    private readonly INavigationContext _navigation;

    public string GameIcon { get; }
    public string GameTitle { get; }
    public string GameDescription { get; }

    public ComingSoonGameView(INavigationContext navigation, string title, string description, string iconEmoji)
    {
        GameIcon = iconEmoji;
        GameTitle = title;
        GameDescription = description;
        _navigation = navigation;
        InitializeComponent();
        DataContext = this;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _navigation.NavigateToCatalog();
    }
}
