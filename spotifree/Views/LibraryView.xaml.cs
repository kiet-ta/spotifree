using Microsoft.Extensions.DependencyInjection;
using Spotifree.IServices;
using Spotifree.Models;
using Spotifree.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;

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
                    _playlistService.PlaylistsChanged += PlaylistService_PlaylistsChanged;
                    _playlistService.ReloadFromDisk();
                    PlaylistsItemsControl.ItemsSource = _playlistService.Playlists;
                }
            }
        }

        private void PlaylistService_PlaylistsChanged()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(PlaylistService_PlaylistsChanged);
                return;
            }

            if (_playlistService == null)
                return;

            if (PlaylistsItemsControl.ItemsSource == null)
            {
                PlaylistsItemsControl.ItemsSource = _playlistService.Playlists;
            }
            else
            {
                PlaylistsItemsControl.Items.Refresh();
            }
        }

        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null)
            {
                MessageBox.Show("Playlist service is not ready.");
                return;
            }

            var dialog = new SimpleTextDialog("New playlist name", "NewPlaylist");
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
                MessageBox.Show($"Can't create playlist: {ex.Message}");
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
                _playlistService.LoadTracksForPlaylist(playlist);

                var tracks = new ObservableCollection<LocalTrack>(playlist.Tracks);

                var albumVm = new AlbumViewModel(
                    playlist.Name,
                    string.Empty,
                    playlist.CoverArt,
                    tracks,
                    null);

                var mainVm = App.ServiceProvider.GetRequiredService<MainViewModel>();

                var detailType = typeof(AlbumDetailViewModel);
                var ctor = detailType.GetConstructors().First();
                var parameters = ctor.GetParameters();
                var args = new object?[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    var pType = parameters[i].ParameterType;

                    if (pType == typeof(AlbumViewModel))
                    {
                        args[i] = albumVm;
                    }
                    else if (!pType.IsValueType)
                    {
                        args[i] = App.ServiceProvider.GetService(pType);
                    }
                    else
                    {
                        args[i] = Activator.CreateInstance(pType);
                    }
                }

                var detailVm = (AlbumDetailViewModel)ctor.Invoke(args);
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

            if (sender is not MenuItem mi || mi.CommandParameter is not Playlist playlist)
                return;

            var dialog = new SimpleTextDialog("Rename playlist", playlist.Name);
            if (dialog.ShowDialog() != true)
                return;

            var name = dialog.Value?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return;

            try
            {
                _playlistService.RenamePlaylist(playlist, name);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not rename: " + ex.Message);
            }
        }

        private void ChangeCover_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null)
                return;

            if (sender is not MenuItem mi || mi.CommandParameter is not Playlist playlist)
                return;

            var ofd = new Forms.OpenFileDialog
            {
                Title = "Choose playlist cover",
                Filter = "Ảnh|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (ofd.ShowDialog() != Forms.DialogResult.OK)
                return;

            try
            {
                _playlistService.ChangeCover(playlist, ofd.FileName);
                // PlaylistService sẽ raise PlaylistsChanged → UI tự refresh
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't change cover: " + ex.Message);
            }
        }

        private void DeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null)
                return;

            if (sender is not MenuItem mi || mi.CommandParameter is not Playlist playlist)
                return;

            var result = MessageBox.Show(
                $"Delete playlist \"{playlist.Name}\"?",
                "Delete playlist",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _playlistService.DeletePlaylist(playlist);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't delete playlist: " + ex.Message);
            }
        }
    }
}
