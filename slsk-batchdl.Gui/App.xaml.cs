using System.IO;
using System.Windows;

namespace slsk_batchdl.Gui;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitializeLogging();
    }

    private static void InitializeLogging()
    {
        Logger.SetupExceptionHandling();
        Logger.AddConsole();

        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "sldl-gui", "logs");
        Directory.CreateDirectory(logDir);

        var logFile = Path.Combine(logDir, $"sldl-gui_{DateTime.Now:yyyy-MM-dd}.log");
        Logger.AddFile(logFile, Logger.LogLevel.Debug, prependDate: true, prependLogLevel: true);

        // Clean up logs older than 7 days
        foreach (var file in Directory.GetFiles(logDir, "sldl-gui_*.log"))
        {
            if (File.GetLastWriteTime(file) < DateTime.Now.AddDays(-7))
            {
                try { File.Delete(file); } catch { }
            }
        }
    }
}

