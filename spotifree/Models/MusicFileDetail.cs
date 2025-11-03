using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotifree.Models
{
    public class MusicFileDetail
    {
        public string Title { get; set; } = "Unknown Title";
        public string Artist { get; set; } = "Unknown Artist";
        public string Album { get; set; } = "Unknown Album";
        public uint Year { get; set; } = 0;
        public string Genre { get; set; } = "Unknown";
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public string FilePath { get; set; } = string.Empty;
    }
}
