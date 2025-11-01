using System.Text.Json.Serialization;

namespace spotifree.Models;

public class SpotifyDataCollection
{
    [JsonPropertyName("saved_tracks")]
    public List<SpotifyTrack> SavedTracks { get; set; } = new();

    [JsonPropertyName("new_tracks")]
    public List<SpotifyTrack> NewTracks { get; set; } = new();

    [JsonPropertyName("podcasts")]
    public List<SpotifyPodcast> Podcasts { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<SpotifyCategory> Categories { get; set; } = new();

    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    [JsonPropertyName("total_items")]
    public int TotalItems => SavedTracks.Count + NewTracks.Count + Podcasts.Count + Categories.Count;
}


