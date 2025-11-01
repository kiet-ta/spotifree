using spotifree.IServices;
using spotifree.Models;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;
using System.IO;
using System.Text.Json.Nodes;

namespace spotifree.Services;

public class SpotifyDataService : ISpotifyDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;
    private const string BaseUrl = "https://api.spotify.com/v1";

    public SpotifyDataService(string accessToken)
    {
        _accessToken = accessToken;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public async Task<SpotifyDataCollection> FetchAllDataAsync()
    {
        Console.WriteLine("🎵 Fetching all Spotify data...");
        
        var collection = new SpotifyDataCollection();

        try
        {
            // Fetch all data in parallel for better performance
            var task1 = Task.Run(async () => collection.SavedTracks = await GetSavedTracksAsync(50));
            var task2 = Task.Run(async () => collection.NewTracks = await GetNewTracksAsync(50));
            var task3 = Task.Run(async () => collection.Podcasts = await GetPodcastsAsync(20));
            var task4 = Task.Run(async () => collection.Categories = await GetCategoriesAsync(20));

            await Task.WhenAll(task1, task2, task3, task4);

            collection.LastUpdated = DateTime.Now;

            Console.WriteLine($"✅ Fetched {collection.TotalItems} items total:");
            Console.WriteLine($"   - Saved Tracks: {collection.SavedTracks.Count}");
            Console.WriteLine($"   - New Tracks: {collection.NewTracks.Count}");
            Console.WriteLine($"   - Podcasts: {collection.Podcasts.Count}");
            Console.WriteLine($"   - Categories: {collection.Categories.Count}");

            return collection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error fetching data: {ex.Message}");
            throw;
        }
    }

    public async Task<List<SpotifyTrack>> GetSavedTracksAsync(int limit = 10, int offset = 0)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/me/tracks?limit={limit}&offset={offset}&market=VN");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"⚠️ Error getting saved tracks: {response.StatusCode}");
                return new List<SpotifyTrack>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonNode.Parse(json);
            var items = data?["items"]?.AsArray();

            if (items == null) return new List<SpotifyTrack>();

            var tracks = new List<SpotifyTrack>();
            foreach (var item in items)
            {
                var track = item?["track"];
                if (track == null) continue;

                tracks.Add(new SpotifyTrack
                {
                    Id = track["id"]?.GetValue<string>() ?? "",
                    Name = track["name"]?.GetValue<string>() ?? "",
                    Artist = string.Join(", ", track["artists"]?.AsArray().Select(a => a?["name"]?.GetValue<string>() ?? "") ?? Array.Empty<string>()),
                    Album = track["album"]?["name"]?.GetValue<string>() ?? "",
                    Duration = FormatDuration(track["duration_ms"]?.GetValue<int>() ?? 0),
                    Popularity = track["popularity"]?.GetValue<int>() ?? 0,
                    PreviewUrl = track["preview_url"]?.GetValue<string>(),
                    ImageUrl = track["album"]?["images"]?[0]?["url"]?.GetValue<string>() ?? "",
                    ReleaseDate = track["album"]?["release_date"]?.GetValue<string>() ?? "",
                    AddedAt = DateTime.Parse(item["added_at"]?.GetValue<string>() ?? DateTime.Now.ToString()),
                    SpotifyUrl = track["external_urls"]?["spotify"]?.GetValue<string>() ?? ""
                });
            }

            return tracks;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GetSavedTracksAsync: {ex.Message}");
            return new List<SpotifyTrack>();
        }
    }

    public async Task<List<SpotifyTrack>> GetNewTracksAsync(int limit = 20)
    {
        try
        {
            var currentYear = DateTime.Now.Year;
            var query = Uri.EscapeDataString($"year:{currentYear} tag:new");
            var response = await _httpClient.GetAsync($"/search?q={query}&type=track&limit={limit}&market=VN");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"⚠️ Error getting new tracks: {response.StatusCode}");
                return new List<SpotifyTrack>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonNode.Parse(json);
            var items = data?["tracks"]?["items"]?.AsArray();

            if (items == null) return new List<SpotifyTrack>();

            var tracks = new List<SpotifyTrack>();
            foreach (var track in items)
            {
                if (track == null) continue;

                tracks.Add(new SpotifyTrack
                {
                    Id = track["id"]?.GetValue<string>() ?? "",
                    Name = track["name"]?.GetValue<string>() ?? "",
                    Artist = string.Join(", ", track["artists"]?.AsArray().Select(a => a?["name"]?.GetValue<string>() ?? "") ?? Array.Empty<string>()),
                    Album = track["album"]?["name"]?.GetValue<string>() ?? "",
                    Duration = FormatDuration(track["duration_ms"]?.GetValue<int>() ?? 0),
                    Popularity = track["popularity"]?.GetValue<int>() ?? 0,
                    PreviewUrl = track["preview_url"]?.GetValue<string>(),
                    ImageUrl = track["album"]?["images"]?[0]?["url"]?.GetValue<string>() ?? "",
                    ReleaseDate = track["album"]?["release_date"]?.GetValue<string>() ?? "",
                    SpotifyUrl = track["external_urls"]?["spotify"]?.GetValue<string>() ?? ""
                });
            }

            return tracks;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GetNewTracksAsync: {ex.Message}");
            return new List<SpotifyTrack>();
        }
    }

    public async Task<List<SpotifyPodcast>> GetPodcastsAsync(int limit = 5)
    {
        try
        {
            var query = Uri.EscapeDataString("genre:podcast");
            var response = await _httpClient.GetAsync($"/search?q={query}&type=show&limit={limit}&market=VN");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"⚠️ Error getting podcasts: {response.StatusCode}");
                return new List<SpotifyPodcast>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonNode.Parse(json);
            var items = data?["shows"]?["items"]?.AsArray();

            if (items == null) return new List<SpotifyPodcast>();

            var podcasts = new List<SpotifyPodcast>();
            foreach (var show in items)
            {
                if (show == null) continue;

                podcasts.Add(new SpotifyPodcast
                {
                    Id = show["id"]?.GetValue<string>() ?? "",
                    Name = show["name"]?.GetValue<string>() ?? "",
                    Publisher = show["publisher"]?.GetValue<string>() ?? "",
                    Description = show["description"]?.GetValue<string>() ?? "",
                    ImageUrl = show["images"]?[0]?["url"]?.GetValue<string>() ?? "",
                    TotalEpisodes = show["total_episodes"]?.GetValue<int>() ?? 0,
                    SpotifyUrl = show["external_urls"]?["spotify"]?.GetValue<string>() ?? ""
                });
            }

            return podcasts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GetPodcastsAsync: {ex.Message}");
            return new List<SpotifyPodcast>();
        }
    }

    public async Task<List<SpotifyCategory>> GetCategoriesAsync(int limit = 5)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/browse/categories?country=VN&locale=vi_VN&limit={limit}");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"⚠️ Error getting categories: {response.StatusCode}");
                return new List<SpotifyCategory>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonNode.Parse(json);
            var items = data?["categories"]?["items"]?.AsArray();

            if (items == null) return new List<SpotifyCategory>();

            var categories = new List<SpotifyCategory>();
            foreach (var category in items)
            {
                if (category == null) continue;

                categories.Add(new SpotifyCategory
                {
                    Id = category["id"]?.GetValue<string>() ?? "",
                    Name = category["name"]?.GetValue<string>() ?? "",
                    IconUrl = category["icons"]?[0]?["url"]?.GetValue<string>() ?? "",
                    Href = category["href"]?.GetValue<string>() ?? ""
                });
            }

            return categories;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GetCategoriesAsync: {ex.Message}");
            return new List<SpotifyCategory>();
        }
    }

    public async Task SaveToJsonAsync(SpotifyDataCollection data, string filePath)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(data, options);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, json);
            Console.WriteLine($"✅ Saved data to: {filePath}");
            Console.WriteLine($"   File size: {new FileInfo(filePath).Length / 1024} KB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error saving to JSON: {ex.Message}");
            throw;
        }
    }

    public async Task<SpotifyDataCollection?> LoadFromJsonAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"⚠️ File not found: {filePath}");
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<SpotifyDataCollection>(json);
            
            Console.WriteLine($"✅ Loaded data from: {filePath}");
            Console.WriteLine($"   Total items: {data?.TotalItems ?? 0}");
            Console.WriteLine($"   Last updated: {data?.LastUpdated}");
            
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading from JSON: {ex.Message}");
            return null;
        }
    }

    private string FormatDuration(int durationMs)
    {
        var duration = TimeSpan.FromMilliseconds(durationMs);
        return $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}";
    }
}


