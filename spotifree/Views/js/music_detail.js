
let audioPlayer, coverArt, songTitle, songArtist;
let seekSlider, volumeSlider;
let playPauseButton, playPauseIcon, prevButton, nextButton, repeatButton, shuffleButton;
let currentTimeDisplay, totalTimeDisplay;

let toggleLibraryBtn, togglePlaylistBtn, playlistContainer, playlistToggleIcon;

let playlist = [];
let currentPlaylistIndex = -1;
let isRepeatPlaylist = false;
let isShuffle = false;
let isPlaylistVisible = true;

function postMessageToCSharp(message) {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage(message);
    } else {
        console.warn("postMessage is not available.");
    }
}
window.addToPlaylist = (song) => {
    if (!song || !song.FilePath) return;
    addSongsToInternalPlaylist([song]);
};
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
window.receiveCommand = (command) => {
    console.log("C# command:", command);
};


function addSongsToInternalPlaylist(songs) {
    const wasPlaylistEmpty = playlist.length === 0;
    songs.forEach(song => {
        if (song && song.FilePath) {
            playlist.push(song);
        }
    });
    renderPlaylist();
    if (wasPlaylistEmpty && playlist.length > 0) {
        currentPlaylistIndex = 0;
        loadSong(currentPlaylistIndex);
    }
}

function showPlaylist(show) {
    if (!playlistContainer) return;

    if (show) {
        playlistContainer.style.width = '300px';
        playlistContainer.style.padding = '20px';
        playlistContainer.style.overflowY = 'auto';
        playlistToggleIcon.style.transform = 'rotate(0deg)';
        isPlaylistVisible = true;
    } else {
        playlistContainer.style.width = '0px';
        playlistContainer.style.padding = '0px';
        playlistContainer.style.overflowY = 'hidden';
        playlistToggleIcon.style.transform = 'rotate(180deg)';
        isPlaylistVisible = false;
    }
}

