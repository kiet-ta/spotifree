using spotifree.IServices;
using spotifree.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Wpf;

namespace spotifree.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _filePath;

        public SettingsService()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Spotifree");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "appsettings.json");
        }

        public async Task<AppSettings> GetAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    var def = new AppSettings();
                    EnsureStorageFolder(def);
                    await SaveAsync(def);
                    return def;
                }
                var json = await File.ReadAllTextAsync(_filePath);
                var s = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                EnsureStorageFolder(s);
                return s;
            }
            catch
            {
                var def = new AppSettings();
                EnsureStorageFolder(def);
                return def;
            }
        }

        public async Task SaveAsync(AppSettings s)
        {
            EnsureStorageFolder(s);
            var json = JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public void ApplyZoom(WebView2 webView, int percent)
        {
            if (webView == null) return;
            var p = Math.Clamp(percent, 50, 300);
            // WPF wrapper có thuộc tính ZoomFactor
            webView.ZoomFactor = p / 100.0;
        }

        public void EnsureStorageFolder(AppSettings s)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(s.OfflineStoragePath))
                {
                    s.OfflineStoragePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Spotifree", "Storage");
                }
                Directory.CreateDirectory(s.OfflineStoragePath);
            }
            catch { /* ignore */ }
        }
    }
}
