using Microsoft.Extensions.DependencyInjection;
using spotifree.IServices;
using spotifree.Services;
using spotifree.ViewModels;
using System.Data;
using System.Windows;

namespace spotifree
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            services.AddSingleton<ISpotifyService, SpotifyApi>();

            services.AddTransient<MusicDetailViewModel>();
           

            
            services.AddTransient<MusicDetail>();
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }

        private const string ClientId = "5289d37828a5414485019a41cccad2bd";

        private const string RedirectUri = "http://127.0.0.1:5173/callback";

        private const string Scopes = "streaming user-read-email user-read-private " +
                                      "user-read-playback-state user-modify-playback-state " +
                                      "playlist-read-private playlist-modify-private " +
                                      "user-read-recently-played user-top-read";

        private void ConfigureServices(IServiceCollection services)
        {
            // Register your services and view models here
            services.AddSingleton(new SpotifyAuth(ClientId, RedirectUri, Scopes));
            services.AddSingleton<ISpotifyService, SpotifyApi>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ISettingsService, SettingsService>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            mainWindow.Show();

            //var musicDetail = _serviceProvider.GetRequiredService<MusicDetail>();
            //musicDetail.Show();

            base.OnStartup(e);
        }
    }
}
