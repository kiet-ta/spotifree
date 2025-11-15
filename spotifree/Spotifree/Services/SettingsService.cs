using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spotifree.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _filePath;

        public SettingsService()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LocalMusicPlayer");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "appsettings.json");
        }

        // Loads the application settings from storage.
        public async Task<AppSettings> GetAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new AppSettings();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        // Saves the application settings to storage.
        public async Task SaveAsync(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
