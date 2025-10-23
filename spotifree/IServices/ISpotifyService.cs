using spotifree.Models;
using System.ComponentModel;

namespace spotifree.IServices;

public interface ISpotifyService
{
    Task<SpotifyPlaylist> CreatePlaylistAsync(string name, string description = "");

    Task TransferPlaybackAsync(string deviceId, bool play);

    Task UpdatePlaylistDetailsAsync(string playlistId, string newName);

    Task PlayUriAsync(string uri);

    Task DeletePlaylistAsync(string playlistId);

    Task<List<SpotifyPlaylist>> GetCurrentUserPlaylistsAsync(int limit = 20, int offset = 0);
}