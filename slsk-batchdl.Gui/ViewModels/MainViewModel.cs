using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace slsk_batchdl.Gui.ViewModels;

public partial class MainViewModel : ObservableObject
{
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
    private string _selectedMode = "Normal";

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private string _statusText = "Ready";

    public ObservableCollection<DownloadItemViewModel> Downloads { get; } = new();

    public string[] AvailableFormats { get; } = ["MP3", "FLAC", "OGG", "M4A", "OPUS", "WAV"];
    public int[] AvailableBitrates { get; } = [128, 192, 256, 320];
    public string[] AvailableModes { get; } = ["Normal", "Album", "Aggregate", "Album Aggregate"];

    public int CompletedCount => Downloads.Count(d => d.IsCompleted);
    public int TotalCount => Downloads.Count;
    public double OverallProgress => TotalCount > 0
        ? (double)CompletedCount / TotalCount * 100
        : 0;

    [RelayCommand(CanExecute = nameof(CanStartDownload))]
    private async Task StartDownload()
    {
        if (string.IsNullOrWhiteSpace(SearchInput)) return;

        IsDownloading = true;
        StatusText = $"Searching: {SearchInput}";

        // TODO: Wire up to DownloaderApplication in next commit
        // For now, add dummy items to show the UI works
        Downloads.Clear();
        var demoTracks = new[]
        {
            "Demo Track 1",
            "Demo Track 2",
            "Demo Track 3",
        };

        foreach (var track in demoTracks)
        {
            var item = new DownloadItemViewModel(track);
            Downloads.Add(item);
        }

        // Simulate progress
        foreach (var item in Downloads)
        {
            item.Status = DownloadStatus.Downloading;
            item.ProgressPercent = 0;

            for (int i = 0; i <= 100; i += 20)
            {
                item.ProgressPercent = i;
                await Task.Delay(100);
            }

            item.Status = DownloadStatus.Completed;
            item.ProgressPercent = 100;
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(OverallProgress));
        }

        IsDownloading = false;
        StatusText = $"Done: {CompletedCount}/{TotalCount} tracks downloaded";
    }

    private bool CanStartDownload() => !IsDownloading && !string.IsNullOrWhiteSpace(SearchInput);

    partial void OnSearchInputChanged(string value) => StartDownloadCommand.NotifyCanExecuteChanged();
    partial void OnIsDownloadingChanged(bool value) => StartDownloadCommand.NotifyCanExecuteChanged();

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
}
