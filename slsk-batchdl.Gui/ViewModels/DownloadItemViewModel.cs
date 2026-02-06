using CommunityToolkit.Mvvm.ComponentModel;
using Enums;
using Models;

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
    public Track? Track { get; }

    [ObservableProperty]
    private string _trackName;

    [ObservableProperty]
    private DownloadStatus _status = DownloadStatus.Waiting;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _filePath = "";

    [ObservableProperty]
    private string _statusDetail = "";

    public string StatusDisplay => Status switch
    {
        DownloadStatus.Waiting => "Waiting",
        DownloadStatus.Searching => "Searching...",
        DownloadStatus.Downloading => $"{ProgressPercent:F0}%",
        DownloadStatus.Completed => "Done",
        DownloadStatus.Failed => FailureText,
        DownloadStatus.Skipped => "Skipped",
        _ => ""
    };

    public bool IsCompleted => Status == DownloadStatus.Completed;
    public bool IsFailed => Status == DownloadStatus.Failed;
    public bool IsActive => Status is DownloadStatus.Searching or DownloadStatus.Downloading;

    private string FailureText =>
        Track?.FailureReason is FailureReason.NoSuitableFileFound
            ? "Not found"
            : "Failed";

    public DownloadItemViewModel(string trackName)
    {
        _trackName = trackName;
    }

    public DownloadItemViewModel(Track track)
    {
        Track = track;
        _trackName = FormatTrackName(track);
    }

    public void UpdateFromTrack()
    {
        if (Track == null) return;

        // Map final TrackState to DownloadStatus
        var newStatus = Track.State switch
        {
            TrackState.Downloaded => DownloadStatus.Completed,
            TrackState.Failed => DownloadStatus.Failed,
            TrackState.AlreadyExists => DownloadStatus.Completed,
            TrackState.NotFoundLastTime => DownloadStatus.Failed,
            _ => Status, // Keep current status for Initial (Searching/Downloading set by polling)
        };

        if (newStatus != Status)
            Status = newStatus;

        if (!string.IsNullOrEmpty(Track.DownloadPath) && Track.DownloadPath != FilePath)
            FilePath = Track.DownloadPath;
    }

    private static string FormatTrackName(Track track)
    {
        if (!string.IsNullOrEmpty(track.Artist) && !string.IsNullOrEmpty(track.Title))
            return $"{track.Artist} - {track.Title}";
        if (!string.IsNullOrEmpty(track.Title))
            return track.Title;
        if (!string.IsNullOrEmpty(track.Album))
            return $"{track.Artist} - {track.Album}";
        return "Unknown Track";
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
