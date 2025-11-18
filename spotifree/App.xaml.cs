using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spotifree.IServices;
using Spotifree.Services;
using Spotifree.ViewModels;
using Spotifree.Views;
using System;
using System.IO;
using System.Windows;

namespace Spotifree
{
    // Interaction logic for App.xaml
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        public static IConfiguration? Configuration { get; private set; }

        public App()
        {
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // ===== Service =====
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IAudioPlayerService, AudioPlayerService>();
            services.AddSingleton<IMusicLibraryService, MusicLibraryService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IViewModeService, ViewModeService>();
            services.AddSingleton<IConnectivityService, ConnectivityService>();
            services.AddSingleton<IFocusTimerService, FocusTimerService>();

            // NEW: Playlist service
            services.AddSingleton<IPlaylistService, PlaylistService>();

            // ===== ViewModel =====
            services.AddSingleton<PlayerViewModel>();
            services.AddSingleton<MainViewModel>();

            services.AddTransient<LibraryViewModel>();
            services.AddTransient<SettingsViewModel>();

            // NEW: AlbumViewModel để DI tạo được AlbumDetailViewModel
            services.AddTransient<AlbumViewModel>();

            services.AddTransient<AlbumDetailViewModel>();
            services.AddTransient<ChatViewModel>();
            services.AddSingleton<TourViewModel>();
            services.AddSingleton<FocusViewModel>();


            // ===== Gemini =====
            services.AddSingleton<IGeminiService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                string apiKey = config["Gemini:ApiKey"] ?? "";
                return new GeminiService(apiKey);
            });

            // ===== Window =====
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MiniPlayerWindow>(sp =>
                new MiniPlayerWindow(sp.GetRequiredService<PlayerViewModel>()));
        }

        // Application startup event handler.
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IConfiguration>(Configuration);

            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var geminiApiKey = Configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(geminiApiKey))
            {
                MessageBox.Show("Error: API Key was not set in appsettings.json!",
                                "Error Config", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }
    }
}
