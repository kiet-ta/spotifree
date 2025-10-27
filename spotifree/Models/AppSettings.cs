namespace spotifree.Models;

public sealed class AppSettings
{
    public string Language { get; set; } = "en-US";
    public bool Autoplay { get; set; } = true;

    // Startup
    public string StartupMode { get; set; } = "off"; // off | normal | minimized
    public bool LaunchOnLogin { get; set; } = false;
    public bool StartMinimized { get; set; } = false;

    // Window
    public bool CloseToTray { get; set; } = false;

    // View
    public int ZoomPercent { get; set; } = 100;

    // Storage
    public string? OfflineStoragePath { get; set; }
}
