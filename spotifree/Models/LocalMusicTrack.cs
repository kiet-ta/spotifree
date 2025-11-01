using System.Text.Json.Serialization;

namespace spotifree.Models;

public class LocalMusicTrack
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("artist")]
    public string Artist { get; set; } = string.Empty;

    [JsonPropertyName("album")]
    public string Album { get; set; } = string.Empty;

    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public int Duration { get; set; } = 0; // in seconds

    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; } = 0; // in bytes

    [JsonPropertyName("dateAdded")]
    public DateTime DateAdded { get; set; } = DateTime.Now;
}

