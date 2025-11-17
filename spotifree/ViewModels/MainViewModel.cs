using Spotifree.IServices;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ChatViewModel ChatViewModel { get; }

        private BaseViewModel _currentPageViewModel;
        private PlayerViewModel _playerViewModel;

        private readonly LibraryViewModel _libraryViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly IAudioPlayerService _audioPlayer;
        private readonly IViewModeService _viewModeService;

        public BaseViewModel CurrentPageViewModel
        {
            get => _currentPageViewModel;
            set => SetProperty(ref _currentPageViewModel, value);
        }

        public PlayerViewModel PlayerViewModel => _playerViewModel;

        public ICommand NavigateLibraryCommand { get; }
        public ICommand NavigateSettingsCommand { get; }

        public MainViewModel(
            IAudioPlayerService audioPlayer,
            IMusicLibraryService libraryService,
            ISettingsService settingsService,
            IThemeService themeService,
            IViewModeService viewModeService,
            IPlaylistService playlistService,
            ChatViewModel chatViewModel,
            PlayerViewModel playerViewModel)
        {
            _audioPlayer = audioPlayer;
            _viewModeService = viewModeService;

            _playerViewModel = playerViewModel;

            _libraryViewModel = new LibraryViewModel(libraryService, audioPlayer, playlistService, this);
            _libraryViewModel.RequestNavigateToSettings += () => NavigateTo(_settingsViewModel);

            _settingsViewModel = new SettingsViewModel(settingsService, libraryService, themeService, this);

            _currentPageViewModel = _libraryViewModel;

            ChatViewModel = chatViewModel;
            ChatViewModel.RequestNavigateToSettings += () => NavigateTo(_settingsViewModel);

            NavigateLibraryCommand = new RelayCommand(_ => NavigateTo(_libraryViewModel));
            NavigateSettingsCommand = new RelayCommand(_ => NavigateTo(_settingsViewModel));
        }

        public void NavigateTo(BaseViewModel viewModel)
        {
            CurrentPageViewModel = viewModel;
        }

        public void NavigateToLibrary()
        {
            NavigateTo(_libraryViewModel);
        }

        public void NavigateToAlbumDetail(AlbumViewModel album)
        {
            var detailViewModel = new AlbumDetailViewModel(album, _audioPlayer, this);
            CurrentPageViewModel = detailViewModel;
        }
    }
}
