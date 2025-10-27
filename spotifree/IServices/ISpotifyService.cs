using spotifree.Models;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace spotifree.IServices;

public interface ISpotifyService
{
    Task<SpotifyPlaylist> CreatePlaylistAsync(string name, string description = "");

    Task TransferPlaybackAsync(string deviceId, bool play);

    Task UpdatePlaylistDetailsAsync(string playlistId, string newName);

    Task PlayUriAsync(string uri);

    Task DeletePlaylistAsync(string playlistId);

    Task<List<SpotifyPlaylist>> GetCurrentUserPlaylistsAsync(int limit = 20, int offset = 0);

    Task StartPlaybackAsync();      // Thay cho void Play()
    Task PausePlaybackAsync();      // Thay cho void Pause()
    Task NextTrackAsync();          // Thay cho void NextTrack()
    Task PreviousTrackAsync();      // Thay cho void PreviousTrack()
    Task SetVolumeAsync(double volume); // Thay cho void SetVolume(double volume)
    Task ToggleShuffleAsync();      // Thay cho void ToggleShuffle(bool isActive)
    Task SetRepeatModeAsync();

    event Action<SpotifyTrack> TrackChanged;
    event Action<bool> PlaybackStateChanged;
    event Action<double> PositionChanged;
}