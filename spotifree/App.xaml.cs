using Microsoft.Extensions.DependencyInjection;
using spotifree.IServices;
using spotifree.Models;
using spotifree.Services;
using spotifree.ViewModels;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace spotifree;

public partial class App : Application
{
    private IServiceProvider _serviceProvider;
    private System.Windows.Forms.NotifyIcon _trayIcon;

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

        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {

        // Services
        services.AddSingleton(new SpotifyAuth(ClientId, RedirectUri, Scopes));
        services.AddSingleton<ISpotifyService, SpotifyApi>();
        services.AddSingleton<ILocalMusicService, LocalMusicService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddSingleton<LocalLibraryService<LocalMusicTrack>>(provider =>
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "library.json");
            return new LocalLibraryService<LocalMusicTrack>(path);
        });


        services.AddTransient<MainViewModel>();

        // Windows

        services.AddSingleton<Spotifree>();
        services.AddTransient<MusicDetail>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var spotifree = _serviceProvider.GetRequiredService<Spotifree>();
        spotifree.Show();
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _trayIcon = new System.Windows.Forms.NotifyIcon();
        var uri = new Uri("pack://application:,,,/app.ico");
        var streamInfo = Application.GetResourceStream(uri);
        _trayIcon.Icon = new Icon(streamInfo.Stream);
        _trayIcon.Text = "Spotifree";
        _trayIcon.Visible = true;


        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open", null, (s, ev) => Open_Click(s, null));
        contextMenu.Items.Add("Exit", null, (s, ev) => Exit_Click(s, null));

        _trayIcon.ContextMenuStrip = contextMenu;

        _trayIcon.DoubleClick += (s, ev) => Open_Click(s, null);
    }
    private void Open_Click(object sender, RoutedEventArgs e)
    {
        Current.MainWindow?.Show();
        Current.MainWindow.WindowState = WindowState.Normal;
        Current.MainWindow.Activate();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Current.Shutdown();
    }

}