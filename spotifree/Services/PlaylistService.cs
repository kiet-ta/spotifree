using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Spotifree.Services
{
    public class PlaylistService : IPlaylistService
    {
        private const string MetadataFileName = "playlist.json";

        public ObservableCollection<Playlist> Playlists { get; } = new();

        private string? _rootPath;
        public string? RootPath
        {
            get => _rootPath;
            set
            {
                _rootPath = string.IsNullOrWhiteSpace(value) ? null : value;
                ReloadFromDisk();
            }
        }

        public void ReloadFromDisk()
        {
            Playlists.Clear();

            if (string.IsNullOrWhiteSpace(_rootPath) || !Directory.Exists(_rootPath))
                return;

            foreach (var dir in Directory.EnumerateDirectories(_rootPath))
            {
                try
                {
                    var metaPath = Path.Combine(dir, MetadataFileName);
                    var name = Path.GetFileName(dir);

                    if (File.Exists(metaPath))
                    {
                        var json = File.ReadAllText(metaPath);
                        var meta = JsonSerializer.Deserialize<PlaylistMetadata>(json);
                        if (!string.IsNullOrWhiteSpace(meta?.Name))
                            name = meta.Name;
                    }

                    var playlist = new Playlist
                    {
                        Name = name,
                        FolderPath = dir
                    };

                    playlist.Tracks.Clear();
                    Playlists.Add(playlist);
                }
                catch
                {
                }
            }
        }

        public Playlist CreatePlaylist(string name)
        {
            if (string.IsNullOrWhiteSpace(_rootPath))
                throw new InvalidOperationException("Playlist root path is not set.");

            name = name.Trim();
            if (name.Length == 0)
                throw new ArgumentException("Invalid playlist name.", nameof(name));

            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            var folderPath = Path.Combine(_rootPath, name);
            var baseName = name;
            var suffix = 2;
            while (Directory.Exists(folderPath))
            {
                folderPath = Path.Combine(_rootPath, $"{baseName} ({suffix})");
                suffix++;
            }

            Directory.CreateDirectory(folderPath);

            var playlist = new Playlist
            {
                Name = Path.GetFileName(folderPath),
                FolderPath = folderPath
            };

            playlist.Tracks.Clear();
            SaveMetadata(playlist);
            Playlists.Add(playlist);

            return playlist;
        }

        public void RenamePlaylist(Playlist playlist, string newName)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));

            if (string.IsNullOrWhiteSpace(_rootPath))
                throw new InvalidOperationException("Playlist root path is not set.");

            newName = newName.Trim();
            if (newName.Length == 0)
                return;

            foreach (var c in Path.GetInvalidFileNameChars())
                newName = newName.Replace(c, '_');

            var newFolderPath = Path.Combine(_rootPath, newName);
            var baseName = newName;
            var suffix = 2;
            while (!string.Equals(newFolderPath, playlist.FolderPath, StringComparison.OrdinalIgnoreCase)
                   && Directory.Exists(newFolderPath))
            {
                newFolderPath = Path.Combine(_rootPath, $"{baseName} ({suffix})");
                suffix++;
            }

            if (!string.Equals(playlist.FolderPath, newFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                Directory.Move(playlist.FolderPath, newFolderPath);
                playlist.FolderPath = newFolderPath;
            }

            playlist.Name = Path.GetFileName(newFolderPath);
            SaveMetadata(playlist);
        }

        public void LoadTracksForPlaylist(Playlist playlist)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));

            playlist.Tracks.Clear();

            if (string.IsNullOrWhiteSpace(playlist.FolderPath) || !Directory.Exists(playlist.FolderPath))
                return;

            var allowedExtensions = new[] { ".mp3", ".m4a", ".flac", ".wav", ".ogg" };

            foreach (var filePath in Directory.EnumerateFiles(playlist.FolderPath, "*.*", SearchOption.TopDirectoryOnly)
                                              .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())))
            {
                try
                {
                    using var tagFile = TagLib.File.Create(filePath);
                    var tag = tagFile.Tag;

                    var track = new LocalTrack
                    {
                        FilePath = filePath,
                        Title = string.IsNullOrEmpty(tag.Title) ? Path.GetFileNameWithoutExtension(filePath) : tag.Title,
                        Artist = string.IsNullOrEmpty(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer,
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

        public void AddTrackToPlaylist(Playlist playlist, LocalTrack track)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));
            if (track == null)
                throw new ArgumentNullException(nameof(track));

            if (string.IsNullOrWhiteSpace(playlist.FolderPath) || !Directory.Exists(playlist.FolderPath))
                Directory.CreateDirectory(playlist.FolderPath);

            var fileName = Path.GetFileName(track.FilePath);
            var destPath = Path.Combine(playlist.FolderPath, fileName);

            if (!File.Exists(destPath))
                File.Copy(track.FilePath, destPath, false);

            LoadTracksForPlaylist(playlist);
        }

        public void RemoveTrackFromPlaylist(Playlist playlist, LocalTrack track)
        {
            if (playlist == null || track == null)
                return;

            playlist.Tracks.Remove(track);

            if (!string.IsNullOrEmpty(track.FilePath) && File.Exists(track.FilePath))
            {
                try
                {
                    File.Delete(track.FilePath);
                }
                catch
                {
                }
            }
        }

        private void SaveMetadata(Playlist playlist)
        {
            if (string.IsNullOrWhiteSpace(playlist.FolderPath))
                return;

            try
            {
                var meta = new PlaylistMetadata
                {
                    Name = playlist.Name
                };

                var json = JsonSerializer.Serialize(meta, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var metaPath = Path.Combine(playlist.FolderPath, MetadataFileName);
                File.WriteAllText(metaPath, json);
            }
            catch
            {
            }
        }

        private class PlaylistMetadata
        {
            public string? Name { get; set; }
        }
    }
}
