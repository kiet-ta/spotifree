using System.Text.Json.Serialization;

namespace spotifree.Models;

public class SpotifyCategory
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; } = string.Empty;

    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
}


