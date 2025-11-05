using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using spotifree.IServices;
using spotifree.Models;
using spotifree.Services;
using Spotifree.Audio;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
namespace spotifree;

public partial class Spotifree : Window
{
    private MiniWeb? _mini;
    private readonly ISpotifyService _spotifyService;
    private readonly ILocalMusicService _localMusicService;
    private readonly SpotifyAuth _auth;
    private readonly LocalAudioService _engine = new();
    private readonly ISettingsService _settings;
    private PlayerBridge? _bridge;
    private string? _lastPlayerStateRawJson; // cache để mini mở lên có state ngay
    private string _viewsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views");
    private ChatbotBridge? _chatbotBridge;

    public Spotifree(ISpotifyService spotifyService, ILocalMusicService localMusicService, SpotifyAuth auth, ISettingsService settings)
    {
        _spotifyService = spotifyService;
        _localMusicService = localMusicService;
        _auth = auth;
        _settings = settings;

        InitializeComponent();
        InitializeAsync();
    }
    // ===== Helper: notify JS =====
    private async Task JsNotifyAsync(string action, object data)
    {
        if (webView?.CoreWebView2 == null) return;
        var json = JsonSerializer.Serialize(new { action, data });
        await webView.CoreWebView2.ExecuteScriptAsync($"window.__fromWpf && window.__fromWpf({json});");
    }
    private async void InitializeAsync()
    {
        try
        {
            // waiting webview2 really to run
            await webView.EnsureCoreWebView2Async(null);

            // Web messages
            webView.CoreWebView2.WebMessageReceived += HandleWebMessage;
            //webView.CoreWebView2.Settings.IsWebMessageEnabled = true;

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
            // áp dụng zoom theo settings
            var cur = await _settings.GetAsync();
            _settings.ApplyZoom(webView, cur.ZoomPercent);

            // Kích hoạt polling nếu token file cũ vẫn còn hạn
            _spotifyService.StartPollingIfNeeded();

            // navigate to the local html file via the virtual host
            webView.CoreWebView2.Navigate("https://spotifree.local/index.html");

            // Inject hardcoded token into JS for testing
            if (_auth != null && !string.IsNullOrEmpty(_auth.AccessToken))
            {
                await webView.CoreWebView2.ExecuteScriptAsync($@"
                    window.__ACCESS_TOKEN__ = '{_auth.AccessToken}';
                    if (window.localStorage) {{
                        window.localStorage.setItem('spotify_access_token', '{_auth.AccessToken}');
                        window.localStorage.setItem('spotify_token_expiry', new Date(Date.now() + 3600000).toISOString());
                    }}
                    console.log('✅ Hardcoded token injected from C#');
                ");
            }

            // check if in debug mode
            webView.CoreWebView2.OpenDevToolsWindow();

            _chatbotBridge = new ChatbotBridge(webView.CoreWebView2);

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.StackTrace.ToString());
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

    private async void HandleWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string jsonMessage = args.WebMessageAsJson;
        Debug.WriteLine($"[C#] Message received: {jsonMessage}");

        try
        {
            using var doc = JsonDocument.Parse(jsonMessage);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                // A. Check for "action" (Playlist/UI messages)
                if (root.TryGetProperty("action", out var actionProperty))
                {
                    string action = actionProperty.GetString();
                    // ✅ Chatbot actions
                    if (action == "chatbot.addPlaylist")
                    {
                        Debug.WriteLine("[Chatbot] Saving playlist to local JSON...");
                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.json");
                        var service = new LocalLibraryService<LocalMusicTrack>(path);
                        var tracks = await service.LoadLibraryAsync();

                        var newTrack = new LocalMusicTrack
                        {
                            Id = Guid.NewGuid().ToString(),
                            Title = root.GetProperty("title").GetString() ?? "Untitled",
                            Artist = root.GetProperty("artist").GetString() ?? "Unknown",
                            Album = root.GetProperty("album").GetString() ?? "Unknown",
                            FilePath = root.GetProperty("filePath").GetString() ?? "",
                            DateAdded = DateTime.Now
                        };

                        tracks.Add(newTrack);
                        await service.SaveLibraryAsync(tracks);

                        await JsNotifyAsync("chatbot.saved", new { ok = true, count = tracks.Count });
                        return;
                    }

                    if (action == "chatbot.getLibrary")
                    {
                        Debug.WriteLine("[Chatbot] Loading playlist from local JSON...");
                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.json");
                        var service = new LocalLibraryService<LocalMusicTrack>(path);
                        var tracks = await service.LoadLibraryAsync();

                        await JsNotifyAsync("chatbot.libraryData", tracks);
                        return;
                    }


                    if (action == "createPlaylist")
                    {
                        string playlistName = "My New Playlist";
                        if (doc.RootElement.TryGetProperty("name", out var nameProperty))
                        {
                            playlistName = nameProperty.GetString();
                        }

                        try
                        {
                            // call service to create playlist
                            Debug.WriteLine($"[C#] Calling CreatePlaylistAsync with name: {playlistName}");
                            var newPlaylist = await _spotifyService.CreatePlaylistAsync(playlistName);
                            Debug.WriteLine($"[C#] Playlist created! ID: {newPlaylist.Id}");

                            // sent to js
                            await JsNotifyAsync("addNewPlaylistCard", newPlaylist);
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
                        if (!doc.RootElement.TryGetProperty("id", out var idProperty) ||
                            !doc.RootElement.TryGetProperty("newName", out var newNameProperty))
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
                        if (!doc.RootElement.TryGetProperty("id", out var idProperty))
                        {
                            Debug.WriteLine("[C#] deletePlaylist: Missing parameters.");
                            return;
                        }
                        string playlistId = idProperty.GetString();
                        try
                        {
                            Debug.WriteLine($"[C#] calling API delete: {playlistId}");
                            await _spotifyService.DeletePlaylistAsync(playlistId);

                            await JsNotifyAsync("playlistDeletedSuccess", playlistId);
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

                            await JsNotifyAsync("populateLibrary", playlists);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[C#] ERROR PLAYLIST: {ex.Message}");
                            string errorScript = $"window.showNotification('ERROR load: {ex.Message.Replace("'", "\\'")}', 'error');";
                            await webView.CoreWebView2.ExecuteScriptAsync(errorScript);
                        }
                    }
                    else if (action == "local.addMusic")
                    {
                        await HandleAddLocalMusicAsync();
                    }

                    else if (action == "getLocalLibrary")
                    {
                        try
                        {
                            Debug.WriteLine($"[C#] Đang lấy local music library...");
                            var localTracks = await _localMusicService.GetLocalLibraryAsync();

                            Debug.WriteLine($"[C#] Found {localTracks.Count} local tracks. sending to JS...");

                            // Convert LocalMusicTrack to format JS expects
                            var libraryData = localTracks.Select(track => new
                            {
                                id = track.Id,
                                name = track.Title,
                                artist = track.Artist,
                                album = track.Album,
                                type = "Local Music",
                                filePath = track.FilePath,
                                duration = track.Duration,
                                dateAdded = track.DateAdded.ToString("yyyy-MM-dd")
                            }).ToList();

                            string script = $"window.populateLocalLibrary({JsonSerializer.Serialize(libraryData)});";
                            await webView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[C#] ERROR LOCAL LIBRARY: {ex.Message}");
                            string errorScript = $"window.showNotification('ERROR load local library: {ex.Message.Replace("'", "\\'")}', 'error');";
                            await webView.CoreWebView2.ExecuteScriptAsync(errorScript);
                        }
                    }
                    else if (action == "scanLocalLibrary")
                    {
                        try
                        {
                            Debug.WriteLine($"[C#] Đang scan thư mục nhạc local...");
                            var directory = _localMusicService.GetLibraryDirectory();
                            
                            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                            {
                                string errorScript = $"window.showNotification('Thư mục nhạc không tồn tại. Vui lòng cấu hình trong Settings.', 'error');";
                                await webView.CoreWebView2.ExecuteScriptAsync(errorScript);
                                return;
                            }

                            var localTracks = await _localMusicService.ScanDirectoryAsync(directory);
                            Debug.WriteLine($"[C#] Scan thành công {localTracks.Count} tracks.");

                            var libraryData = localTracks.Select(track => new
                            {
                                id = track.Id,
                                name = track.Title,
                                artist = track.Artist,
                                album = track.Album,
                                type = "Local Music",
                                filePath = track.FilePath,
                                duration = track.Duration,
                                dateAdded = track.DateAdded.ToString("yyyy-MM-dd")
                            }).ToList();

                            string script = $"window.populateLocalLibrary({JsonSerializer.Serialize(libraryData)});";
                            await webView.CoreWebView2.ExecuteScriptAsync(script);

                            string successScript = $"window.showNotification('Đã tìm thấy {localTracks.Count} bài hát local.', 'success');";
                            await webView.CoreWebView2.ExecuteScriptAsync(successScript);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[C#] ERROR SCAN LOCAL: {ex.Message}");
                            string errorScript = $"window.showNotification('Lỗi scan: {ex.Message.Replace("'", "\\'")}', 'error');";
                            await webView.CoreWebView2.ExecuteScriptAsync(errorScript);
                        }
                    }
                }

                    // ========== SETTINGS INTEROP ==========
                    if (root.TryGetProperty("action", out var actProp))
                    {
                        string actionName = actProp.GetString() ?? string.Empty;

                        switch (actionName)
                        {
                            case "spotify.login":
                                {
                                    await HandleSpotifyLoginAsync();
                                    return;
                                }

                        case "nav.openMusicDetail":
                            {
                                // Đảm bảo chạy trên luồng giao diện chính
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    // 1. Lấy ServiceProvider từ App.xaml.cs
                                    var serviceProvider = (Application.Current as App)?.ServiceProvider;
                                    if (serviceProvider == null)
                                    {
                                        MessageBox.Show("Lỗi nghiêm trọng: ServiceProvider bị null!");
                                        return;
                                    }

                                    // 2. Yêu cầu ServiceProvider tạo một cửa sổ MusicDetail MỚI
                                    // (Đây là lý do bạn đã đăng ký nó là 'AddTransient')
                                    var musicDetailWindow = serviceProvider.GetRequiredService<MusicDetail>();

                                    // 3. Hiển thị cửa sổ đó
                                    musicDetailWindow.Show();
                                         });
                                return; // Đừng quên return
                            }
                        case "settings.get":
                                {
                                    var s = await _settings.GetAsync();
                                    await JsNotifyAsync("settings.current", s);
                                    return;
                                }

                            case "settings.setLanguage":
                                {
                                    var s = await _settings.GetAsync();
                                    string lang = root.TryGetProperty("language", out var v)
                                        ? (v.GetString() ?? "en-US")
                                        : "en-US";
                                    s.Language = lang;
                                    await _settings.SaveAsync(s);
                                    await JsNotifyAsync("settings.current", s);
                                    return;
                                }

                            case "settings.setAutoplay":
                                {
                                    var s = await _settings.GetAsync();
                                    bool enable = root.TryGetProperty("enable", out var v) && v.GetBoolean();
                                    s.Autoplay = enable;
                                    await _settings.SaveAsync(s);
                                    await JsNotifyAsync("settings.current", s);
                                    return;
                                }

                            case "settings.applyZoom":
                                {
                                    var s = await _settings.GetAsync();
                                    int percent = root.TryGetProperty("percent", out var p) ? p.GetInt32() : 100;
                                    s.ZoomPercent = percent;
                                    await _settings.SaveAsync(s);
                                    _settings.ApplyZoom(webView, percent);   // dùng WPF WebView2.ZoomFactor
                                    await JsNotifyAsync("settings.current", s);
                                    return;
                                }

                            case "storage.pickFolder":
                                {
                                    var dlg = new Microsoft.Win32.OpenFileDialog()
                                    {
                                        Title = "Select Offline Storage Folder",
                                        CheckFileExists = false,
                                        CheckPathExists = true,
                                        FileName = "Select Folder"
                                    };
                                    if (dlg.ShowDialog() == true)
                                    {
                                        string? picked = System.IO.Path.GetDirectoryName(dlg.FileName);
                                        if (!string.IsNullOrWhiteSpace(picked))
                                        {
                                            var s = await _settings.GetAsync();
                                            s.OfflineStoragePath = picked!;
                                            _settings.EnsureStorageFolder(s);
                                            await _settings.SaveAsync(s);

                                            await JsNotifyAsync("storage.folderPicked", new { ok = true, path = picked });
                                            await JsNotifyAsync("settings.current", s);
                                            return;
                                        }
                                    }
                                    await JsNotifyAsync("storage.folderPicked", new { ok = false });
                                    return;
                                }

                            case "storage.clearOffline":
                                {
                                    var s = await _settings.GetAsync();
                                    _settings.EnsureStorageFolder(s);

                                    string downloadsDir = System.IO.Path.Combine(s.OfflineStoragePath, "Downloads");
                                    if (System.IO.Directory.Exists(downloadsDir))
                                    {
                                        try { System.IO.Directory.Delete(downloadsDir, recursive: true); }
                                        catch
                                        {
                                            foreach (var f in System.IO.Directory.EnumerateFiles(downloadsDir, "*", System.IO.SearchOption.AllDirectories))
                                            { try { System.IO.File.Delete(f); } catch { } }
                                        }
                                    }
                                    System.IO.Directory.CreateDirectory(downloadsDir);

                                    await JsNotifyAsync("storage.clearOffline.done", new { ok = true });
                                    await JsNotifyAsync("settings.current", s);
                                    return;
                                }

                            case "nav.goHome":
                                {
                                    await webView.CoreWebView2.ExecuteScriptAsync("window.loadPage && window.loadPage('home');");
                                    return;
                                }
                        }
                    }
                //  "type" (Mini-player messages)
                else if (root.TryGetProperty("type", out var typeProperty))
                {
                    string type = typeProperty.GetString();
                    switch (type)
                    {
                        case "openMini":
                            {
                                OpenMiniAsync();
                                if (!string.IsNullOrEmpty(_lastPlayerStateRawJson))
                                    _mini?.SendToMiniRaw(_lastPlayerStateRawJson);
                                break;
                            }
                        case "playerState":
                            {
                                _lastPlayerStateRawJson = jsonMessage;
                                _mini?.SendToMiniRaw(jsonMessage);     // forward sang mini
                                break;
                            }
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.String)
            {
                string messageText = root.GetString();

                // Debug
                Console.WriteLine($"[JS -> C#] Nhận tin nhắn chuỗi: {messageText}");

                if (messageText.Contains("playMusic"))
                {
                    MessageBox.Show("🎧 Đang phát nhạc từ chatbot!");
                    // musicService.Play("Let Her Go");
                }
                else if (messageText.Contains("pauseMusic"))
                {
                    MessageBox.Show("⏸ Tạm dừng nhạc");
                    // musicService.Pause();
                }
                else
                {
                    Console.WriteLine($"Không nhận dạng được hành động từ chatbot: {messageText}");
                }
            }
        }
        catch (JsonException jsonEx)
        {
            Debug.WriteLine($"[C#] Lỗi parse JSON: {jsonEx.Message}. Message gốc: {jsonMessage}");
        }
        catch (Exception ex)
        {
            // Bắt các lỗi chung khác
            Debug.WriteLine($"[C#] Lỗi không xác định trong HandleWebMessage: {ex.Message}");
            MessageBox.Show($"Lỗi xử lý message: {ex.Message}");
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
    private async Task HandleAddLocalMusicAsync()
    {
        Debug.WriteLine("[C#] Received local.addMusic request.");

        var dlg = new OpenFileDialog
        {
            Title = "Select Music Files",
            Multiselect = true, // Cho phép chọn nhiều file
            Filter = "Music Files|*.mp3;*.wav;*.flac;*.m4a|All Files|*.*"
        };

        // ShowDialog() cần chạy trên UI thread, nhưng HandleWebMessage
        // có thể đang ở background. Dùng Dispatcher để đảm bảo an toàn.
        bool? result = await Application.Current.Dispatcher.InvokeAsync(() => dlg.ShowDialog());

        if (result == true)
        {
            // Lấy danh sách các đường dẫn file đã chọn
            string[] filePaths = dlg.FileNames;
            Debug.WriteLine($"[C#] User selected {filePaths.Length} files.");

            // Gửi mảng đường dẫn file về lại cho JS
            // (JS sẽ nhận được một object: { action: "local.musicAdded", data: ["C:\\path1.mp3", ...] })
            await JsNotifyAsync("local.musicAdded", filePaths);
        }
        else
        {
            Debug.WriteLine("[C#] User cancelled file dialog.");
            await JsNotifyAsync("local.musicAdded", Array.Empty<string>()); // Gửi mảng rỗng
        }
    }
    private async Task HandleSpotifyLoginAsync()
    {
        try
        {
            Debug.WriteLine("[C#] Received spotify.login request.");

            bool ok = await _auth.EnsureTokenAsync(); // Hàm này sẽ mở browser nếu cần

            if (ok)
            {
                Debug.WriteLine("[C#] Login successful!");
                // Báo cho JS biết đã login OK
                await JsNotifyAsync("spotify.login.success", new { status = "connected" });
                _spotifyService.StartPollingIfNeeded();
            }
            else
            {
                Debug.WriteLine("[C#] Login failed!");
                // Báo cho JS biết login thất bại
                await JsNotifyAsync("spotify.login.failed", new { error = "Login failed or was cancelled." });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[C#] Login exception: {ex.Message}");
            await JsNotifyAsync("spotify.login.failed", new { error = ex.Message });
        }
    }
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await webView.EnsureCoreWebView2Async();
        _bridge = new PlayerBridge(_engine, webView);
        webView.CoreWebView2.AddHostObjectToScript("player", _bridge);
    }

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            // Tắt mini nếu còn mở
            if (_mini != null)
            {
                _mini.Close();
                _mini = null;
            }

            // Giải phóng WebView2 để tránh giữ lock exe khi rebuild
            if (webView != null)
            {
                try { webView.CoreWebView2?.Navigate("about:blank"); } catch { }
                try { webView.Dispose(); } catch { }
            }
        }
        finally
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}
