using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using spotifree.Services;
using spotifree.IServices;
using spotifree.Models;
using Microsoft.Extensions.DependencyInjection;

namespace spotifree
{
    /// <summary>
    /// Interaction logic for Homepage.xaml
    /// </summary>
    public partial class Homepage : Window
    {
        private readonly SpotifyAuth _spotifyAuth;
        private readonly ISpotifyService _spotifyService;
        private List<SpotifyPlaylist> _userPlaylists;
        private bool _isPlaying = false;

        public Homepage()
        {
            InitializeComponent();
            
            // Initialize Spotify services
            _spotifyAuth = new SpotifyAuth(
                clientId: "your_client_id_here", // Replace with actual client ID
                redirectUri: "http://127.0.0.1:5173/callback",
                scope: "playlist-read-private playlist-modify-private user-read-private user-read-email"
            );
            
            _spotifyService = new SpotifyApi(_spotifyAuth);
            _userPlaylists = new List<SpotifyPlaylist>();
            
            // Initialize UI
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                // Check authentication status
                await CheckAuthenticationStatus();
                
                // Load user playlists if authenticated
                if (_spotifyAuth.IsValid())
                {
                    await LoadUserPlaylists();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CheckAuthenticationStatus()
        {
            try
            {
                if (!_spotifyAuth.IsValid())
                {
                    // Show login prompt
                    var result = MessageBox.Show(
                        "You need to authenticate with Spotify to use this application. Would you like to login now?",
                        "Spotify Authentication Required",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await AuthenticateWithSpotify();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication check failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AuthenticateWithSpotify()
        {
            try
            {
                var success = await _spotifyAuth.EnsureTokenAsync();
                if (success)
                {
                    MessageBox.Show("Successfully authenticated with Spotify!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadUserPlaylists();
                }
                else
                {
                    MessageBox.Show("Failed to authenticate with Spotify. Please try again.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadUserPlaylists()
        {
            try
            {
                _userPlaylists = await _spotifyService.GetCurrentUserPlaylistsAsync();
                UpdatePlaylistDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load playlists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePlaylistDisplay()
        {
            // This would update the UI with the loaded playlists
            // For now, we'll just show a message
            if (_userPlaylists.Any())
            {
                // Update the playlist section in the sidebar
                // This is a placeholder - in a real implementation, you'd bind to a collection
                System.Diagnostics.Debug.WriteLine($"Loaded {_userPlaylists.Count} playlists");
            }
        }

        // Navigation button event handlers
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to home view
            System.Diagnostics.Debug.WriteLine("Home button clicked");
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to search view
            System.Diagnostics.Debug.WriteLine("Search button clicked");
            ShowSearchDialog();
        }

        private async void ShowSearchDialog()
        {
            try
            {
                var searchQuery = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter search query:",
                    "Search Music",
                    "");

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    await PerformSearch(searchQuery);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PerformSearch(string query)
        {
            try
            {
                if (!_spotifyAuth.IsValid())
                {
                    MessageBox.Show("Please authenticate with Spotify first.", "Authentication Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var searchResults = await _spotifyService.SearchAsync(query);
                MessageBox.Show($"Search completed for: {query}\nResults: {searchResults.Length} characters", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to library view
            System.Diagnostics.Debug.WriteLine("Library button clicked");
        }

        // Playlist filter button handlers
        private void PlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            // Filter to show playlists
            System.Diagnostics.Debug.WriteLine("Playlists filter clicked");
        }

        private void PodcastsButton_Click(object sender, RoutedEventArgs e)
        {
            // Filter to show podcasts
            System.Diagnostics.Debug.WriteLine("Podcasts filter clicked");
        }

        private void AlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            // Filter to show albums
            System.Diagnostics.Debug.WriteLine("Albums filter clicked");
        }

        // Content filter button handlers
        private void AllButton_Click(object sender, RoutedEventArgs e)
        {
            // Show all content
            System.Diagnostics.Debug.WriteLine("All filter clicked");
        }

        private void MusicButton_Click(object sender, RoutedEventArgs e)
        {
            // Show only music
            System.Diagnostics.Debug.WriteLine("Music filter clicked");
        }

        private void PodcastsContentButton_Click(object sender, RoutedEventArgs e)
        {
            // Show only podcasts
            System.Diagnostics.Debug.WriteLine("Podcasts content filter clicked");
        }

        // Featured section button handlers
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // Play the featured content
            System.Diagnostics.Debug.WriteLine("Play button clicked");
            TogglePlayPause();
        }

        private void FollowButton_Click(object sender, RoutedEventArgs e)
        {
            // Follow/unfollow the featured content
            System.Diagnostics.Debug.WriteLine("Follow button clicked");
        }

        private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            // Show more options menu
            System.Diagnostics.Debug.WriteLine("More options button clicked");
        }

        // Player control handlers
        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle shuffle mode
            System.Diagnostics.Debug.WriteLine("Shuffle button clicked");
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            // Go to previous track
            System.Diagnostics.Debug.WriteLine("Previous button clicked");
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle play/pause
            TogglePlayPause();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Go to next track
            System.Diagnostics.Debug.WriteLine("Next button clicked");
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle repeat mode
            System.Diagnostics.Debug.WriteLine("Repeat button clicked");
        }

        private void TogglePlayPause()
        {
            _isPlaying = !_isPlaying;
            System.Diagnostics.Debug.WriteLine($"Play/Pause toggled. Now playing: {_isPlaying}");
        }

        // Player action handlers
        private void QueueButton_Click(object sender, RoutedEventArgs e)
        {
            // Show queue
            System.Diagnostics.Debug.WriteLine("Queue button clicked");
        }

        private void DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            // Show device selection
            System.Diagnostics.Debug.WriteLine("Device button clicked");
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Adjust volume
            System.Diagnostics.Debug.WriteLine($"Volume changed to: {e.NewValue}");
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle fullscreen
            System.Diagnostics.Debug.WriteLine("Fullscreen button clicked");
        }

        // Music card click handlers
        private void MusicCard_Click(object sender, RoutedEventArgs e)
        {
            // Handle music card click - could be album, playlist, or track
            System.Diagnostics.Debug.WriteLine("Music card clicked");
        }

        // Show all handlers
        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Show all items in the section
            System.Diagnostics.Debug.WriteLine("Show all button clicked");
        }

        // Library add button handler
        private void AddToLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            // Add new playlist or folder to library
            System.Diagnostics.Debug.WriteLine("Add to library button clicked");
        }

        // Navigation arrow handlers
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back
            System.Diagnostics.Debug.WriteLine("Back button clicked");
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate forward
            System.Diagnostics.Debug.WriteLine("Forward button clicked");
        }

        // Notification and profile handlers
        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            // Show notifications
            System.Diagnostics.Debug.WriteLine("Notification button clicked");
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Show profile menu
            System.Diagnostics.Debug.WriteLine("Profile button clicked");
        }

        // Liked songs handler
        private void LikedSongs_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to liked songs
            System.Diagnostics.Debug.WriteLine("Liked songs clicked");
        }

        // Current track handlers
        private void CurrentTrack_Click(object sender, RoutedEventArgs e)
        {
            // Show current track details
            System.Diagnostics.Debug.WriteLine("Current track clicked");
        }

        private void LikeTrackButton_Click(object sender, RoutedEventArgs e)
        {
            // Like/unlike current track
            System.Diagnostics.Debug.WriteLine("Like track button clicked");
        }

        // Progress bar handlers
        private void ProgressBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Seek to position
            System.Diagnostics.Debug.WriteLine("Progress bar clicked");
        }

        // Cleanup
        protected override void OnClosed(EventArgs e)
        {
            _spotifyAuth?.Dispose();
            base.OnClosed(e);
        }
    }
}
