using System.Windows;
using HoroshieIgry.Core.Games;
using HoroshieIgry.Core.Updates;

namespace HoroshieIgry;

public partial class App : Application
{
    public static GameCatalog Catalog { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        Catalog = new GameCatalog();
        GameCatalogRegistrar.RegisterAll(Catalog);

        AppUpdateService.Instance.Initialize();
        base.OnStartup(e);
    }
}
