using spotifree.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace spotifree.IServices;

public interface ILocalMusicService
{
    Task<List<LocalMusicTrack>> GetLocalLibraryAsync();
    Task<List<LocalMusicTrack>> ScanDirectoryAsync(string directoryPath);
    Task AddToLibraryAsync(string filePath);
    Task RemoveFromLibraryAsync(string trackId);
    string? GetLibraryDirectory();
    void SetLibraryDirectory(string? directoryPath);
}

