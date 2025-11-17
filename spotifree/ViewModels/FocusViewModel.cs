using Spotifree.Constances;
using Spotifree.IServices;
using Spotifree.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public class FocusViewModel : BaseViewModel
    {
        private readonly IMusicLibraryService _libraryService;
        private readonly IAudioPlayerService _playerService;
        private readonly IFocusTimerService _timerService;
        private RepeatMode _originalRepeatMode;

        private int _timerMinutes = 30;

        public int TimerMinutes
        {
            get => _timerMinutes;
            set => SetProperty(ref _timerMinutes, value);
        }

        private string _searchQuery = string.Empty;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                SetProperty(ref _searchQuery, value);
                PerformSearch(value);
            }
        }

        public ObservableCollection<LocalTrack> SearchResults { get; }
        public ObservableCollection<LocalTrack> SelectedPlaylist { get; }

        private LocalTrack? _selectedSearchTrack;

        public LocalTrack? SelectedSearchTrack
        {
            get => _selectedSearchTrack;
            set
            {
                if (value != null)
                {
                    AddTrackToPlaylist(value);
                    SetProperty(ref _selectedSearchTrack, null);
                }
            }
        }

        private LocalTrack? _selectedTrackToRemove;

        public LocalTrack? SelectedTrackToRemove
        {
            get => _selectedTrackToRemove;
            set
            {
                if (value != null)
                {
                    RemoveTrackFromPlaylist(value);
                    SetProperty(ref _selectedTrackToRemove, null);
                }
            }
        }

        private bool _isSessionRunning;

        public bool IsSessionRunning
        {
            get => _isSessionRunning;
            set => SetProperty(ref _isSessionRunning, value);
        }

        private string _countdownText = "00:00";

        public string CountdownText
        {
            get => _countdownText;
            set => SetProperty(ref _countdownText, value);
        }

        public ICommand StartSessionCommand { get; }
        public ICommand CancelSessionCommand { get; }

        public FocusViewModel(
            IMusicLibraryService libraryService,
            IAudioPlayerService playerService,
            IFocusTimerService timerService)
        {
            _libraryService = libraryService;
            _playerService = playerService;
            _timerService = timerService;

            SearchResults = new ObservableCollection<LocalTrack>();
            SelectedPlaylist = new ObservableCollection<LocalTrack>();

            StartSessionCommand = new RelayCommand(async _ => await StartSession(), _ => CanStartSession());
            CancelSessionCommand = new RelayCommand(_ => CancelSession());

            _timerService.TimerTick += OnTimerTick;
        }

        private async void PerformSearch(string query)
        {
            SearchResults.Clear();
            if (string.IsNullOrWhiteSpace(query)) return;

            var results = await _libraryService.SearchTracksAsync(query);
            foreach (var track in results)
            {
                SearchResults.Add(track);
            }
        }

        private void AddTrackToPlaylist(LocalTrack track)
        {
            if (!SelectedPlaylist.Contains(track))
            {
                SelectedPlaylist.Add(track);
            }
        }

        private void RemoveTrackFromPlaylist(LocalTrack track)
        {
            SelectedPlaylist.Remove(track);
        }

        private bool CanStartSession()
        {
            return TimerMinutes > 0 && SelectedPlaylist.Any() && !IsSessionRunning;
        }

        private async Task StartSession()
        {
            IsSessionRunning = true;
            _originalRepeatMode = _playerService.RepeatMode;
            await _playerService.LoadPlaylist(SelectedPlaylist, 0);
            _playerService.Play();
            _timerService.StartTimer(TimeSpan.FromMinutes(TimerMinutes));
        }

        private void CancelSession()
        {
            IsSessionRunning = false;
            _timerService.StopTimer();
            FinishSession();
        }
        public void FinishSession()
        {
            IsSessionRunning = false;
            _playerService.Stop();
            CountdownText = "00:00";

            _playerService.RepeatMode = _originalRepeatMode;
        }
        private void OnTimerTick(TimeSpan remaining)
        {
            CountdownText = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }
}