using Spotifree.Constances;
using Spotifree.Models;

namespace Spotifree.IServices
{
    

    public interface IAudioPlayerService
    {
        PlayerState CurrentState { get; }
        LocalTrack? CurrentTrack { get; }
        double CurrentPosition { get; }
        double Duration { get; }
        RepeatMode RepeatMode { get; set; }



        event Action<PlayerState> PlaybackStateChanged;

        event Action<double, double> PositionChanged;

        event Action TrackEnded;

        event Action<float[]> FrequencyDataAvailable;



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