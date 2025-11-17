using System.Collections.Generic;

namespace Spotifree.Models
{
    public class AppSettings
    {
        public List<string> MusicFolderPaths { get; set; } = new();
        public bool IsDarkTheme { get; set; }

        // Thư mục lưu playlist (.json)
        public string PlaylistsFolderPath { get; set; } = string.Empty;
    }
}
