using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Controls;

/// <summary>Компактный логотип организации для угла экрана.</summary>
public partial class OrganizationLogo : UserControl
{
    public OrganizationLogo()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadLogo();
    }

    private void LoadLogo()
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, BrandPaths.Logo.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            return;
        }

        LogoImage.Source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
    }
}
