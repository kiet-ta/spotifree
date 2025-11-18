using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spotifree.Services
{
    public class MusicLibraryService : IMusicLibraryService
    {
        private List<LocalTrack> _trackCache = new();

        private readonly ISettingsService _settingsService;
        private readonly IPlaylistService _playlistService;

        public event Action? LibraryChanged;

        public MusicLibraryService(ISettingsService settingsService, IPlaylistService playlistService)
        {
            _settingsService = settingsService;
            _playlistService = playlistService;
        }

        public Task<IEnumerable<LocalTrack>> GetLibraryAsync()
        {
            return Task.FromResult(_trackCache.AsEnumerable());
        }

        public async Task ScanLibraryAsync()
        {
            var settings = await _settingsService.GetAsync();
            if (settings.MusicFolderPaths == null || !settings.MusicFolderPaths.Any())
            {
                _trackCache = new List<LocalTrack>();
                LibraryChanged?.Invoke();
                return;
            }

            var allTracksFound = new List<LocalTrack>();
            var allowedExtensions = new[] { ".mp3", ".m4a", ".flac", ".wav", ".ogg" };

            foreach (var folderPath in settings.MusicFolderPaths)
            {
                if (!Directory.Exists(folderPath))
                    continue;

                try
                {
                    var filesInFolder = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                        .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));

                    foreach (var filePath in filesInFolder)
                    {
                        if (IsInPlaylistFolder(filePath))
                            continue;

                        try
                        {
                            using var tagFile = TagLib.File.Create(filePath);
                            var tag = tagFile.Tag;

                            string lrcPath = Path.ChangeExtension(filePath, ".lrc");
                            string? lrcContent = null;
                            if (File.Exists(lrcPath))
                            {
                                try
                                {
                                    lrcContent = await File.ReadAllTextAsync(lrcPath);
                                }
                                catch {  }
                            }

                            var track = new LocalTrack
                            {
                                FilePath = filePath,
                                Title = string.IsNullOrEmpty(tag.Title) ? Path.GetFileNameWithoutExtension(filePath) : tag.Title,
                                Artist = string.IsNullOrEmpty(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer,
                                Album = string.IsNullOrEmpty(tag.Album) ? "Unknown Album" : tag.Album,
                                Duration = tagFile.Properties.Duration.TotalSeconds,
                                TrackNumber = tag.Track,
                                Year = tag.Year,
                                CoverArt = tag.Pictures.Length > 0 ? tag.Pictures[0].Data.Data : null,
                                RawLrcContent = lrcContent
                            };

                            allTracksFound.Add(track);
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }

            _trackCache = allTracksFound;
            LibraryChanged?.Invoke();
        }

        public async Task UpdateAlbumNameAsync(string currentAlbumName, string artist, string newAlbumName)
        {
            var tracksToUpdate = _trackCache
                .Where(t => t.Album == currentAlbumName && t.Artist == artist)
                .ToList();

            await Task.Run(() =>
            {
                foreach (var track in tracksToUpdate)
                {
                    try
                    {
                        using var tagFile = TagLib.File.Create(track.FilePath);
                        tagFile.Tag.Album = newAlbumName;
                        tagFile.Save();

                        track.Album = newAlbumName;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi khi đổi tên file {track.FilePath}: {ex.Message}");
                    }
                }
            });

            LibraryChanged?.Invoke();
        }

        private static bool IsInPlaylistFolder(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
                return false;

            var metadataPath = Path.Combine(directory, "playlist.json");
            return File.Exists(metadataPath);
        }
    }
}
