using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Spotifree.Models
{
    public class Playlist
    {
        public string Name { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public BitmapImage? CoverArt { get; set; }

        public ObservableCollection<LocalTrack> Tracks { get; } = new();
    }
}
