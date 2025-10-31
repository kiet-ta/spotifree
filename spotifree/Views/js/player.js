(() => {
    const audio = document.getElementById("music-player");
    const btnMini = document.getElementById("btn-mini");

    // Dùng một nguồn state duy nhất
    const state = (window.nowPlaying = window.nowPlaying || {
        title: "Thời thanh xuân",
        artist: "",
        cover: "/assets/thoi-thanh-xuan.jpg"
    });

    function sendState() {
        const webview = window.chrome && window.chrome.webview;
        if (!webview) return;
        webview.postMessage({
            type: "playerState",
            title: state.title,
            artist: state.artist,
            cover: state.cover,
            currentTime: audio ? (audio.currentTime || 0) : 0,
            duration: audio && Number.isFinite(audio.duration) ? audio.duration : 0
        });
    }

    audio?.addEventListener("timeupdate", sendState);
    audio?.addEventListener("loadedmetadata", sendState);

    // Nút mở Mini (1 nơi duy nhất)
    btnMini?.addEventListener("click", () => {
        window.chrome?.webview?.postMessage({ type: "openMini", track: state });
    });

    // Nhận lệnh seek từ host
    window.chrome?.webview?.addEventListener?.("message", (e) => {
        const m = e.data || {};
        if (m.type === "seek" && Number.isFinite(m.seconds) && audio) {
            audio.currentTime = m.seconds;
        }
    });

    // Shortcut Alt+M
    document.addEventListener("keydown", (e) => {
        if (e.altKey && e.key.toLowerCase() === "m") btnMini?.click();
    });
    // Double click để về main
    document.querySelector(".cover")?.addEventListener("dblclick", () => {
        window.chrome?.webview?.postMessage({ type: "backToMain" });
    });

})();
