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
                window.chrome.webview.postMessage(JSON.stringify(msg));
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

    // Drag window bằng cách nhấn giữ vùng root
    if (dragArea) {
        dragArea.addEventListener("mousedown", (e) => {
            // tránh khi click vào slider/nút
            const tag = (e.target && e.target.tagName || "").toLowerCase();
            if (tag === "button" || tag === "input") return;
            send({ type: "dragWindow" });
        });
    }

    // ===== Nhận state từ WPF (forward từ app chính) =====
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.addEventListener("message", (event) => {
            let data;
            try {
                data = JSON.parse(event.data);
            } catch {
                return;
            }

            if (!data) return;

            switch (data.type) {
                case "state": {
                    const p = data.payload || {};
                    if (p.title && nowTitle) nowTitle.textContent = p.title;
                    if (p.artist && nowArtist) nowArtist.textContent = p.artist;
                    if (p.coverUrl && nowCover) nowCover.src = p.coverUrl;

                    isPlaying = !!p.isPlaying;
                    isRepeat = !!p.isRepeat;

                    updatePlayButton();
                    updateRepeatButton();
                    updateSeekUI(p.position || 0, p.duration || 0);
                    break;
                }
            }
        });
    }

    // Báo cho WPF biết mini đã ready để WPF push state & queue
    send({ type: "miniReady" });
})();
