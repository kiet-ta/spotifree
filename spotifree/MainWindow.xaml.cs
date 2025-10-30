using Microsoft.Web.WebView2.Core;
using spotifree.IServices;
using spotifree.Services;
using System;
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
    private readonly ISettingsService _settings;
    private string? _lastPlayerStateRawJson; // cache để mini mở lên có state ngay
    private string _viewsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views");

    public MainWindow(ISpotifyService spotifyService, SpotifyAuth auth, ISettingsService settings)
    {
        
        _spotifyService = spotifyService;
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
            // áp dụng zoom theo settings
            var cur = await _settings.GetAsync();
            _settings.ApplyZoom(webView, cur.ZoomPercent);

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

                    // ========== SETTINGS INTEROP ==========
                    // ===== SETTINGS INTEROP =====
                    if (root.TryGetProperty("action", out var actProp))
                    {
                        string actionName = actProp.GetString() ?? string.Empty;

                        switch (actionName)
                        {
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
}
