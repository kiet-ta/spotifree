// js/album.js
// Album page = Queue view + LocalAudioService player + Add Local (FLAC cover)

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
    let lastStateSentAt = 0;
    // playback engine
    let usingHtmlAudio = false;
    let htmlAudio = null;

    // DOM refs
    let heroPlayBtn;
    let footerPlayBtn, prevBtn, nextBtn, repeatBtn;
    let seekSlider, curLabel, durLabel;
    let volSlider;
    let nowCover, nowTitle, nowArtist;
    let heroCover, heroTitle, heroSub;
    let rowsContainer, sideContainer;
    let addLocalBtn, filePicker;

    function postToHost(msg) {
        if (!window.chrome || !window.chrome.webview) return;
        try {
            window.chrome.webview.postMessage(JSON.stringify(msg));
        } catch (e) {
            console.warn("postToHost error", e);
        }
    }

    function sendPlayerState(force = false) {
        const now = Date.now();
        if (!force && now - lastStateSentAt < 200) return; // throttle ~200ms
        lastStateSentAt = now;

        const song =
            currentIndex >= 0 && currentIndex < queue.length
                ? queue[currentIndex]
                : null;

        postToHost({
            type: "playerState",
            payload: {
                page: "album",
                isPlaying: !!isPlaying,
                isRepeat: !!isRepeat,
                position: currentPosition || 0,
                duration: currentDuration || 0,
                currentIndex,
                queueLength: queue.length,
                song: song
                    ? {
                        Id: song.Id,
                        Title: song.Title || song.name || "Unknown Title",
                        Artist: song.Artist || song.artist || "Unknown Artist",
                        Album: song.Album || song.album || "Local File",
                        CoverArtUrl: song.CoverArtUrl || song.coverArtUrl || "/assets/playlist-demo.jpg",
                        FilePath: song.FilePath || null,
                        FileUrl: song.FileUrl || null
                    }
                    : null
            }
        });
    }
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

    function setQueue(newQueue) {
        queue = newQueue.slice();
        window.__localQueue = queue.slice();
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
        sendPlayerState(false);
    }

    function updatePlayButton() {
        isPlaying = !!isPlaying;

        // Footer
        if (footerPlayBtn) {
            const use = footerPlayBtn.querySelector("use");
            if (use) {
                use.setAttribute("href", isPlaying ? "#i-pause" : "#i-play");
            }
            footerPlayBtn.title = isPlaying ? "Pause" : "Play";
        }

        // Hero chip
        if (heroPlayBtn) {
            const use = heroPlayBtn.querySelector("use");
            if (use) {
                use.setAttribute("href", isPlaying ? "#i-pause" : "#i-play");
            }
            const label = heroPlayBtn.querySelector("span");
            if (label) {
                label.textContent = isPlaying ? "Pause" : "Play";
            }
        }
    }


    function ensureHtmlAudio() {
        if (htmlAudio) return htmlAudio;

        htmlAudio = document.getElementById("alb-html-audio");
        if (!htmlAudio) {
            htmlAudio = document.createElement("audio");
            htmlAudio.id = "alb-html-audio";
            htmlAudio.style.display = "none";
            document.body.appendChild(htmlAudio);
        }

        htmlAudio.addEventListener("timeupdate", () => {
            if (!usingHtmlAudio) return;
            updateSeekUI(htmlAudio.currentTime || 0, htmlAudio.duration || currentDuration || 0);
        });

        htmlAudio.addEventListener("loadedmetadata", () => {
            if (!usingHtmlAudio) return;
            updateSeekUI(htmlAudio.currentTime || 0, htmlAudio.duration || 0);
        });

        htmlAudio.addEventListener("ended", () => {
            if (!usingHtmlAudio) return;
            isPlaying = false;
            updatePlayButton();
            handleEnded();
        });

        htmlAudio.addEventListener("play", () => {
            if (!usingHtmlAudio) return;
            isPlaying = true;
            updatePlayButton();
        });

        htmlAudio.addEventListener("pause", () => {
            if (!usingHtmlAudio) return;
            isPlaying = false;
            updatePlayButton();
        });

        htmlAudio.addEventListener("error", (e) => {
            if (!usingHtmlAudio) return;
            console.error("HTML audio error", e);
            alert("Lỗi phát nhạc (HTML): " + (htmlAudio.error?.message || "unknown"));
        });

        return htmlAudio;
    }

    // ===== STOP HELPERS: đảm bảo không bao giờ chạy 2 engine cùng lúc =====
    function stopWpf() {
        if (!WPFPlayer) return;
        try {
            if (typeof WPFPlayer.pause === "function") WPFPlayer.pause();
        } catch (e) {
            console.warn("WPF pause error", e);
        }
        try {
            if (typeof WPFPlayer.stop === "function") WPFPlayer.stop();
        } catch (e) {
            // có thể WPFPlayer không có stop, bỏ qua
        }
    }

    function stopHtml() {
        if (!htmlAudio) return;
        try {
            htmlAudio.pause();
        } catch (e) {
            console.warn("HTML pause error", e);
        }
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

    function playWithWPF(song, autoPlay) {
        if (!WPFPlayer) {
            console.error("WPFPlayer host object not found.");
            return;
        }
        if (!song || !song.FilePath) {
            console.error("Song missing FilePath", song);
            return;
        }

        // khi chuyển sang WPF, chắc chắn tắt HTML audio
        stopHtml();
        usingHtmlAudio = false;

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

    function playWithHtmlAudio(song, autoPlay) {
        const audio = ensureHtmlAudio();

        // khi chuyển sang HTML audio, chắc chắn tắt WPF
        stopWpf();
        usingHtmlAudio = true;

        const src = song.FileUrl || song.FilePath;
        if (!src) {
            console.error("No FileUrl/FilePath for HTML audio", song);
            return;
        }

        audio.src = src;
        audio.currentTime = 0;

        if (autoPlay) {
            audio.play().catch(err => {
                console.error("Error play HTML audio", err);
            });
        } else {
            updateSeekUI(0, audio.duration || 0);
        }
    }

    function loadSong(index, autoPlay) {
        if (index < 0 || index >= queue.length) return;

        currentIndex = index;
        const song = queue[index];
        if (!song) return;

        setNowUI(song);

        // Ưu tiên WPFPlayer cho bài có FilePath (nhạc từ Library/DB)
        if (WPFPlayer && song.FilePath && !song.IsLocalTemp) {
            playWithWPF(song, autoPlay);
        } else {
            // File thêm từ Add Local (Blob) hoặc không có WPFPlayer -> dùng HTML audio ẩn
            playWithHtmlAudio(song, autoPlay);
        }
        sendPlayerState(true);
    }

    function togglePlayPause() {
        if (queue.length === 0) return;

        if (currentIndex === -1) {
            loadSong(0, true);
            return;
        }

        if (usingHtmlAudio) {
            const audio = ensureHtmlAudio();
            // vẫn chắc chắn WPF đã tắt
            stopWpf();
            if (audio.paused) {
                audio.play().catch(e => console.error("Error play HTML audio", e));
                isPlaying = true;
            } else {
                audio.pause();
                isPlaying = false;
            }
            updatePlayButton();
            sendPlayerState(true);
            return;
        }

        // WPF mode: đảm bảo HTML audio đã pause
        stopHtml();
        if (!WPFPlayer) return;

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
        if (!volSlider) return;
        const v = parseFloat(volSlider.value);
        if (isNaN(v)) return;

        // Volume cho WPF
        if (WPFPlayer && !usingHtmlAudio) {
            try {
                WPFPlayer.setVolume(v);
            } catch (e) {
                console.error("Error setVolume WPF", e);
            }
        }

        // Volume cho HTML audio
        if (htmlAudio) {
            htmlAudio.volume = v;
        }
    }

    // ===== FLAC parsing (cover + basic tags) =====

    function guessMetaFromFilename(name) {
        const noExt = name.replace(/\.[^/.]+$/, "");
        const parts = noExt.split(" - ");
        if (parts.length >= 2) {
            return {
                artist: parts[0].trim(),
                title: parts.slice(1).join(" - ").trim()
            };
        }
        return { title: noExt.trim() };
    }

    async function parseFlacMeta(file) {
        const buffer = await file.arrayBuffer();
        const view = new DataView(buffer);
        const u8 = new Uint8Array(buffer);
        const dec = new TextDecoder("utf-8");

        // Check magic "fLaC"
        if (dec.decode(u8.subarray(0, 4)) !== "fLaC") {
            throw new Error("Not a FLAC file");
        }

        let offset = 4;
        let title, artist, album, coverUrl;

        let done = false;
        while (!done && offset + 4 <= view.byteLength) {
            const header = view.getUint8(offset);
            const isLast = (header & 0x80) !== 0;
            const type = header & 0x7F;
            const length = (view.getUint8(offset + 1) << 16) |
                (view.getUint8(offset + 2) << 8) |
                view.getUint8(offset + 3);
            offset += 4;

            const blockStart = offset;

            // VORBIS_COMMENT (type = 4)
            if (type === 4) {
                let p = offset;
                if (p + 4 > blockStart + length) {
                    offset += length;
                    if (isLast) done = true;
                    continue;
                }
                const vendorLen = view.getUint32(p, true); p += 4;
                p += vendorLen; // skip vendor
                if (p + 4 > blockStart + length) {
                    offset += length;
                    if (isLast) done = true;
                    continue;
                }
                const listLen = view.getUint32(p, true); p += 4;

                for (let i = 0; i < listLen && p + 4 <= blockStart + length; i++) {
                    const strLen = view.getUint32(p, true); p += 4;
                    if (p + strLen > blockStart + length) break;
                    const bytes = u8.subarray(p, p + strLen);
                    p += strLen;
                    const str = dec.decode(bytes);
                    const eqIdx = str.indexOf("=");
                    if (eqIdx <= 0) continue;
                    const key = str.slice(0, eqIdx).toUpperCase();
                    const val = str.slice(eqIdx + 1).trim();
                    if (!val) continue;
                    if (key === "TITLE") title = val;
                    else if (key === "ARTIST") artist = val;
                    else if (key === "ALBUM") album = val;
                }
            }
            // PICTURE (type = 6)
            else if (type === 6) {
                let p = offset;
                if (p + 4 * 4 > blockStart + length) {
                    offset += length;
                    if (isLast) done = true;
                    continue;
                }

                // picture type
                const pictureType = view.getUint32(p, false); p += 4;
                // MIME
                const mimeLen = view.getUint32(p, false); p += 4;
                const mimeBytes = u8.subarray(p, p + mimeLen); p += mimeLen;
                const mime = (dec.decode(mimeBytes) || "image/jpeg").trim() || "image/jpeg";
                // description
                const descLen = view.getUint32(p, false); p += 4;
                p += descLen;
                // skip width, height, depth, colors
                p += 4 * 4;
                const picDataLen = view.getUint32(p, false); p += 4;

                if (p + picDataLen <= blockStart + length) {
                    const picBytes = u8.subarray(p, p + picDataLen);
                    const blob = new Blob([picBytes], { type: mime });
                    coverUrl = URL.createObjectURL(blob);
                }
            }

            offset += length;
            if (isLast) done = true;
        }

        return { title, artist, album, coverUrl };
    }

    async function buildSongFromFile(file) {
        const ext = (file.name.split(".").pop() || "").toLowerCase();
        const guess = guessMetaFromFilename(file.name);

        let meta = {};
        if (ext === "flac") {
            try {
                meta = await parseFlacMeta(file);
            } catch (e) {
                console.warn("FLAC parse failed, fallback to filename", e);
            }
        }

        const title = meta.title || guess.title || file.name;
        const artist = meta.artist || guess.artist || "Unknown Artist";
        const album = meta.album || "Local File";
        const coverUrl = meta.coverUrl || "/assets/playlist-demo.jpg";

        const blobUrl = URL.createObjectURL(file);

        return {
            Id: "temp-" + Date.now() + "-" + Math.random().toString(16).slice(2),
            Title: title,
            Artist: artist,
            Album: album,
            Duration: 0, // có thể update sau nếu muốn đo bằng <audio>
            CoverArtUrl: coverUrl,
            FileUrl: blobUrl,
            FileName: file.name,
            IsLocalTemp: true // đánh dấu là nhạc add từ file input
        };
    }

    async function handleAddLocalFiles(files) {
        if (!files || !files.length) return;

        const list = Array.from(files);
        const newSongs = [];
        for (const f of list) {
            try {
                const song = await buildSongFromFile(f);
                newSongs.push(song);
            } catch (e) {
                console.error("Failed to build song from file", f.name, e);
            }
        }

        if (!newSongs.length) return;

        const merged = queue.concat(newSongs);
        setQueue(merged);

        renderHero(queue);
        renderMainQueue(queue);
        renderSidebarQueue(queue);

        if (currentIndex === -1 && queue.length > 0) {
            setNowUI(queue[0]);
        }
    }

    // ===== UI render =====

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

    // ===== Event wiring =====

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
                if (isNaN(sec)) return;

                if (usingHtmlAudio) {
                    const audio = ensureHtmlAudio();
                    audio.currentTime = sec;
                    return;
                }

                if (!WPFPlayer) return;
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

        // Add Local
        if (addLocalBtn && filePicker) {
            addLocalBtn.addEventListener("click", () => {
                filePicker.click();
            });

            filePicker.addEventListener("change", async () => {
                const files = filePicker.files;
                await handleAddLocalFiles(files);
                // reset để lần sau chọn lại cùng file vẫn trigger change
                filePicker.value = "";
            });
        }

        // LocalAudioService -> chỉ dùng khi đang chơi bằng WPF
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.addEventListener('message', (event) => {
                // Nếu đang dùng HTML audio (Add Local) thì bỏ qua message từ WPF
                if (usingHtmlAudio) return;

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
        addLocalBtn = qs('#alb-addlocal-btn');
        filePicker = qs('#alb-filepicker');

        renderHero(queue);
        renderMainQueue(queue);
        renderSidebarQueue(queue);
        updateSeekUI(0, 0);
        updatePlayButton();
        wireEvents();
    };
})();
