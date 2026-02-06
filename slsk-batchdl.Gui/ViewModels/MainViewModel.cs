using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using slsk_batchdl.Gui.Models;
using slsk_batchdl.Gui.Services;

namespace slsk_batchdl.Gui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SettingsService _settingsService = new();
    private DownloadService? _downloadService;
    private DispatcherTimer? _pollTimer;
    private bool _tracksPopulated;

    [ObservableProperty]
    private string _searchInput = "";

    [ObservableProperty]
    private string _downloadPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
        "sldl");

    [ObservableProperty]
    private string _selectedFormat = "MP3";

    [ObservableProperty]
    private int _preferredBitrate = 320;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private string _statusText = "Ready";

    public ObservableCollection<DownloadItemViewModel> Downloads { get; } = new();

    public string[] AvailableFormats { get; } = ["MP3", "FLAC", "OGG", "M4A", "OPUS", "WAV"];
    public int[] AvailableBitrates { get; } = [128, 192, 256, 320];

    public int CompletedCount => Downloads.Count(d => d.IsCompleted);
    public int TotalCount => Downloads.Count;
    public double OverallProgress => TotalCount > 0
        ? (double)CompletedCount / TotalCount * 100
        : 0;

    public MainViewModel()
    {
        var settings = _settingsService.Load();
        SelectedFormat = settings.PreferredFormat;
        PreferredBitrate = settings.PreferredBitrate;
        DownloadPath = settings.DownloadPath;
    }

    public void ApplySettings(SettingsViewModel settingsVm)
    {
        SelectedFormat = settingsVm.PreferredFormat;
        PreferredBitrate = settingsVm.PreferredBitrate;
        DownloadPath = settingsVm.DownloadPath;
    }

    [RelayCommand(CanExecute = nameof(CanStartDownload))]
    private async Task StartDownload()
    {
        if (string.IsNullOrWhiteSpace(SearchInput)) return;

        // Validate input: must be a URL supported by one of the extractors
        var input = SearchInput.Trim().ToLower();
        bool isSupported = input.Contains("spotify.com")
            || input is "spotify-likes" or "spotify-albums"
            || input.Contains("youtube.com") || input.Contains("youtu.be")
            || input.Contains("bandcamp.com")
            || input.Contains("musicbrainz.org")
            || input.StartsWith("slsk://");

        if (!isSupported)
        {
            StatusText = "Unsupported input. Use a Spotify, YouTube, Bandcamp or MusicBrainz URL";
            return;
        }

        // Validate credentials
        var settings = _settingsService.Load();
        if (string.IsNullOrWhiteSpace(settings.SoulseekUsername) ||
            string.IsNullOrWhiteSpace(settings.SoulseekPassword))
        {
            StatusText = "Set Soulseek credentials in Settings first";
            return;
        }

        // Apply current quick settings to the download
        settings.PreferredFormat = SelectedFormat;
        settings.PreferredBitrate = PreferredBitrate;
        settings.DownloadPath = DownloadPath;

        IsDownloading = true;
        _tracksPopulated = false;
        Downloads.Clear();
        StatusText = $"Starting: {SearchInput}";

        // Wait briefly for any previous run to finish cleanup
        if (_downloadService != null)
        {
            await Task.WhenAny(
                _downloadService.WaitForCompletionAsync(),
                Task.Delay(2000));
            _downloadService = null;
        }

        _downloadService = new DownloadService();
        await _downloadService.StartAsync(SearchInput, settings);

        // Start polling timer
        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _pollTimer.Tick += PollTimer_Tick;
        _pollTimer.Start();
    }

    private void PollTimer_Tick(object? sender, EventArgs e)
    {
        if (_downloadService == null) return;

        // Populate tracks once they're extracted
        if (!_tracksPopulated)
        {
            var tracks = _downloadService.GetAllTracks();
            if (tracks.Count > 0)
            {
                _tracksPopulated = true;
                foreach (var track in tracks)
                {
                    Downloads.Add(new DownloadItemViewModel(track));
                }
                StatusText = $"Downloading: {tracks.Count} tracks";
                OnPropertyChanged(nameof(TotalCount));
            }
        }

        // Update track states + real-time progress from app internals
        var app = _downloadService.App;
        foreach (var item in Downloads)
        {
            item.UpdateFromTrack();

            // Detect searching/downloading from app's live dictionaries
            if (app != null && item.Track != null && item.Status == DownloadStatus.Waiting)
            {
                if (app.searches.ContainsKey(item.Track))
                    item.Status = DownloadStatus.Searching;
            }

            // Update real download progress from DownloadWrapper
            if (app != null && item.Track != null)
            {
                foreach (var (_, wrapper) in app.downloads)
                {
                    if (wrapper.track == item.Track)
                    {
                        item.Status = DownloadStatus.Downloading;
                        if (wrapper.file.Size > 0)
                            item.ProgressPercent = wrapper.bytesTransferred / (double)wrapper.file.Size * 100;
                        item.UpdateFromWrapper(wrapper);
                        break;
                    }
                }
            }
        }

        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(OverallProgress));

        // Check if done
        if (!_downloadService.IsRunning)
        {
            _pollTimer?.Stop();
            _pollTimer = null;
            IsDownloading = false;

            if (_downloadService.Error != null)
            {
                StatusText = $"Error: {_downloadService.Error.Message}";
            }
            else
            {
                StatusText = $"Done: {CompletedCount}/{TotalCount} tracks downloaded";
            }
        }
    }

    private bool CanStartDownload() => !IsDownloading && !string.IsNullOrWhiteSpace(SearchInput);

    partial void OnSearchInputChanged(string value) => StartDownloadCommand.NotifyCanExecuteChanged();
    partial void OnIsDownloadingChanged(bool value) => StartDownloadCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void StopDownload()
    {
        _downloadService?.Cancel();

        _pollTimer?.Stop();
        _pollTimer = null;

        foreach (var item in Downloads)
        {
            if (item.Status is DownloadStatus.Waiting or DownloadStatus.Searching or DownloadStatus.Downloading)
                item.Status = DownloadStatus.Cancelled;
        }

        IsDownloading = false;
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(OverallProgress));
        StatusText = $"Cancelled: {CompletedCount}/{TotalCount} tracks downloaded";
    }

    [RelayCommand]
    private void BrowseDownloadPath()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Download Folder",
            InitialDirectory = DownloadPath
        };

        if (dialog.ShowDialog() == true)
        {
            DownloadPath = dialog.FolderName;
        }
    }

    [RelayCommand]
    private void OpenLogFile()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "sldl-gui", "logs");
        var logFile = Path.Combine(logDir, $"sldl-gui_{DateTime.Now:yyyy-MM-dd}.log");

        if (File.Exists(logFile))
        {
            Process.Start(new ProcessStartInfo(logFile) { UseShellExecute = true });
        }
        else
        {
            Process.Start(new ProcessStartInfo(logDir) { UseShellExecute = true });
        }
    }
}
