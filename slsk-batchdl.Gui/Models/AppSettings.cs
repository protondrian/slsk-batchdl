using System.IO;

namespace slsk_batchdl.Gui.Models;

public class AppSettings
{
    // Soulseek
    public string SoulseekUsername { get; set; } = "";
    public string SoulseekPassword { get; set; } = "";

    // Download defaults
    public string PreferredFormat { get; set; } = "MP3";
    public int PreferredBitrate { get; set; } = 320;
    public string DownloadPath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "sldl");
}
