/* ==============================================
   MUSIC_DETAIL.JS - ĐÃ SỬA LẠI ĐỂ DÙNG WPF PLAYER
   ============================================== */

// --- 1. BIẾN TOÀN CỤC ---
let audioPlayer, coverArt, songTitle, songArtist;
let seekSlider, volumeSlider;
let playPauseButton, playPauseIcon, prevButton, nextButton, repeatButton, shuffleButton;
let timeDisplay; // DIV cha cho #currentTimeDisplay và #totalTimeDisplay
let currentTimeDisplay, totalTimeDisplay;

let toggleLibraryBtn, togglePlaylistBtn, playlistContainer, playlistToggleIcon;

let playlist = [];
let currentPlaylistIndex = -1;
let isRepeatPlaylist = false;
let isShuffle = false;
let isPlaylistVisible = true;
let _isPlaying = false; // Trạng thái phát nhạc
let _seekDragging = false; // Cờ cho biết người dùng có đang kéo slider không

// --- 2. ALIAS TRÌNH PHÁT C# (WPF) ---
const WPFPlayer = window.chrome?.webview?.hostObjects?.player;

if (!WPFPlayer) {
    console.error("LỖI NGHIÊM TRỌNG: Không tìm thấy 'window.chrome.webview.hostObjects.player'. PlayerBridge.cs chưa được tiêm vào!");
    alert("Lỗi: Không thể kết nối với trình phát nhạc C#.");
}

// --- 3. HÀM GIAO TIẾP C# ---

/** Gửi tin nhắn về C# (cho các nút không thuộc WPFPlayer) */
function postMessageToCSharp(message) {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage(message);
    } else {
        console.warn("postMessage is not available.");
    }
}

/** (HÀM NÀY SẼ ĐƯỢC GỌI TỪ C#) Thêm bài hát từ WPF ViewModel */
window.addMultipleToPlaylist = (songs) => {
    if (!songs || !Array.isArray(songs) || songs.length === 0) return;
    addSongsToInternalPlaylist(songs);
};

/** (HÀM NÀY SẼ ĐƯỢC GỌI TỪ C#) Tải toàn bộ thư viện từ WPF ViewModel */
window.loadFullLibrary = (library) => {
    playlist = library;
    renderPlaylist();
    if (playlist.length > 0) {
        playSongFromPlaylist(0);
    }
};

/** (HÀM NÀY SẼ ĐƯỢC GỌI TỪ C#) Phát 1 bài hát (từ library.js) */
window.playSingleSong = (song) => {
    if (!song) {
        console.error("Data bài hát không hợp lệ", song);
        return;
    }

    console.log("[Player] Nhận lệnh playSingleSong:", song);

    // Map lại Title/Artist nếu C# gửi format gốc
    song.Title = song.Title || song.name || "Unknown Title";
    song.Artist = song.Artist || song.artist || "Unknown Artist";
    song.FilePath = song.FilePath || song.filePath; // Đảm bảo FilePath (viết hoa) tồn tại
    song.CoverArtUrl = song.CoverArtUrl || "https://placehold.co/300x300/1e1e1e/b3b3b3?text=Music";

    // Đặt bài này làm playlist hiện tại
    playlist = [song];
    currentPlaylistIndex = 0;

    // Tải và phát bằng C#
    loadAndPlaySong(song);
    renderPlaylist(); // Cập nhật UI danh sách phát
};


// --- 4. HÀM XỬ LÝ PLAYER ---

/** Tải và phát 1 bài hát qua C# */
function loadAndPlaySong(song) {
    if (!song || !song.FilePath) {
        console.error("Lỗi: loadAndPlaySong không tìm thấy 'FilePath' trong đối tượng song:", song);
        return;
    }

    // Cập nhật UI ngay lập tức
    songTitle.textContent = song.Title;
    songArtist.textContent = song.Artist;
    if (song.CoverArtUrl && song.CoverArtUrl !== "Unknow") {
        coverArt.src = song.CoverArtUrl;
    } else {
        // Dùng placeholder nếu không có ảnh
        coverArt.src = "https://placehold.co/300x300/1e1e1e/b3b3b3?text=Music";
    }
    // Yêu cầu C# tải và phát file
    // (Hàm .load() trong PlayerBridge.cs đã tự động .Play())
    WPFPlayer.load(song.FilePath);

    // Cập nhật trạng thái (vì .load() tự động phát)
    _isPlaying = true;
    updatePlayPauseIcon(true);
}

