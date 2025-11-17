using Microsoft.VisualBasic;
using Spotifree.IServices;
using Spotifree.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.Generic; // Thêm
using System.Linq; // Thêm

namespace Spotifree.ViewModels
{
    public class LibraryViewModel : BaseViewModel
    {
        private readonly IMusicLibraryService _libraryService;
        private readonly IAudioPlayerService _player;
        private readonly MainViewModel _mainViewModel;
        private bool _hasAlbums;
        public event Action RequestNavigateToSettings;
        public ObservableCollection<AlbumViewModel> Albums { get; }

        // --- BẮT ĐẦU CODE MỚI CHO TÌM KIẾM ---

        // 1. Lưu trữ danh sách tracks gốc để tìm kiếm
        private List<LocalTrack> _allTracks = new();

        // 2. Danh sách kết quả tìm kiếm
        public ObservableCollection<LocalTrack> SearchResults { get; }

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                SetProperty(ref _searchQuery, value);
                FilterTracks(value);
            }
        }

        private bool _isSearching;
        // 3. Cờ để biết đang tìm kiếm hay đang xem album
        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        private LocalTrack? _selectedSearchTrack;
        public LocalTrack? SelectedSearchTrack
        {
            get => _selectedSearchTrack;
            set
            {
                // Khi người dùng chọn 1 track từ kết quả tìm kiếm
                if (SetProperty(ref _selectedSearchTrack, value) && value != null)
                {
                    PlayTrackFromSearch(value);
                    // Đặt lại thành null để có thể chọn lại cùng một mục
                    SetProperty(ref _selectedSearchTrack, null);
                }
            }
        }

        // --- KẾT THÚC CODE MỚI CHO TÌM KIẾM ---


        public bool HasAlbums
        {
            get => _hasAlbums;
            set => SetProperty(ref _hasAlbums, value);
        }

        public ICommand SelectAlbumCommand { get; }
        public ICommand GoToSettingsCommand { get; }
        public LibraryViewModel(
            IMusicLibraryService library,
            IAudioPlayerService player,
            MainViewModel mainViewModel)
        {
            _libraryService = library;
            _player = player;
            _mainViewModel = mainViewModel;
            Albums = new ObservableCollection<AlbumViewModel>();
            SearchResults = new ObservableCollection<LocalTrack>(); // Khởi tạo

            _libraryService.LibraryChanged += OnLibraryChanged;
            LoadAlbums();
            SelectAlbumCommand = new RelayCommand(ExecuteSelectAlbum);
            GoToSettingsCommand = new RelayCommand(_ => RequestNavigateToSettings?.Invoke());

        }
        private void ExecuteSelectAlbum(object? param)
        {
            if (param is AlbumViewModel album)
            {
                _mainViewModel.NavigateToAlbumDetail(album);
            }
        }

        private async void ExecuteRenameAlbum(object? param)
        {
            if (param is AlbumViewModel album)
            {
                // Hiện hộp thoại nhập tên từ thư viện VisualBasic
                string newName = Interaction.InputBox(
                    $"Nhập tên mới cho album '{album.Name}':",
                    "Đổi tên Album",
                    album.Name
                );

                // Nếu người dùng nhập gì đó và khác tên cũ
                if (!string.IsNullOrWhiteSpace(newName) && newName != album.Name)
                {
                    // Gọi Service đổi tên file
                    await _libraryService.UpdateAlbumNameAsync(album.Name, album.Artist, newName);
                }
            }
        }

        private async void LoadAlbums()
        {
            Albums.Clear();
            var tracks = await _libraryService.GetLibraryAsync();

            // --- BẮT ĐẦU CODE MỚI ---
            // 4. Lưu lại danh sách tracks gốc
            _allTracks.Clear();
            if (tracks != null)
            {
                _allTracks.AddRange(tracks);
            }
            // --- KẾT THÚC CODE MỚI ---

            if (tracks == null || !tracks.Any())
            {
                HasAlbums = false; // Cập nhật HasAlbums ở đây
                return;
            }

            var renameCommand = new RelayCommand(ExecuteRenameAlbum);

            var groupedByAlbum = tracks
                .GroupBy(t => new { AlbumName = t.Album ?? "Unknown Album", ArtistName = t.Artist ?? "Unknown Artist" })
                .Select(g => new AlbumViewModel(
                    g.Key.AlbumName,
                    g.Key.ArtistName,
                    LoadImageFromBytes(g.First().CoverArt),
                    new ObservableCollection<LocalTrack>(g.ToList()),
                    renameCommand
                ));

            foreach (var album in groupedByAlbum)
            {
                Albums.Add(album);
            }
            HasAlbums = Albums.Any();
        }

        private void OnLibraryChanged()
        {
            // Dùng Dispatcher để đảm bảo việc cập nhật UI xảy ra trên đúng luồng
            System.Windows.Application.Current.Dispatcher.Invoke(LoadAlbums);
        }

        private BitmapImage? LoadImageFromBytes(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        // --- BẮT ĐẦU CODE MỚI ---

        // 5. Lọc danh sách tracks dựa trên query
        private void FilterTracks(string query)
        {
            SearchResults.Clear();
            IsSearching = !string.IsNullOrWhiteSpace(query);

            if (IsSearching)
            {
                var results = _allTracks.Where(t =>
                    t.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    t.Artist.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    t.Album.Contains(query, StringComparison.OrdinalIgnoreCase)
                );

                foreach (var track in results)
                {
                    SearchResults.Add(track);
                }
            }
        }

        // 6. Phát nhạc từ kết quả tìm kiếm
        private async void PlayTrackFromSearch(LocalTrack track)
        {
            // Tìm index của track trong danh sách kết quả
            int trackIndex = SearchResults.IndexOf(track);
            if (trackIndex >= 0)
            {
                // Load toàn bộ danh sách kết quả vào playlist và chơi từ track đã chọn
                await _player.LoadPlaylist(SearchResults, trackIndex);
                _player.Play();
            }
        }
        // --- KẾT THÚC CODE MỚI ---
    }
}