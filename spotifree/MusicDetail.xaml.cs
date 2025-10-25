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
using System.Windows.Threading;

namespace spotifree
{
    /// <summary>
    /// Interaction logic for MusicDetail.xaml
    /// </summary>
    public partial class MusicDetail : Window
    {
        private bool isPlaying = false;
        private DispatcherTimer progressTimer;
        private TimeSpan currentTime;
        private TimeSpan totalTime;
        public MusicDetail()
        {
            InitializeComponent();
            InitializePlayer();

            // Enable window dragging
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        private void InitializePlayer()
        {
            // Initialize timer for progress bar
            progressTimer = new DispatcherTimer();
            progressTimer.Interval = TimeSpan.FromSeconds(1);
            progressTimer.Tick += ProgressTimer_Tick;

            // Set initial values
            currentTime = TimeSpan.FromSeconds(46);
            totalTime = TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(5));

            sliderProgress.Maximum = totalTime.TotalSeconds;
            sliderProgress.Value = currentTime.TotalSeconds;

            UpdateTimeDisplay();

            // Add event handlers
            btnPlayPause.Click += BtnPlayPause_Click;
            btnPrevious.Click += BtnPrevious_Click;
            btnNext.Click += BtnNext_Click;
            btnShuffle.Click += BtnShuffle_Click;
            btnRepeat.Click += BtnRepeat_Click;

            sliderProgress.ValueChanged += SliderProgress_ValueChanged;
            sliderVolume.ValueChanged += SliderVolume_ValueChanged;

            btnLyrics.Click += BtnLyrics_Click;
            btnQueue.Click += BtnQueue_Click;
            btnVolume.Click += BtnVolume_Click;
            btnFullscreen.Click += BtnFullscreen_Click;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            isPlaying = !isPlaying;

            if (isPlaying)
            {
                btnPlayPause.Content = "⏸";
                progressTimer.Start();
                // TODO: Add your audio playback logic here
            }
            else
            {
                btnPlayPause.Content = "▶";
                progressTimer.Stop();
                // TODO: Add your audio pause logic here
            }
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement previous track functionality
            MessageBox.Show("Previous track");
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement next track functionality
            MessageBox.Show("Next track");
        }

        private void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement shuffle functionality
            MessageBox.Show("Shuffle toggled");
        }

        private void BtnRepeat_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement repeat functionality
            MessageBox.Show("Repeat toggled");
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (sliderProgress.Value < sliderProgress.Maximum)
            {
                sliderProgress.Value += 1;
                currentTime = TimeSpan.FromSeconds(sliderProgress.Value);
                UpdateTimeDisplay();
            }
            else
            {
                // Song ended
                progressTimer.Stop();
                isPlaying = false;
                btnPlayPause.Content = "▶";
            }
        }

        private void SliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            currentTime = TimeSpan.FromSeconds(sliderProgress.Value);
            UpdateTimeDisplay();
            // TODO: Seek audio to this position
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // TODO: Implement volume control
            double volume = sliderVolume.Value / 100.0;
        }

        private void UpdateTimeDisplay()
        {
            txtCurrentTime.Text = $"{(int)currentTime.TotalMinutes}:{currentTime.Seconds:D2}";
            txtTotalTime.Text = $"{(int)totalTime.TotalMinutes}:{totalTime.Seconds:D2}";
        }

        private void BtnLyrics_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Show lyrics panel
            MessageBox.Show("Show lyrics");
        }

        private void BtnQueue_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Show queue panel
            MessageBox.Show("Show queue");
        }

        private void BtnVolume_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Toggle mute
            MessageBox.Show("Toggle mute");
        }

        private void BtnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Toggle fullscreen
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Additional methods you can implement:

        public void LoadSong(string filePath)
        {
            // TODO: Load audio file using MediaPlayer or NAudio
            // Update album art, song title, artist, etc.
        }

        public void UpdateSongInfo(string title, string artist, string albumArt)
        {
            txtSongTitle.Text = title;
            txtArtist.Text = artist;

            if (!string.IsNullOrEmpty(albumArt))
            {
                // Load album art image
                // imgAlbumArt.Source = new BitmapImage(new Uri(albumArt));
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
