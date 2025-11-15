using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Spotifree.ViewModels
{
    public class AlbumViewModel : BaseViewModel
    {
        public string Name { get; }
        public string Artist { get; }
        public BitmapImage? CoverArt { get; }
        public ObservableCollection<LocalTrack> Tracks { get; }

        public AlbumViewModel(string name, string artist, BitmapImage? coverArt, ObservableCollection<LocalTrack> tracks)
        {
            Name = name;
            Artist = artist;
            CoverArt = coverArt;
            Tracks = tracks;
        }
    }
}
