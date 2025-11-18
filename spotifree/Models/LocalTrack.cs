namespace Spotifree.Models;

public class LocalTrack
{
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public double Duration { get; set; }

    public uint TrackNumber { get; set; }
    public uint Year { get; set; }

    public byte[]? CoverArt { get; set; }

    public string? RawLrcContent { get; set; }
}