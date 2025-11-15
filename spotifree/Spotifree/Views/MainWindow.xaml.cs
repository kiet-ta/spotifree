using Spotifree.Helpers;
using System.Windows;

namespace Spotifree
{
    // Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            TrayIconHelper.HandleTrayDoubleClick(this);
        }

        private void ShowApp_Click(object sender, RoutedEventArgs e)
        {
            TrayIconHelper.HandleShowAppClick(this);
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            TrayIconHelper.HandleExitAppClick(SpotifreeIcon);
        }

        protected override void OnStateChanged(System.EventArgs e)
        {
            TrayIconHelper.HandleStateChange(this);
            base.OnStateChanged(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            TrayIconHelper.HandleClosing(SpotifreeIcon);
            base.OnClosing(e);
        }
    }
}