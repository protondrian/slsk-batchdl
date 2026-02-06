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
    Cancelled,
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
        DownloadStatus.Cancelled => "Cancelled",
        DownloadStatus.Skipped => "Skipped",
        _ => ""
    };

    public bool IsCompleted => Status == DownloadStatus.Completed;
    public bool IsFailed => Status == DownloadStatus.Failed;
    public bool IsActive => Status is DownloadStatus.Searching or DownloadStatus.Downloading;

    private string FailureText => Track?.FailureReason switch
    {
        FailureReason.NoSuitableFileFound => "Not found",
        FailureReason.InvalidSearchString => "Invalid search",
        FailureReason.OutOfDownloadRetries => "Retries exhausted",
        FailureReason.AllDownloadsFailed => "All sources failed",
        _ => "Failed"
    };

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
        {
            Status = newStatus;

            // Rebuild detail for completed downloads (final metadata, no speed)
            if (Status == DownloadStatus.Completed && Track.FirstDownload != null)
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(Track.FirstUsername))
                    parts.Add(Track.FirstUsername);

                var file = Track.FirstDownload;
                var ext = file.Extension?.TrimStart('.').ToUpper() ?? "";
                if (!string.IsNullOrEmpty(ext))
                    parts.Add(ext);

                if (file.BitRate is > 0)
                    parts.Add($"{file.BitRate}kbps");

                if (file.Size > 0)
                    parts.Add(FormatSize(file.Size));

                StatusDetail = string.Join("  |  ", parts);
            }
        }

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

    public void UpdateFromWrapper(DownloadWrapper wrapper)
    {
        var parts = new List<string>();

        var username = wrapper.response.Username;
        if (!string.IsNullOrEmpty(username))
            parts.Add(username);

        var ext = wrapper.file.Extension?.TrimStart('.').ToUpper() ?? "";
        if (!string.IsNullOrEmpty(ext))
            parts.Add(ext);

        var bitrate = wrapper.file.BitRate;
        if (bitrate is > 0)
            parts.Add($"{bitrate}kbps");

        if (wrapper.file.Size > 0)
            parts.Add(FormatSize(wrapper.file.Size));

        var elapsed = DateTime.Now - wrapper.startTime;
        if (wrapper.bytesTransferred > 0 && elapsed.TotalSeconds > 0.5)
            parts.Add(FormatSpeed(wrapper.bytesTransferred / elapsed.TotalSeconds));

        StatusDetail = string.Join("  |  ", parts);
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    private static string FormatSpeed(double bytesPerSec)
    {
        if (bytesPerSec < 1024) return $"{bytesPerSec:F0} B/s";
        if (bytesPerSec < 1024 * 1024) return $"{bytesPerSec / 1024.0:F1} KB/s";
        return $"{bytesPerSec / (1024.0 * 1024.0):F1} MB/s";
    }
}
