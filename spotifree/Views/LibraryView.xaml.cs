using Microsoft.Extensions.DependencyInjection;
using Spotifree.IServices;
using Spotifree.Models;
using Spotifree.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Spotifree.Views
{
    public partial class LibraryView : UserControl
    {
        private readonly IPlaylistService? _playlistService;

        public LibraryView()
        {
            InitializeComponent();

            if (App.ServiceProvider != null)
            {
                _playlistService = App.ServiceProvider.GetService<IPlaylistService>();
                if (_playlistService != null)
                {
                    PlaylistsItemsControl.ItemsSource = _playlistService.Playlists;
                }
            }
        }

        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null)
            {
                MessageBox.Show("Playlist service chưa sẵn sàng.");
                return;
            }

            var dialog = new SimpleTextDialog("Tên playlist mới", "Playlist mới");
            if (dialog.ShowDialog() != true)
                return;

            var name = dialog.Value?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return;

            try
            {
                _playlistService.CreatePlaylist(name);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tạo được playlist: {ex.Message}");
            }
        }

        private void ViewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null || App.ServiceProvider == null)
                return;

            if (sender is not Button btn || btn.DataContext is not Playlist playlist)
                return;

            try
            {
                var mainVm = App.ServiceProvider.GetRequiredService<MainViewModel>();

                _playlistService.LoadTracksForPlaylist(playlist);

                var tracks = new ObservableCollection<LocalTrack>(playlist.Tracks);
                var albumVm = new AlbumViewModel(
                    playlist.Name,
                    string.Empty,
                    null,
                    tracks,
                    null);

                var detailType = typeof(AlbumDetailViewModel);
                var detailCtor = detailType.GetConstructors().First();
                var parameters = detailCtor.GetParameters();
                var ctorArgs = new object?[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    var pType = parameters[i].ParameterType;

                    if (pType == typeof(AlbumViewModel))
                    {
                        ctorArgs[i] = albumVm;
                    }
                    else if (!pType.IsValueType)
                    {
                        ctorArgs[i] = App.ServiceProvider.GetService(pType);
                    }
                    else
                    {
                        ctorArgs[i] = Activator.CreateInstance(pType);
                    }
                }

                var detailVm = (AlbumDetailViewModel)detailCtor.Invoke(ctorArgs);
                mainVm.CurrentPageViewModel = detailVm;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở playlist: " + ex.Message,
                    "Playlist", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenamePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null)
                return;

            if (sender is not MenuItem mi || mi.DataContext is not Playlist playlist)
                return;

            var dialog = new SimpleTextDialog("Đổi tên playlist", playlist.Name);
            if (dialog.ShowDialog() != true)
                return;

            var newName = dialog.Value?.Trim();
            if (string.IsNullOrWhiteSpace(newName) || string.Equals(newName, playlist.Name, StringComparison.Ordinal))
                return;

            try
            {
                _playlistService.RenamePlaylist(playlist, newName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không đổi tên được playlist: " + ex.Message,
                    "Playlist", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
