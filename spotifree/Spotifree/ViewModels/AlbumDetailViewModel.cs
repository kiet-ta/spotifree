using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public class AlbumDetailViewModel : BaseViewModel
    {
        private readonly IAudioPlayerService _audioPlayer;
        private readonly MainViewModel _mainViewModel;
        private AlbumViewModel _album;
        private LocalTrack? _selectedTrack;

        public AlbumViewModel Album
        {
            get => _album;
            set => SetProperty(ref _album, value);
        }

        public LocalTrack? SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                if (SetProperty(ref _selectedTrack, value) && value != null)
                {
                    PlayTrack(value);
                }
            }
        }

        public ICommand GoBackCommand { get; }
        public ICommand PlayAlbumCommand { get; }

        public AlbumDetailViewModel(AlbumViewModel album, IAudioPlayerService audioPlayer, MainViewModel mainViewModel)
        {
            _album = album;
            _audioPlayer = audioPlayer;
            _mainViewModel = mainViewModel;

            GoBackCommand = new RelayCommand(_ => _mainViewModel.NavigateToLibrary());
            PlayAlbumCommand = new RelayCommand(OnPlayAlbum);
        }

        // Plays the entire album starting from the first track.
        private async void OnPlayAlbum(object? obj)
        {
            if (Album.Tracks.Count > 0)
            {
                await _audioPlayer.LoadPlaylist(Album.Tracks, 0);
                _audioPlayer.Play();
            }
        }

        // Loads and plays a specific track.
        private async void PlayTrack(LocalTrack track)
        {
            int trackIndex = Album.Tracks.IndexOf(track);
            if (trackIndex >= 0)
            {
                await _audioPlayer.LoadPlaylist(Album.Tracks, trackIndex);
                _audioPlayer.Play();
            }
        }
    }
}
