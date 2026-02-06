using Models;
using Enums;
using slsk_batchdl.Gui.Models;

namespace slsk_batchdl.Gui.Services;

public class DownloadService
{
    private DownloaderApplication? _app;
    private Task? _runTask;

    public bool IsRunning => _runTask is { IsCompleted: false };
    public DownloaderApplication? App => _app;
    public Exception? Error { get; private set; }

    public Task StartAsync(string input, AppSettings settings)
    {
        if (IsRunning)
            throw new InvalidOperationException("Download already in progress.");

        Error = null;

        var args = BuildArgs(input, settings);
        var config = new Config(args);
        _app = new DownloaderApplication(config);

        _runTask = Task.Run(async () =>
        {
            try
            {
                await _app.RunAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected on cancel
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        });

        return Task.CompletedTask;
    }

    public void Cancel()
    {
        _app?.Cancel();
    }

    public async Task WaitForCompletionAsync()
    {
        if (_runTask != null)
            await _runTask;
    }

    public List<Track> GetAllTracks()
    {
        var tracks = new List<Track>();
        var trackLists = _app?.trackLists;
        if (trackLists == null) return tracks;

        foreach (var tle in trackLists.lists)
        {
            if (tle.list == null) continue;
            foreach (var group in tle.list)
            {
                foreach (var track in group)
                {
                    tracks.Add(track);
                }
            }
        }

        return tracks;
    }

    private static string[] BuildArgs(string input, AppSettings settings)
    {
        var args = new List<string> { input };

        if (!string.IsNullOrWhiteSpace(settings.SoulseekUsername))
        {
            args.Add("--user");
            args.Add(settings.SoulseekUsername);
        }

        if (!string.IsNullOrWhiteSpace(settings.SoulseekPassword))
        {
            args.Add("--pass");
            args.Add(settings.SoulseekPassword);
        }

        args.Add("-p");
        args.Add(settings.DownloadPath);

        args.Add("--pref-format");
        args.Add(settings.PreferredFormat.ToLower());

        args.Add("--pref-min-bitrate");
        args.Add(settings.PreferredBitrate.ToString());

        args.Add("--no-progress");

        // Pick a random high port to avoid conflict with other Soulseek clients
        args.Add("--listen-port");
        args.Add(new Random().Next(50000, 65000).ToString());

        return args.ToArray();
    }
}
