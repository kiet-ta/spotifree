using spotifree.IServices;
using spotifree.Models;
using System;                   
using System.Collections.Generic; 
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;  

namespace spotifree.Services;

public class SpotifyApi : ISpotifyService
{
    private readonly SpotifyAuth _auth;
    private readonly HttpClient _http = new HttpClient();
    private bool _isPolling = false;

    // --- State cho Polling và Toggles ---
    private string _lastTrackId = null;
    private bool _lastIsPlaying = false;

    private bool _isShuffle = false;
    private int _repeatModeIndex = 0; // 0=off, 1=context, 2=track
    private readonly string[] _repeatModes = { "off", "context", "track" };

    // --- Events từ Interface ---
    public event Action<SpotifyTrack> TrackChanged;
    public event Action<bool> PlaybackStateChanged;
    public event Action<double> PositionChanged;

    public SpotifyApi(SpotifyAuth auth)
    {
        _auth = auth;
        _http.BaseAddress = new System.Uri("https://api.spotify.com/v1/");

    }

    private async Task<HttpRequestMessage> BuildAsync(HttpMethod method, string url, HttpContent? content = null)
    {
        bool valid = await _auth.EnsureTokenValidAsync_NoPopup();
        if (!valid)
        {
            // Nếu token hết hạn và không thể refresh, 
            // dừng polling thay vì mở browser
            _isPolling = false;
            throw new System.Exception("Spotify token invalid/expired.");
        }
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
    public async Task StartPlaybackAsync()
    {
        var url = "me/player/play";
        var req = await BuildAsync(HttpMethod.Put, url);
        var res = await _http.SendAsync(req);
        // Có thể lỗi 404 (No device) hoặc 403 (Premium needed)
        if (res.IsSuccessStatusCode) _lastIsPlaying = true;
    }

    public async Task PausePlaybackAsync()
    {
        var url = "me/player/pause";
        var req = await BuildAsync(HttpMethod.Put, url);
        var res = await _http.SendAsync(req);
        if (res.IsSuccessStatusCode) _lastIsPlaying = false;
    }

    public async Task NextTrackAsync()
    {
        var url = "me/player/next";
        var req = await BuildAsync(HttpMethod.Post, url);
        await _http.SendAsync(req); // Không cần check lỗi, cứ gửi
    }

    public async Task PreviousTrackAsync()
    {
        var url = "me/player/previous";
        var req = await BuildAsync(HttpMethod.Post, url);
        await _http.SendAsync(req); // Không cần check lỗi, cứ gửi
    }

    public async Task SetVolumeAsync(double volume)
    {
        int volumePercent = (int)Math.Clamp(volume, 0, 100);
        var url = $"me/player/volume?volume_percent={volumePercent}";
        var req = await BuildAsync(HttpMethod.Put, url);
        await _http.SendAsync(req);
    }

    public async Task ToggleShuffleAsync()
    {
        _isShuffle = !_isShuffle; // Đảo trạng thái nội bộ
        var url = $"me/player/shuffle?state={_isShuffle}";
        var req = await BuildAsync(HttpMethod.Put, url);
        await _http.SendAsync(req);
    }

    public async Task SetRepeatModeAsync()
    {
        _repeatModeIndex = (_repeatModeIndex + 1) % 3; // Quay vòng 0, 1, 2
        string newMode = _repeatModes[_repeatModeIndex];
        var url = $"me/player/repeat?state={newMode}";
        var req = await BuildAsync(HttpMethod.Put, url);
        await _http.SendAsync(req);
    }


    // --- HỆ THỐNG POLLING ĐỂ KÍCH HOẠT EVENT ---

    private async void StartPolling()
    {
        // Kiểm tra trạng thái liên tục
        while (_isPolling)
        {
            await Task.Delay(2000); // Kiểm tra mỗi 2 giây
            try
            {
                await PollPlayerStateAsync();
            }
            catch (Exception ex)
            {
                // (Nên log lỗi)
                Console.WriteLine($"Error polling player state: {ex.Message}");
                // Nếu lỗi token, đợi 10s rồi thử lại
                await Task.Delay(10000);
            }
        }
    }
    public void StartPollingIfNeeded()
    {
        // Chỉ start nếu CHƯA polling VÀ đã login
        if (_isPolling || !_auth.IsValid()) return;

        _isPolling = true;
        Debug.WriteLine("[C# SpotifyApi] Polling started.");
        StartPolling();
    }

    private async Task PollPlayerStateAsync()
    {
        // Đây là endpoint quan trọng nhất
        var req = await BuildAsync(HttpMethod.Get, "me/player");
        var res = await _http.SendAsync(req);
        Console.WriteLine("MewMew: ", res.Content);
        // 204 No Content = không có gì đang phát
        if (!res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            if (_lastIsPlaying)
            {
                _lastIsPlaying = false;
                PlaybackStateChanged?.Invoke(false);
            }
            return;
        }

        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // 1. Kiểm tra trạng thái Play/Pause
        if (root.TryGetProperty("is_playing", out var isPlayingElement))
        {
            bool isPlaying = isPlayingElement.GetBoolean();
            if (isPlaying != _lastIsPlaying)
            {
                _lastIsPlaying = isPlaying;
                PlaybackStateChanged?.Invoke(isPlaying); // Kích hoạt sự kiện
            }
        }

        // 2. Kiểm tra bài hát
        if (root.TryGetProperty("item", out var itemElement) && itemElement.ValueKind == JsonValueKind.Object)
        {
            string trackId = itemElement.GetProperty("id").GetString();
            if (trackId != _lastTrackId)
            {
                _lastTrackId = trackId;

                // Map dữ liệu từ API về Model 'SpotifyTrack' của bạn
                var durationMs = itemElement.GetProperty("duration_ms").GetInt32();
                var duration = TimeSpan.FromMilliseconds(durationMs);
                var durationFormatted = $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}";
                
                var newTrack = new SpotifyTrack
                {
                    Id = trackId,
                    Name = itemElement.GetProperty("name").GetString() ?? "",
                    Duration = durationFormatted,
                    Artist = string.Join(", ", itemElement.GetProperty("artists").EnumerateArray().Select(a => a.GetProperty("name").GetString())),
                    Album = itemElement.GetProperty("album").GetProperty("name").GetString() ?? "",
                    ImageUrl = itemElement.GetProperty("album").GetProperty("images").EnumerateArray().FirstOrDefault().GetProperty("url").GetString() ?? "",
                    Popularity = itemElement.GetProperty("popularity").GetInt32(),
                    PreviewUrl = itemElement.TryGetProperty("preview_url", out var previewUrl) ? previewUrl.GetString() : null,
                    SpotifyUrl = itemElement.GetProperty("external_urls").GetProperty("spotify").GetString() ?? ""
                };
                TrackChanged?.Invoke(newTrack); // Kích hoạt sự kiện
            }
        }

        // 3. Kiểm tra tiến trình
        if (root.TryGetProperty("progress_ms", out var progressElement))
        {
            double positionSeconds = progressElement.GetInt32() / 1000.0;
            PositionChanged?.Invoke(positionSeconds); // Kích hoạt sự kiện
        }
    }
}