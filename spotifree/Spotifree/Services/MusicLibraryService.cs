using Spotifree.IServices;
using Spotifree.Models;
using System.IO;
using System.Windows.Media.Imaging;

namespace Spotifree.Services
{
    public class MusicLibraryService : IMusicLibraryService
    {
        private List<LocalTrack> _libraryCache = new();
        private static readonly string[] SupportedExtensions = { ".mp3", ".m4a", ".flac", ".wav", ".aac" };
        public event Action? LibraryChanged;
        // Gets the cached library of tracks.
        public Task<List<LocalTrack>> GetLibraryAsync()
        {
            return Task.FromResult(_libraryCache);
        }

        // Scans the specified folder, extracts metadata, and updates the library cache.
        public async Task<List<LocalTrack>> ScanLibraryAsync(string folderPath)
        {
            var tracks = new List<LocalTrack>();
            if (!Directory.Exists(folderPath))
            {
                return tracks;
            }

            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                                 .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    try
                    {
                        using var tagFile = TagLib.File.Create(file);

                        tracks.Add(new LocalTrack
                        {
                            FilePath = file,
                            Title = tagFile.Tag.Title ?? Path.GetFileNameWithoutExtension(file),
                            Artist = tagFile.Tag.FirstPerformer ?? "Unknown Artist",
                            Album = tagFile.Tag.Album ?? "Unknown Album",
                            Duration = (int)tagFile.Properties.Duration.TotalSeconds,
                            CoverArt = LoadCoverArt(tagFile.Tag.Pictures)
                        });
                    }
                    catch { /* Ignore corrupted or unreadable files */ }
                }
            });

            _libraryCache = tracks;
            LibraryChanged?.Invoke();

            return _libraryCache;
        }

        // Extracts and converts cover art for WPF.
        private BitmapImage? LoadCoverArt(TagLib.IPicture[] pictures)
        {
            if (pictures == null || pictures.Length == 0) return null;

            try
            {
                var pic = pictures[0];
                using var ms = new MemoryStream(pic.Data.Data);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }
    }
}