using Microsoft.VisualBasic;
using Spotifree.IServices;
using Spotifree.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.Generic; 
using System.Linq; 

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

        private List<LocalTrack> _allTracks = new();

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
                if (SetProperty(ref _selectedSearchTrack, value) && value != null)
                {
                    PlayTrackFromSearch(value);
                    SetProperty(ref _selectedSearchTrack, null);
                }
            }
        }


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
            SearchResults = new ObservableCollection<LocalTrack>();

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
                string newName = Interaction.InputBox(
                    $"Nhập tên mới cho album '{album.Name}':",
                    "Đổi tên Album",
                    album.Name
                );

                if (!string.IsNullOrWhiteSpace(newName) && newName != album.Name)
                {
                    await _libraryService.UpdateAlbumNameAsync(album.Name, album.Artist, newName);
                }
            }
        }

        private async void LoadAlbums()
        {
            Albums.Clear();
            var tracks = await _libraryService.GetLibraryAsync();

            _allTracks.Clear();
            if (tracks != null)
            {
                _allTracks.AddRange(tracks);
            }

            if (tracks == null || !tracks.Any())
            {
                HasAlbums = false;
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

        private async void FilterTracks(string query)
        {
            SearchResults.Clear();
            IsSearching = !string.IsNullOrWhiteSpace(query);

            if (IsSearching)
            {
                var results = await _libraryService.SearchTracksAsync(query);

                foreach (var track in results)
                {
                    SearchResults.Add(track);
                }
            }
        }

        private async void PlayTrackFromSearch(LocalTrack track)
        {
            int trackIndex = SearchResults.IndexOf(track);
            if (trackIndex >= 0)
            {
                await _player.LoadPlaylist(SearchResults, trackIndex);
                _player.Play();
            }
        }
    }
}