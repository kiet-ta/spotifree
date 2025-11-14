using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace spotifree
{
    public partial class MiniWeb : Window
    {
        public event Action<double>? SeekRequested;
        public event Action? CloseRequested;

        // callback ngược về Spotifree
        public event Action<double>? OnSeek;
        public event Action? OnBackToMain;
        public event Action? OnPlayPause;
        public event Action? OnPrev;
        public event Action? OnNext;
        public event Action? OnToggleRepeat;
        public event Action? OnMiniReady; // <- cái này đang báo lỗi, giờ khai báo luôn

        public MiniWeb() => InitializeComponent();

        public async Task InitAsync(string viewsPath)
        {
            await miniWeb.EnsureCoreWebView2Async();
            var core = miniWeb.CoreWebView2;

            core.SetVirtualHostNameToFolderMapping(
                "spotifree.local", viewsPath, CoreWebView2HostResourceAccessKind.Allow);

            core.Settings.IsWebMessageEnabled = true;

            // NHẬN MESSAGE TỪ mini.html
            miniWeb.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                try
                {
                    // JS đang postMessage(object) nên đọc bằng WebMessageAsJson
                    var raw = e.WebMessageAsJson;
                    using var doc = JsonDocument.Parse(raw);
                    var root = doc.RootElement;
                    if (root.ValueKind != JsonValueKind.Object) return;

                    var type = root.TryGetProperty("type", out var tEl) ? tEl.GetString() : null;
                    switch (type)
                    {
                        case "dragWindow":
                            Dispatcher.Invoke(() =>
                            {
                                try { DragMove(); } catch { }
                            });
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
                                Dispatcher.Invoke(() =>
                                {
                                    OnSeek?.Invoke(sec);
                                    SeekRequested?.Invoke(sec);
                                });
                            }
                            break;

                        case "backToMain":
                            Dispatcher.Invoke(() => OnBackToMain?.Invoke());
                            break;

                        case "playPause":
                            Dispatcher.Invoke(() => OnPlayPause?.Invoke());
                            break;

                        case "prev":
                            Dispatcher.Invoke(() => OnPrev?.Invoke());
                            break;

                        case "next":
                            Dispatcher.Invoke(() => OnNext?.Invoke());
                            break;

                        case "toggleRepeat":
                            Dispatcher.Invoke(() => OnToggleRepeat?.Invoke());
                            break;

                        case "miniReady":
                            Dispatcher.Invoke(() => OnMiniReady?.Invoke());
                            break;
                    }
                }
                catch
                {
                    // ignore
                }
            };

            core.Navigate("https://spotifree.local/mini.html");
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
            System.Diagnostics.Debug.WriteLine("[MiniWeb] SendToMiniRaw: " + json);
            miniWeb.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void DragBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { /* ignore */ }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
