using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows; // Thêm 'using' này
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

            // Những event này đã chạy 'đúng' (correctly) rồi,
            // vì 'LocalAudioService' đã dùng 'Timer'
            _player.PositionChanged += (_, pos) =>
                Send("position", new { position = pos, duration = _player.DurationSeconds });
            _player.PlaybackEnded += (_, __) => Send("ended", new { });
            _player.PlaybackError += (_, ex) => Send("error", new { message = ex.Message });
        }

        // Bọc (wrap) mọi hàm public trong Dispatcher.Invoke
        // để đảm bảo chúng chạy trên UI thread

        public void load(string path)
        {
            // Dùng 'Task.Run' để chạy nền (background)
            // và 'Dispatcher.Invoke' để gọi UI-thread
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    // Hàm LoadAsync đã được 'vá' (patched) để an toàn
                    await _player.LoadAsync(path);

                    // 'Play' và 'Send' phải chạy trên UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Send("loaded", new { path, duration = _player.DurationSeconds });
                        _player.Play();
                        Send("playing", new { path });
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Send("error", new { where = "load", message = ex.Message, path });
                    });
                }
            });
        }

        public void browseAndLoad()
        {
            // 'ShowDialog' BẮT BUỘC (MUST) chạy trên UI thread
            Application.Current.Dispatcher.Invoke(() =>
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
                        // Dùng 'Task.Run' để 'fire-and-forget' (bắn và quên)
                        // việc load file, không 'await' nó
                        _ = System.Threading.Tasks.Task.Run(async () =>
                        {
                            try
                            {
                                await _player.LoadAsync(dlg.FileName);

                                // Sau khi load xong, 'nhảy' (switch) về UI thread
                                // để 'Play' và 'Send'
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Send("loaded", new { path = dlg.FileName, duration = _player.DurationSeconds });
                                    _player.Play();
                                    Send("playing", new { path = _player.CurrentPath });
                                });
                            }
                            catch (Exception ex)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Send("error", new { where = "LoadAsync_in_browse", message = ex.Message });
                                });
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Send("error", new { where = "browseAndLoad_ShowDialog", message = ex.Message });
                }
            });
        }

        public void play()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _player.Play();
                Send("playing", new { path = _player.CurrentPath });
            });
        }

        public void pause()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _player.Pause();
                Send("paused", new { });
            });
        }

        public void stop()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _player.Stop();
                Send("stopped", new { });
            });
        }

        public void seek(double seconds)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _player.Seek(seconds);
                Send("seeked", new { seconds });
            });
        }

        public void setVolume(double v)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _player.Volume = v;
                Send("volume", new { volume = _player.Volume });
            });
        }

        public object getState()
        {
            return Application.Current.Dispatcher.Invoke<object>(() =>
            {
                try
                {
                    return new
                    {
                        state = _player.State.ToString(),
                        position = _player.PositionSeconds,
                        duration = _player.DurationSeconds,
                        path = _player.CurrentPath
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        state = "Error",
                        position = 0,
                        duration = 0,
                        path = $"Error getting state: {ex.Message}"
                    };
                }
            });
        }

        private void Send(string type, object payload)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var msg = JsonSerializer.Serialize(new { source = "LocalAudioService", type, payload });
                    _web.CoreWebView2?.PostWebMessageAsString(msg);
                }
                catch { }
            });
        }
    }
}