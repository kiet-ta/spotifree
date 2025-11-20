using Spotifree.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Spotifree.ViewModels;

public class AlbumViewModel : BaseViewModel
{
    public string Name { get; }
    public string Artist { get; }
    public string CoverFile { get; }
    public ObservableCollection<LocalTrack> Tracks { get; }
    public ICommand RenameCommand { get; }

    public AlbumViewModel(string name, string artist, string coverFile, ObservableCollection<LocalTrack> tracks, ICommand renameCommand)
    {
        Name = name;
        Artist = artist;
        Tracks = tracks;
        RenameCommand = renameCommand;
    }
}