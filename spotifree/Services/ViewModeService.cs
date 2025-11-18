using Microsoft.Extensions.DependencyInjection;
using Spotifree.IServices;
using Spotifree.Views;
using System.Windows;

namespace Spotifree.Services;

public class ViewModeService : IViewModeService
{
    private readonly IServiceProvider _serviceProvider;
    private Window? _mainWindow;

    public ViewModeService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void SwitchToMiniPlayer()
    {
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var miniPlayer = _serviceProvider.GetRequiredService<MiniPlayerWindow>();

        mainWindow.Hide();
        miniPlayer.Show();
    }

    public void SwitchToMainPlayer()
    {
        var miniPlayer = _serviceProvider.GetRequiredService<MiniPlayerWindow>();
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        miniPlayer.Hide();
        mainWindow.Show();
    }

    public void ShowMainWindow()
    {
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        if (!mainWindow.Dispatcher.CheckAccess())
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            });
        }
        else
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        }
    }
}