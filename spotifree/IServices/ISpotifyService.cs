using spotifree.Models;

namespace spotifree.IServices;

public interface ISpotifyService
{
    Task<SpotifyPlaylist> CreatePlaylistAsync(string name, string description = "");

    Task TransferPlaybackAsync(string deviceId, bool play);

    Task PlayUriAsync(string uri);
}