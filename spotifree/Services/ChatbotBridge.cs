using Microsoft.Web.WebView2.Core;
using spotifree.IServices;
using spotifree.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace spotifree.Services
{
    public class ChatbotBridge
    {
        private readonly ILocalMusicService _libraryService;
        private readonly CoreWebView2 _webView;

        public ChatbotBridge(CoreWebView2 webView)
        {
            _libraryService = new LocalMusicService();
            _webView = webView;
            _webView.WebMessageReceived += OnWebMessageReceived;
        }

        private async void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var msg = JsonSerializer.Deserialize<ChatbotCommand>(e.TryGetWebMessageAsString());
                if (msg == null) return;

                switch (msg.Action)
                {
                    case "addSong":
                        if (msg.Song != null)
                        {
                            await _libraryService.AddTrackAsync(msg.Song);
                            await SendAsync("✅ Added to playlist!");
                        }
                        break;

                    case "getPlaylist":
                        var data = await _libraryService.GetLocalLibraryAsync();
                        if (data.Count == 0)
                            await SendAsync("📭 Your playlist is empty!");
                        else
                        {
                            string list = string.Join("\\n", data.ConvertAll(x => $"🎵 {x.Title} - {x.Artist}"));
                            await SendAsync("🎧 Your playlist:\\n" + list);
                        }
                        break;

                    default:
                        await SendAsync("🤔 Unknown command.");
                        break;
                }
            }
            catch (Exception ex)
            {
                await SendAsync("⚠️ Error: " + ex.Message);
            }
        }

        private Task SendAsync(string message)
        {
            return _webView.ExecuteScriptAsync($"addMessage({JsonSerializer.Serialize(message)}, false)");
        }

        private class ChatbotCommand
        {
            public string? Action { get; set; }
            public LocalMusicTrack? Song { get; set; }
        }
    }
}
