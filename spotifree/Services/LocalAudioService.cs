using System;
using System.IO;
using NAudio.Wave;
using Timer = System.Timers.Timer;

namespace Spotifree.Audio
{
    public sealed class LocalAudioService : ILocalAudioService
    {
        private IWavePlayer? _output;
        private AudioFileReader? _reader;
        private readonly Timer _positionTick;
        private readonly object _gate = new();

        private volatile PlayerState _state = PlayerState.Idle;

        public event EventHandler<double>? PositionChanged;
        public event EventHandler? PlaybackEnded;
        public event EventHandler<Exception>? PlaybackError;

        public PlayerState State => _state;
        public string? CurrentPath { get; private set; }

        public double Volume
        {
            get => _reader?.Volume ?? 1.0;
            set { if (_reader != null) _reader.Volume = (float)Math.Clamp(value, 0.0, 1.0); }
        }

        public double PositionSeconds => _reader?.CurrentTime.TotalSeconds ?? 0;
        public double DurationSeconds => _reader?.TotalTime.TotalSeconds ?? 0;

        public LocalAudioService()
        {
            _positionTick = new Timer(250) { AutoReset = true };
            _positionTick.Elapsed += (_, __) =>
            {
                if (_reader != null && _output != null &&
                    (_state == PlayerState.Playing || _state == PlayerState.Paused))
                {
                    PositionChanged?.Invoke(this, _reader.CurrentTime.TotalSeconds);
                }
            };
            _positionTick.Start();
        }

        public async System.Threading.Tasks.Task LoadAsync(string filePath, System.Threading.CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath)) throw new FileNotFoundError(filePath);

            await System.Threading.Tasks.Task.Yield();

            lock (_gate)
            {
                CleanupOutput();

                _state = PlayerState.Loading;
                CurrentPath = filePath;

                _reader = new AudioFileReader(filePath);   // MP3/WAV/AIFF...
                _output = new WaveOutEvent();
                _output.Init(_reader);
                _output.PlaybackStopped += OnPlaybackStopped;

                _state = PlayerState.Paused;               // ready
            }
        }

        public void Play()
        {
            lock (_gate)
            {
                if (_output == null) return;
                _output.Play();
                _state = PlayerState.Playing;
            }
        }

        public void Pause()
        {
            lock (_gate)
            {
                if (_output == null) return;
                _output.Pause();
                _state = PlayerState.Paused;
            }
        }

        public void Stop()
        {
            lock (_gate)
            {
                if (_output == null) return;
                _output.Stop();

                if (_reader != null)
                {
                    _reader.CurrentTime = TimeSpan.Zero;   // dùng CurrentTime thay cho SetPosition
                    PositionChanged?.Invoke(this, 0);
                }

                _state = PlayerState.Stopped;
            }
        }

        public void Seek(double seconds)
        {
            lock (_gate)
            {
                if (_reader == null) return;
                var s = Math.Clamp(seconds, 0, DurationSeconds);
                _reader.CurrentTime = TimeSpan.FromSeconds(s);
                PositionChanged?.Invoke(this, s);
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                _state = PlayerState.Error;
                PlaybackError?.Invoke(this, e.Exception);
                return;
            }

            if (_reader != null && _reader.CurrentTime >= _reader.TotalTime - TimeSpan.FromMilliseconds(5))
            {
                _state = PlayerState.Ended;
                PlaybackEnded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _state = PlayerState.Stopped;
            }
        }

        private void CleanupOutput()
        {
            if (_output != null)
            {
                _output.PlaybackStopped -= OnPlaybackStopped;
                _output.Dispose();
                _output = null;
            }
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        public void Dispose()
        {
            _positionTick.Stop();
            _positionTick.Dispose();
            CleanupOutput();
        }
    }

    public sealed class FileNotFoundError : Exception
    {
        public FileNotFoundError(string path) : base($"Local file not found: {path}") { }
    }
}
