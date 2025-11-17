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

            var item = sender as ListViewItem;
            if (item?.DataContext is not LocalTrack track)
                return;

            if (string.IsNullOrWhiteSpace(_playlistService.RootPath))
            {
                MessageBox.Show("Chưa cấu hình thư mục playlist trong Settings.");
                return;
            }

            // Playlist hiện tại: dựa theo filePath nằm trong folder nào
            Playlist? currentPlaylist = null;
            if (!string.IsNullOrEmpty(track.FilePath))
            {
                currentPlaylist = _playlistService.Playlists
                    .FirstOrDefault(p => track.FilePath!
                        .StartsWith(p.FolderPath, System.StringComparison.OrdinalIgnoreCase));
            }

            var menu = new ContextMenu();

            // NÚT XÓA KHỎI PLAYLIST nếu track nằm trong playlist nào đó
            if (currentPlaylist != null)
            {
                var removeItem = new MenuItem { Header = "Xóa khỏi playlist này" };
                removeItem.Click += (_, __) =>
                {
                    _playlistService.RemoveTrackFromPlaylist(currentPlaylist, track);

                    if (DataContext is AlbumDetailViewModel vm &&
                        vm.Album != null &&
                        vm.Album.Tracks.Contains(track))
                    {
                        vm.Album.Tracks.Remove(track);
                    }
                };
                menu.Items.Add(removeItem);
                menu.Items.Add(new Separator());
            }

            // Submenu: thêm vào playlist khác
            var addTo = new MenuItem { Header = "Thêm vào playlist" };
            foreach (var pl in _playlistService.Playlists)
            {
                var plItem = new MenuItem { Header = pl.Name, Tag = pl };
                plItem.Click += (_, __) =>
                {
                    _playlistService.AddTrackToPlaylist((Playlist)plItem.Tag!, track);
                };
                addTo.Items.Add(plItem);
            }
            if (addTo.Items.Count == 0)
                addTo.IsEnabled = false;

            menu.Items.Add(addTo);

            item.ContextMenu = menu;
            menu.IsOpen = true;
            e.Handled = true;
        }
    }
}
