using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace spotifree.Services
{
    public class LocalLibraryService<T>
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        public LocalLibraryService(string filePath)
        {
            _filePath = filePath;
            _options = new JsonSerializerOptions { WriteIndented = true };

            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
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
