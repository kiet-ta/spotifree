using Spotifree.Models;
using System.Collections.ObjectModel;

namespace Spotifree.ViewModels
{
    public class PlaylistViewModel : BaseViewModel
    {
        private string _name;

        public string Id { get; }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<LocalTrack> Tracks { get; }

        public PlaylistViewModel(Playlist playlist)
        {
            Id = playlist.Id;
            _name = playlist.Name;
            Tracks = new ObservableCollection<LocalTrack>(playlist.Tracks);
        }
    }
}
