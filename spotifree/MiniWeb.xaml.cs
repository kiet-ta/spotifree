using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;

namespace spotifree
{
    /// <summary>
    /// Interaction logic for MiniWeb.xaml
    /// </summary>
    public partial class MiniWeb : Window
    {
        public event Action<double>? SeekRequested;
        public event Action? CloseRequested;
        public event Action<double>? OnSeek;
        public event Action? OnBackToMain;
        public event Action? OnPlayPause;
        public event Action? OnPrev;
        public event Action? OnNext;
        public event Action? OnToggleRepeat;
        public MiniWeb() => InitializeComponent();

        public async Task InitAsync(string viewsPath)
        {
            await miniWeb.EnsureCoreWebView2Async();
            var core = miniWeb.CoreWebView2;

            core.SetVirtualHostNameToFolderMapping(
                "spotifree.local", viewsPath, CoreWebView2HostResourceAccessKind.Allow);

            core.Settings.IsWebMessageEnabled = true;
            core.WebMessageReceived += Core_WebMessageReceived;
            miniWeb.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                try
                {
                    var raw = e.TryGetWebMessageAsString();
                    using var doc = JsonDocument.Parse(raw ?? "{}");
                    var root = doc.RootElement;
                    var type = root.TryGetProperty("type", out var t) ? t.GetString() : null;
                    switch (type)
                    {
                        case "dragWindow":
                            Dispatcher.Invoke(() => { try { DragMove(); } catch { } });
                            break;
                        case "close":
                            Dispatcher.Invoke(() =>
                            {
                                CloseRequested?.Invoke();
                                Close();
                            });
                            break;
                        case "seek":
                            if (root.TryGetProperty("seconds", out var secEl) &&
                                secEl.TryGetDouble(out var sec))
                            {
                                // chuyển tiếp về spotifree → trang chính
                                OnSeek?.Invoke(sec);
                                SeekRequested?.Invoke(sec);
                            }
                            break;
                        case "backToMain":
                            OnBackToMain?.Invoke();
                            break;

                        // ===== mới thêm =====
                        case "playPause":
                            OnPlayPause?.Invoke();
                            break;
                        case "prev":
                            OnPrev?.Invoke();
                            break;
                        case "next":
                            OnNext?.Invoke();
                            break;
                        case "toggleRepeat":
                            OnToggleRepeat?.Invoke();
                            break;
                        case "miniReady":
                            // MainWindow có thể lắng nghe event này để push state/queue hiện tại
                            // ví dụ: MiniReady?.Invoke();
                            break;
                    }
                }
                catch { /* ignore */ }
            };

            core.Navigate("https://spotifree.local/mini.html");

        }

        private void Core_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            using var doc = JsonDocument.Parse(e.WebMessageAsJson);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();

            switch (type)
            {
                case "seek":
                    if (root.TryGetProperty("seconds", out var s) && s.TryGetDouble(out var sec))
                        SeekRequested?.Invoke(sec);
                    break;
                case "close":
                    CloseRequested?.Invoke();
                    this.Close();
                    break;
                case "dragWindow":
                    try { DragMove(); } catch { /* ignore */ }
                    break;
            }
        }

        public void SendToMini(object payload)
        {
            if (miniWeb.CoreWebView2 == null) return;
            var json = JsonSerializer.Serialize(payload);
            miniWeb.CoreWebView2.PostWebMessageAsJson(json);
        }

        public void SendToMiniRaw(string json)
        {
            if (miniWeb.CoreWebView2 == null) return;
            miniWeb.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void DragBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { /* ignore */ }
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
