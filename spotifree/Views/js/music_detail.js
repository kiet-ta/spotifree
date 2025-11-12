let audioPlayer, coverArt, songTitle, songArtist;
let seekSlider, volumeSlider;
let playPauseButton, playPauseIcon, prevButton, nextButton, repeatButton, shuffleButton;
let timeDisplay;
let currentTimeDisplay, totalTimeDisplay;

let toggleLibraryBtn, togglePlaylistBtn, playlistContainer, playlistToggleIcon;

let playlist = [];
let currentPlaylistIndex = -1;
let isRepeatPlaylist = false;
let isShuffle = false;
let isPlaylistVisible = true;
let _isPlaying = false;
let _seekDragging = false;

const WPFPlayer = window.chrome?.webview?.hostObjects?.player;

if (!WPFPlayer) {
    console.error("LỖI NGHIÊM TRỌNG: Không tìm thấy 'window.chrome.webview.hostObjects.player'. PlayerBridge.cs chưa được tiêm vào!");
    alert("Lỗi: Không thể kết nối với trình phát nhạc C#.");
}

function postMessageToCSharp(message) {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage(message);
    } else {
        console.warn("postMessage is not available.");
    }
}

window.addMultipleToPlaylist = (songs) => {
    if (!songs || !Array.isArray(songs) || songs.length === 0) return;
    addSongsToInternalPlaylist(songs);
};

window.loadFullLibrary = (library) => {
    playlist = library;
    renderPlaylist();
    if (playlist.length > 0) {
        playSongFromPlaylist(0);
    }
};

window.playSingleSong = (song) => {
    if (!song) {
        console.error("Data bài hát không hợp lệ", song);
        return;
    }

    console.log("[Player] Nhận lệnh playSingleSong (Đã sửa: Add to Queue):", song);

    song.Title = song.Title || song.name || "Unknown Title";
    song.Artist = song.Artist || song.artist || "Unknown Artist";
    song.FilePath = song.FilePath || song.filePath;
    song.CoverArtUrl = song.CoverArtUrl || "https://placehold.co/300x300/1e1e1e/b3b3b3?text=Music";

    playlist.push(song);

    renderPlaylist();

    if (!_isPlaying && currentPlaylistIndex === -1) {
        console.log("Trình phát rảnh, phát bài hát mới thêm.");
        currentPlaylistIndex = playlist.length - 1;
        loadAndPlaySong(playlist[currentPlaylistIndex]);
    } else {
        console.log("Trình phát đang bận, đã thêm bài hát vào hàng đợi.");
    }
};

function loadAndPlaySong(song) {
    if (!song || !song.FilePath) {
        console.error("Lỗi: loadAndPlaySong không tìm thấy 'FilePath' trong đối tượng song:", song);
        return;
    }

    songTitle.textContent = song.Title;
    songArtist.textContent = song.Artist;

    if (song.CoverArtUrl && song.CoverArtUrl !== "Unknow") {
        coverArt.src = song.CoverArtUrl;
    } else {
        coverArt.src = "https://placehold.co/300x300/1e1e1e/b3b3b3?text=Music";
    }

    WPFPlayer.load(song.FilePath);

    _isPlaying = true;
    updatePlayPauseIcon(true);
}

function playSongFromPlaylist(index) {
    if (index < 0 || index >= playlist.length) {
        console.log("Playlist index out of bounds.");
        return;
    }
    currentPlaylistIndex = index;
    const song = playlist[currentPlaylistIndex];
    loadAndPlaySong(song);
    renderPlaylist();
}

function togglePlayPause() {
    if (!WPFPlayer) return;

    if (_isPlaying) {
        WPFPlayer.pause();
    } else {
        if (currentPlaylistIndex === -1 && playlist.length > 0) {
            playSongFromPlaylist(0);
        } else {
            WPFPlayer.play();
        }
    }
}

function playNext() {
    if (playlist.length === 0) return;

    let nextIndex;
    if (isShuffle) {
        nextIndex = Math.floor(Math.random() * playlist.length);
    } else {
        nextIndex = currentPlaylistIndex + 1;
        if (nextIndex >= playlist.length) {
            if (isRepeatPlaylist) {
                nextIndex = 0;
            } else {
                return;
            }
        }
    }
    playSongFromPlaylist(nextIndex);
}