/** Phát bài hát từ danh sách playlist (nội bộ) */
function playSongFromPlaylist(index) {
    if (index < 0 || index >= playlist.length) {
        console.log("Playlist index out of bounds.");
        return;
    }
    currentPlaylistIndex = index;
    const song = playlist[currentPlaylistIndex];
    loadAndPlaySong(song);
    renderPlaylist(); // Cập nhật highlight
}

/** Bật/Tắt Play/Pause */
function togglePlayPause() {
    if (!WPFPlayer) return;

    if (_isPlaying) {
        WPFPlayer.pause();
    } else {
        // Nếu chưa có bài hát, phát bài đầu tiên
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
                nextIndex = 0; // Quay về đầu
            } else {
                return; // Dừng phát
            }
        }
    }
    playSongFromPlaylist(nextIndex);
}

function playPrevious() {
    if (playlist.length === 0) return;

    // Logic tua lại nếu bài hát đã phát > 3 giây (được C# xử lý)
    // Hoặc quay về bài trước
    let prevIndex = currentPlaylistIndex - 1;
    if (prevIndex < 0) {
        if (isRepeatPlaylist) {
            prevIndex = playlist.length - 1; // Về bài cuối
        } else {
            prevIndex = 0; // Về bài đầu
        }
    }
    playSongFromPlaylist(prevIndex);
}

// --- 5. HÀM CẬP NHẬT UI ---

