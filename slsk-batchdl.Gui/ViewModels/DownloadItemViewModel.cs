using CommunityToolkit.Mvvm.ComponentModel;

namespace slsk_batchdl.Gui.ViewModels;

public enum DownloadStatus
{
    Waiting,
    Searching,
    Downloading,
    Completed,
    Failed,
    Skipped
}

public partial class DownloadItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _trackName;

    [ObservableProperty]
    private DownloadStatus _status = DownloadStatus.Waiting;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _filePath = "";

    [ObservableProperty]
    private string _errorMessage = "";

    public string StatusDisplay => Status switch
    {
        DownloadStatus.Waiting => "Waiting",
        DownloadStatus.Searching => "Searching...",
        DownloadStatus.Downloading => $"{ProgressPercent:F0}%",
        DownloadStatus.Completed => "Done",
        DownloadStatus.Failed => "Failed",
        DownloadStatus.Skipped => "Skipped",
        _ => ""
    };

    public bool IsCompleted => Status == DownloadStatus.Completed;
    public bool IsFailed => Status == DownloadStatus.Failed;
    public bool IsActive => Status is DownloadStatus.Searching or DownloadStatus.Downloading;

    public DownloadItemViewModel(string trackName)
    {
        _trackName = trackName;
    }

    partial void OnStatusChanged(DownloadStatus value)
    {
        OnPropertyChanged(nameof(StatusDisplay));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsFailed));
        OnPropertyChanged(nameof(IsActive));
    }

    partial void OnProgressPercentChanged(double value)
    {
        OnPropertyChanged(nameof(StatusDisplay));
    }
}
