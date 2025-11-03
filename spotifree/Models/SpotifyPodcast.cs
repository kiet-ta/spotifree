using System.Text.Json.Serialization;

namespace spotifree.Models;

public class SpotifyPodcast
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("publisher")]
    public string Publisher { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("total_episodes")]
    public int TotalEpisodes { get; set; }

    [JsonPropertyName("spotify_url")]
    public string SpotifyUrl { get; set; } = string.Empty;
}


