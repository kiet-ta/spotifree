using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotifree.IServices
{
    // Fires when the library has been updated (e.g., after a rescan)
    public interface IMusicLibraryService
    {
        event Action? LibraryChanged;

        // Gets the cached library of tracks.
        Task<IEnumerable<LocalTrack>> GetLibraryAsync();

        // Scans the specified folder, extracts metadata, and updates the library cache.
        Task ScanLibraryAsync();

        Task<IEnumerable<LocalTrack>> SearchTracksAsync(string query);
        Task UpdateAlbumNameAsync(string currentAlbumName, string artist, string newAlbumName);
    }
}
