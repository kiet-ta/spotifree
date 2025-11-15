using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using NAudio.Wave;
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

    public PlayerState CurrentState { get; private set; } = PlayerState.Stopped;
    public LocalTrack? CurrentTrack { get; private set; }
    public double CurrentPosition => _audioFile?.CurrentTime.TotalSeconds ?? 0;
    public double Duration => _audioFile?.TotalTime.TotalSeconds ?? 0;

    public event Action<PlayerState>? PlaybackStateChanged;
    public event Action<double, double>? PositionChanged;
    public event Action? TrackEnded;

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

    // Loads a track for playback.
    private Task LoadTrack(LocalTrack track)
    {
        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
        }
        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
        }

        Stop();
        CurrentTrack = track;
        _audioFile = new AudioFileReader(track.FilePath);

        _waveOut = _waveOut ?? new WaveOutEvent();
        _waveOut.Init(_audioFile);
        _waveOut.PlaybackStopped += OnPlaybackStopped;

        _waveOut.Volume = (float)_currentVolume;
        return Task.CompletedTask;
    }

    // Starts or resumes playback.
    public void Play()
    {
        if (_waveOut == null || CurrentTrack == null) return;

        _waveOut.Play();
        SetState(PlayerState.Playing);
        _positionTimer.Start();
    }

    // Pauses playback.
    public void Pause()
    {
        if (_waveOut == null) return;
        _waveOut.Pause();
        SetState(PlayerState.Paused);
        _positionTimer.Stop();
    }

    /// <summary>
    /// Stops playback and resets position.
    /// </summary>
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

    // Seeks to a specific position in the track.
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
        if (_currentIndex < _playlist.Count - 1)
        {
            _currentIndex++;
            await LoadTrack(_playlist[_currentIndex]);
            Play();
        }
    }

    public async void SkipPrevious()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            await LoadTrack(_playlist[_currentIndex]);
            Play();
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (CurrentState == PlayerState.Playing && _currentIndex < _playlist.Count - 1)
        {
            TrackEnded?.Invoke(); 
            SkipNext(); 
            return;
        }

        SetState(PlayerState.Stopped);
        _positionTimer.Stop();
        CleanUp();
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
    }

    // Disposes NAudio resources.
    public void Dispose()
    {
        Stop();
        _positionTimer.Stop();
    }
}
#pragma warning disable CA1416