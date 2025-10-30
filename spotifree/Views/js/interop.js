// js/interop.js
(() => {
    try {
        window.chrome?.webview?.addEventListener("message", (e) => {
            if (typeof window.__fromWpf === "function") {
                window.__fromWpf(e?.data);
            }
        });
    } catch { }
})();
