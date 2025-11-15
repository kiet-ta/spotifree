using Spotifree.IServices;
using Spotifree.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public class LibraryViewModel : BaseViewModel
    {
        private readonly IMusicLibraryService _libraryService;
        private readonly IAudioPlayerService _audioPlayer;
        private readonly MainViewModel _mainViewModel;

        public ObservableCollection<AlbumViewModel> Albums { get; } = new();

        public ICommand SelectAlbumCommand { get; }

        public LibraryViewModel(IMusicLibraryService libraryService, IAudioPlayerService audioPlayer, MainViewModel mainViewModel)
        {
            _libraryService = libraryService;
            _audioPlayer = audioPlayer;
            _mainViewModel = mainViewModel;

            SelectAlbumCommand = new RelayCommand(OnSelectAlbum);
            _libraryService.LibraryChanged += OnLibraryChanged;

            LoadAlbums();
        }

        // Called by the service event when the library is rescanned.
        private void OnLibraryChanged()
        {
            LoadAlbums();
        }

        // Loads tracks and groups them into albums.
        public async void LoadAlbums()
        {
            Albums.Clear();
            var tracks = await _libraryService.GetLibraryAsync();

            var groupedByAlbum = tracks
                .GroupBy(t => t.Album ?? "Unknown Album")
                .Select(g => new AlbumViewModel(g.Key, g.First().Artist, g.First().CoverArt, new ObservableCollection<LocalTrack>(g)));

            foreach (var album in groupedByAlbum)
            {
                Albums.Add(album);
            }
        }

        // Navigates to the detail view for the selected album.
        private void OnSelectAlbum(object? albumObj)
        {
            if (albumObj is AlbumViewModel album)
            {
                var detailViewModel = new AlbumDetailViewModel(album, _audioPlayer, _mainViewModel);
                _mainViewModel.NavigateTo(detailViewModel);
            }
        }
    }
}