using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace Spotifree.Helpers
{
    public static class TrayIconHelper
    {
        public static void HandleTrayDoubleClick(Window window)
        {
            ShowApp(window);
        }

        public static void HandleShowAppClick(Window window)
        {
            ShowApp(window);
        }

        public static void HandleExitAppClick(TaskbarIcon notifyIcon)
        {
            notifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        public static void HandleStateChange(Window window)
        {
            if (window.WindowState == WindowState.Minimized)
            {
                window.Hide();
            }
        }

        public static void HandleClosing(TaskbarIcon notifyIcon)
        {
            notifyIcon.Dispose();
        }

        private static void ShowApp(Window window)
        {
            window.Show();
            window.WindowState = WindowState.Normal;
            window.Activate();
        }
    }
}