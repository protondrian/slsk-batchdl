using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using slsk_batchdl.Gui.Models;
using slsk_batchdl.Gui.Services;

namespace slsk_batchdl.Gui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService = new();

    [ObservableProperty]
    private string _soulseekUsername = "";

    [ObservableProperty]
    private string _soulseekPassword = "";

    [ObservableProperty]
    private string _preferredFormat = "MP3";

    [ObservableProperty]
    private int _preferredBitrate = 320;

    [ObservableProperty]
    private string _downloadPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "sldl");

    public string[] AvailableFormats { get; } = ["MP3", "FLAC", "OGG", "M4A", "OPUS", "WAV"];
    public int[] AvailableBitrates { get; } = [128, 192, 256, 320];

    public bool HasCredentials => !string.IsNullOrWhiteSpace(SoulseekUsername)
                                  && !string.IsNullOrWhiteSpace(SoulseekPassword);

    public SettingsViewModel()
    {
        LoadFromDisk();
    }

    public void LoadFromDisk()
    {
        var settings = _settingsService.Load();
        SoulseekUsername = settings.SoulseekUsername;
        SoulseekPassword = settings.SoulseekPassword;
        PreferredFormat = settings.PreferredFormat;
        PreferredBitrate = settings.PreferredBitrate;
        DownloadPath = settings.DownloadPath;
    }

    [RelayCommand]
    private void Save()
    {
        var settings = new AppSettings
        {
            SoulseekUsername = SoulseekUsername,
            SoulseekPassword = SoulseekPassword,
            PreferredFormat = PreferredFormat,
            PreferredBitrate = PreferredBitrate,
            DownloadPath = DownloadPath,
        };
        _settingsService.Save(settings);
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
}
