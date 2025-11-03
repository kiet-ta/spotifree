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
        Task<List<MusicFileDetail>> ScanLibraryAsync(string directoryPath);
        MusicFileDetail? GetMusicFileDetails(string filePath);
    }
}
