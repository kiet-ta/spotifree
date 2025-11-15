using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        private readonly IAudioPlayerService _player;
        private readonly IMusicLibraryService _library;
        private readonly IGeminiService _geminiService;
        private readonly IConnectivityService _connectivityService;

        public ObservableCollection<ChatMessage> Messages { get; }

        private string _currentMessage;
        public string CurrentMessage
        {
            get => _currentMessage;
            set => SetProperty(ref _currentMessage, value);
        }
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        // Commands
        public ICommand SendCommand { get; }
        public ICommand ToggleVisibilityCommand { get; }
        public ICommand HideCommand { get; }

        // Constructor Injection
        public ChatViewModel(IAudioPlayerService audioPlayer, IMusicLibraryService libraryService, IGeminiService geminiService, IConnectivityService connectivityService)
        {
            _player = audioPlayer;
            _library = libraryService;

            Messages = new ObservableCollection<ChatMessage>();
            SendCommand = new RelayCommand(async _ => await OnSend(), _ => !string.IsNullOrWhiteSpace(CurrentMessage));

            IsVisible = false;
            ToggleVisibilityCommand = new RelayCommand(_ => IsVisible = !IsVisible);
            HideCommand = new RelayCommand(_ => IsVisible = false);

            AddBotMessage("Chào bro, tôi giúp gì được? Gõ 'help' để xem lệnh.");
            _geminiService = geminiService;
            _connectivityService = connectivityService;
        }

        private async Task OnSend()
        {
            var userMessage = CurrentMessage;
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            Messages.Add(new ChatMessage { Content = userMessage, IsUserMessage = true });
            CurrentMessage = ""; 

            await ProcessCommand(userMessage);
        }

        private void AddBotMessage(string message)
        {
            Messages.Add(new ChatMessage { Content = message, IsUserMessage = false });
        }

        private async Task ProcessCommand(string command)
        {
            if (_connectivityService.IsConnected())
            {
                // connect network
                var loadingMsg = new ChatMessage { Content = "...Đang kết nối tới Gemini...", IsUserMessage = false };
                Messages.Add(loadingMsg);

                await ProcessCommandOnlineAsync(command);

                Messages.Remove(loadingMsg);
            }
            else
            {
                // disconnect network
                await Task.Delay(500);
                await ProcessCommandOfflineAsync(command);
            }
        }

        private async Task ProcessCommandOnlineAsync(string command)
        {
            try
            {
                string botResponse = await _geminiService.GetChatResponseAsync(command);
                AddBotMessage(botResponse);

                // TODO: Function Calling
            }
            catch (Exception ex)
            {
                AddBotMessage($"Lỗi Gemini: {ex.Message}. Chuyển sang chế độ offline.");
                await ProcessCommandOfflineAsync(command);
            }
        }
        private async Task ProcessCommandOfflineAsync(string command)
        {
            string cmdLower = command.ToLower().Trim();

            if (cmdLower == "help")
            {
                AddBotMessage("Đang offline. Các lệnh có sẵn:\n- 'play [tên bài hát]'\n- 'show library'\n- 'pause'\n- 'next song'");
            }
            else if (cmdLower.StartsWith("play "))
            {
                string songName = cmdLower.Substring(5).Trim();
                await PlaySongByName(songName);
            }
            else if (cmdLower == "show library")
            {
                await ShowLibrary();
            }
            else if (cmdLower == "pause")
            {
                _player.Pause();
                AddBotMessage("Đã pause.");
            }
            else if (cmdLower == "next song")
            {
                _player.SkipNext();
                AddBotMessage("Next...");
            }
            else
            {
                AddBotMessage("Đang offline, tôi không hiểu lệnh này 😅");
            }
        }

        private async Task PlaySongByName(string songName)
        {
            if (string.IsNullOrWhiteSpace(songName))
            {
                AddBotMessage("Gõ 'play [tên bài hát]' nhé.");
                return;
            }

            var allTracks = await _library.GetLibraryAsync();
            var trackToPlay = allTracks.FirstOrDefault(t => t.Title.ToLower().Contains(songName));

            if (trackToPlay != null)
            {
                await _player.LoadPlaylist(new List<LocalTrack> { trackToPlay }, 0);
                _player.Play();
                AddBotMessage($"Đang play: {trackToPlay.Title} - {trackToPlay.Artist}");
            }
            else
            {
                AddBotMessage($"Hmm, không tìm thấy bài nào tên '{songName}'.");
            }
        }

        private async Task ShowLibrary()
        {
            var allTracks = await _library.GetLibraryAsync();
            if (!allTracks.Any())
            {
                AddBotMessage("Thư viện của ông trống trơn.");
                return;
            }

            string trackList = string.Join("\n", allTracks.Select(t => $"🎵 {t.Title} - {t.Artist}"));
            AddBotMessage("Library của bro đây:\n" + trackList);
        }
    }
}
