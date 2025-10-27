using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotifree.Models
{
    public class SpotifyTrack
    {
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string AlbumArtLargeUrl { get; set; }
        public string AlbumArtSmallUrl { get; set; } 
        public TimeSpan Duration { get; set; }
        public string SpotifyTrackId { get; set; }
    }
}
