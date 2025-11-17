using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public enum LibrarySection
    {
        ScannedMusic,
        Playlists
    }

    public class LibraryViewModel : BaseViewModel
    {
        private readonly IMusicLibraryService _libraryService;
        private readonly IAudioPlayerService _player;
        private readonly IPlaylistService _playlistService;
        private readonly MainViewModel _mainViewModel;

        private bool _hasTracks;
        private LibrarySection _currentSection = LibrarySection.ScannedMusic;
        private PlaylistViewModel? _selectedPlaylist;

        public event Action? RequestNavigateToSettings;

        public ObservableCollection<LocalTrack> ScannedTracks { get; } = new();
        public ObservableCollection<PlaylistViewModel> Playlists { get; } = new();

        public bool HasTracks
        {
            get => _hasTracks;
            set => SetProperty(ref _hasTracks, value);
        }

        public LibrarySection CurrentSection
        {
            get => _currentSection;
            set
            {
                if (SetProperty(ref _currentSection, value))
                {
                    OnPropertyChanged(nameof(IsScannedMusicSelected));
                    OnPropertyChanged(nameof(IsPlaylistsSelected));
                }
            }
        }

        public bool IsScannedMusicSelected
        {
            get => CurrentSection == LibrarySection.ScannedMusic;
            set
            {
                if (value)
                    CurrentSection = LibrarySection.ScannedMusic;
            }
        }

        public bool IsPlaylistsSelected
        {
            get => CurrentSection == LibrarySection.Playlists;
            set
            {
                if (value)
                    CurrentSection = LibrarySection.Playlists;
            }
        }

        public PlaylistViewModel? SelectedPlaylist
        {
            get => _selectedPlaylist;
            set
            {
                if (SetProperty(ref _selectedPlaylist, value))
                {
                    ((RelayCommand)PlayPlaylistCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand GoToSettingsCommand { get; }
        public ICommand PlayTrackCommand { get; }
        public ICommand CreatePlaylistCommand { get; }
        public ICommand PlayPlaylistCommand { get; }
        public ICommand AddTrackToPlaylistCommand { get; }

        public LibraryViewModel(
            IMusicLibraryService library,
            IAudioPlayerService player,
            IPlaylistService playlistService,
            MainViewModel mainViewModel)
        {
            _libraryService = library;
            _player = player;
            _playlistService = playlistService;
            _mainViewModel = mainViewModel;

            _libraryService.LibraryChanged += OnLibraryChanged;

            GoToSettingsCommand = new RelayCommand(_ => RequestNavigateToSettings?.Invoke());

            PlayTrackCommand = new RelayCommand(async param =>
            {
                if (param is LocalTrack track)
                    await ExecutePlayTrackAsync(track);
            });

            CreatePlaylistCommand = new RelayCommand(async _ => await ExecuteCreatePlaylistAsync());

            PlayPlaylistCommand = new RelayCommand(
                async _ => await ExecutePlayPlaylistAsync(),
                _ => SelectedPlaylist != null && SelectedPlaylist.Tracks.Any());

            AddTrackToPlaylistCommand = new RelayCommand(
                async param =>
                {
                    if (param is LocalTrack track)
                        await ExecuteAddTrackToPlaylistAsync(track);
                },
                param => SelectedPlaylist != null && param is LocalTrack);

            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            await LoadScannedTracksAsync();
            await LoadPlaylistsAsync();
        }

        private async Task LoadScannedTracksAsync()
        {
            ScannedTracks.Clear();

            var tracks = await _libraryService.GetLibraryAsync();
            foreach (var t in tracks.OrderBy(t => t.Title))
            {
                ScannedTracks.Add(t);
            }

            HasTracks = ScannedTracks.Any();
        }

        private async Task LoadPlaylistsAsync()
        {
            Playlists.Clear();

            var playlists = await _playlistService.GetPlaylistsAsync();
            foreach (var pl in playlists)
            {
                Playlists.Add(new PlaylistViewModel(pl));
            }

            if (Playlists.Any() && SelectedPlaylist == null)
            {
                SelectedPlaylist = Playlists.First();
            }
        }

        private void OnLibraryChanged()
        {
            Application.Current.Dispatcher.Invoke(async () => await LoadScannedTracksAsync());
        }

        private async Task ExecutePlayTrackAsync(LocalTrack track)
        {
            await _player.LoadPlaylist(new[] { track }, 0);
            _player.Play();
        }

        private async Task ExecuteCreatePlaylistAsync()
        {
            var name = $"New Playlist {Playlists.Count + 1}";
            var playlist = await _playlistService.CreatePlaylistAsync(name, Enumerable.Empty<LocalTrack>());

            var vm = new PlaylistViewModel(playlist);
            Playlists.Add(vm);
            SelectedPlaylist = vm;
        }

        private async Task ExecutePlayPlaylistAsync()
        {
            if (SelectedPlaylist == null || !SelectedPlaylist.Tracks.Any())
                return;

            await _player.LoadPlaylist(SelectedPlaylist.Tracks, 0);
            _player.Play();
        }

        private async Task ExecuteAddTrackToPlaylistAsync(LocalTrack track)
        {
            if (SelectedPlaylist == null)
                return;

            await _playlistService.AddTracksAsync(SelectedPlaylist.Id, new[] { track });

            if (!SelectedPlaylist.Tracks.Any(t =>
                    string.Equals(t.FilePath, track.FilePath, StringComparison.OrdinalIgnoreCase)))
            {
                SelectedPlaylist.Tracks.Add(track);
            }
        }
    }
}
