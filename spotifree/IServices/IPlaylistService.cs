using Spotifree.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spotifree.IServices
{
    public interface IPlaylistService
    {
        Task<IReadOnlyList<Playlist>> GetPlaylistsAsync();
        Task<Playlist> CreatePlaylistAsync(string name, IEnumerable<LocalTrack> tracks);
        Task AddTracksAsync(string playlistId, IEnumerable<LocalTrack> tracks);
        Task DeletePlaylistAsync(string playlistId);
    }
}
