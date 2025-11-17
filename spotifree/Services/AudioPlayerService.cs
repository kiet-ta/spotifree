using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using NAudio.Wave;
using Spotifree.Constances;
namespace Spotifree.Services;

#pragma warning disable CA1416
public class AudioPlayerService : IAudioPlayerService, IDisposable
{
    private WaveOutEvent? _waveOut;
    private AudioFileReader? _audioFile;
    private DispatcherTimer _positionTimer;
    private double _currentVolume = 1.0;
    private List<LocalTrack> _playlist = new();
    private int _currentIndex = -1;
    private FrequencySpectrumProvider? _frequencyProvider;

    public PlayerState CurrentState { get; private set; } = PlayerState.Stopped;
    public LocalTrack? CurrentTrack { get; private set; }
    public double CurrentPosition => _audioFile?.CurrentTime.TotalSeconds ?? 0;
    public double Duration => _audioFile?.TotalTime.TotalSeconds ?? 0;

    public RepeatMode RepeatMode { get; set; } = RepeatMode.RepeatAll;

    public event Action<PlayerState>? PlaybackStateChanged;
    public event Action<double, double>? PositionChanged;
    public event Action? TrackEnded;
    public event Action<float[]>? FrequencyDataAvailable;

    public AudioPlayerService()
    {
        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _positionTimer.Tick += OnPositionTimerTick;
        _waveOut = new WaveOutEvent();
    }

    public async Task LoadPlaylist(IEnumerable<LocalTrack> playlist, int startIndex = 0)
    {
        _playlist = playlist.ToList();
        _currentIndex = Math.Clamp(startIndex, 0, _playlist.Count - 1);

        if (_currentIndex >= 0)
        {
            await LoadTrack(_playlist[_currentIndex]);
        }
    }

    private Task LoadTrack(LocalTrack track)
    {
        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
        }

        Stop();
        CurrentTrack = track;
        try
        {
            _audioFile = new AudioFileReader(track.FilePath);
            _frequencyProvider = new FrequencySpectrumProvider(_audioFile);
            _frequencyProvider.FrequencyDataAvailable += OnFrequencyDataAvailable;

            _waveOut = _waveOut ?? new WaveOutEvent();
            _waveOut.Init(_frequencyProvider);
            //_waveOut = _waveOut ?? new WaveOutEvent();
            //_waveOut.Init(_audioFile);
            _waveOut.PlaybackStopped += OnPlaybackStopped;
            _waveOut.Volume = (float)_currentVolume;
        }
        catch (Exception)
        {
            // Handle corrupt file gracefully, maybe skip to next
            SkipNext();
        }
        return Task.CompletedTask;
    }

    private void OnFrequencyDataAvailable(float[] frequencyData)
    {
        FrequencyDataAvailable?.Invoke(frequencyData);
    }

    //private void OnStreamVolume(float level)
    //{
    //    AudioLevelChanged?.Invoke(level);
    //}

    public void Play()
    {
        if (_waveOut == null || CurrentTrack == null) return;

        _waveOut.Play();
        SetState(PlayerState.Playing);
        _positionTimer.Start();
    }

    public void Pause()
    {
        if (_waveOut == null) return;
        _waveOut.Pause();
        SetState(PlayerState.Paused);
        _positionTimer.Stop();
    }

    public void Stop()
    {
        if (_waveOut != null)
        {
            _waveOut.Stop();
        }
        if (_audioFile != null)
        {
            _audioFile.Position = 0;
        }
        SetState(PlayerState.Stopped);
        _positionTimer.Stop();
        CleanUp();
    }

    public void Seek(double positionSeconds)
    {
        if (_audioFile == null) return;
        _audioFile.CurrentTime = TimeSpan.FromSeconds(
            Math.Max(0, Math.Min(positionSeconds, Duration))
        );
        PositionChanged?.Invoke(CurrentPosition, Duration);
    }

    public void SetVolume(double volume)
    {
        _currentVolume = Math.Clamp(volume, 0.0, 1.0);
        if (_waveOut != null)
        {
            _waveOut.Volume = (float)_currentVolume;
        }
    }

    public double GetVolume()
    {
        return _currentVolume;
    }

    private void OnPositionTimerTick(object? sender, EventArgs e)
    {
        PositionChanged?.Invoke(CurrentPosition, Duration);
    }

    public async void SkipNext()
    {
        if (!_playlist.Any()) return;

        // Logic for manual skip (User clicks Next)
        // Usually manual skip ignores RepeatOne, but respects Playlist boundaries
        if (_currentIndex < _playlist.Count - 1)
        {
            _currentIndex++;
            await LoadTrack(_playlist[_currentIndex]);
            Play();
        }
        else if (RepeatMode == RepeatMode.RepeatAll)
        {
            // Loop back to start
            _currentIndex = 0;
            await LoadTrack(_playlist[_currentIndex]);
            Play();
        }
    }

    public async void SkipPrevious()
    {
        if (CurrentPosition > 3.0)
        {
            // If playing for more than 3s, replay from start
            Seek(0);
            return;
        }

        if (_currentIndex > 0)
        {
            _currentIndex--;
            await LoadTrack(_playlist[_currentIndex]);
            Play();
        }
    }

    // Auto logic when track ends
    private async void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (CurrentState != PlayerState.Playing) return; // Stopped manually

        TrackEnded?.Invoke();

        if (RepeatMode == RepeatMode.RepeatOne)
        {
            // Replay current
            _audioFile!.Position = 0;
            Play();
            return;
        }

        // Logic for auto skip
        if (_currentIndex < _playlist.Count - 1)
        {
            _currentIndex++;
            await LoadTrack(_playlist[_currentIndex]);
            Play();
        }
        else if (RepeatMode == RepeatMode.RepeatAll)
        {
            // Loop back
            _currentIndex = 0;
            await LoadTrack(_playlist[_currentIndex]);
            Play();
        }
        else
        {
            // End of playlist, stop
            Stop();
        }
    }

    private void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        PlaybackStateChanged?.Invoke(CurrentState);
    }

    private void CleanUp()
    {
        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _waveOut.Dispose();
            _waveOut = null;
        }
        if (_audioFile != null)
        {
            _audioFile.Dispose();
            _audioFile = null;
        }

        if (_frequencyProvider != null)
        {
            _frequencyProvider.FrequencyDataAvailable -= OnFrequencyDataAvailable;
            _frequencyProvider.Dispose();
            _frequencyProvider = null;
        }
    }

    public void Dispose()
    {
        Stop();
        _positionTimer.Stop();
    }
}
#pragma warning restore CA1416