using Spotifree.IServices;
using Spotifree.Models;
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
                _playlistService = App.ServiceProvider.GetService(typeof(IPlaylistService)) as IPlaylistService;
            }
        }

        private void TrackItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (_playlistService == null)
                return;

            if (sender is not ListViewItem item || item.DataContext is not LocalTrack track)
                return;

            if (string.IsNullOrWhiteSpace(_playlistService.RootPath))
            {
                MessageBox.Show("Chưa cấu hình thư mục playlist trong Settings.");
                return;
            }

            var menu = new ContextMenu();
            var addMenu = new MenuItem { Header = "Thêm vào playlist" };

            foreach (var pl in _playlistService.Playlists)
            {
                var mi = new MenuItem { Header = pl.Name, Tag = pl };
                mi.Click += (_, __) => _playlistService.AddTrackToPlaylist(pl, track);
                addMenu.Items.Add(mi);
            }

            menu.Items.Add(addMenu);

            item.ContextMenu = menu;
            menu.IsOpen = true;
        }
    }
}
