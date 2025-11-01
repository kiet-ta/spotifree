using spotifree.Commands;
using spotifree.IServices;
using spotifree.Models; // Sử dụng Model SpotifyTrack của bạn
using System.Windows;
using System.Windows.Input;
using System;
using System.Linq;
using System.Windows.Threading;

// Đảm bảo namespace này khớp với BaseViewModel của bạn
namespace spotifree.ViewModels
{
    public class MusicDetailViewModel : BaseViewModel // Kế thừa BaseViewModel của bạn
    {
        private readonly ISpotifyService _spotifyService;
        private DispatcherTimer _progressTimer;

        // --- Properties cho UI Binding ---

        private SpotifyTrack _currentTrack;
        public SpotifyTrack CurrentTrack
        {
            get => _currentTrack;
            set
            {
                _currentTrack = value;
                OnPropertyChanged();
                // Cập nhật các thuộc tính liên quan
                OnPropertyChanged(nameof(CurrentTrackDurationSeconds));
                OnPropertyChanged(nameof(TotalDurationString));
                OnPropertyChanged(nameof(ArtistName)); // SỬA LỖI: Đảm bảo UI cập nhật ArtistName
                OnPropertyChanged(nameof(AlbumArtUrl));
                OnPropertyChanged(nameof(ThumbnailUrl));
                OnPropertyChanged(nameof(TrackTitle)); // SỬA LỖI: Thêm cập nhật cho Title
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayPauseIcon)); // Cập nhật icon
            }
        }

        private double _volume = 70;
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VolumeIcon));
                // _spotifyService.SetVolumeAsync(value / 100.0);
            }
        }

        private double _currentPositionSeconds;
        public double CurrentPositionSeconds
        {
            get => _currentPositionSeconds;
            set
            {
                _currentPositionSeconds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPositionString));
            }
        }

        private bool _isLyricsVisible;
        public bool IsLyricsVisible
        {
            get => _isLyricsVisible;
            set { _isLyricsVisible = value; OnPropertyChanged(); }
        }

        // --- Thuộc tính phát sinh (cho View) ---
        public string PlayPauseIcon => IsPlaying ? "⏸" : "▶";
        public string VolumeIcon => Volume > 0 ? "🔊" : "🔇";

        // SỬA LỖI: Lấy trực tiếp từ các thuộc tính 'flat' của SpotifyTrack
        public string TrackTitle => CurrentTrack?.Name ?? "Unknown Title";
        public string ArtistName => CurrentTrack?.Artist ?? "Unknown Artist";
        public string AlbumArtUrl => CurrentTrack?.ImageUrl ?? "";
        public string ThumbnailUrl => CurrentTrack?.ImageUrl ?? ""; // Dùng ảnh cover

        // SỬA LỖI: Xử lý thời gian từ thuộc tính 'Duration' (string "3:42")
        public double CurrentTrackDurationSeconds
        {
            get
            {
                if (CurrentTrack?.Duration == null) return 100.0;
                var parts = CurrentTrack.Duration.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds))
                {
                    return minutes * 60 + seconds;
                }
                return 100.0;
            }
        }
        public string CurrentPositionString => TimeSpan.FromSeconds(CurrentPositionSeconds).ToString(@"m\:ss");
        public string TotalDurationString => CurrentTrack?.Duration ?? "0:00";

        // --- Commands cho các Buttons ---
        public ICommand PlayPauseCommand { get; }
        public ICommand NextTrackCommand { get; }
        public ICommand PreviousTrackCommand { get; }
        public ICommand ToggleShuffleCommand { get; }
        public ICommand ToggleRepeatCommand { get; }
        public ICommand ToggleLyricsCommand { get; }
        public ICommand MuteCommand { get; }
        public ICommand CloseWindowCommand { get; }
        public ICommand MaximizeWindowCommand { get; }
        public ICommand MinimizeWindowCommand { get; }

        public MusicDetailViewModel(ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService;

            // Khởi tạo Commands
            PlayPauseCommand = new RelayCommand(OnPlayPause);
            NextTrackCommand = new RelayCommand(async _ => await _spotifyService.NextTrackAsync());
            PreviousTrackCommand = new RelayCommand(async _ => await _spotifyService.PreviousTrackAsync());
            ToggleShuffleCommand = new RelayCommand(async _ => await _spotifyService.ToggleShuffleAsync());
            ToggleRepeatCommand = new RelayCommand(async _ => await _spotifyService.SetRepeatModeAsync());
            ToggleLyricsCommand = new RelayCommand(_ => IsLyricsVisible = !IsLyricsVisible);
            MuteCommand = new RelayCommand(OnMute);

            // Commands cho cửa sổ
            CloseWindowCommand = new RelayCommand(OnWindowClose);
            MaximizeWindowCommand = new RelayCommand(OnWindowMaximize);
            MinimizeWindowCommand = new RelayCommand(OnWindowMinimize);

            // Giả lập timer cập nhật tiến trình
            SetupProgressTimer();

            // Giả lập tải dữ liệu ban đầu
            LoadMockData();
        }

        private void OnPlayPause(object obj)
        {
            if (IsPlaying)
            {
                //_spotifyService.PausePlaybackAsync();
                _progressTimer.Stop();
            }
            else
            {
                //_spotifyService.StartPlaybackAsync();
                _progressTimer.Start();
            }
            IsPlaying = !IsPlaying;
        }

        private void OnMute(object obj)
        {
            Volume = Volume > 0 ? 0 : 70; // Giả lập
        }

        // --- Điều khiển cửa sổ ---
        private void OnWindowClose(object param) => (param as Window)?.Close();
        private void OnWindowMaximize(object param)
        {
            if (param is Window window)
                window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        private void OnWindowMinimize(object param) => (param as Window).WindowState = WindowState.Minimized;

        // --- Timer & Dữ liệu giả ---
        private void SetupProgressTimer()
        {
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _progressTimer.Tick += (s, e) =>
            {
                if (IsPlaying && CurrentPositionSeconds < CurrentTrackDurationSeconds)
                {
                    CurrentPositionSeconds += 0.5;
                }
            };
        }

        private void LoadMockData()
        {
            // SỬA LỖI: Cập nhật MockData để khớp với model SpotifyTrack.cs mới
            CurrentTrack = new SpotifyTrack
            {
                Id = "mock-track-1",
                Name = "Chuyện Chúng Ta Sau Này",
                Artist = "Hà Đăng Quyền, Wean Le",
                Album = "Single",
                Duration = "4:05", // Format: "M:SS"
                Popularity = 85,
                ImageUrl = "https://i.scdn.co/image/ab67616d0000b273bcfc65feb861c009c664b9d3",
                PreviewUrl = null,
                ReleaseDate = "2023-01-01",
                SpotifyUrl = "https://open.spotify.com/track/mock"
            };
            CurrentPositionSeconds = 46; // 0:46
        }
    }
}