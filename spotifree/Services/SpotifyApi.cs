using spotifree.IServices;
using spotifree.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace spotifree.Services;

public class SpotifyApi : ISpotifyService
{
    private readonly SpotifyAuth _auth;
    private readonly HttpClient _http = new HttpClient();

    public SpotifyApi(SpotifyAuth auth)
    {
        _auth = auth;
        _http.BaseAddress = new System.Uri("https://api.spotify.com/v1/");
    }

    private async Task<HttpRequestMessage> BuildAsync(HttpMethod method, string url, HttpContent? content = null)
    {
        await _auth.EnsureTokenAsync();
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.AccessToken);
        if (content != null) req.Content = content;
        return req;
    }

    public async Task<SpotifyPlaylist> CreatePlaylistAsync(string name, string description = "")
    {
        var meReq = await BuildAsync(HttpMethod.Get, "me");
        var meRes = await _http.SendAsync(meReq);

        if (!meRes.IsSuccessStatusCode)
        {
            var error = await meRes.Content.ReadAsStringAsync();
            throw new System.Exception($"Cannot get UserID (error /me): {error}");
        }

        var meJson = await meRes.Content.ReadAsStringAsync();
        var me = JsonSerializer.Deserialize<SpotifyUser>(meJson);

        var url = $"users/{me.Id}/playlists";

        var body = new CreatePlaylistRequest
        {
            Name = name,
            Description = description,
            IsPublic = false
        };

        var jsonBody = JsonSerializer.Serialize(body);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var req = await BuildAsync(HttpMethod.Post, url, content);

        var res = await _http.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
        {
            throw new System.Exception($"Error API (call {url}): {json}");
        }

        return JsonSerializer.Deserialize<SpotifyPlaylist>(json);
    }

    public async Task TransferPlaybackAsync(string deviceId, bool play)
    {
        var url = "https://api.spotify.com/v1/me/player";
        var body = new { device_ids = new[] { deviceId }, play };
        var req = await BuildAsync(HttpMethod.Put, url, new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
        var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    public async Task PlayUriAsync(string uri)
    {
        var url = "https://api.spotify.com/v1/me/player/play";
        var body = new { uris = new[] { uri } };
        var req = await BuildAsync(HttpMethod.Put, url, new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
        var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    public async Task UpdatePlaylistDetailsAsync(string playlistId, string newName)
    {
        var url = $"playlists/{playlistId}";

        var body = new
        {
            name = newName
        };

        var jsonBody = JsonSerializer.Serialize(body);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var req = await BuildAsync(HttpMethod.Put, url, content);

        var res = await _http.SendAsync(req);

        if (!res.IsSuccessStatusCode)
        {
            var json = await res.Content.ReadAsStringAsync();
            throw new System.Exception($"Lỗi API (gọi {url}): {json}");
        }
    }

    public async Task DeletePlaylistAsync(string id)
    {
        var url = $"playlists/{id}/followers";
        var req = await BuildAsync(HttpMethod.Delete, url);
        var res = await _http.SendAsync(req);
        if (!res.IsSuccessStatusCode)
        {
            var json = await res.Content.ReadAsStringAsync();
            throw new Exception($"Error API (call {url}): {json}");
        }
    }

    public async Task<List<SpotifyPlaylist>> GetCurrentUserPlaylistsAsync(int limit = 20, int offset = 0)
    {
        var url = $"me/playlists?limit={limit}&offset={offset}";
        var req = await BuildAsync(HttpMethod.Get, url);
        var res = await _http.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
        {
            throw new System.Exception($"Error API (call GET {url}): {json}");
        }
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
        {
            var playlists = itemsElement.EnumerateArray()
                .Select(item => JsonSerializer.Deserialize<SpotifyPlaylist>(item.GetRawText()))
                .Where(playlist => playlist != null)
                .ToList()!;
            return playlists;
        }
        return new List<SpotifyPlaylist>();
    }

    public async Task<string> SearchAsync(string query, string type = "track,artist,album,playlist", int limit = 20)
    {
        var url = $"search?q={Uri.EscapeDataString(query)}&type={type}&limit={limit}";
        var req = await BuildAsync(HttpMethod.Get, url);
        var res = await _http.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
        {
            throw new System.Exception($"Error API (call GET {url}): {json}");
        }
        return json;
    }
}