function playPrevious() {
    if (playlist.length === 0) return;

    let prevIndex = currentPlaylistIndex - 1;
    if (prevIndex < 0) {
        if (isRepeatPlaylist) {
            prevIndex = playlist.length - 1;
        } else {
            prevIndex = 0;
        }
    }
    playSongFromPlaylist(prevIndex);
}

function updatePlayPauseIcon(isPlaying) {
    _isPlaying = isPlaying;
    if (playPauseIcon) {
        const playSVG = '<path d="M8 5v14l11-7z" />';
        const pauseSVG = '<path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z"/>';
        playPauseIcon.innerHTML = isPlaying ? pauseSVG : playSVG;
    }
    if (playPauseButton) {
        playPauseButton.title = isPlaying ? "Pause" : "Play";
    }
}

function formatTime(seconds) {
    if (isNaN(seconds) || seconds < 0) return "0:00";
    const minutes = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${minutes}:${secs < 10 ? '0' : ''}${secs}`;
}

function updateSeekUI(position, duration) {
    if (isNaN(position) || isNaN(duration) || duration === 0) {
        position = 0;
        duration = 100;
    }

    if (!_seekDragging) {
        seekSlider.value = position;
    }

    seekSlider.max = duration;
    currentTimeDisplay.textContent = formatTime(position);
    totalTimeDisplay.textContent = formatTime(duration);
}

function initMusicDetailPage() {
    console.log("🚀 Khởi tạo trang Music Detail (Phiên bản WPFPlayer)...");

    toggleLibraryBtn = document.getElementById('toggleLibraryBtn');
    playlistContainer = document.getElementById('playlist-container');
    togglePlaylistBtn = document.getElementById('togglePlaylistBtn');
    playlistToggleIcon = togglePlaylistBtn ? togglePlaylistBtn.querySelector('svg') : null;

    audioPlayer = document.getElementById('audioPlayer');

    coverArt = document.getElementById('coverArt');
    songTitle = document.getElementById('songTitle');
    songArtist = document.getElementById('songArtist');

    seekSlider = document.getElementById('seekSlider');
    volumeSlider = document.getElementById('volumeSlider');

    timeDisplay = document.getElementById('timeDisplay');
    if (timeDisplay) {
        timeDisplay.innerHTML = '<span id="currentTimeDisplay">0:00</span> / <span id="totalTimeDisplay">0:00</span>';
        currentTimeDisplay = document.getElementById('currentTimeDisplay');
        totalTimeDisplay = document.getElementById('totalTimeDisplay');
    }

    playPauseButton = document.getElementById('playPauseButton');
    playPauseIcon = document.getElementById('playPauseIcon');
    prevButton = document.getElementById('prevButton');
    nextButton = document.getElementById('nextButton');
    repeatButton = document.getElementById('repeatButton');

    if (toggleLibraryBtn) {
        toggleLibraryBtn.addEventListener('click', () => {
            postMessageToCSharp({ type: 'toggleLibrary' });
        });
    }

    if (togglePlaylistBtn) {
        togglePlaylistBtn.addEventListener('click', () => {
            showPlaylist(!isPlaylistVisible);
        });
    }

    if (playPauseButton) playPauseButton.addEventListener('click', togglePlayPause);
    if (nextButton) nextButton.addEventListener('click', playNext);
    if (prevButton) prevButton.addEventListener('click', playPrevious);

    if (repeatButton) {
        repeatButton.addEventListener('click', () => {
            isRepeatPlaylist = !isRepeatPlaylist;
            repeatButton.classList.toggle('active', isRepeatPlaylist);
        });
    }

    if (seekSlider) {
        seekSlider.addEventListener('input', () => {
            _seekDragging = true;
        });
        seekSlider.addEventListener('change', () => {
            const newTime = parseFloat(seekSlider.value);
            WPFPlayer.seek(newTime);
            _seekDragging = false;
        });
    }

    if (volumeSlider) {
        volumeSlider.addEventListener('input', () => {
            WPFPlayer.setVolume(parseFloat(volumeSlider.value));
        });
        WPFPlayer.setVolume(parseFloat(volumeSlider.value));
    }

    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.addEventListener('message', (event) => {
            let data;
            try {
                data = JSON.parse(event.data);
            } catch (e) {
                return;
            }

            if (data.source !== 'LocalAudioService') {
                return;
            }

            switch (data.type) {
                case 'position':
                    updateSeekUI(data.payload.position, data.payload.duration);
                    break;
                case 'loaded':
                    updateSeekUI(0, data.payload.duration);
                    break;
                case 'playing':
                    updatePlayPauseIcon(true);
                    showPlaylist(false);
                    break;
                case 'paused':
                    updatePlayPauseIcon(false);
                    break;
                case 'stopped':
                    updatePlayPauseIcon(false);
                    updateSeekUI(0, seekSlider.max);
                    break;
                case 'ended':
                    updatePlayPauseIcon(false);
                    playNext();
                    break;
                case 'error':
                    console.error("Lỗi từ C# Player:", data.payload.message);
                    alert("Lỗi phát nhạc: " + data.payload.message);
                    break;
            }
        });
    }

    renderPlaylist();
    showPlaylist(true);
}

function showPlaylist(show) {
    if (!playlistContainer) return;
    if (show) {
        playlistContainer.style.width = '300px';
        playlistContainer.style.padding = '20px';
        if (playlistToggleIcon) playlistToggleIcon.style.transform = 'rotate(0deg)';
        isPlaylistVisible = true;
    } else {
        playlistContainer.style.width = '0px';
        playlistContainer.style.padding = '0px';
        if (playlistToggleIcon) playlistToggleIcon.style.transform = 'rotate(180deg)';
        isPlaylistVisible = false;
    }
}

function addSongsToInternalPlaylist(songs) {
    const wasPlaylistEmpty = playlist.length === 0;
    songs.forEach(song => {
        if (song && (song.FilePath || song.filePath)) {
            song.Title = song.Title || song.name;
            song.Artist = song.Artist || song.artist;
            song.FilePath = song.FilePath || song.filePath;
            playlist.push(song);
        }
    });
    renderPlaylist();
    if (wasPlaylistEmpty && playlist.length > 0) {
        playSongFromPlaylist(0);
    }
}

function removeSongFromPlaylist(index) {
    let wasPlaying = (index === currentPlaylistIndex);
    playlist.splice(index, 1);

    if (index < currentPlaylistIndex) {
        currentPlaylistIndex--;
    }
    else if (wasPlaying) {
        WPFPlayer.stop();

        if (playlist.length === 0) {
            currentPlaylistIndex = -1;
            songTitle.textContent = "WPF Music Player";
            songArtist.textContent = "MVVM + WebView2";
            coverArt.src = "https://placehold.co/300x300/1e1e1e/b3b3b3?text=WPF+Player";
            updatePlayPauseIcon(false);
            updateSeekUI(0, 0);
        }
        else if (index >= playlist.length) {
            currentPlaylistIndex = 0;
            playSongFromPlaylist(currentPlaylistIndex);
        }
        else {
            currentPlaylistIndex = index;
            playSongFromPlaylist(currentPlaylistIndex);
        }
    }
    renderPlaylist();
}

function renderPlaylist() {
    const playlistItemsContainer = document.getElementById('playlist-items');
    if (!playlistItemsContainer) return;

    playlistItemsContainer.innerHTML = "";
    if (playlist.length === 0) {
        playlistItemsContainer.innerHTML = '<div class="playlist-item artist">Danh sách phát trống...</div>';
        return;
    }

    playlist.forEach((song, index) => {
        const item = document.createElement('div');
        item.className = 'playlist-item';
        if (index === currentPlaylistIndex) {
            item.classList.add('active');
        }
        item.innerHTML = `
            <button class="delete-btn" title="Xóa khỏi danh sách">×</button>
            <div class="title">${song.Title}</div>
            <div class="artist">${song.Artist}</div>
        `;
        const deleteBtn = item.querySelector('.delete-btn');
        deleteBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            removeSongFromPlaylist(index);
        });
        item.addEventListener('click', () => {
            playSongFromPlaylist(index);
        });
        playlistItemsContainer.appendChild(item);
    });
}