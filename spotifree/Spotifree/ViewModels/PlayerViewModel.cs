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
    public class PlayerViewModel : BaseViewModel
    {
        private readonly IAudioPlayerService _player;
        private LocalTrack? _currentTrack;
        private bool _isPlaying;
        private double _currentPosition;
        private double _duration;
        private double _volume;
        private readonly IViewModeService _viewModeService;

        public LocalTrack? CurrentTrack
        {
            get => _currentTrack;
            set => SetProperty(ref _currentTrack, value);
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public double CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

        public double Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public double Volume
        {
            get => _volume;
            set
            {
                if (SetProperty(ref _volume, value))
                {
                    _player.SetVolume(value); 
                }
            }
        }
        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SkipNextCommand { get; }
        public ICommand SkipPreviousCommand { get; }
        public ICommand SwitchToMiniModeCommand { get; }
        public ICommand SwitchToMainModeCommand { get; }

        public PlayerViewModel(IAudioPlayerService player, IViewModeService viewModeService)
        {

            _player = player;
            _viewModeService = viewModeService;

            _player.PlaybackStateChanged += OnPlaybackStateChanged;
            _player.PositionChanged += OnPositionChanged;

            PlayPauseCommand = new RelayCommand(TogglePlayPause);
            StopCommand = new RelayCommand(_ => _player.Stop());
            SkipNextCommand = new RelayCommand(_ => _player.SkipNext());
            SkipPreviousCommand = new RelayCommand(_ => _player.SkipPrevious());

            SwitchToMiniModeCommand = new RelayCommand(_ => _viewModeService.SwitchToMiniPlayer());
            SwitchToMainModeCommand = new RelayCommand(_ => _viewModeService.SwitchToMainPlayer());

            Volume = _player.GetVolume();
        }

        // Toggles between Play and Pause.
        private void TogglePlayPause(object? param)
        {
            if (IsPlaying)
            {
                _player.Pause();
            }
            else
            {
                _player.Play();
            }
        }

        private void OnPlaybackStateChanged(PlayerState state)
        {
            IsPlaying = (state == PlayerState.Playing);
            if (state == PlayerState.Stopped)
            {
                CurrentTrack = null;
                CurrentPosition = 0;
                Duration = 0;
            }
        }

        private void OnPositionChanged(double position, double duration)
        {
            CurrentPosition = position;
            Duration = duration;
            CurrentTrack = _player.CurrentTrack;
        }
    }
}