function updatePlayPauseIcon(isPlaying) {
    _isPlaying = isPlaying; // Cập nhật trạng thái
    if (playPauseIcon) {
        // SVG path cho Play và Pause (thay vì <use>)
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

/** Cập nhật thanh seek và thời gian (ĐƯỢC GỌI TỪ C#) */
function updateSeekUI(position, duration) {
    if (isNaN(position) || isNaN(duration) || duration === 0) {
        position = 0;
        duration = 100;
    }

    // Chỉ cập nhật nếu người dùng KHÔNG đang kéo
    if (!_seekDragging) {
        seekSlider.value = position;
    }

    seekSlider.max = duration;
    currentTimeDisplay.textContent = formatTime(position);
    totalTimeDisplay.textContent = formatTime(duration);
}

// --- 6. HÀM KHỞI TẠO (INIT) ---
function initMusicDetailPage() {
    console.log("🚀 Khởi tạo trang Music Detail (Phiên bản WPFPlayer)...");

    // Lấy các element UI
    toggleLibraryBtn = document.getElementById('toggleLibraryBtn');
    playlistContainer = document.getElementById('playlist-container');
    togglePlaylistBtn = document.getElementById('togglePlaylistBtn');
    playlistToggleIcon = togglePlaylistBtn ? togglePlaylistBtn.querySelector('svg') : null;

    // Thẻ <audio> không còn dùng để phát, nhưng giữ lại để... dự phòng
    audioPlayer = document.getElementById('audioPlayer');

    coverArt = document.getElementById('coverArt');
    songTitle = document.getElementById('songTitle');
    songArtist = document.getElementById('songArtist');

    seekSlider = document.getElementById('seekSlider');
    volumeSlider = document.getElementById('volumeSlider');

    // Sửa lại cách lấy time display
    timeDisplay = document.getElementById('timeDisplay');
    if (timeDisplay) {
        // Tách timeDisplay thành 2 span riêng
        timeDisplay.innerHTML = '<span id="currentTimeDisplay">0:00</span> / <span id="totalTimeDisplay">0:00</span>';
        currentTimeDisplay = document.getElementById('currentTimeDisplay');
        totalTimeDisplay = document.getElementById('totalTimeDisplay');
    }

    playPauseButton = document.getElementById('playPauseButton');
    playPauseIcon = document.getElementById('playPauseIcon');
    prevButton = document.getElementById('prevButton');
    nextButton = document.getElementById('nextButton');
    repeatButton = document.getElementById('repeatButton');

    // 'shuffleButton' không có trong HTML, bạn có thể thêm vào sau
    // shuffleButton = document.getElementById('shuffleButton');

    // --- NỐI DÂY SỰ KIỆN ---

    // Nút giao tiếp với C# (Window)
    if (toggleLibraryBtn) {
        toggleLibraryBtn.addEventListener('click', () => {
            postMessageToCSharp({ type: 'toggleLibrary' });
        });
    }

    // Nút giao tiếp nội bộ (JS)
    if (togglePlaylistBtn) {
        togglePlaylistBtn.addEventListener('click', () => {
            showPlaylist(!isPlaylistVisible);
        });
    }

    // Nút điều khiển Player (GỌI C# PLAYER)
    if (playPauseButton) playPauseButton.addEventListener('click', togglePlayPause);
    if (nextButton) nextButton.addEventListener('click', playNext);
    if (prevButton) prevButton.addEventListener('click', playPrevious);

    if (repeatButton) {
        repeatButton.addEventListener('click', () => {
            isRepeatPlaylist = !isRepeatPlaylist;
            repeatButton.classList.toggle('active', isRepeatPlaylist);
        });
    }

    // if (shuffleButton) { ... }

    // Thanh Seek (TUA)
    if (seekSlider) {
        // Đánh dấu khi người dùng bắt đầu kéo
        seekSlider.addEventListener('input', () => {
            _seekDragging = true;
        });
        // Gửi lệnh seek về C# KHI người dùng thả chuột
        seekSlider.addEventListener('change', () => {
            const newTime = parseFloat(seekSlider.value);
            WPFPlayer.seek(newTime);
            _seekDragging = false;
        });
    }

    // Thanh Volume
    if (volumeSlider) {
        // Gửi lệnh volume về C# KHI kéo
        volumeSlider.addEventListener('input', () => {
            // C# PlayerBridge.setVolume() nhận giá trị 0.0 đến 1.0
            // HTML volumeSlider của bạn có min=0, max=1, step=0.01
            // NÊN CHÚNG TA GIỮ NGUYÊN
            WPFPlayer.setVolume(parseFloat(volumeSlider.value));
        });
        // Khởi tạo giá trị
        WPFPlayer.setVolume(parseFloat(volumeSlider.value));
    }

    // --- 7. LẮNG NGHE SỰ KIỆN TỪ C# ---
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.addEventListener('message', (event) => {
            let data;
            try {
                // PlayerBridge.cs gửi message bằng PostWebMessageAsString
                data = JSON.parse(event.data);
            } catch (e) {
                // Bỏ qua các message không phải JSON
                return;
            }

            // Chỉ xử lý message từ PlayerBridge
            if (data.source !== 'LocalAudioService') {
                // (interop.js sẽ xử lý các message khác)
                return;
            }

            // console.log("[WPF->JS] Player Event:", data.type, data.payload);

            switch (data.type) {
                case 'position':
                    // Cập nhật UI thanh seek
                    updateSeekUI(data.payload.position, data.payload.duration);
                    break;
                case 'loaded':
                    // Nhạc đã được tải, C# báo về độ dài
                    updateSeekUI(0, data.payload.duration);
                    break;
                case 'playing':
                    updatePlayPauseIcon(true);
                    showPlaylist(false); // Tự động ẩn playlist khi nhạc phát
                    break;
                case 'paused':
                    updatePlayPauseIcon(false);
                    break;
                case 'stopped':
                    updatePlayPauseIcon(false);
                    updateSeekUI(0, seekSlider.max); // Reset về 0
                    break;
                case 'ended':
                    // Khi C# báo nhạc hết, tự động phát bài tiếp theo
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

    // Khởi tạo các phần còn lại
    renderPlaylist();
    showPlaylist(true);
}

// --- CÁC HÀM PHỤ (giữ nguyên) ---

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
            // Chuẩn hóa dữ liệu khi thêm vào
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
        WPFPlayer.stop(); // Dừng trình phát C#

        if (playlist.length === 0) {
            currentPlaylistIndex = -1;
            songTitle.textContent = "WPF Music Player";
            songArtist.textContent = "MVVM + WebView2";
            coverArt.src = "https://placehold.co/300x300/1e1e1e/b3b3b3?text=WPF+Player";
            updatePlayPauseIcon(false);
            updateSeekUI(0, 0);
        }
        else if (index >= playlist.length) {
            currentPlaylistIndex = 0; // Phát bài đầu
            playSongFromPlaylist(currentPlaylistIndex);
        }
        else {
            // Phát bài kế tiếp (tại vị trí index cũ)
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