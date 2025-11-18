using Spotifree.Models;
using System;
using System.Collections.ObjectModel;

namespace Spotifree.IServices
{
    public interface IPlaylistService
    {
        ObservableCollection<Playlist> Playlists { get; }

        string? RootPath { get; set; }

        event Action? PlaylistsChanged;

        void ReloadFromDisk();
        void LoadFromDisk();

        Playlist CreatePlaylist(string name);
        void RenamePlaylist(Playlist playlist, string newName);
        void DeletePlaylist(Playlist playlist);

        void AddTrackToPlaylist(Playlist playlist, LocalTrack track);
        void RemoveTrackFromPlaylist(Playlist playlist, LocalTrack track);

        void ChangeCover(Playlist playlist, string? imageFilePath);

        void LoadTracksForPlaylist(Playlist playlist);
    }
}
