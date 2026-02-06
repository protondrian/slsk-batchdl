using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soulseek;
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

    [ObservableProperty]
    private string _credentialStatus = "";

    [ObservableProperty]
    private bool _credentialSuccess;

    [ObservableProperty]
    private bool _isTesting;

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
    private async Task TestCredentials()
    {
        if (!HasCredentials)
        {
            CredentialStatus = "Enter username and password first";
            CredentialSuccess = false;
            return;
        }

        IsTesting = true;
        CredentialStatus = "";

        try
        {
            using var client = new SoulseekClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await client.ConnectAsync(SoulseekUsername, SoulseekPassword, cts.Token);
            CredentialStatus = "Login successful";
            CredentialSuccess = true;
        }
        catch (SoulseekClientException ex)
        {
            CredentialStatus = ex.Message;
            CredentialSuccess = false;
        }
        catch (TimeoutException)
        {
            CredentialStatus = "Connection timed out";
            CredentialSuccess = false;
        }
        catch (Exception ex)
        {
            CredentialStatus = ex.Message;
            CredentialSuccess = false;
        }
        finally
        {
            IsTesting = false;
        }
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
