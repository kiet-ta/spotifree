using Microsoft.Extensions.DependencyInjection;
using Spotifree.IServices;
using System.Windows;
using System.Windows.Controls;

namespace Spotifree.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly IPlaylistService? _playlistService;

        public SettingsView()
        {
            InitializeComponent();

            if (App.ServiceProvider != null)
                _playlistService = App.ServiceProvider.GetService<IPlaylistService>();

            if (_playlistService != null)
                PlaylistRootPathTextBox.Text = _playlistService.RootPath ?? string.Empty;
        }

        private void SelectPlaylistRoot_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null)
                return;

            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _playlistService.RootPath = dialog.SelectedPath;
                PlaylistRootPathTextBox.Text = dialog.SelectedPath;
                _playlistService.ReloadFromDisk();
            }
        }
    }
}
