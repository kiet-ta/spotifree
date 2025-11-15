using Microsoft.Win32;
using Spotifree.IServices;
using Spotifree.Models;
using Spotifree.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly IMusicLibraryService _libraryService;
        private string? _musicFolderPath;
        private bool _isScanning;
        private bool _isDarkTheme;
        private readonly IThemeService _themeService;
        private readonly MainViewModel _mainViewModel;

        public ICommand SelectFolderCommand { get; }
        public ICommand RescanLibraryCommand { get; }
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                {
                    _themeService.SetTheme(value);
                    _mainViewModel.NavigateTo(this);
                    SaveSettings();
                }
            }
        }

        public string? MusicFolderPath
        {
            get => _musicFolderPath;
            set => SetProperty(ref _musicFolderPath, value);
        }

        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }


        public SettingsViewModel(ISettingsService settingsService, IMusicLibraryService libraryService, IThemeService themeService, MainViewModel mainViewModel)
        {
            _settingsService = settingsService;
            _libraryService = libraryService;
            _themeService = themeService;

            SelectFolderCommand = new RelayCommand(SelectFolder);
            RescanLibraryCommand = new RelayCommand(RescanLibrary, _ => !IsScanning);

            LoadSettings();
            _mainViewModel = mainViewModel;
        }

        // Loads the current music folder path from settings.
        private async void LoadSettings()
        {
            var settings = await _settingsService.GetAsync();
            MusicFolderPath = settings.MusicFolderPath;
            IsDarkTheme = settings.IsDarkTheme;
        }

        // Opens a dialog for the user to select their music folder.
        private async void SelectFolder(object? param)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Music Folder",
                InitialDirectory = MusicFolderPath ?? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            };

            if (dialog.ShowDialog() == true)
            {
                MusicFolderPath = dialog.FolderName;
                await SaveSettings();
                var settings = await _settingsService.GetAsync();
                settings.MusicFolderPath = MusicFolderPath;
                await _settingsService.SaveAsync(settings);
            }
        }

        // Triggers a full rescan of the selected music library folder.
        private async void RescanLibrary(object? param)
        {
            if (string.IsNullOrEmpty(MusicFolderPath)) return;

            IsScanning = true;
            await _libraryService.ScanLibraryAsync(MusicFolderPath);
            IsScanning = false;
        }
        private async Task SaveSettings()
        {
            var settings = new AppSettings
            {
                MusicFolderPath = this.MusicFolderPath,
                IsDarkTheme = this.IsDarkTheme
            };
            await _settingsService.SaveAsync(settings);
        }
    }
}
