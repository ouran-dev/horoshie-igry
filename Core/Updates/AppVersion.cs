using System.Reflection;

namespace HoroshieIgry.Core.Updates;

public static class AppVersion
{
    public static string Display
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version is null ? "—" : $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
