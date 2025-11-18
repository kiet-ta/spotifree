using System.Collections.Generic;

namespace Spotifree.Models
{
    public class AppSettings
    {
        public List<string> MusicFolderPaths { get; set; } = new();
        public bool IsDarkTheme { get; set; }
        public bool HasCompletedFirstRunTour { get; set; } = false;

        public string? PlaylistRootPath { get; set; }
    }
}
