using System.Text.Json.Serialization;

namespace spotifree.Models;

public class SpotifyUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }
}