using Microsoft.Win32;
using Spotifree.IServices;
using Spotifree.Models;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly IMusicLibraryService _libraryService;
        private readonly IThemeService _themeService;
        private readonly MainViewModel _mainViewModel;

        private AppSettings _settings = new AppSettings();
        private string _selectedFolderPath = string.Empty;
        private bool _isScanning;

        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                if (SetProperty(ref _isScanning, value))
                {
                    ((RelayCommand)RescanLibraryCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<string> MusicFolderPaths { get; }

        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set
            {
                if (SetProperty(ref _selectedFolderPath, value))
                {
                    ((RelayCommand)RemoveFolderCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsDarkTheme
        {
            get => _settings?.IsDarkTheme ?? false;
            set
            {
                if (_settings == null || _settings.IsDarkTheme == value) return;
                _settings.IsDarkTheme = value;
                OnPropertyChanged(nameof(IsDarkTheme));
                _themeService.SetTheme(value);
                _settingsService.SaveAsync(_settings);
                _mainViewModel.NavigateTo(this);
            }
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand RemoveFolderCommand { get; }
        public ICommand RescanLibraryCommand { get; }

        public SettingsViewModel(
            ISettingsService settingsService,
            IMusicLibraryService libraryService,
            IThemeService themeService,
            MainViewModel mainViewModel)
        {
            _settingsService = settingsService;
            _libraryService = libraryService;
            _themeService = themeService;
            _mainViewModel = mainViewModel;
            MusicFolderPaths = new ObservableCollection<string>();

            SelectFolderCommand = new RelayCommand(async (_) => await ExecuteSelectFolderAsync());
            RemoveFolderCommand = new RelayCommand(async (_) => await ExecuteRemoveFolderAsync(), (_) => CanExecuteRemoveFolder(_));
            RescanLibraryCommand = new RelayCommand(async (_) => await ExecuteRescanLibraryAsync(), (_) => CanExecuteRescan(_));

            LoadSettingsAsync();
        }


        private async void LoadSettingsAsync()
        {
            try
            {
                _settings = await _settingsService.GetAsync();
                MusicFolderPaths.Clear();
                foreach (var path in _settings.MusicFolderPaths)
                {
                    MusicFolderPaths.Add(path);
                }
                OnPropertyChanged(nameof(IsDarkTheme));
            }
            catch (Exception)
            {
                _settings = new AppSettings();
            }
        }

        private async Task ExecuteSelectFolderAsync()
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Select a music folder to add",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string newPath = dialog.SelectedPath;

            if (string.IsNullOrEmpty(newPath) || MusicFolderPaths.Contains(newPath))
            {
                return;
            }

            await _settingsService.AddMusicFolderAsync(newPath);
            MusicFolderPaths.Add(newPath);
        }

        private async Task ExecuteRemoveFolderAsync()
        {
            var pathToRemove = SelectedFolderPath;
            if (string.IsNullOrEmpty(pathToRemove)) return;
            await _settingsService.RemoveMusicFolderAsync(pathToRemove);
            MusicFolderPaths.Remove(pathToRemove);
        }

        private bool CanExecuteRemoveFolder(object? _)
        {
            return !string.IsNullOrEmpty(SelectedFolderPath);
        }

        private async Task ExecuteRescanLibraryAsync()
        {
            IsScanning = true;
            try
            {
                await _libraryService.ScanLibraryAsync();
            }
            catch (Exception)
            {
            }
            finally
            {
                IsScanning = false;
            }
        }

        private bool CanExecuteRescan(object? _)
        {
            return !IsScanning;
        }
    }
}