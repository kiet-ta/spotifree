using System;
using System.Threading;
using System.Threading.Tasks;

namespace Spotifree.Audio
{
    public enum PlayerState
    {
        Idle, Loading, Playing, Paused, Stopped, Ended, Error
    }

    public interface ILocalAudioService : IDisposable
    {
        PlayerState State { get; }
        double Volume { get; set; }          // 0..1
        double PositionSeconds { get; }      // current position
        double DurationSeconds { get; }      // total length
        string? CurrentPath { get; }

        Task LoadAsync(string filePath, CancellationToken ct = default);
        void Play();
        void Pause();
        void Stop();
        void Seek(double seconds);

        // (Tùy chọn) events cho UI native WPF (không bắt buộc với JS)
        event EventHandler<double>? PositionChanged; // seconds
        event EventHandler? PlaybackEnded;
        event EventHandler<Exception>? PlaybackError;
    }
}
