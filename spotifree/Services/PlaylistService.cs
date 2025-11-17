using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spotifree.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly ISettingsService _settingsService;
        private const string DefaultPlaylistFolderName = "Playlists";

        public PlaylistService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private async Task<string> GetPlaylistFolderAsync()
        {
            var settings = await _settingsService.GetAsync();
            var folder = settings.PlaylistsFolderPath;

            if (string.IsNullOrWhiteSpace(folder))
            {
                var baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "LocalMusicPlayer");
                folder = Path.Combine(baseDir, DefaultPlaylistFolderName);
            }

            Directory.CreateDirectory(folder);
            return folder;
        }

        public async Task<IReadOnlyList<Playlist>> GetPlaylistsAsync()
        {
            var folder = await GetPlaylistFolderAsync();
            var result = new List<Playlist>();

            foreach (var file in Directory.EnumerateFiles(folder, "*.json"))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var playlist = JsonSerializer.Deserialize<Playlist>(json);
                    if (playlist != null)
                    {
                        result.Add(playlist);
                    }
                }
                catch
                {
                    // bỏ qua file hỏng
                }
            }

            return result;
        }

        public async Task<Playlist> CreatePlaylistAsync(string name, IEnumerable<LocalTrack> tracks)
        {
            var folder = await GetPlaylistFolderAsync();

            var playlist = new Playlist
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = string.IsNullOrWhiteSpace(name) ? "New Playlist" : name.Trim(),
                Tracks = tracks?.ToList() ?? new List<LocalTrack>()
            };

            var path = Path.Combine(folder, $"{playlist.Id}.json");
            await SavePlaylistInternalAsync(path, playlist);

            return playlist;
        }

        public async Task AddTracksAsync(string playlistId, IEnumerable<LocalTrack> tracks)
        {
            var folder = await GetPlaylistFolderAsync();
            var path = Path.Combine(folder, $"{playlistId}.json");
            if (!File.Exists(path)) return;

            var playlist = await LoadPlaylistInternalAsync(path);
            if (playlist == null) return;

            foreach (var track in tracks)
            {
                if (!playlist.Tracks.Any(t =>
                        string.Equals(t.FilePath, track.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    playlist.Tracks.Add(track);
                }
            }

            await SavePlaylistInternalAsync(path, playlist);
        }

        public async Task DeletePlaylistAsync(string playlistId)
        {
            var folder = await GetPlaylistFolderAsync();
            var path = Path.Combine(folder, $"{playlistId}.json");

            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }

        private static async Task SavePlaylistInternalAsync(string path, Playlist playlist)
        {
            var json = JsonSerializer.Serialize(
                playlist,
                new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }

        private static async Task<Playlist?> LoadPlaylistInternalAsync(string path)
        {
            try
            {
                var json = await File.ReadAllTextAsync(path);
                return JsonSerializer.Deserialize<Playlist>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
