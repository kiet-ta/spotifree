using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace spotifree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MiniWeb? _mini;

        private string? _lastPlayerStateRawJson; // cache để mini mở lên có state ngay
        private string _viewsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views");
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
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
                MessageBox.Show(ex.ToString());
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
