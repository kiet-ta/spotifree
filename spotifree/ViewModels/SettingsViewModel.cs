using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

        private AppSettings _settings = new();
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

        // Đường dẫn thư mục lưu playlist
        public string PlaylistsFolderPath
        {
            get => _settings?.PlaylistsFolderPath ?? string.Empty;
            set
            {
                if (_settings == null || _settings.PlaylistsFolderPath == value) return;
                _settings.PlaylistsFolderPath = value;
                OnPropertyChanged(nameof(PlaylistsFolderPath));
                _settingsService.SaveAsync(_settings);
            }
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand RemoveFolderCommand { get; }
        public ICommand RescanLibraryCommand { get; }
        public ICommand SelectPlaylistsFolderCommand { get; }

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

            SelectFolderCommand = new RelayCommand(async _ => await ExecuteSelectFolderAsync());
            RemoveFolderCommand = new RelayCommand(async _ => await ExecuteRemoveFolderAsync(), _ => CanExecuteRemoveFolder());
            RescanLibraryCommand = new RelayCommand(async _ => await ExecuteRescanLibraryAsync(), _ => CanExecuteRescan());
            SelectPlaylistsFolderCommand = new RelayCommand(async _ => await ExecuteSelectPlaylistsFolderAsync());

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
                OnPropertyChanged(nameof(PlaylistsFolderPath));
            }
            catch (Exception)
            {
                _settings = new AppSettings();
            }
        }

        private async Task ExecuteSelectFolderAsync()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select a music folder to add",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var newPath = dialog.SelectedPath;

            if (string.IsNullOrEmpty(newPath) || MusicFolderPaths.Contains(newPath))
                return;

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

        private bool CanExecuteRemoveFolder() => !string.IsNullOrEmpty(SelectedFolderPath);

        private async Task ExecuteRescanLibraryAsync()
        {
            IsScanning = true;
            try
            {
                await _libraryService.ScanLibraryAsync();
            }
            catch (Exception)
            {
                // TODO: thông báo lỗi nếu muốn
            }
            finally
            {
                IsScanning = false;
            }
        }

        private bool CanExecuteRescan() => !IsScanning;

        private async Task ExecuteSelectPlaylistsFolderAsync()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select folder to store playlists",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            PlaylistsFolderPath = dialog.SelectedPath;
        }
    }
}
