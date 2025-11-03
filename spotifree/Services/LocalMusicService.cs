using System.Diagnostics;
using System.IO;
using System.Text.Json;
using spotifree.IServices;
using spotifree.Models;

namespace spotifree.Services;

public class LocalMusicService : ILocalMusicService
{
    private readonly string _libraryFilePath;
    private List<LocalMusicTrack> _cachedLibrary = new();
    private string? _libraryDirectory;

    // Supported audio formats
    private static readonly string[] SupportedExtensions = { ".mp3", ".flac", ".m4a", ".wav", ".ogg", ".aac", ".wma" };

    public LocalMusicService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var spotifreeDir = Path.Combine(localAppData, "spotifree");
        Directory.CreateDirectory(spotifreeDir);
        _libraryFilePath = Path.Combine(spotifreeDir, "local-music-library.json");
        LoadLibraryFromDisk();
    }

    public string? GetLibraryDirectory()
    {
        if (_libraryDirectory != null) return _libraryDirectory;

        // Try to load from settings
        var settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "spotifree",
            "settings.json");

        if (File.Exists(settingsPath))
        {
            try
            {
                var json = File.ReadAllText(settingsPath);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("LocalMusicDirectory", out var dirProp))
                {
                    _libraryDirectory = dirProp.GetString();
                    return _libraryDirectory;
                }
            }
            catch { }
        }

        // Default to Music folder
        _libraryDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        return _libraryDirectory;
    }

    public void SetLibraryDirectory(string? directoryPath)
    {
        _libraryDirectory = directoryPath;
        // Save to settings if needed
    }

    public async Task<List<LocalMusicTrack>> GetLocalLibraryAsync()
    {
        if (_cachedLibrary.Count > 0)
        {
            return _cachedLibrary;
        }

        // Load from disk
        LoadLibraryFromDisk();
        return _cachedLibrary;
    }

    public async Task<List<LocalMusicTrack>> ScanDirectoryAsync(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Debug.WriteLine($"[LocalMusic] Directory not found: {directoryPath}");
            return new List<LocalMusicTrack>();
        }

        var tracks = new List<LocalMusicTrack>();
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
            .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        Debug.WriteLine($"[LocalMusic] Found {files.Count} audio files in {directoryPath}");

        foreach (var filePath in files)
        {
            try
            {
                var track = await CreateTrackFromFileAsync(filePath);
                if (track != null)
                {
                    tracks.Add(track);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalMusic] Error processing {filePath}: {ex.Message}");
            }
        }

        // Update cached library
        _cachedLibrary = tracks;
        await SaveLibraryToDiskAsync();

        return tracks;
    }

    public async Task AddToLibraryAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var existing = _cachedLibrary.FirstOrDefault(t => t.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            return;

        var track = await CreateTrackFromFileAsync(filePath);
        if (track != null)
        {
            _cachedLibrary.Add(track);
            await SaveLibraryToDiskAsync();
        }
    }

    public async Task RemoveFromLibraryAsync(string trackId)
    {
        var track = _cachedLibrary.FirstOrDefault(t => t.Id == trackId);
        if (track != null)
        {
            _cachedLibrary.Remove(track);
            await SaveLibraryToDiskAsync();
        }
    }

    private async Task<LocalMusicTrack?> CreateTrackFromFileAsync(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            // Try to extract metadata from filename (e.g., "Artist - Title.mp3")
            var parts = fileName.Split(new[] { " - ", "-", "–" }, StringSplitOptions.RemoveEmptyEntries);
            var artist = parts.Length > 1 ? parts[0].Trim() : "Unknown Artist";
            var title = parts.Length > 1 ? parts[1].Trim() : fileName;

            var track = new LocalMusicTrack
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Artist = artist,
                Album = Path.GetDirectoryName(filePath)?.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? "Unknown Album",
                FilePath = filePath,
                FileSize = fileInfo.Length,
                DateAdded = fileInfo.CreationTime > fileInfo.LastWriteTime ? fileInfo.CreationTime : fileInfo.LastWriteTime
            };

            // TODO: Extract duration and metadata from audio file using TagLib# or similar library

            return track;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocalMusic] Error creating track from {filePath}: {ex.Message}");
            return null;
        }
    }

    private void LoadLibraryFromDisk()
    {
        if (!File.Exists(_libraryFilePath))
        {
            _cachedLibrary = new List<LocalMusicTrack>();
            return;
        }

        try
        {
            var json = File.ReadAllText(_libraryFilePath);
            _cachedLibrary = JsonSerializer.Deserialize<List<LocalMusicTrack>>(json) ?? new List<LocalMusicTrack>();

            // Validate file paths still exist
            _cachedLibrary = _cachedLibrary.Where(t => File.Exists(t.FilePath)).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocalMusic] Error loading library: {ex.Message}");
            _cachedLibrary = new List<LocalMusicTrack>();
        }
    }
    public async Task AddTrackAsync(LocalMusicTrack track)
    {
        if (_cachedLibrary.Count == 0)
        {
            LoadLibraryFromDisk();
        }

        // check existed
        var existing = _cachedLibrary.FirstOrDefault(t => t.Id == track.Id);
        if (existing == null)
        {
            _cachedLibrary.Add(track);
            await SaveLibraryToDiskAsync();
        }
    }
    private async Task SaveLibraryToDiskAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cachedLibrary, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_libraryFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocalMusic] Error saving library: {ex.Message}");
        }
    }
}

