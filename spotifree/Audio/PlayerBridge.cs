using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;

namespace Spotifree.Audio
{
    [ComVisible(true)]
    public sealed class PlayerBridge
    {
        private readonly LocalAudioService _player;
        private readonly WebView2 _web;

        public PlayerBridge(LocalAudioService player, WebView2 web)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _web = web ?? throw new ArgumentNullException(nameof(web));

            _player.PositionChanged += (_, pos) =>
                Send("position", new { position = pos, duration = _player.DurationSeconds });
            _player.PlaybackEnded += (_, __) => Send("ended", new { });
            _player.PlaybackError += (_, ex) => Send("error", new { message = ex.Message });
        }

        public async void load(string path)
        {
            try
            {
                await _player.LoadAsync(path);
                Send("loaded", new { path, duration = _player.DurationSeconds });
                _player.Play();                               // auto-play để test nhanh
                Send("playing", new { path });
            }
            catch (Exception ex)
            {
                Send("error", new { where = "load", message = ex.Message, path });
            }
        }

        public async void browseAndLoad()
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Title = "Chọn file nhạc",
                    Filter = "Audio|*.mp3;*.wav;*.aiff|All files|*.*",
                    Multiselect = false
                };
                if (dlg.ShowDialog() == true)
                {
                    await _player.LoadAsync(dlg.FileName);                // ✅ KHÔNG dùng GetAwaiter().GetResult()
                    Send("loaded", new { path = dlg.FileName, duration = _player.DurationSeconds });
                    _player.Play();
                    Send("playing", new { path = _player.CurrentPath });
                }
            }
            catch (Exception ex)
            {
                Send("error", new { where = "browseAndLoad", message = ex.Message });
            }
        }

        public void play() { _player.Play(); Send("playing", new { path = _player.CurrentPath }); }
        public void pause() { _player.Pause(); Send("paused", new { }); }
        public void stop() { _player.Stop(); Send("stopped", new { }); }
        public void seek(double seconds) { _player.Seek(seconds); Send("seeked", new { seconds }); }

        public void setVolume(double v)
        {
            _player.Volume = v;
            Send("volume", new { volume = _player.Volume });
        }

        public object getState() => new
        {
            state = _player.State.ToString(),
            position = _player.PositionSeconds,
            duration = _player.DurationSeconds,
            path = _player.CurrentPath
        };

        private void Send(string type, object payload)
        {
            try
            {
                var msg = JsonSerializer.Serialize(new { source = "LocalAudioService", type, payload });
                _web.CoreWebView2?.PostWebMessageAsString(msg);
            }
            catch { /* swallow */ }
        }
    }
}
