using Microsoft.Web.WebView2.Wpf;
using spotifree.Models;

namespace spotifree.IServices;

public interface ISettingsService
{
    AppSettings Current { get; }

    Task LoadAsync();
    Task SaveAsync();

    // Áp dụng/thiết lập
    void ApplyZoom(WebView2 web, int percent);
    void SetStartup(string mode); // off|normal|minimized
    void SetCloseToTray(bool enable);

    // Storage
    string? PickFolder();
    Task ClearWebCacheAsync(WebView2 web);
    Task RemoveAllDownloadsAsync();
}
