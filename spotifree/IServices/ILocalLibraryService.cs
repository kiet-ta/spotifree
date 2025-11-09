using spotifree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotifree.IServices
{
    public interface ILocalLibraryService
    {
        Task<List<Song>> ScanLibraryAsync(string directoryPath);
        Song? GetMusicFileDetails(string filePath);
    }
}
