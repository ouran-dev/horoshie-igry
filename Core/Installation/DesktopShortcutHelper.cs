using System.IO;
using System.Windows;

namespace HoroshieIgry.Core.Installation;

/// <summary>Создание ярлыка с выбором папки (первый запуск после установки).</summary>
public static class DesktopShortcutHelper
{
    private const string MarkerFileName = ".shortcut-setup-done";

    public static void OfferCreateShortcutOnFirstRun()
    {
        try
        {
            var marker = Path.Combine(AppContext.BaseDirectory, MarkerFileName);
            if (File.Exists(marker))
                return;

            var create = MessageBox.Show(
                "Создать ярлык «Хорошие игры» на рабочем столе?\n\nДалее можно выбрать папку для ярлыка.",
                "Ярлык на рабочем столе",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (create != MessageBoxResult.Yes)
            {
                File.WriteAllText(marker, DateTime.UtcNow.ToString("O"));
                return;
            }

            var folder = PickShortcutFolder();
            if (string.IsNullOrWhiteSpace(folder))
            {
                File.WriteAllText(marker, DateTime.UtcNow.ToString("O"));
                return;
            }

            var exePath = Path.Combine(AppContext.BaseDirectory, "HoroshieIgry.exe");
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Brand", "AppIcon.ico");
            if (!File.Exists(iconPath))
                iconPath = exePath;

            var linkPath = Path.Combine(folder, "Хорошие игры.lnk");
            CreateShellLink(linkPath, exePath, iconPath, AppContext.BaseDirectory);
            File.WriteAllText(marker, DateTime.UtcNow.ToString("O"));
        }
        catch
        {
            // Не мешаем запуску игры из-за ярлыка.
        }
    }

    public static void MarkShortcutConfigured()
    {
        try
        {
            var marker = Path.Combine(AppContext.BaseDirectory, MarkerFileName);
            File.WriteAllText(marker, DateTime.UtcNow.ToString("O"));
        }
        catch
        {
            // ignore
        }
    }

    private static string? PickShortcutFolder()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Выберите папку, куда положить ярлык «Хорошие игры»",
            UseDescriptionForTitle = true,
            SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        };

        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            ? dialog.SelectedPath
            : null;
    }

    private static void CreateShellLink(string linkPath, string targetPath, string iconPath, string workingDir)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("WScript.Shell недоступен.");

        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic shortcut = shell.CreateShortcut(linkPath);
        shortcut.TargetPath = targetPath;
        shortcut.WorkingDirectory = workingDir;
        shortcut.IconLocation = iconPath;
        shortcut.Description = "Хорошие игры";
        shortcut.Save();

        if (shortcut is IDisposable disposable)
            disposable.Dispose();

        if (shell is IDisposable shellDisposable)
            shellDisposable.Dispose();
    }
}
