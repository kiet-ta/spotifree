using Microsoft.Extensions.DependencyInjection;
using Spotifree.IServices;
using Spotifree.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace Spotifree.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly ISettingsService? _settingsService;
        private readonly IPlaylistService? _playlistService;

        public SettingsView()
        {
            InitializeComponent();

            if (App.ServiceProvider != null)
            {
                _settingsService = App.ServiceProvider.GetService<ISettingsService>();
                _playlistService = App.ServiceProvider.GetService<IPlaylistService>();
                _ = LoadSettingsAsync();
            }
        }

        private async Task LoadSettingsAsync()
        {
            if (_settingsService == null)
                return;

            AppSettings settings = await _settingsService.GetAsync();

            PlaylistRootPathTextBox.Text = settings.PlaylistRootPath ?? string.Empty;

            if (_playlistService != null && !string.IsNullOrWhiteSpace(settings.PlaylistRootPath))
            {
                _playlistService.RootPath = settings.PlaylistRootPath;
                _playlistService.ReloadFromDisk();
            }
        }

        private async void SelectPlaylistRoot_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsService == null || _playlistService == null)
                return;

            using var dialog = new WinForms.FolderBrowserDialog();
            if (dialog.ShowDialog() != WinForms.DialogResult.OK)
                return;

            var selectedPath = dialog.SelectedPath;

            PlaylistRootPathTextBox.Text = selectedPath;

            var settings = await _settingsService.GetAsync();
            settings.PlaylistRootPath = selectedPath;
            await _settingsService.SaveAsync(settings);

            _playlistService.RootPath = selectedPath;
            _playlistService.ReloadFromDisk();
        }
    }
}
