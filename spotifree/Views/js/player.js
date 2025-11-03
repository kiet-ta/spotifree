document.addEventListener("DOMContentLoaded", () => {
    const wpfPlayer = window.chrome?.webview?.hostObjects?.player;
    if (!wpfPlayer) {
        console.warn("C# PlayerBridge (hostObjects.player) not found. Local playback will not work.");
    }

    // Single source of state, now updated by C#
    const state = (window.nowPlaying = window.nowPlaying || {
        title: "Chưa phát",
        artist: "",
        cover: "",
        currentTime: 0,
        duration: 1
    });

    function sendStateToMiniWeb() {
        const webview = window.chrome && window.chrome.webview;
        if (!webview) return;

        // sent current state for miniweb
        webview.postMessage({
            type: "playerState",
            title: state.title,
            artist: state.artist,
            cover: state.cover,
            currentTime: state.currentTime,
            duration: state.duration
        });
    }



    // --- listening from C# (LocalAudioService) ---
    if (window.chrome?.webview) {
        window.chrome.webview.addEventListener('message', (event) => {
            try {
                const msg = JSON.parse(event.data);
                if (msg.source !== "LocalAudioService") return;

                // update commond state from C#
                if (msg.type === "loaded" || msg.type === "playing") {
                    const fileName = msg.payload.path.split('\\').pop().split('/').pop().replace(".mp3", "");
                    state.title = fileName;
                    state.artist = "Local File";
                    state.cover = "/assets/thoi-thanh-xuan.jpg"; 
                    state.duration = msg.payload.duration || 1;
                }
                else if (msg.type === "position") {
                    state.currentTime = msg.payload.position;
                    state.duration = msg.payload.duration || 1;
                }
                else if (msg.type === "ended" || msg.type === "stopped") {
                    state.currentTime = 0;
                }

                updatePlayerUI(msg.type);
                // After updating the state, send it to miniweb
                sendStateToMiniWeb();

            } catch (e) {
                // Ignore non-JSON messages
            }
        });
    }
    function updatePlayerUI(playerState) {
        const btnPlay = document.getElementById("player-play-btn");
        const btnPause = document.getElementById("player-pause-btn");
        if (!btnPlay || !btnPause) return; // Không tìm thấy nút thì thôi

        if (playerState === "playing" || playerState === "loaded") {
            btnPlay.classList.add("hidden");
            btnPause.classList.remove("hidden");
        } else { // "paused", "stopped", "ended"
            btnPlay.classList.remove("hidden");
            btnPause.classList.add("hidden");
        }
    }

    // ---Send state to Spotify (if applicable)---
    // This code is for Spotify music (keep the same)
    audio?.addEventListener("timeupdate", () => {
        state.title = "Spotify Track";
        state.artist = "Spotify Artist";
        state.currentTime = audio.currentTime || 0;
        state.duration = audio.duration || 1;
        sendStateToMiniWeb();
    });
    audio?.addEventListener("loadedmetadata", sendStateToMiniWeb);


    // --- control MiniWeb ---
    btnMini?.addEventListener("click", () => {
        window.chrome?.webview?.postMessage({ type: "openMini", track: state });
    });

    // Get seek command from MiniWeb
    window.chrome?.webview?.addEventListener?.("message", (e) => {
        const m = e.data || {};
        if (m.type === "seek" && Number.isFinite(m.seconds)) {
            // Send this seek command to C#
            if (window.chrome?.webview?.hostObjects?.player) {
                window.chrome.webview.hostObjects.player.seek(m.seconds);
            }
        }
    });

    // Shortcut Alt+M
    document.addEventListener("keydown", (e) => {
        if (e.altKey && e.key.toLowerCase() === "m") btnMini?.click();
    });
})();