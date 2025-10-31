let seekDragging = false;
const $ = sel => document.querySelector(sel);
const img = $("#img"), titleEl = $("#title"), artistEl = $("#artist");
const seek = $("#seek"), btnClose = $("#btnClose"), dragArea = $("#dragArea");

let duration = 1;

window.chrome?.webview?.addEventListener?.("message", (e) => {
    const msg = e.data || {};
    if (msg.type === "playerState") {
        // dùng y nguyên state từ web chính
        const cover = msg.cover || "";
        if (cover) img.src = cover;
        titleEl.textContent = msg.title || "";
        artistEl.textContent = msg.artist || "";
        duration = Math.max(1, Number(msg.duration || 0));
        seek.max = duration;
        const cur = Number(msg.currentTime || 0);
        if (!seekDragging) seek.value = cur;
    }
});

seek.addEventListener("input", () => { seekDragging = true; });
seek.addEventListener("change", () => {
    seekDragging = false;
    const seconds = Number(seek.value || 0);
    window.chrome?.webview?.postMessage({ type: "seek", seconds });
});

btnClose.addEventListener("click", () => {
    window.chrome?.webview?.postMessage({ type: "close" });
});

dragArea.addEventListener("mousedown", () => {
    window.chrome?.webview?.postMessage({ type: "dragWindow" });
});
