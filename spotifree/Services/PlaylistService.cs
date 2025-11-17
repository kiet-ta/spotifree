using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Media.Imaging;
using TagLib;

namespace Spotifree.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly string[] _audioExtensions = { ".mp3", ".m4a", ".flac", ".wav", ".ogg" };
        private readonly BitmapImage _defaultCover;
        private string? _rootPath;

        public ObservableCollection<Playlist> Playlists { get; } = new();

        public string? RootPath
        {
            get => _rootPath;
            set
            {
                if (_rootPath == value) return;
                _rootPath = value;
                ReloadFromDisk();
            }
        }

        public event Action? PlaylistsChanged;

        private class PlaylistMeta
        {
            public string Name { get; set; } = string.Empty;
            public string? CoverFile { get; set; }
        }

        public PlaylistService()
        {
            _defaultCover = new BitmapImage(
                new Uri("pack://application:,,,/Spotifree;component/Assets/defaultImage.png"));
        }

        public void ReloadFromDisk()
        {
            Playlists.Clear();

            if (string.IsNullOrWhiteSpace(_rootPath) || !Directory.Exists(_rootPath))
            {
                PlaylistsChanged?.Invoke();
                return;
            }

            foreach (var dir in Directory.EnumerateDirectories(_rootPath))
            {
                try
                {
                    var meta = LoadMeta(dir);
                    var playlist = new Playlist
                    {
                        Name = meta.Name,
                        FolderPath = dir
                    };

                    var cover = LoadCoverFromMeta(dir, meta.CoverFile);
                    playlist.CoverArt = cover ?? _defaultCover;

                    Playlists.Add(playlist);
                }
                catch
                {
                }
            }

            PlaylistsChanged?.Invoke();
        }

        public void LoadFromDisk() => ReloadFromDisk();

        public Playlist CreatePlaylist(string name)
        {
            if (string.IsNullOrWhiteSpace(_rootPath))
                throw new InvalidOperationException("Playlist root path is not set.");

            Directory.CreateDirectory(_rootPath);

            var safeName = GetSafeFolderName(name);
            var folder = Path.Combine(_rootPath, safeName);
            var index = 1;
            while (Directory.Exists(folder))
            {
                folder = Path.Combine(_rootPath, $"{safeName} ({index})");
                index++;
            }

            Directory.CreateDirectory(folder);

            var playlist = new Playlist
            {
                Name = name,
                FolderPath = folder,
                CoverArt = _defaultCover
            };

            SaveMeta(playlist, null);

            Playlists.Add(playlist);
            PlaylistsChanged?.Invoke();

            return playlist;
        }

        public void RenamePlaylist(Playlist playlist, string newName)
        {
            if (playlist == null) return;
            if (string.IsNullOrWhiteSpace(newName)) return;
            if (string.IsNullOrWhiteSpace(_rootPath)) return;

            var oldFolder = playlist.FolderPath;
            if (!Directory.Exists(oldFolder)) return;

            var safeName = GetSafeFolderName(newName);
            var newFolder = Path.Combine(_rootPath, safeName);
            var index = 1;
            while (Directory.Exists(newFolder) &&
                   !string.Equals(newFolder, oldFolder, StringComparison.OrdinalIgnoreCase))
            {
                newFolder = Path.Combine(_rootPath, $"{safeName} ({index})");
                index++;
            }

            if (!string.Equals(oldFolder, newFolder, StringComparison.OrdinalIgnoreCase))
            {
                Directory.Move(oldFolder, newFolder);
                playlist.FolderPath = newFolder;
            }

            playlist.Name = newName;
            var coverFile = GetCoverFileName(newFolder);
            SaveMeta(playlist,
                System.IO.File.Exists(coverFile) ? Path.GetFileName(coverFile) : null);

            PlaylistsChanged?.Invoke();
        }

        public void DeletePlaylist(Playlist playlist)
        {
            if (playlist == null) return;

            var folder = playlist.FolderPath;
            if (Directory.Exists(folder))
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch
                {
                }
            }

            Playlists.Remove(playlist);
            PlaylistsChanged?.Invoke();
        }

        public void ChangeCover(Playlist playlist, string? imageFilePath)
        {
            if (playlist == null) return;

            var folder = playlist.FolderPath;
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return;

            string? coverFileName = null;

            if (!string.IsNullOrWhiteSpace(imageFilePath) &&
                System.IO.File.Exists(imageFilePath))
            {
                var ext = Path.GetExtension(imageFilePath);
                if (string.IsNullOrWhiteSpace(ext)) ext = ".png";

                var destPath = GetCoverFileName(folder, ext);
                try
                {
                    System.IO.File.Copy(imageFilePath, destPath, true);
                    playlist.CoverArt = LoadBitmap(destPath) ?? _defaultCover;
                    coverFileName = Path.GetFileName(destPath);
                }
                catch
                {
                    playlist.CoverArt = _defaultCover;
                }
            }
            else
            {
                var existing = Directory.EnumerateFiles(folder, "cover.*").FirstOrDefault();
                if (existing != null)
                {
                    try
                    {
                        System.IO.File.Delete(existing);
                    }
                    catch
                    {
                    }
                }
                playlist.CoverArt = _defaultCover;
            }

            SaveMeta(playlist, coverFileName);
            PlaylistsChanged?.Invoke();
        }

        public void AddTrackToPlaylist(Playlist playlist, LocalTrack track)
        {
            if (playlist == null || track == null) return;
            if (string.IsNullOrWhiteSpace(track.FilePath)) return;

            var folder = playlist.FolderPath;
            if (string.IsNullOrWhiteSpace(folder)) return;

            Directory.CreateDirectory(folder);

            var fileName = Path.GetFileName(track.FilePath);
            var destPath = Path.Combine(folder, fileName);

            if (!System.IO.File.Exists(destPath))
            {
                try
                {
                    System.IO.File.Copy(track.FilePath, destPath);
                }
                catch
                {
                    return;
                }
            }

            var newTrack = new LocalTrack
            {
                FilePath = destPath,
                Title = track.Title,
                Artist = track.Artist,
                Album = track.Album,
                Duration = track.Duration,
                TrackNumber = track.TrackNumber,
                Year = track.Year,
                CoverArt = track.CoverArt
            };

            playlist.Tracks.Add(newTrack);
        }

        public void RemoveTrackFromPlaylist(Playlist playlist, LocalTrack track)
        {
            if (playlist == null || track == null) return;

            var folder = playlist.FolderPath;
            if (string.IsNullOrWhiteSpace(folder)) return;

            var path = track.FilePath;
            if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
            {
                try
                {
                    System.IO.File.Delete(path);
                }
                catch
                {
                }
            }

            playlist.Tracks.Remove(track);
        }

        public void LoadTracksForPlaylist(Playlist playlist)
        {
            if (playlist == null) return;

            playlist.Tracks.Clear();

            var folder = playlist.FolderPath;
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return;

            var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => _audioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

            foreach (var filePath in files)
            {
                try
                {
                    using var tagFile = TagLib.File.Create(filePath);
                    var tag = tagFile.Tag;

                    var track = new LocalTrack
                    {
                        FilePath = filePath,
                        Title = string.IsNullOrEmpty(tag.Title)
                            ? Path.GetFileNameWithoutExtension(filePath)
                            : tag.Title,
                        Artist = string.IsNullOrEmpty(tag.FirstPerformer)
                            ? "Unknown Artist"
                            : tag.FirstPerformer,
                        Album = playlist.Name,
                        Duration = tagFile.Properties.Duration.TotalSeconds,
                        TrackNumber = tag.Track,
                        Year = tag.Year,
                        CoverArt = tag.Pictures.Length > 0 ? tag.Pictures[0].Data.Data : null
                    };

                    playlist.Tracks.Add(track);
                }
                catch
                {
                }
            }
        }

        private static string GetSafeFolderName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            if (string.IsNullOrWhiteSpace(name)) name = "Playlist";
            return name;
        }

        private PlaylistMeta LoadMeta(string folder)
        {
            var metaPath = Path.Combine(folder, "playlist.json");
            if (System.IO.File.Exists(metaPath))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(metaPath);
                    var meta = JsonSerializer.Deserialize<PlaylistMeta>(json);
                    if (meta != null && !string.IsNullOrWhiteSpace(meta.Name))
                        return meta;
                }
                catch
                {
                }
            }

            return new PlaylistMeta { Name = Path.GetFileName(folder) };
        }

        private void SaveMeta(Playlist playlist, string? coverFileName)
        {
            var folder = playlist.FolderPath;
            if (string.IsNullOrWhiteSpace(folder)) return;

            var meta = new PlaylistMeta
            {
                Name = playlist.Name,
                CoverFile = coverFileName
            };

            var json = JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true });
            var metaPath = Path.Combine(folder, "playlist.json");
            try
            {
                System.IO.File.WriteAllText(metaPath, json);
            }
            catch
            {
            }
        }

        private static string GetCoverFileName(string folder, string? ext = null)
        {
            ext ??= ".png";
            return Path.Combine(folder, "cover" + ext);
        }

        private BitmapImage? LoadCoverFromMeta(string folder, string? coverFile)
        {
            if (string.IsNullOrWhiteSpace(coverFile)) return null;

            var path = Path.Combine(folder, coverFile);
            if (!System.IO.File.Exists(path)) return null;

            return LoadBitmap(path);
        }

        private static BitmapImage? LoadBitmap(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
