using System.Text.Json.Serialization;

namespace spotifree.Models;

public class CreatePlaylistRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("public")]
    public bool IsPublic { get; set; }
}