using Spotifree.Constances;
using Spotifree.IServices;
using Spotifree.Models;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;

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
        private RepeatMode _repeatMode;
        private readonly IViewModeService _viewModeService;

        private const int NumberOfBands = 32;
        public ObservableCollection<FrequencyBandViewModel> FrequencyBands { get; }

        

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
            set
            {
                if (SetProperty(ref _currentPosition, value))
                {
                    _player.Seek(value);
                }
            }
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

        public RepeatMode RepeatMode
        {
            get => _repeatMode;
            set
            {
                if (SetProperty(ref _repeatMode, value))
                {
                    _player.RepeatMode = value;
                }
            }
        }
        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SkipNextCommand { get; }
        public ICommand SkipPreviousCommand { get; }
        public ICommand SwitchToMiniModeCommand { get; }
        public ICommand SwitchToMainModeCommand { get; }
        public ICommand ToggleRepeatCommand { get; }
        public PlayerViewModel(IAudioPlayerService player, IViewModeService viewModeService)
        {
            _player = player;
            _viewModeService = viewModeService;

            FrequencyBands = new ObservableCollection<FrequencyBandViewModel>();
            for (int i = 0; i < NumberOfBands; i++)
            {
                FrequencyBands.Add(new FrequencyBandViewModel { Level = 0 });
            }

            _player.PlaybackStateChanged += OnPlaybackStateChanged;
            _player.PositionChanged += OnPositionChanged;

            PlayPauseCommand = new RelayCommand(TogglePlayPause);
            StopCommand = new RelayCommand(_ => _player.Stop());
            SkipNextCommand = new RelayCommand(_ => _player.SkipNext());
            SkipPreviousCommand = new RelayCommand(_ => _player.SkipPrevious());
            ToggleRepeatCommand = new RelayCommand(ExecuteToggleRepeat);

            SwitchToMiniModeCommand = new RelayCommand(_ => _viewModeService.SwitchToMiniPlayer());
            SwitchToMainModeCommand = new RelayCommand(_ => _viewModeService.SwitchToMainPlayer());

            Volume = _player.GetVolume();
            RepeatMode = _player.RepeatMode;

            _player.FrequencyDataAvailable += OnFrequencyDataAvailable;
        }

        private void OnFrequencyDataAvailable(float[] frequencyData)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (!IsPlaying)
                {
                    for (int i = 0; i < FrequencyBands.Count; i++)
                    {
                        FrequencyBands[i].Level = 0;
                    }
                    return;
                }

                for (int i = 0; i < frequencyData.Length && i < FrequencyBands.Count; i++)
                {
                    FrequencyBands[i].Level = frequencyData[i];
                }
            });
        }

        private void ExecuteToggleRepeat(object? obj)
        {
            // Cycle: None -> All -> One -> None
            RepeatMode = RepeatMode switch
            {
                RepeatMode.None => RepeatMode.RepeatAll,
                RepeatMode.RepeatAll => RepeatMode.RepeatOne,
                RepeatMode.RepeatOne => RepeatMode.None,
                _ => RepeatMode.None
            };
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
                CurrentPosition = 0;
            }
        }

        private void OnPositionChanged(double position, double duration)
        {
            if (Math.Abs(_currentPosition - position) > 1.0)
            {
                SetProperty(ref _currentPosition, position, nameof(CurrentPosition));
            }
            SetProperty(ref _duration, duration, nameof(Duration));

            // Ensure track info is synced
            if (CurrentTrack != _player.CurrentTrack)
                CurrentTrack = _player.CurrentTrack;
        }
    }
}