using System.Windows;
using HoroshieIgry.Core.Installation;
using Velopack;

namespace HoroshieIgry;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build()
            .SetArgs(args)
            .SetAutoApplyOnStartup(true)
            .OnFirstRun(_ => DesktopShortcutHelper.OfferCreateShortcutOnFirstRun())
            .Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
