using Microsoft.Web.WebView2.Core;
using spotifree.ViewModels; // Cần cho ViewModel
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input; // Cần cho MouseButtonEventArgs

namespace spotifree
{
    public partial class MusicDetail : Window
    {
        private MainViewModel _viewModel;
        // Sửa hàm khởi tạo để NHẬN ViewModel (Dependency Injection)
        public MusicDetail()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //// 1. Khởi tạo CoreWebView2
            //await webView.EnsureCoreWebView2Async(null);

            //// 2. ÁNH XẠ ĐƯỜNG DẪN TUYỆT ĐỐI
            //webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            //    "c.drive.local", @"C:\", CoreWebView2HostResourceAccessKind.Allow);

            //webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            //    "d.drive.local", @"D:\", CoreWebView2HostResourceAccessKind.Allow);
            await webView.EnsureCoreWebView2Async(null);

            // 2. ÁNH XẠ TẤT CẢ CÁC Ổ ĐĨA
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    // Ví dụ: C:\ -> "c.drive.local"
                    string hostName = $"{drive.Name[0]}.drive.local".ToLower();
                    string folderPath = drive.Name; // Ví dụ: "C:\"

                    webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        hostName, folderPath, CoreWebView2HostResourceAccessKind.Allow);

                    Console.WriteLine($"Mapped {hostName} -> {folderPath}");
                }
            }

            // 3. Kết nối ViewModel với WebView2
            _viewModel.InitializeBridge(webView.CoreWebView2);

            // 4. --- QUAN TRỌNG: Lắng nghe tin nhắn tại đây (View) ---
            // Hàm HandleAppWebMessage sẽ xử lý "toggleLibrary"
            webView.CoreWebView2.WebMessageReceived += HandleAppWebMessage;

            // 5. Điều hướng WebView2 đến file HTML
            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", "pages", "music_detail.html");
            webView.CoreWebView2.Navigate(htmlPath);
        }

        // --- HÀM XỬ LÝ ẨN/HIỆN ---

        // *** THAY ĐỔI: Bắt đầu với trạng thái "ẩn" (false) ***
        private bool isLibraryVisible = false;

        private void HandleAppWebMessage(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            string jsonMessage;
            try
            {
                jsonMessage = args.WebMessageAsJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi đọc tin nhắn WebView2: " + ex.Message);
                return;
            }

            if (string.IsNullOrEmpty(jsonMessage)) return;

            // Chỉ cần kiểm tra tin nhắn mà View quan tâm
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonMessage))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("type", out JsonElement typeElement) &&
                        typeElement.GetString() == "toggleLibrary")
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            isLibraryVisible = !isLibraryVisible;
                            LibraryColumn.Width = isLibraryVisible ? new GridLength(250) : new GridLength(0);
                        });
                    }
                    // Không cần phần 'else' nữa
                    else
                    {
                        //_viewModel.ReceiveWebMessage(jsonMessage);
                        // }
                    }
                }
            }
            catch (JsonException) { /* Bỏ qua tin nhắn không phải JSON */ }
        }

        //private async void Window_Loaded_1(object sender, RoutedEventArgs e)
        //{
            
        //}
    }
}