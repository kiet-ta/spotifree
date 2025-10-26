using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Wpf;
using spotifree.IServices;
using spotifree.Models;

namespace spotifree.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly string _path;
    private readonly string _defaultStorage;

    public AppSettings Current { get; private set; } = new();

    public SettingsService()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(local, "spotifree");
        Directory.CreateDirectory(dir);

        _path = Path.Combine(dir, "settings.json");
        _defaultStorage = Path.Combine(dir, "Storage");
        Directory.CreateDirectory(_defaultStorage);
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_path))
        {
            Current.OfflineStoragePath = _defaultStorage;
            await SaveAsync();
            return;
        }

        try
        {
            using var fs = File.OpenRead(_path);
            var data = await JsonSerializer.DeserializeAsync<AppSettings>(fs);
            if (data != null) Current = data;
            if (string.IsNullOrWhiteSpace(Current.OfflineStoragePath))
                Current.OfflineStoragePath = _defaultStorage;
        }
        catch
        {
            Current = new AppSettings { OfflineStoragePath = _defaultStorage };
            await SaveAsync();
        }
    }

    public async Task SaveAsync()
    {
        using var fs = File.Create(_path);
        await JsonSerializer.SerializeAsync(fs, Current, new JsonSerializerOptions { WriteIndented = true });
    }

    public void ApplyZoom(WebView2 web, int percent)
    {
        var p = Math.Clamp(percent, 50, 300);
        try { web.ZoomFactor = p / 100.0; } catch { }
        Current.ZoomPercent = p;
    }

    public void SetStartup(string mode)
    {
        Current.StartupMode = mode;

        using var rk = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
        if (rk is null) return;

        const string name = "Spotifree";
        if (mode == "off")
        {
            rk.DeleteValue(name, false);
            Current.LaunchOnLogin = false;
            Current.StartMinimized = false;
            return;
        }

        string exe = Process.GetCurrentProcess().MainModule!.FileName;
        string args = mode == "minimized" ? " --minimized" : "";
        rk.SetValue(name, $"\"{exe}\"{args}");
        Current.LaunchOnLogin = true;
        Current.StartMinimized = (mode == "minimized");
    }

    public void SetCloseToTray(bool enable) => Current.CloseToTray = enable;

    public string? PickFolder()
    {
        using var dlg = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Choose folder for Spotifree offline storage",
            ShowNewFolderButton = true
        };
        var rs = dlg.ShowDialog();
        if (rs == System.Windows.Forms.DialogResult.OK)
        {
            Current.OfflineStoragePath = dlg.SelectedPath;
            return dlg.SelectedPath;
        }
        return null;
    }

    public async Task ClearWebCacheAsync(WebView2 web)
    {
        try { await web.CoreWebView2!.Profile.ClearBrowsingDataAsync(); } catch { }
    }

    public async Task RemoveAllDownloadsAsync()
    {
        var root = Current.OfflineStoragePath ?? _defaultStorage;
        if (!Directory.Exists(root)) return;

        foreach (var f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            try { File.Delete(f); } catch { }

        foreach (var d in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
            try { Directory.Delete(d, true); } catch { }

        await Task.CompletedTask;
    }
}
