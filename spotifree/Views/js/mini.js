// /js/mini.js
(function () {
    const qs = (s, r = document) => (r || document).querySelector(s);

    const nowCover = qs("#m-now-cover");
    const nowTitle = qs("#m-now-title");
    const nowArtist = qs("#m-now-artist");

    const playBtn = qs("#m-play-btn");
    const prevBtn = qs("#m-prev-btn");
    const nextBtn = qs("#m-next-btn");
    const repeatBtn = qs("#m-repeat-btn");

    const seek = qs("#m-seek");
    const curLabel = qs("#m-cur");
    const durLabel = qs("#m-dur");

    const backBtn = qs("#m-back-btn");
    const closeBtn = qs("#m-close-btn");
    const dragArea = qs("#dragArea");

    let isPlaying = false;
    let isRepeat = false;
    let seekDragging = false;
    let curPos = 0;
    let curDur = 0;

    function fmtTime(sec) {
        if (!sec || isNaN(sec)) return "0:00";
        sec = Math.floor(sec);
        const m = Math.floor(sec / 60);
        const s = sec % 60;
        return m + ":" + String(s).padStart(2, "0");
    }

    function send(msg) {
        try {
            if (window.chrome && window.chrome.webview) {
                // GỬI OBJECT
                window.chrome.webview.postMessage(msg);
            }
        } catch (e) {
            console.error("postMessage error", e);
        }
    }


    function updatePlayButton() {
        const use = playBtn?.querySelector("use");
        if (use) {
            use.setAttribute("href", isPlaying ? "#i-pause" : "#i-play");
        }
        if (playBtn) {
            playBtn.title = isPlaying ? "Pause" : "Play";
        }
    }

    function updateRepeatButton() {
        if (!repeatBtn) return;
        repeatBtn.classList.toggle("active", isRepeat);
    }

    function updateSeekUI(pos, dur) {
        curPos = pos || 0;
        curDur = dur || 0;
        if (!Number.isFinite(curDur) || curDur <= 0) curDur = 1;

        if (seek) {
            seek.max = curDur;
            if (!seekDragging) {
                seek.value = curPos;
            }
        }
        if (curLabel) curLabel.textContent = fmtTime(curPos);
        if (durLabel) durLabel.textContent = fmtTime(curDur);
    }

    // ===== DOM events =====
    if (playBtn) {
        playBtn.addEventListener("click", () => {
            send({ type: "playPause" });
        });
    }

    if (prevBtn) {
        prevBtn.addEventListener("click", () => {
            send({ type: "prev" });
        });
    }

    if (nextBtn) {
        nextBtn.addEventListener("click", () => {
            send({ type: "next" });
        });
    }

    if (repeatBtn) {
        repeatBtn.addEventListener("click", () => {
            send({ type: "toggleRepeat" });
        });
    }

    if (seek) {
        seek.addEventListener("input", () => {
            seekDragging = true;
            const v = parseFloat(seek.value);
            if (!isNaN(v) && curLabel) {
                curLabel.textContent = fmtTime(v);
            }
        });

        seek.addEventListener("change", () => {
            seekDragging = false;
            const v = parseFloat(seek.value);
            if (isNaN(v)) return;
            send({ type: "seek", seconds: v });
        });
    }

    if (backBtn) {
        backBtn.addEventListener("click", () => {
            send({ type: "backToMain" });
        });
    }

    if (closeBtn) {
        closeBtn.addEventListener("click", () => {
            send({ type: "close" });
        });
    }

    // Drag toàn bộ card để di chuyển window
    if (dragArea) {
        dragArea.addEventListener("mousedown", (e) => {
            const tag = (e.target && e.target.tagName || "").toLowerCase();
            if (tag === "button" || tag === "input") return;
            send({ type: "dragWindow" });
        });
    }

    // ===== Nhận state từ Spotifree (forward từ album.js) =====
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.addEventListener("message", (event) => {
            let data = event.data;

            // Nếu C# sau này dùng PostWebMessageAsString thì vẫn chơi được
            if (typeof data === "string") {
                try {
                    data = JSON.parse(data);
                } catch (err) {
                    console.error("[Mini] JSON.parse failed:", err, "raw:", event.data);
                    return;
                }
            }

            console.log("[Mini] got message:", data);

            if (!data || !data.type) return;

            switch (data.type) {
                case "playerState": {
                    const hasPayload = data.payload && typeof data.payload === "object";
                    const p = hasPayload ? data.payload : {};
                    const song = hasPayload ? p.song : null;

                    const title =
                        (song && (song.Title || song.name)) ||
                        data.title ||
                        "Unknown Title";

                    const artist =
                        (song && (song.Artist || song.artist)) ||
                        data.artist ||
                        "Unknown Artist";

                    const coverUrl =
                        p.miniCover ||
                        (song && (song.CoverArtUrl || song.coverArtUrl)) ||
                        data.cover ||
                        "/assets/playlist-demo.jpg";

                    const position = hasPayload ? (p.position || 0) : (data.currentTime || 0);
                    const duration = hasPayload ? (p.duration || 0) : (data.duration || 0);

                    if (nowTitle) nowTitle.textContent = title;
                    if (nowArtist) nowArtist.textContent = artist;
                    if (nowCover) nowCover.src = coverUrl;

                    isPlaying = hasPayload ? !!p.isPlaying : !!data.isPlaying;
                    isRepeat = hasPayload ? !!p.isRepeat : !!data.isRepeat;

                    updatePlayButton();
                    updateRepeatButton();
                    updateSeekUI(position, duration);
                    break;
                }
            }
        });
    }

    // Báo C# biết mini đã sẵn sàng
    send({ type: "miniReady" });

})();
