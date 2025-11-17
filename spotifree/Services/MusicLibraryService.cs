using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace Spotifree.Services
{
    public class MusicLibraryService : IMusicLibraryService
    {
        private List<LocalTrack> _trackCache = new();

        private readonly ISettingsService _settingsService;

        public event Action? LibraryChanged;

        public MusicLibraryService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
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
                {
                    continue;
                }

                try
                {
                    var filesInFolder = Directory.EnumerateFiles(
                        folderPath, "*.*", SearchOption.AllDirectories
                    ).Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));

                    foreach (var filePath in filesInFolder)
                    {
                        try
                        {
                            using (var tagFile = TagLib.File.Create(filePath))
                            {
                                var tag = tagFile.Tag;
                                var track = new LocalTrack
                                {
                                    FilePath = filePath,
                                    Title = string.IsNullOrEmpty(tag.Title) ? Path.GetFileNameWithoutExtension(filePath) : tag.Title,
                                    Artist = string.IsNullOrEmpty(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer,
                                    Album = string.IsNullOrEmpty(tag.Album) ? "Unknown Album" : tag.Album,

                                    Duration = tagFile.Properties.Duration.TotalSeconds,
                                    TrackNumber = tag.Track, 
                                    Year = tag.Year, 
                                    CoverArt = tag.Pictures.Length > 0 ? tag.Pictures[0].Data.Data : null
                                };
                                allTracksFound.Add(track);
                            }
                        }
                        catch (Exception)
                        {
                            // Skip file corrupt
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip folder did not access
                }
            }
            _trackCache = allTracksFound;
            LibraryChanged?.Invoke();
        }

        public async Task UpdateAlbumNameAsync(string currentAlbumName, string artist, string newAlbumName)
        {
            // 1. Tìm các bài hát cần sửa trong bộ nhớ đệm
            var tracksToUpdate = _trackCache
                .Where(t => t.Album == currentAlbumName && t.Artist == artist)
                .ToList();

            // 2. Thực hiện sửa file ở luồng phụ (Task.Run) để không đơ UI
            await Task.Run(() =>
            {
                foreach (var track in tracksToUpdate)
                {
                    try
                    {
                        // Dùng TagLib mở file
                        using (var tagFile = TagLib.File.Create(track.FilePath))
                        {
                            // Đổi tên Album
                            tagFile.Tag.Album = newAlbumName;

                            // Lưu xuống ổ cứng
                            tagFile.Save();
                        }

                        // Cập nhật luôn cache để UI hiển thị ngay tên mới
                        track.Album = newAlbumName;
                    }
                    catch (Exception ex)
                    {
                        // File có thể đang được phát hoặc bị khóa, bỏ qua hoặc log lỗi
                        Debug.WriteLine($"Lỗi khi đổi tên file {track.FilePath}: {ex.Message}");
                    }
                }
            });

            // 3. Thông báo giao diện load lại
            LibraryChanged?.Invoke();
        }

        public Task<IEnumerable<LocalTrack>> SearchTracksAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Task.FromResult(Enumerable.Empty<LocalTrack>());
            }

            var results = _trackCache.Where(t =>
                t.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                t.Artist.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                t.Album.Contains(query, StringComparison.OrdinalIgnoreCase)
            );

            return Task.FromResult(results);
        }
    }
}