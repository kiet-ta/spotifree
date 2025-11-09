using Microsoft.Extensions.DependencyInjection;
using spotifree.IServices;
using spotifree.Models;
using spotifree.Services;
using spotifree.ViewModels;
using System; // Thêm
using System.IO; // Thêm
using System.Data;
using System.Windows;

namespace spotifree
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        // Các h?ng s? c?a b?n (gi? nguyên)
        private const string ClientId = "5289d37828a5414485019a41cccad2bd";
        private const string RedirectUri = "http://127.0.0.1:5173/callback";
        private const string Scopes = "streaming user-read-email user-read-private " +
                                      "user-read-playback-state user-modify-playback-state " +
                                      "playlist-read-private playlist-modify-private " +
                                      "user-read-recently-played user-top-read";
        public IServiceProvider ServiceProvider => _serviceProvider;
        public App()
        {
            IServiceCollection services = new ServiceCollection();
            
            // 1. Ch? g?i ConfigureServices
            ConfigureServices(services);
            
            // 2. Build ServiceProvider M?T L?N DUY NH?T ? cu?i
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // --- ??ng ký t?t c? service c?a b?n ? ?ÂY ---

            // Services
            services.AddSingleton(new SpotifyAuth(ClientId, RedirectUri, Scopes));
            services.AddSingleton<ISpotifyService, SpotifyApi>();
            services.AddSingleton<ILocalMusicService, LocalMusicService>();
            services.AddSingleton<ISettingsService, SettingsService>();

            // ??ng ký LocalLibraryService<T> (cách này ?úng)
            services.AddSingleton<LocalLibraryService<LocalMusicTrack>>(provider =>
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.json");
                return new LocalLibraryService<LocalMusicTrack>(path);
            });

            // L?i c?a b?n ? ?ây:
            // services.AddSingleton<ILocalLibraryService, LocalLibraryService>();
            // ^^^ XÓA DÒNG NÀY. LocalLibraryService là generic (LocalLibraryService<T>)
            // b?n không th? ??ng ký nó mà không có ki?u d? li?u.

            // ViewModels
            services.AddTransient<MainViewModel>();

            // Windows
            // AddSingleton cho c?a s? chính (ch? có 1)
            services.AddSingleton<Spotifree>(); 
            // AddTransient cho các c?a s? ph? (có th? m? nhi?u)
            services.AddTransient<MusicDetail>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Ph?n này c?a b?n ?ã ?úng!
            var spotifree = _serviceProvider.GetRequiredService<Spotifree>();
            spotifree.Show();
            base.OnStartup(e);
        }
    }
}