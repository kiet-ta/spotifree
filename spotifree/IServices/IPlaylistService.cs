using Spotifree.Models;
using System.Collections.ObjectModel;

namespace Spotifree.IServices
{
    public interface IPlaylistService
    {
        ObservableCollection<Playlist> Playlists { get; }

        string? RootPath { get; set; }

        void ReloadFromDisk();

        Playlist CreatePlaylist(string name);

        void RenamePlaylist(Playlist playlist, string newName);

        void LoadTracksForPlaylist(Playlist playlist);

        void AddTrackToPlaylist(Playlist playlist, LocalTrack track);

        void RemoveTrackFromPlaylist(Playlist playlist, LocalTrack track);
    }
}
