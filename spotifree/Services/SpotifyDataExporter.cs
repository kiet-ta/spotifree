using spotifree.Models;
using System.IO;

namespace spotifree.Services;

/// <summary>
/// Helper class để export Spotify data ra JSON files
/// </summary>
public static class SpotifyDataExporter
{
    private static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Spotifree",
        "Data"
    );

    /// <summary>
    /// Export tất cả Spotify data và lưu vào file JSON
    /// </summary>
    public static async Task<string> ExportAllDataAsync(string accessToken)
    {
        try
        {
            Console.WriteLine("🚀 Starting Spotify data export...");
            
            // Ensure data directory exists
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
                Console.WriteLine($"📁 Created directory: {DataDirectory}");
            }

            // Create service
            var service = new SpotifyDataService(accessToken);

            // Fetch all data
            var data = await service.FetchAllDataAsync();

            // Generate filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"spotify_data_{timestamp}.json";
            var filePath = Path.Combine(DataDirectory, filename);

            // Save to JSON
            await service.SaveToJsonAsync(data, filePath);

            // Also save as "latest.json" for easy access
            var latestPath = Path.Combine(DataDirectory, "spotify_data_latest.json");
            await service.SaveToJsonAsync(data, latestPath);

            Console.WriteLine("✅ Export completed successfully!");
            Console.WriteLine($"📂 Files saved to: {DataDirectory}");
            Console.WriteLine($"   - {filename}");
            Console.WriteLine($"   - spotify_data_latest.json");

            return filePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Export failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Export chỉ saved tracks
    /// </summary>
    public static async Task<string> ExportSavedTracksAsync(string accessToken, int limit = 50)
    {
        var service = new SpotifyDataService(accessToken);
        var tracks = await service.GetSavedTracksAsync(limit);
        
        var filePath = Path.Combine(DataDirectory, $"saved_tracks_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        
        var collection = new SpotifyDataCollection
        {
            SavedTracks = tracks,
            LastUpdated = DateTime.Now
        };
        
        await service.SaveToJsonAsync(collection, filePath);
        return filePath;
    }

    /// <summary>
    /// Export chỉ new tracks
    /// </summary>
    public static async Task<string> ExportNewTracksAsync(string accessToken, int limit = 50)
    {
        var service = new SpotifyDataService(accessToken);
        var tracks = await service.GetNewTracksAsync(limit);
        
        var filePath = Path.Combine(DataDirectory, $"new_tracks_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        
        var collection = new SpotifyDataCollection
        {
            NewTracks = tracks,
            LastUpdated = DateTime.Now
        };
        
        await service.SaveToJsonAsync(collection, filePath);
        return filePath;
    }

    /// <summary>
    /// Export chỉ podcasts
    /// </summary>
    public static async Task<string> ExportPodcastsAsync(string accessToken, int limit = 20)
    {
        var service = new SpotifyDataService(accessToken);
        var podcasts = await service.GetPodcastsAsync(limit);
        
        var filePath = Path.Combine(DataDirectory, $"podcasts_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        
        var collection = new SpotifyDataCollection
        {
            Podcasts = podcasts,
            LastUpdated = DateTime.Now
        };
        
        await service.SaveToJsonAsync(collection, filePath);
        return filePath;
    }

    /// <summary>
    /// Export chỉ categories
    /// </summary>
    public static async Task<string> ExportCategoriesAsync(string accessToken, int limit = 20)
    {
        var service = new SpotifyDataService(accessToken);
        var categories = await service.GetCategoriesAsync(limit);
        
        var filePath = Path.Combine(DataDirectory, $"categories_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        
        var collection = new SpotifyDataCollection
        {
            Categories = categories,
            LastUpdated = DateTime.Now
        };
        
        await service.SaveToJsonAsync(collection, filePath);
        return filePath;
    }

    /// <summary>
    /// Load latest exported data
    /// </summary>
    public static async Task<SpotifyDataCollection?> LoadLatestDataAsync()
    {
        var latestPath = Path.Combine(DataDirectory, "spotify_data_latest.json");
        
        if (!File.Exists(latestPath))
        {
            Console.WriteLine("⚠️ No latest data file found");
            return null;
        }

        var service = new SpotifyDataService("dummy"); // Token not needed for loading
        return await service.LoadFromJsonAsync(latestPath);
    }

    /// <summary>
    /// Get đường dẫn thư mục data
    /// </summary>
    public static string GetDataDirectory() => DataDirectory;

    /// <summary>
    /// Mở thư mục data trong File Explorer
    /// </summary>
    public static void OpenDataDirectory()
    {
        if (!Directory.Exists(DataDirectory))
        {
            Directory.CreateDirectory(DataDirectory);
        }

        System.Diagnostics.Process.Start("explorer.exe", DataDirectory);
    }
}


