using Spotifree.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ChatViewModel ChatViewModel { get; }

        private BaseViewModel _currentPageViewModel;
        private readonly PlayerViewModel _playerViewModel;

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
        public MainViewModel(IAudioPlayerService audioPlayer, IMusicLibraryService libraryService, ISettingsService settingsService, IThemeService themeService, IViewModeService viewModeService, ChatViewModel chatViewModel)
        {
            //Integration
            _audioPlayer = audioPlayer;
            _viewModeService = viewModeService;

            //ViewModel
            _playerViewModel = new PlayerViewModel(audioPlayer, viewModeService);
            _libraryViewModel = new LibraryViewModel(libraryService, audioPlayer, this);
            _settingsViewModel = new SettingsViewModel(settingsService, libraryService, themeService, this);
            _currentPageViewModel = _libraryViewModel;
            ChatViewModel = chatViewModel;

            //Command
            NavigateLibraryCommand = new RelayCommand(_ => NavigateTo(_libraryViewModel));
            NavigateSettingsCommand = new RelayCommand(_ => NavigateTo(_settingsViewModel));


        }

        // Navigates to a specific child ViewModel.
        public void NavigateTo(BaseViewModel viewModel)
        {
            CurrentPageViewModel = viewModel;
        }

        // Navigates back to the main library view.
        public void NavigateToLibrary()
        {
            NavigateTo(_libraryViewModel);
        }
    }
}
