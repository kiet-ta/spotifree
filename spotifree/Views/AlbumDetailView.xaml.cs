using Microsoft.Extensions.DependencyInjection;
using Spotifree.IServices;
using Spotifree.Models;
using Spotifree.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spotifree.Views
{
    public partial class AlbumDetailView : UserControl
    {
        private readonly IPlaylistService? _playlistService;

        public AlbumDetailView()
        {
            InitializeComponent();

            if (App.ServiceProvider != null)
            {
                _playlistService = App.ServiceProvider.GetService<IPlaylistService>();
            }
        }

        private void TrackItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (_playlistService == null)
                return;

            if (sender is not ListViewItem item || item.DataContext is not LocalTrack track)
                return;

            var menu = new ContextMenu();

            var addMenu = new MenuItem { Header = "Thêm vào playlist" };

            foreach (var pl in _playlistService.Playlists)
            {
                var mi = new MenuItem
                {
                    Header = pl.Name,
                    Tag = (pl, track)
                };
                mi.Click += AddTrackToPlaylist_Click;
                addMenu.Items.Add(mi);
            }

            var newPlaylistItem = new MenuItem { Header = "Playlist mới..." };
            newPlaylistItem.Click += (s, _) =>
            {
                var dlg = new SimpleTextDialog("Tên playlist mới", "Playlist mới");
                if (dlg.ShowDialog() != true)
                    return;

                var name = dlg.Value?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    return;

                var playlist = _playlistService.CreatePlaylist(name);
                _playlistService.AddTrackToPlaylist(playlist, track);
            };
            addMenu.Items.Add(new Separator());
            addMenu.Items.Add(newPlaylistItem);

            menu.Items.Add(addMenu);

            Playlist? currentPlaylist = null;
            if (DataContext is AlbumDetailViewModel vm && vm.Album != null)
            {
                var albumName = vm.Album.Name;
                currentPlaylist = _playlistService.Playlists
                    .FirstOrDefault(p => p.Name == albumName);
            }

            if (currentPlaylist != null)
            {
                menu.Items.Add(new Separator());

                var removeItem = new MenuItem
                {
                    Header = "Xóa khỏi playlist này",
                    Tag = (currentPlaylist, track)
                };
                removeItem.Click += RemoveTrackFromPlaylist_Click;
                menu.Items.Add(removeItem);
            }

            item.ContextMenu = menu;
            menu.IsOpen = true;
        }

        private void AddTrackToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null)
                return;

            if (sender is not MenuItem mi || mi.Tag is not (Playlist playlist, LocalTrack track))
                return;

            _playlistService.AddTrackToPlaylist(playlist, track);
        }

        private void RemoveTrackFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistService == null)
                return;

            if (sender is not MenuItem mi || mi.Tag is not (Playlist playlist, LocalTrack track))
                return;

            _playlistService.RemoveTrackFromPlaylist(playlist, track);

            if (DataContext is AlbumDetailViewModel vm && vm.Album?.Tracks != null)
            {
                var toRemove = vm.Album.Tracks
                    .FirstOrDefault(t => t.FilePath == track.FilePath);

                if (toRemove != null)
                    vm.Album.Tracks.Remove(toRemove);
            }
        }
    }
}
