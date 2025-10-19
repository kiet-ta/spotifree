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
    }
}