function removeSongFromPlaylist(index) {
    let wasPlaying = (index === currentPlaylistIndex);
    playlist.splice(index, 1);

    if (index < currentPlaylistIndex) {
        currentPlaylistIndex--;
    }
    else if (wasPlaying) {
        audioPlayer.pause();
        audioPlayer.src = '';

        if (playlist.length === 0) {
            currentPlaylistIndex = -1;
            songTitle.textContent = "WPF Music Player";
            songArtist.textContent = "MVVM + WebView2";
            coverArt.src = "https://placehold.co/300x300/1e1e1e/b3b3b3?text=WPF+Player";
            updatePlayPauseIcon(false);
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

function loadSong(index) {
    const song = playlist[index];
    if (!song) return;

    songTitle.textContent = song.Title || song.name || "Unknown Title";
    songArtist.textContent = song.Artist || song.artist || "Unknown Artist";
    coverArt.src = song.CoverArtUrl || "https://placehold.co/300x300/1e1e1e/b3b3b3?text=Music";

    let filePath = song.filePath || song.FilePath;
    if (!filePath) {
        console.error("loadSong failed: Không tìm thấy FilePath hoặc filePath trong object:", song);
        return;
    }
    const driveLetter = filePath.substring(0, 1).toLowerCase();
    const hostName = `${driveLetter}.drive.local`;
    const webUrl = `https://${hostName}${filePath.substring(2).replace(/\\/g, '/')}`;

    audioPlayer.src = webUrl;

    currentTimeDisplay.textContent = "0:00";
    totalTimeDisplay.textContent = "0:00";
}

function playSongFromPlaylist(index) {
    if (index < 0 || index >= playlist.length) {
        console.log("Playlist index out of bounds.");
        return;
    }
    currentPlaylistIndex = index;
    loadSong(currentPlaylistIndex);
    audioPlayer.play().catch(e => console.error("Lỗi phát nhạc:", e));
    renderPlaylist();
}

function playNext() {
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
    if (audioPlayer.currentTime > 3) {
        audioPlayer.currentTime = 0;
    } else {
        let prevIndex = currentPlaylistIndex - 1;
        if (prevIndex < 0) {
            if (isRepeatPlaylist && playlist.length > 0) {
                prevIndex = playlist.length - 1;
            } else {
                prevIndex = 0;
            }
        }
        playSongFromPlaylist(prevIndex);
    }
}

function renderPlaylist() {
    const playlistItemsContainer = document.getElementById('playlist-items');
    if (!playlistItemsContainer) return;

    playlistItemsContainer.innerHTML = "";
    if (playlist.length === 0) {
        playlistItemsContainer.innerHTML = '<div class="playlist-item artist">Thêm bài hát từ Thư viện...</div>';
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
function updatePlayPauseIcon(isPlaying) {
    if (playPauseIcon) {
        const iconName = isPlaying ? "#i-pause" : "#i-play";
        playPauseIcon.innerHTML = `<use href="${iconName}" />`;
    }
    if (playPauseButton) {
        playPauseButton.title = isPlaying ? "Pause" : "Play";
    }
}

function formatTime(seconds) {
    if (isNaN(seconds)) return "0:00";
    const minutes = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${minutes}:${secs < 10 ? '0' : ''}${secs}`;
}

function initMusicDetailPage() {
    console.log("🚀 Khởi tạo trang Music Detail với Footer Player mới...");

    toggleLibraryBtn = document.getElementById('toggleLibraryBtn');
    playlistContainer = document.getElementById('playlist-container');
    togglePlaylistBtn = document.getElementById('togglePlaylistBtn');
    playlistToggleIcon = togglePlaylistBtn.querySelector('svg');

    audioPlayer = document.getElementById('audioPlayer');
    coverArt = document.getElementById('coverArt');
    songTitle = document.getElementById('songTitle');
    songArtist = document.getElementById('songArtist');

    seekSlider = document.getElementById('seekSlider');
    volumeSlider = document.getElementById('volumeSlider');

    currentTimeDisplay = document.getElementById('currentTimeDisplay');
    totalTimeDisplay = document.getElementById('totalTimeDisplay');

    playPauseButton = document.getElementById('playPauseButton');
    playPauseIcon = document.getElementById('playPauseIcon');
    prevButton = document.getElementById('prevButton');
    nextButton = document.getElementById('nextButton');
    repeatButton = document.getElementById('repeatButton');
    shuffleButton = document.getElementById('shuffleButton');

    toggleLibraryBtn.addEventListener('click', () => {
        postMessageToCSharp({ type: 'toggleLibrary' });
    });
    togglePlaylistBtn.addEventListener('click', () => {
        showPlaylist(!isPlaylistVisible);
    });

    playPauseButton.addEventListener('click', () => {
        if (audioPlayer.paused) {
            if (currentPlaylistIndex === -1 && playlist.length > 0) {
                playSongFromPlaylist(0);
            } else {
                audioPlayer.play();
            }
        } else {
            audioPlayer.pause();
        }
    });
    nextButton.addEventListener('click', playNext);
    prevButton.addEventListener('click', playPrevious);

    repeatButton.addEventListener('click', () => {
        isRepeatPlaylist = !isRepeatPlaylist;
        repeatButton.classList.toggle('active', isRepeatPlaylist);
    });
    shuffleButton.addEventListener('click', () => {
        isShuffle = !isShuffle;
        shuffleButton.classList.toggle('active', isShuffle);
    });
    audioPlayer.addEventListener('ended', () => {
        playNext();
    });
    audioPlayer.addEventListener('play', () => {
        updatePlayPauseIcon(true);
        showPlaylist(false);
    });
    audioPlayer.addEventListener('pause', () => updatePlayPauseIcon(false));

    audioPlayer.addEventListener('timeupdate', () => {
        if (audioPlayer.duration) {
            const percentage = (audioPlayer.currentTime / audioPlayer.duration) * 100;
            seekSlider.value = percentage;

            currentTimeDisplay.textContent = formatTime(audioPlayer.currentTime);
            totalTimeDisplay.textContent = formatTime(audioPlayer.duration);
        }
    });

    audioPlayer.addEventListener('loadedmetadata', () => {
        totalTimeDisplay.textContent = formatTime(audioPlayer.duration);
    });

    seekSlider.addEventListener('input', () => {
        if (audioPlayer.duration) {
            audioPlayer.currentTime = (seekSlider.value / 100) * audioPlayer.duration;
        }
    });

    volumeSlider.addEventListener('input', () => {
        audioPlayer.volume = volumeSlider.value / 100;
    });
    renderPlaylist();
    showPlaylist(true);
    audioPlayer.volume = volumeSlider.value / 100;
}
function playFromLibrary(playlistId, index) {
    this.currentPlaylistId = playlistId;
    this.currentIndex = index;
    // Gửi message yêu cầu data bài hát
    window.chrome.webview.postMessage({
        type: 'requestSongData',
        playlistId: this.currentPlaylistId,
        index: this.currentIndex
    });

}

window.playSingleSong = (song) => {
    if (!song) {
        console.error("Data bài hát không hợp lệ", song);
        return;
    }
    
    console.log("[Player] Nhận lệnh playSingleSong:", song);

    // Map lại Title/Artist nếu C# gửi format gốc
    song.Title = song.Title || song.name || "Unknown Title";
    song.Artist = song.Artist || song.artist || "Unknown Artist";
    song.CoverArtUrl = song.CoverArtUrl || "https://placehold.co/300x300/1e1e1e/b3b3b3?text=Music";

    playlist = [song]; // Xóa list cũ, thêm bài này vào
    currentPlaylistIndex = 0;

    loadSong(currentPlaylistIndex); // Tải data bài hát
    audioPlayer.play().catch(e => console.error("Lỗi phát nhạc:", e)); // Phát nhạc
    renderPlaylist(); // Cập nhật UI danh sách phát
};