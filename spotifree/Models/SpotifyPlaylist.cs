using System.Text.Json.Serialization;

namespace spotifree.Models;

public class SpotifyPlaylist
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } // this is "playlist"

    [JsonPropertyName("description")]
    public string Description { get; set; }
}