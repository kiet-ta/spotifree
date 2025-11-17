using Spotifree.IServices;
using System.Windows.Input;

namespace Spotifree.ViewModels;

public class MainViewModel : BaseViewModel
{
    public ChatViewModel ChatViewModel { get; }
    public TourViewModel TourViewModel { get; }
    public PlayerViewModel PlayerViewModel => _playerViewModel;

    private BaseViewModel _currentPageViewModel;
    private PlayerViewModel _playerViewModel;

    private readonly LibraryViewModel _libraryViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly IAudioPlayerService _audioPlayer;
    private readonly IViewModeService _viewModeService;
    private readonly ISettingsService _settingsService;

    public BaseViewModel CurrentPageViewModel
    {
        get => _currentPageViewModel;
        set => SetProperty(ref _currentPageViewModel, value);
    }


    //command
    public ICommand NavigateLibraryCommand { get; }

    public ICommand NavigateSettingsCommand { get; }

    public MainViewModel(IAudioPlayerService audioPlayer, IMusicLibraryService libraryService, ISettingsService settingsService, IThemeService themeService, IViewModeService viewModeService, ChatViewModel chatViewModel, TourViewModel tourViewModel, PlayerViewModel playerViewModel)
    {
        //Integration
        _audioPlayer = audioPlayer;
        _viewModeService = viewModeService;
        _settingsService = settingsService;

        //ViewModel
        TourViewModel = tourViewModel;
        _settingsViewModel = new SettingsViewModel(settingsService, libraryService, themeService, this);
        _playerViewModel = playerViewModel;
        _libraryViewModel = new LibraryViewModel(libraryService, audioPlayer, this);
        
        // event
        _libraryViewModel.RequestNavigateToSettings += () => NavigateTo(_settingsViewModel);
        TourViewModel.TourEnded += OnTourEnded;

        _currentPageViewModel = _libraryViewModel;

        ChatViewModel = chatViewModel;
        ChatViewModel.RequestNavigateToSettings += () => NavigateTo(_settingsViewModel);

        //Command
        NavigateLibraryCommand = new RelayCommand(_ => NavigateTo(_libraryViewModel));
        NavigateSettingsCommand = new RelayCommand(_ => NavigateTo(_settingsViewModel));

        CheckForFirstRunTour();
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

    public void NavigateToAlbumDetail(AlbumViewModel album)
    {
        var detailViewModel = new AlbumDetailViewModel(album, _audioPlayer, this);

        CurrentPageViewModel = detailViewModel;
    }
    private async void CheckForFirstRunTour()
    {
        await Task.Delay(1000); 

        var settings = await _settingsService.GetAsync();
        if (!settings.HasCompletedFirstRunTour)
        {
            TourViewModel.StartTour();
        }
    }

    private async void OnTourEnded()
    {
        var settings = await _settingsService.GetAsync();
        if (!settings.HasCompletedFirstRunTour)
        {
            settings.HasCompletedFirstRunTour = true;
            await _settingsService.SaveAsync(settings);
        }
    }
}
