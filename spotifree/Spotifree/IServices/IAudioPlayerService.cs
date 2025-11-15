using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotifree.IServices
{
    public enum PlayerState { Stopped, Playing, Paused }

    public interface IAudioPlayerService
    {
        PlayerState CurrentState { get; }
        LocalTrack? CurrentTrack { get; }
        double CurrentPosition { get; }
        double Duration { get; }

        event Action<PlayerState> PlaybackStateChanged;
        event Action<double, double> PositionChanged;
        event Action TrackEnded;

        Task LoadPlaylist(IEnumerable<LocalTrack> playlist, int startIndex = 0);
        void SkipNext();
        void SkipPrevious();

        void Play();
        void Pause();
        void Stop();
        void Seek(double positionSeconds);
        void SetVolume(double volume);
        double GetVolume();

    }
}
