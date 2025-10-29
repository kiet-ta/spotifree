using Microsoft.Web.WebView2.Core;
using spotifree.IServices;
using spotifree.Services;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace spotifree;

public partial class MainWindow : Window
{
    private MiniWeb? _mini;
    private readonly ISpotifyService _spotifyService;
    private readonly SpotifyAuth _auth;
    private string? _lastPlayerStateRawJson; // cache để mini mở lên có state ngay
    private string _viewsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views");
    
    public MainWindow(ISpotifyService spotifyService, SpotifyAuth auth)
    {
        InitializeComponent();
        InitializeAsync();
        _spotifyService = spotifyService;
        _auth = auth;
    }

    private async void InitializeAsync()
    {
        try
        {
            // waiting webview2 really to run
            await webView.EnsureCoreWebView2Async(null);

            webView.CoreWebView2.WebMessageReceived += HandleWebMessage;

            Debug.WriteLine("[C#] Checking token...");
            bool ok = await _auth.EnsureTokenAsync();

            if (!ok)
            {
                Debug.WriteLine("[C#] Login failed!");
                MessageBox.Show("Unable to connect to Spotify. Please try again..");
                return;
            }

            // Get the folder path where the .exe file is running
            // (Ex: C:\MyProject\bin\Debug\)
            string appDir = AppDomain.CurrentDomain.BaseDirectory;

            // Combine that path with the WebApp folder and the index.html file
            string webAppPath = Path.Combine(appDir, "Views");
            if (!Directory.Exists(webAppPath))
            {
                MessageBox.Show($"Critical Error: Could not find the index.html file at: {webAppPath}\n\n" + "Have you set 'Copy to Output Directory' for the files in WebApp?", "UI Load Error"
);
                return;
            }
            // create the virtual host mapping
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        "spotifree.local",
                        webAppPath,
                        CoreWebView2HostResourceAccessKind.Allow
                    );

            // navigate to the local html file via the virtual host
            webView.CoreWebView2.Navigate("https://spotifree.local/index.html");

            // check if in debug mode
            webView.CoreWebView2.OpenDevToolsWindow();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.StackTrace.ToString());
        }
    }

    private async void HandleWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string jsonMessage = args.WebMessageAsJson;
        Debug.WriteLine($"[C#] Message received: {jsonMessage}");

        var message = JsonDocument.Parse(jsonMessage);
        if (!message.RootElement.TryGetProperty("action", out var actionProperty))
            return;

        string action = actionProperty.GetString();

        // Check "action"
        if (action == "createPlaylist")
        {
            string playlistName = "My New Playlist";
            if (message.RootElement.TryGetProperty("name", out var nameProperty))
            {
                playlistName = nameProperty.GetString();
            }

            try
            {
                // waiting webview2 really to run
                await webView.EnsureCoreWebView2Async(null);
                var core = webView.CoreWebView2;
                if (!Directory.Exists(_viewsDir))
                {
                    MessageBox.Show($"Not found Views folder: {_viewsDir}");
                    return;
                }
                core.SetVirtualHostNameToFolderMapping(
                    "spotifree.local", _viewsDir, CoreWebView2HostResourceAccessKind.Allow);

                core.Settings.IsWebMessageEnabled = true;
                core.WebMessageReceived += CoreWebView2_WebMessageReceived;

                core.Navigate("https://spotifree.local/index.html");
                // Get the folder path where the .exe file is running
                // (Ex: C:\MyProject\bin\Debug\)
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                // call service to create playlist
                Debug.WriteLine($"[C#] Calling CreatePlaylistAsync with name: {playlistName}");
                var newPlaylist = await _spotifyService.CreatePlaylistAsync(playlistName);
                Debug.WriteLine($"[C#] Playlist created! ID: {newPlaylist.Id}");

                // sent to js
                string script = $"window.addNewPlaylistCard({JsonSerializer.Serialize(newPlaylist)});";
                await webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                // sent error to js
                Debug.WriteLine($"[C#] ERROR: {ex.Message}");
                string errorScript = $"window.showNotification('error create playlist: {ex.Message.Replace("'", "\\'")}', 'error');";
                await webView.CoreWebView2.ExecuteScriptAsync(errorScript);
            }
        }
        else if (action == "renamePlaylist")
        {
            if (!message.RootElement.TryGetProperty("id", out var idProperty) ||
                !message.RootElement.TryGetProperty("newName", out var newNameProperty))
            {
                Debug.WriteLine("[C#] renamePlaylist: Missing parameters.");
                return;
            }
            string playlistId = idProperty.GetString();
            string newName = newNameProperty.GetString();
            try
            {
                Debug.WriteLine($"[C#] calling API rename: {playlistId} -> {newName}");
                await _spotifyService.UpdatePlaylistDetailsAsync(playlistId, newName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[C#] ERROR RENAME: {ex.Message}");
            }
        }
        else if (action == "deletePlaylist")
        {
            if (!message.RootElement.TryGetProperty("id", out var idProperty))
            {
                Debug.WriteLine("[C#] deletePlaylist: Missing parameters.");
                return;
            }
            string playlistId = idProperty.GetString();
            try
            {
                Debug.WriteLine($"[C#] calling API delete: {playlistId}");
                await _spotifyService.DeletePlaylistAsync(playlistId);

                string script = $"window.playlistDeletedSuccess('{playlistId}');";
                await webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[C#] ERROR DELETE: {ex.Message}");
            }
        }
        else if (action == "getLibraryPlaylists")
        {
            try
            {
                Debug.WriteLine($"[C#] Đang gọi API lấy playlists...");
                var playlists = await _spotifyService.GetCurrentUserPlaylistsAsync();

                Debug.WriteLine($"[C#] Get susscess {playlists.Count} playlist. sending to JS...");

                string script = $"window.populateLibrary({JsonSerializer.Serialize(playlists)});";
                await webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[C#] ERROR PLAYLIST: {ex.Message}");
                string errorScript = $"window.showNotification('ERROR load: {ex.Message.Replace("'", "\\'")}', 'error');";
                await webView.CoreWebView2.ExecuteScriptAsync(errorScript);
            }
        }
    }
        /// <summary>
        /// ✅ Hàm nhận tin nhắn từ chatbot.js gửi sang qua window.chrome.webview.postMessage()
        /// </summary>
        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // Lấy nội dung JSON hoặc string từ JS
                string message = e.TryGetWebMessageAsString();

                // Debug
                Console.WriteLine($"[JS -> C#] Nhận tin nhắn: {message}");

                // ✅ Tùy theo message mà xử lý hành động
                if (message.Contains("playMusic"))
                {
                    // Ở đây bạn có thể gọi service phát nhạc của bạn
                    MessageBox.Show("🎧 Đang phát nhạc từ chatbot!");
                    // Ví dụ:
                    // musicService.Play("Let Her Go");
                }
                else if (message.Contains("pauseMusic"))
                {
                    MessageBox.Show("⏸ Tạm dừng nhạc");
                    // musicService.Pause();
                }
                else
                {
                    // Xử lý các lệnh khác nếu cần
                    Console.WriteLine($"Không nhận dạng được hành động từ chatbot: {message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xử lý chatbot message: {ex.Message}");
            }
        }


        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string raw = e.WebMessageAsJson;
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();

            switch (type)
            {
                case "openMini":
                    {
                        var tr = root.GetProperty("track");
                        OpenMiniAsync(); // mở mini
                                         // Optionally: nếu đang có state thì bắn qua ngay
                        if (!string.IsNullOrEmpty(_lastPlayerStateRawJson))
                            _mini?.SendToMiniRaw(_lastPlayerStateRawJson);
                        break;
                    }

                case "playerState":
                    {
                        _lastPlayerStateRawJson = raw; // giữ nguyên đường JSON gốc
                        _mini?.SendToMiniRaw(raw);     // forward sang mini
                        break;
                    }
            }
        }

        private async void OpenMiniAsync()
        {
            if (_mini != null)
            {
                if (!_mini.IsVisible) _mini.Show();
                _mini.Activate();
                this.WindowState = WindowState.Minimized;
                return;
            }

            _mini = new MiniWeb();
            _mini.SeekRequested += sec =>
            {
                // gửi lệnh seek về web chính
                SendToMainPage(new { type = "seek", seconds = sec });
            };
            _mini.CloseRequested += () =>
            {
                _mini = null;
                this.WindowState = WindowState.Normal;
                this.Show();
            };
            _mini.OnSeek += (sec) =>
            {
                SendToMainPage(new { type = "seek", seconds = sec });
            };

            _mini.OnBackToMain += () =>
            {
                this.WindowState = WindowState.Normal;
                _mini.Hide();
                this.Activate();
            };

            await _mini.InitAsync(_viewsDir);
            _mini.Show();
            _mini.Activate();
            this.WindowState = WindowState.Minimized;
        }

        private void SendToMainPage(object payload)
        {
            if (webView?.CoreWebView2 == null) return;
            var json = JsonSerializer.Serialize(payload);
            webView.CoreWebView2.PostWebMessageAsJson(json);
        }
    }
}
