using System.Windows;
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
            .Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
