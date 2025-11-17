using System.Collections.Generic;

namespace Spotifree.Models
{
    public class Playlist
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<LocalTrack> Tracks { get; set; } = new();
    }
}
