using spotifree.IServices;
using spotifree.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace spotifree.Services
{
    public class LocalLibraryService<T> : ILocalLibraryService
    {
        private readonly string[] _supportedExtensions = { ".mp3", ".m4a", ".flac", ".wav", ".aac" };
		private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        
		public LocalLibraryService(string filePath)
        {
            _filePath = filePath;
            _options = new JsonSerializerOptions { WriteIndented = true };

            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
        }
        
		public async Task<List<Song>> ScanLibraryAsync(string directoryPath)
        {
            var musicFiles = new List<Song>();
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Lỗi: Không tìm thấy thư mục '{directoryPath}'");
                return musicFiles; 
            }

            await Task.Run(() =>
            {
                try
                {
                    var files = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                                         .Where(f => _supportedExtensions.Contains(Path.GetExtension(f).ToLower()));

                    foreach (var filePath in files)
                    {
                        var fileDetail = GetMusicFileDetails(filePath);
                        if (fileDetail != null)
                        {
                            musicFiles.Add(fileDetail);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi quét thư viện: {ex.Message}");
                }
            });

            return musicFiles;
        }

        public Song? GetMusicFileDetails(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            try
            {
                using (var tagFile = TagLib.File.Create(filePath))
                {
                    var tag = tagFile.Tag;

                    var detail = new Song
                    {
                        FilePath = filePath,
                        Title = !string.IsNullOrEmpty(tag.Title) ? tag.Title : Path.GetFileNameWithoutExtension(filePath),
                        Artist = tag.Performers != null && tag.Performers.Length > 0 ? string.Join(", ", tag.Performers) : "Unknown Artist",
                        //Album = !string.IsNullOrEmpty(tag.Album) ? tag.Album : "Unknown Album",
                        //Year = tag.Year,
                        //Genre = tag.Genres != null && tag.Genres.Length > 0 ? string.Join(", ", tag.Genres) : "Unknown",
                        //Duration = tagFile.Properties.Duration
                    };

                    // TODO: Trích xuất ảnh bìa nếu bạn muốn
                    // if (tag.Pictures != null && tag.Pictures.Length > 0)
                    // {
                    //     detail.AlbumArt = tag.Pictures[0].Data.Data;
                    // }

                    return detail;
                }
            }
            catch (TagLib.CorruptFileException)
            {
                return new Song
                {
                    FilePath = filePath,
                    Title = Path.GetFileNameWithoutExtension(filePath)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đọc tag file '{filePath}': {ex.Message}");
                return null;
            }
        }
		        public async Task<List<T>> LoadLibraryAsync()
        {
            if (!File.Exists(_filePath)) return new List<T>();
            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        public async Task SaveLibraryAsync(List<T> data)
        {
            var json = JsonSerializer.Serialize(data, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task AddItemAsync(T item)
        {
            var list = await LoadLibraryAsync();
            list.Add(item);
            await SaveLibraryAsync(list);
        }
    }
}
