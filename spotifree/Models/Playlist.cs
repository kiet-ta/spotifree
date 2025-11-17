using System.Collections.ObjectModel;

namespace Spotifree.Models
{
    public class Playlist
    {
        public string Name { get; set; } = string.Empty;

        public string FolderPath { get; set; } = string.Empty;

        public ObservableCollection<LocalTrack> Tracks { get; } = new();
    }
}
