// === Interop helper: gửi sang WPF ===
function postMsg(obj) {
    try { window.chrome?.webview?.postMessage?.(JSON.stringify(obj)); } catch { }
}

// === Gắn sự kiện cho các control ===
// 1) Account
document.getElementById("btnAccount")?.addEventListener("click", () =>
    postMsg({ type: "openAccount" })
);

// 2) Language (select#language)
document.getElementById("language")?.addEventListener("change", (e) =>
    postMsg({ type: "setLanguage", value: e.target.value })
);

// 3) Autoplay (checkbox#autoplay)
document.getElementById("autoplay")?.addEventListener("change", (e) =>
    postMsg({ type: "setAutoplay", value: e.target.checked })
);

// 4) Zoom (các nút/radio có data-zoom="70|80|...|130")
document.querySelectorAll("[data-zoom]").forEach((el) => {
    el.addEventListener("click", () => {
        const pct = parseInt(el.dataset.zoom, 10);
        if (!Number.isNaN(pct)) postMsg({ type: "setZoom", value: pct });
    });
});

// 5) Startup mode (select#startupMode: off|normal|minimized)
document.getElementById("startupMode")?.addEventListener("change", (e) =>
    postMsg({ type: "setStartupMode", value: e.target.value })
);

// 6) Close to tray (checkbox#closeToTray)
document.getElementById("closeToTray")?.addEventListener("change", (e) =>
    postMsg({ type: "setCloseToTray", value: e.target.checked })
);

// 7) Storage
document.getElementById("btnChangeLocation")?.addEventListener("click", () =>
    postMsg({ type: "changeStorageLocation" })
);
document.getElementById("btnClearCache")?.addEventListener("click", () =>
    postMsg({ type: "clearCache" })
);
document.getElementById("btnRemoveDownloads")?.addEventListener("click", () =>
    postMsg({ type: "removeAllDownloads" })
);
