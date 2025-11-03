function initMusicDetail() {
    // DOM elements
    const playBtn = document.querySelector(".play-btn");
    const prevBtn = document.querySelector(".prev-btn");
    const nextBtn = document.querySelector(".next-btn");
    const shuffleBtn = document.querySelector(".shuffle-btn");
    const repeatBtn = document.querySelector(".repeat-btn");
    const progressSlider = document.querySelector(".progress-slider");
    const progressFill = document.querySelector(".progress-fill");
    const currentTimeEl = document.querySelector(".current-time");
    const durationTimeEl = document.querySelector(".duration-time");
    const volumeSlider = document.querySelector(".volume-slider");
    const songTitleEl = document.querySelector(".song-title");
    const songArtistEl = document.querySelector(".song-artist");
    const songThumbnailEl = document.querySelector(".song-thumbnail");

    let csharpPlayer;

    let localDuration = 1;
    let isPlaying = false;
    let isDraggingSlider = false;

    // Format time helper
    function formatTime(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, "0")}`;
    }

    // --- control (JS call C#) ---

    async function onPlayPauseClick() {
        if (!csharpPlayer) {
            console.error("csharpPlayer is still null.");
            alert("Lỗi: Đối tượng C# player chưa sẵn sàng.");
            return;
        }

        try {
            // Lỗi của bạn xảy ra ở đây
            const state = await csharpPlayer.getState();

            if (state.state === "Idle" || state.state === "Stopped" || state.state === "Ended") {
                csharpPlayer.browseAndLoad();
            } else if (state.state === "Paused") {
                csharpPlayer.play();
            } else if (state.state === "Playing") {
                csharpPlayer.pause();
            }
        } catch (e) {
            console.error("Play/Pause error:", e);
        }
    }

    function onProgressSliderChange(e) {
        if (!csharpPlayer) return;
        const newTime = (e.target.value / 100) * localDuration;
        csharpPlayer.seek(newTime);
    }

    function onVolumeSliderInput(e) {
        if (!csharpPlayer) return;
        csharpPlayer.setVolume(e.target.value / 100.0);
    }

    // Listen (C# send to JS) 
    function handleBridgeMessage(event) {
        const msg = JSON.parse(event.data);
        if (msg.source !== "LocalAudioService") return;

        console.log("[C# -> JS]", msg.type, msg.payload);

        if (msg.type === "loaded" || msg.type === "playing") {
            localDuration = msg.payload.duration || 1;
            durationTimeEl.textContent = formatTime(localDuration);
            const fileName = msg.payload.path.split('\\').pop().split('/').pop().replace(".mp3", "");
            songTitleEl.textContent = fileName;
            songTitleEl.setAttribute('title', fileName); // Thêm tooltip
            songArtistEl.textContent = "Local File";
            playBtn.textContent = "⏸️";
            isPlaying = true;
        }
        else if (msg.type === "paused") {
            playBtn.textContent = "▶️";
            isPlaying = false;
        }
        else if (msg.type === "ended" || msg.type === "stopped") {
            playBtn.textContent = "▶️";
            isPlaying = false;
            progressFill.style.width = "0%";
            progressSlider.value = 0;
            currentTimeEl.textContent = formatTime(0);
        }
        else if (msg.type === "position") {
            if (isPlaying && !isDraggingSlider) {
                const percentage = (msg.payload.position / (msg.payload.duration || 1)) * 100;
                progressFill.style.width = percentage + "%";
                progressSlider.value = percentage;
                currentTimeEl.textContent = formatTime(msg.payload.position);
            }
        }
        else if (msg.type === "error") {
            alert(`Lỗi phát nhạc: ${msg.payload.message}`);
        }
    }

    function waitForHostObject() {
        csharpPlayer = window.chrome?.webview?.hostObjects?.player;

        if (csharpPlayer) {
            console.log("PlayerBridge (C#) found! Initializing listeners...");

            // Chỉ gán listener SAU KHI csharpPlayer tồn tại
            playBtn.addEventListener("click", onPlayPauseClick);

            progressSlider.addEventListener('mousedown', () => { isDraggingSlider = true; });
            progressSlider.addEventListener('mouseup', () => { isDraggingSlider = false; });
            progressSlider.addEventListener('change', onProgressSliderChange);

            volumeSlider.addEventListener('input', onVolumeSliderInput);

            // Gán listener cho C# -> JS (Chỉ gán 1 lần ở đây)
            if (window.chrome?.webview) {
                window.chrome.webview.addEventListener('message', handleBridgeMessage);
            }
        } else {
            console.warn("PlayerBridge (C#) not found, retrying in 100ms...");
            setTimeout(waitForHostObject, 100);
        }
    }

    // Bắt đầu quá trình "thăm dò"
    waitForHostObject();
}