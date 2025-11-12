using Microsoft.Web.WebView2.Core;
using spotifree.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace spotifree.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private CoreWebView2 _coreWebView;
        private Song _currentSong;
        private int _currentSongIndex = -1;

        private readonly string[] _supportedExtensions = { ".mp3", ".m4a", ".wav", ".aac", ".flac", ".ogg" };

        public ObservableCollection<Song> Songs { get; set; }

        public Song CurrentSong
        {
            get => _currentSong;
            set
            {
                _currentSong = value;
                OnPropertyChanged();
                // --- XÓA LOGIC TỰ ĐỘNG PHÁT ---
                // Việc chọn bài hát bây giờ chỉ là "chọn"
            }
        }

        // Xóa PlayCommand và PauseCommand
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; } // <-- MỚI
        public ICommand RefreshCommand { get; }
        public ICommand PlayAllCommand { get; }
        public ICommand AddSelectedToPlaylistCommand { get; } // <-- MỚI
        public ICommand AddDoubleClickedSongCommand { get; }

        public MainViewModel()
        {
            Songs = new ObservableCollection<Song>();
            LoadSongs();

            // Lệnh Next (dùng bởi JS)
            NextCommand = new RelayCommand(param =>
            {
                // Logic này giờ được xử lý 100% trong JS
                // Nhưng chúng ta vẫn giữ nó nếu JS cần
                SendJavaScriptMessage("next_from_csharp");
            });

            // Lệnh Previous (dùng bởi JS)
            PreviousCommand = new RelayCommand(param =>
            {
                // Logic này giờ được xử lý 100% trong JS
                SendJavaScriptMessage("previous_from_csharp");
            });

            // Lệnh Refresh (tải lại thư viện WPF)
            RefreshCommand = new RelayCommand(param =>
            {
                Songs.Clear();
                LoadSongs();
            });

            // --- LỆNH MỚI: THÊM BÀI HÁT ĐANG CHỌN VÀO PLAYLIST CỦA JS ---
            AddSelectedToPlaylistCommand = new RelayCommand(param =>
            {
                // 1. Nhận danh sách các mục đã chọn từ CommandParameter
                var selectedItems = param as IList;
                if (selectedItems == null || selectedItems.Count == 0)
                {
                    // Nếu không có gì được chọn, thử fallback về 1 bài (CurrentSong)
                    if (CurrentSong != null)
                    {
                        // Gửi 1 bài hát (theo logic cũ)
                        string jsonSong = JsonSerializer.Serialize(CurrentSong);
                        _coreWebView?.ExecuteScriptAsync($"window.addToPlaylist({jsonSong});");
                    }
                    return;
                }

                // 2. Chuyển đổi danh sách đã chọn thành List<Song>
                var songsToAdd = selectedItems.OfType<Song>().ToList();
                if (!songsToAdd.Any()) return;

                // 3. Serialize *toàn bộ danh sách* thành một mảng JSON
                string jsonSongList = JsonSerializer.Serialize(songsToAdd);

                // 4. Gọi một hàm JS MỚI để nhận mảng
                _coreWebView?.ExecuteScriptAsync($"window.addMultipleToPlaylist({jsonSongList});");

            }, param => (param as IList)?.Count > 0 || CurrentSong != null); // Button sẽ sáng nếu có > 0 mục được chọn HOẶC 1 mục được set

            // --- 2. KHỞI TẠO COMMAND MỚI TRONG CONSTRUCTOR ---
            //AddDoubleClickedSongCommand = new RelayCommand(param =>
            //{
            //    var song = param as Song;
            //    if (song == null || string.IsNullOrEmpty(song.FilePath))
            //    {
            //        return;
            //    }

            //    // Gửi chỉ một bài hát cho JS (tái sử dụng hàm JS cũ)
            //    string jsonSong = JsonSerializer.Serialize(song);
            //    _coreWebView?.ExecuteScriptAsync($"window.addToPlaylist({jsonSong});");

            //}, param => param is Song); // Lệnh chỉ chạy khi tham số là một bài hát

            // --- LỆNH CẬP NHẬT: "PHÁT TẤT CẢ" (GỬI TOÀN BỘ THƯ VIỆN CHO JS) ---
            PlayAllCommand = new RelayCommand(param =>
            {
                var playableSongs = Songs.Where(s => !string.IsNullOrEmpty(s.FilePath)).ToList();
                if (!playableSongs.Any()) return;

                // Gửi toàn bộ danh sách cho JS
                string script = $"window.playFromLibrary('wpf_library', 0);";
                _coreWebView?.ExecuteScriptAsync(script);
            });
        }

        private void LoadSongs()
        {

            string exePath = AppDomain.CurrentDomain.BaseDirectory;

            string libraryPath = Path.Combine(exePath, "Library");
            Console.WriteLine(libraryPath);
            if (!Directory.Exists(libraryPath))
            {
                Songs.Add(new Song { Title = "Lỗi: Không tìm thấy thư mục", Artist = libraryPath });
                return;
            }
            try
            {
                var musicFiles = Directory.EnumerateFiles(libraryPath, "*.*", SearchOption.AllDirectories)
                                        .Where(file => _supportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));
                foreach (var filePath in musicFiles)
                {
                    try
                    {
                        using (var tagFile = TagLib.File.Create(filePath))
                        {
                            string title = !string.IsNullOrEmpty(tagFile.Tag.Title) ? tagFile.Tag.Title : Path.GetFileNameWithoutExtension(filePath);
                            string artist = tagFile.Tag.Performers.Length > 0 ? string.Join(", ", tagFile.Tag.Performers) : "Unknown Artist";
                            Songs.Add(new Song { Title = title, Artist = artist, FilePath = filePath, CoverArtUrl = "https://placehold.co/300x300/666/fff?text=Music" });
                        }
                    }
                    catch (Exception fileEx) { Console.WriteLine($"WARNING: Skipped corrupt file: {filePath} ({fileEx.Message})"); }
                }
                if (!Songs.Any()) { Songs.Add(new Song { Title = "Không tìm thấy file nhạc", Artist = "Thư mục của bạn trống." }); }
            }
            catch (Exception ex) { Songs.Add(new Song { Title = "Lỗi khi quét thư mục", Artist = ex.Message }); }
        }

        public void InitializeBridge(CoreWebView2 coreWebView)
        {
            _coreWebView = coreWebView;
            // Cập nhật để lắng nghe tin nhắn "previous"
            //_coreWebView.WebMessageReceived += HandleWebMessage;
        }

        // --- HÀM PlaySong ĐÃ ĐƯỢC CẬP NHẬT ---
        // Hàm này giờ chỉ gửi thông tin, không tự động phát
        private void PlaySong(Song song)
        {
            if (_coreWebView == null) return;
            // (Chúng ta không còn dùng hàm này để phát nữa)
        }

        // Gửi một lệnh đơn giản
        private void SendJavaScriptMessage(string command)
        {
            if (_coreWebView == null) return;
            _coreWebView.ExecuteScriptAsync($"window.receiveCommand('{command}');");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
