// js/album.js
// Album page = Queue view + LocalAudioService player

(function () {
    const qs = (s, r = document) => r.querySelector(s);
    const qsa = (s, r = document) => Array.from(r.querySelectorAll(s));

    const WPFPlayer = window.chrome?.webview?.hostObjects?.player;

    let queue = [];
    let currentIndex = -1;
    let isPlaying = false;
    let isRepeat = false;
    let seekDragging = false;
    let currentPosition = 0;
    let currentDuration = 0;

    let heroPlayBtn;
    let footerPlayBtn, prevBtn, nextBtn, repeatBtn;
    let seekSlider, curLabel, durLabel;
    let volSlider;
    let nowCover, nowTitle, nowArtist;
    let heroCover, heroTitle, heroSub;
    let rowsContainer, sideContainer;

    function fmtTime(sec) {
        if (!sec || isNaN(sec)) return "0:00";
        sec = Math.floor(sec);
        const m = Math.floor(sec / 60);
        const s = sec % 60;
        return m + ":" + String(s).padStart(2, "0");
    }

    function getQueue() {
        return (window.__localQueue || []).slice();
    }

    function setNowUI(song) {
        if (!song) return;
        const title = song.Title || song.name || "Unknown Title";
        const artist = song.Artist || song.artist || "Unknown Artist";
        const cover = song.CoverArtUrl || song.coverArtUrl || "/assets/playlist-demo.jpg";

        if (nowTitle) nowTitle.textContent = title;
        if (nowArtist) nowArtist.textContent = artist;
        if (nowCover) nowCover.src = cover;

        if (heroTitle) heroTitle.textContent = "Local Queue";
        if (heroSub) heroSub.textContent = `${queue.length} songs`;
        if (heroCover) heroCover.src = cover;
    }

    function updateSeekUI(pos, dur) {
        currentPosition = pos || 0;
        currentDuration = dur || 0;

        if (!seekSlider) return;

        if (!Number.isFinite(currentDuration) || currentDuration <= 0) {
            currentDuration = 1;
        }

        seekSlider.max = currentDuration;

        if (!seekDragging) {
            seekSlider.value = currentPosition;
        }

        if (curLabel) curLabel.textContent = fmtTime(currentPosition);
        if (durLabel) durLabel.textContent = fmtTime(currentDuration);
    }

    function updatePlayButton() {
        isPlaying = !!isPlaying;
        if (!footerPlayBtn) return;
        footerPlayBtn.title = isPlaying ? "Pause" : "Play";
        // Icon vẫn dùng play, không đổi – chỉ đổi title là đủ.
    }

    function handleEnded() {
        if (queue.length === 0) return;
        if (isRepeat) {
            const next = (currentIndex + 1) % queue.length;
            loadSong(next, true);
        } else {
            const next = currentIndex + 1;
            if (next < queue.length) {
                loadSong(next, true);
            } else {
                isPlaying = false;
                updatePlayButton();
            }
        }
    }

    function loadSong(index, autoPlay) {
        if (!WPFPlayer) {
            console.error("WPFPlayer host object not found.");
            return;
        }
        if (index < 0 || index >= queue.length) return;

        currentIndex = index;
        const song = queue[index];
        if (!song || !song.FilePath) {
            console.error("Song missing FilePath", song);
            return;
        }

        setNowUI(song);

        try {
            WPFPlayer.load(song.FilePath);
        } catch (e) {
            console.error("Error calling WPFPlayer.load", e);
        }

        if (autoPlay) {
            try {
                WPFPlayer.play();
                isPlaying = true;
                updatePlayButton();
            } catch (e) {
                console.error("Error calling WPFPlayer.play", e);
            }
        }
    }

    function togglePlayPause() {
        if (!WPFPlayer) return;

        if (queue.length === 0) return;

        if (currentIndex === -1) {
            loadSong(0, true);
            return;
        }

        if (isPlaying) {
            try {
                WPFPlayer.pause();
                isPlaying = false;
                updatePlayButton();
            } catch (e) {
                console.error("Error pause", e);
            }
        } else {
            try {
                WPFPlayer.play();
                isPlaying = true;
                updatePlayButton();
            } catch (e) {
                console.error("Error play", e);
            }
        }
    }

    function playPrev() {
        if (queue.length === 0) return;
        if (currentIndex <= 0) {
            if (isRepeat && queue.length > 0) {
                loadSong(queue.length - 1, true);
            } else {
                loadSong(0, true);
            }
        } else {
            loadSong(currentIndex - 1, true);
        }
    }

    function playNext() {
        if (queue.length === 0) return;
        if (currentIndex < 0) {
            loadSong(0, true);
            return;
        }
        if (currentIndex >= queue.length - 1) {
            if (isRepeat) {
                loadSong(0, true);
            }
        } else {
            loadSong(currentIndex + 1, true);
        }
    }

    function applyVolumeFromSlider() {
        if (!volSlider || !WPFPlayer) return;
        const v = parseFloat(volSlider.value);
        if (!isNaN(v)) {
            try {
                WPFPlayer.setVolume(v);
            } catch (e) {
                console.error("Error setVolume", e);
            }
        }
    }

    function wireEvents() {
        if (heroPlayBtn) {
            heroPlayBtn.addEventListener("click", () => {
                if (queue.length === 0) return;
                if (currentIndex === -1) {
                    loadSong(0, true);
                } else {
                    togglePlayPause();
                }
            });
        }

        if (footerPlayBtn) {
            footerPlayBtn.addEventListener("click", togglePlayPause);
        }
        if (prevBtn) prevBtn.addEventListener("click", playPrev);
        if (nextBtn) nextBtn.addEventListener("click", playNext);

        if (repeatBtn) {
            repeatBtn.addEventListener("click", () => {
                isRepeat = !isRepeat;
                repeatBtn.classList.toggle("active", isRepeat);
            });
        }

        if (seekSlider) {
            seekSlider.addEventListener("input", () => {
                seekDragging = true;
            });
            seekSlider.addEventListener("change", () => {
                seekDragging = false;
                const sec = parseFloat(seekSlider.value);
                if (!WPFPlayer || isNaN(sec)) return;
                try {
                    WPFPlayer.seek(sec);
                } catch (e) {
                    console.error("Error seek", e);
                }
            });
        }

        if (volSlider) {
            volSlider.addEventListener("input", applyVolumeFromSlider);
            applyVolumeFromSlider();
        }

        const main = qs('.content-scroll');
        if (main) {
            main.addEventListener('click', (e) => {
                const row = e.target.closest('.trow[data-index]');
                if (!row) return;
                const idx = Number(row.dataset.index || '0');
                loadSong(idx, true);
            });
        }

        if (sideContainer) {
            sideContainer.addEventListener('click', (e) => {
                const item = e.target.closest('.pl-item[data-index]');
                if (!item) return;
                const idx = Number(item.dataset.index || '0');
                loadSong(idx, true);
            });
        }

        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.addEventListener('message', (event) => {
                let data;
                try {
                    data = JSON.parse(event.data);
                } catch {
                    return;
                }

                if (!data || data.source !== 'LocalAudioService') return;

                switch (data.type) {
                    case 'position':
                        updateSeekUI(data.payload.position, data.payload.duration);
                        break;
                    case 'loaded':
                        updateSeekUI(0, data.payload.duration);
                        break;
                    case 'playing':
                        isPlaying = true;
                        updatePlayButton();
                        break;
                    case 'paused':
                        isPlaying = false;
                        updatePlayButton();
                        break;
                    case 'stopped':
                        isPlaying = false;
                        updateSeekUI(0, currentDuration);
                        updatePlayButton();
                        break;
                    case 'ended':
                        isPlaying = false;
                        updatePlayButton();
                        handleEnded();
                        break;
                    case 'error':
                        console.error("Lỗi từ LocalAudioService:", data.payload.message);
                        alert("Lỗi phát nhạc: " + data.payload.message);
                        break;
                }
            });
        }
    }

    function renderHero(songs) {
        if (!heroTitle || !heroSub || !heroCover) return;
        if (songs.length === 0) {
            heroTitle.textContent = "Local Queue";
            heroSub.textContent = "No songs in queue";
            heroCover.src = "/assets/playlist-demo.jpg";
            return;
        }
        const first = songs[0];
        heroTitle.textContent = "Local Queue";
        heroSub.textContent = `${songs.length} songs`;
        heroCover.src = first.CoverArtUrl || first.coverArtUrl || "/assets/playlist-demo.jpg";
    }

    function renderMainQueue(songs) {
        if (!rowsContainer) return;
        rowsContainer.innerHTML = "";

        if (!songs.length) {
            const empty = document.createElement('div');
            empty.className = 'trow';
            empty.innerHTML = `
                <div></div>
                <div class="tcell-title">
                    <div></div>
                    <div>Không có bài hát nào trong queue. Hãy scan hoặc thêm nhạc ở Library.</div>
                </div>
                <div></div><div></div><div></div>
            `;
            rowsContainer.appendChild(empty);
            return;
        }

        songs.forEach((song, idx) => {
            const row = document.createElement('div');
            row.className = 'trow';
            row.dataset.index = String(idx);

            const title = song.Title || song.name || 'Unknown Title';
            const artist = song.Artist || song.artist || 'Unknown Artist';
            const album = song.Album || song.album || 'Local File';
            const durSec = song.Duration || song.durationSeconds || song.duration;
            const timeStr = fmtTime(durSec);
            const cover = song.CoverArtUrl || song.coverArtUrl || '/assets/playlist-demo.jpg';

            row.innerHTML = `
                <div>${idx + 1}</div>
                <div class="tcell-title">
                    <img class="tcover" src="${cover}" alt="">
                    <div>
                        <div style="font-weight:700;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">
                            ${title}
                        </div>
                        <div class="tmeta">${artist}</div>
                    </div>
                </div>
                <div style="white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${album}</div>
                <div></div>
                <div>${timeStr}</div>
            `;

            rowsContainer.appendChild(row);
        });
    }

    function renderSidebarQueue(songs) {
        if (!sideContainer) return;
        sideContainer.innerHTML = "";

        if (!songs.length) {
            sideContainer.innerHTML = '<div class="tmeta" style="padding:8px;">Queue trống.</div>';
            return;
        }

        songs.forEach((song, idx) => {
            const item = document.createElement('div');
            item.className = 'pl-item';
            item.dataset.index = String(idx);

            const title = song.Title || song.name || 'Unknown Title';
            const artist = song.Artist || song.artist || 'Unknown Artist';
            const cover = song.CoverArtUrl || song.coverArtUrl || '/assets/playlist-demo.jpg';

            item.innerHTML = `
                <img class="pl-cover" src="${cover}" alt="">
                <div>
                    <div style="font-weight:700;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">
                        ${title}
                    </div>
                    <div class="tmeta">${artist}</div>
                </div>
            `;

            sideContainer.appendChild(item);
        });
    }

    window.initAlbum = function () {
        queue = getQueue();

        heroPlayBtn = qs('#alb-hero-play');
        footerPlayBtn = qs('#alb-play-btn');
        prevBtn = qs('#alb-prev-btn');
        nextBtn = qs('#alb-next-btn');
        repeatBtn = qs('#alb-repeat-btn');
        seekSlider = qs('#alb-seek');
        curLabel = qs('#alb-cur');
        durLabel = qs('#alb-dur');
        volSlider = qs('#alb-volume');
        nowCover = qs('#alb-now-cover');
        nowTitle = qs('#alb-now-title');
        nowArtist = qs('#alb-now-artist');
        heroCover = qs('#queue-cover');
        heroTitle = qs('#queue-title');
        heroSub = qs('#queue-sub');
        rowsContainer = qs('#queue-rows');
        sideContainer = qs('#queue-sidebar');

        renderHero(queue);
        renderMainQueue(queue);
        renderSidebarQueue(queue);
        updateSeekUI(0, 0);

        wireEvents();
    };
})();
