using Spotifree.Models;
using System.Windows;
using System.Windows.Input;

namespace Spotifree.ViewModels;

public class TourViewModel : BaseViewModel
{
    public event Action TourEnded;


    private bool _isTourVisible;
    public bool IsTourVisible
    {
        get => _isTourVisible;
        set => SetProperty(ref _isTourVisible, value);
    }

    private TourStep _currentStep;

    public TourStep CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    private List<TourStep> _allSteps;
    private int _currentStepIndex;

    public ICommand NextCommand { get; }
    public ICommand PrevCommand { get; }
    public ICommand EndTourCommand { get; }

    public TourViewModel()
    {
        _allSteps = new List<TourStep>();
        _currentStep = new TourStep();
        _currentStepIndex = -1;

        NextCommand = new RelayCommand(OnNext, CanGoNext);
        PrevCommand = new RelayCommand(OnPrev, CanGoPrev);
        EndTourCommand = new RelayCommand(_ => EndTour());

        InitializeSteps();
    }

    private void InitializeSteps()
    {
        _allSteps.Clear();

        _allSteps.Add(new TourStep
        {
            Text = "Chào mừng bro đến với Spotifree! Đây là app 'clone' xịn xò.",
            ShowHighlight = false 
        });

        _allSteps.Add(new TourStep
        {
            Text = "Đây là nút 'Settings', nơi ông 'custom' (tùy chỉnh) mọi thứ.",

            PopupAlignmentH = HorizontalAlignment.Left,
            PopupAlignmentV = VerticalAlignment.Top,
            PopupMargin = new Thickness(100, 50, 0, 0), 

            ShowHighlight = true,
            HighlightAlignmentH = HorizontalAlignment.Left,
            HighlightAlignmentV = VerticalAlignment.Top,
            HighlightMargin = new Thickness(80, 15, 0, 0), 
            HighlightWidth = 80, 
            HighlightHeight = 40 
        });

        _allSteps.Add(new TourStep
        {
            Text = "Còn đây là ChatBot, 'flex' (khoe) tí là nó 'real-time' (thời gian thực) đó.",

            PopupAlignmentH = HorizontalAlignment.Right,
            PopupAlignmentV = VerticalAlignment.Bottom,
            PopupMargin = new Thickness(0, 350, 50, 0), 

            ShowHighlight = false
        });

        _allSteps.Add(new TourStep
        {
            Text = "Hết rồi. Giờ thì 'chill' (thư giãn) thôi!",
            ShowHighlight = false
        });
    }

    public void StartTour()
    {
        InitializeSteps();
        _currentStepIndex = 0;
        CurrentStep = _allSteps[_currentStepIndex];
        IsTourVisible = true;
        UpdateCommandStates();
    }

    public void EndTour()
    {
        IsTourVisible = false;
        _currentStepIndex = -1;

        TourEnded?.Invoke();
    }

    private void OnNext(object? param)
    {
        if (CanGoNext(param))
        {
            _currentStepIndex++;
            CurrentStep = _allSteps[_currentStepIndex];
            UpdateCommandStates();
        }
    }

    private bool CanGoNext(object? param) => _allSteps.Count > 0 && _currentStepIndex < _allSteps.Count - 1;

    private void OnPrev(object? param)
    {
        if (CanGoPrev(param))
        {
            _currentStepIndex--;
            CurrentStep = _allSteps[_currentStepIndex];
            UpdateCommandStates();
        }
    }

    private bool CanGoPrev(object? param) => _currentStepIndex > 0;

    private void UpdateCommandStates()
    {
        ((RelayCommand)NextCommand).RaiseCanExecuteChanged();
        ((RelayCommand)PrevCommand).RaiseCanExecuteChanged();
    }
}