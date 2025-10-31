using System.Threading.Tasks;
using spotifree.Models;
using Microsoft.Web.WebView2.Wpf;

namespace spotifree.IServices
{
    public interface ISettingsService
    {
        Task<AppSettings> GetAsync();
        Task SaveAsync(AppSettings s);

        // tiện ích
        void ApplyZoom(WebView2 webView, int percent);
        void EnsureStorageFolder(AppSettings s);
    }
}
