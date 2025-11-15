using Microsoft.Extensions.DependencyInjection;
using Spotifree.IServices;
using Spotifree.Services;
using Spotifree.ViewModels;
using Spotifree.Views;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
namespace Spotifree
{
    // Interaction logic for App.xaml
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        // Configures the dependency injection container.
        private void ConfigureServices(IServiceCollection services)
        {
            //Service
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IAudioPlayerService, AudioPlayerService>();
            services.AddSingleton<IMusicLibraryService, MusicLibraryService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IViewModeService, ViewModeService>();
            services.AddSingleton<IConnectivityService, ConnectivityService>();

            //ViewModel
            services.AddSingleton<PlayerViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<LibraryViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<AlbumDetailViewModel>();
            services.AddTransient<ChatViewModel>(); 
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IGeminiService, GeminiService>();

            //Window
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MiniPlayerWindow>();
        }

        // Application startup event handler.
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            if (mainWindow != null)
            {
                mainWindow.DataContext = _serviceProvider.GetService<MainViewModel>();
                var viewModeService = _serviceProvider.GetService<IViewModeService>();

                mainWindow.Show();

                var themeService = _serviceProvider.GetService<IThemeService>();
                themeService?.ApplyThemeOnStart();
            }
            base.OnStartup(e);
        }
    }
}
