using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
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
                    _playlistService.PlaylistsChanged += PlaylistService_PlaylistsChanged;
                }
            }
        }

        private void PlaylistService_PlaylistsChanged()
        {
            Dispatcher.Invoke(() =>
            {
                if (_playlistService != null)
                    PlaylistsItemsControl.ItemsSource = _playlistService.Playlists;
            });
        }

        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null)
            {
                MessageBox.Show("Playlist service chưa sẵn sàng.");
                return;
            }

            var dialog = new SimpleTextDialog("Tên playlist mới", "Playlist mới");
            if (dialog.ShowDialog() != true) return;

            var name = dialog.Value?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

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
                    playlist.CoverArt,
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

        private Playlist? GetPlaylistFromMenu(object sender)
        {
            if (sender is not MenuItem mi) return null;

            if (mi.DataContext is Playlist p1) return p1;

            if (mi.Parent is ContextMenu cm &&
                cm.PlacementTarget is FrameworkElement fe &&
                fe.DataContext is Playlist p2)
            {
                return p2;
            }

            return null;
        }

        private void RenamePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null) return;

            var playlist = GetPlaylistFromMenu(sender);
            if (playlist == null) return;

            var dialog = new SimpleTextDialog("Đổi tên playlist", playlist.Name);
            if (dialog.ShowDialog() != true) return;

            var newName = dialog.Value?.Trim();
            if (string.IsNullOrWhiteSpace(newName)) return;

            _playlistService.RenamePlaylist(playlist, newName);
        }

        private void ChangeCover_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null) return;

            var playlist = GetPlaylistFromMenu(sender);
            if (playlist == null) return;

            var dialog = new OpenFileDialog
            {
                Filter = "Ảnh|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Chọn ảnh playlist"
            };

            if (dialog.ShowDialog() == true)
            {
                _playlistService.ChangeCover(playlist, dialog.FileName);
            }
        }

        private void ResetCover_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null) return;

            var playlist = GetPlaylistFromMenu(sender);
            if (playlist == null) return;

            _playlistService.ChangeCover(playlist, null);
        }

        private void DeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null) return;

            var playlist = GetPlaylistFromMenu(sender);
            if (playlist == null) return;

            var result = MessageBox.Show(
                $"Xóa playlist \"{playlist.Name}\"?",
                "Xóa playlist",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _playlistService.DeletePlaylist(playlist);
        }
    }
}
