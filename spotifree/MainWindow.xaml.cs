using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Windows;

namespace spotifree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
    }
}