using spotifree.Models;
using System.Threading.Tasks;

namespace spotifree.IServices;

public interface ISpotifyDataService
{
    Task<SpotifyDataCollection> FetchAllDataAsync();
    Task<List<SpotifyTrack>> GetSavedTracksAsync(int limit = 10, int offset = 0);
    Task<List<SpotifyTrack>> GetNewTracksAsync(int limit = 20);
    Task<List<SpotifyPodcast>> GetPodcastsAsync(int limit = 5);
    Task<List<SpotifyCategory>> GetCategoriesAsync(int limit = 5);
    Task SaveToJsonAsync(SpotifyDataCollection data, string filePath);
    Task<SpotifyDataCollection?> LoadFromJsonAsync(string filePath);
}


