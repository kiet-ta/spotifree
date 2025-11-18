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
    private readonly IFocusTimerService _timerService;
    private readonly FocusViewModel _focusViewModel;

    public BaseViewModel CurrentPageViewModel
    {
        get => _currentPageViewModel;
        set => SetProperty(ref _currentPageViewModel, value);
    }


    //command
    public ICommand NavigateLibraryCommand { get; }
    public ICommand NavigateFocusCommand { get; }
    public ICommand NavigateSettingsCommand { get; }

    public MainViewModel(IAudioPlayerService audioPlayer, IMusicLibraryService libraryService, ISettingsService settingsService, IThemeService themeService, IViewModeService viewModeService, ChatViewModel chatViewModel, TourViewModel tourViewModel, PlayerViewModel playerViewModel, IFocusTimerService timerService, FocusViewModel focusViewModel)
    {
        //Integration
        _audioPlayer = audioPlayer;
        _viewModeService = viewModeService;
        _settingsService = settingsService;
        _timerService = timerService;

        //ViewModel
        TourViewModel = tourViewModel;
        _settingsViewModel = new SettingsViewModel(settingsService, libraryService, themeService, this);
        _playerViewModel = playerViewModel;
        _libraryViewModel = new LibraryViewModel(libraryService, audioPlayer, this);
        _focusViewModel = focusViewModel;

        // event
        _libraryViewModel.RequestNavigateToSettings += () => NavigateTo(_settingsViewModel);
        TourViewModel.TourEnded += OnTourEnded;
        _timerService.TimerFinished += OnFocusTimerFinished;

        _currentPageViewModel = _libraryViewModel;

        ChatViewModel = chatViewModel;
        ChatViewModel.RequestNavigateToSettings += () => NavigateTo(_settingsViewModel);

        //Command
        NavigateLibraryCommand = new RelayCommand(_ => NavigateTo(_libraryViewModel));
        NavigateSettingsCommand = new RelayCommand(_ => NavigateTo(_settingsViewModel));
        NavigateFocusCommand = new RelayCommand(_ => NavigateTo(_focusViewModel));


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

    private void OnFocusTimerFinished()
    {
        _focusViewModel.FinishSession();

        _viewModeService.ShowMainWindow();

        NavigateTo(_focusViewModel);
    }
}
