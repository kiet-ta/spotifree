(() => {
    // central dispatcher
    window.__fromWpf = (payload) => {
        const { action, data } = payload || {};
        if (!action) return;

        // Ghi log để debug
        console.log(`[WPF->JS] Action: ${action}`, data);

        // 1. Gửi tin nhắn cho trang SETTINGS 
        if (action === "settings.current") {
            if (typeof window.handleSettingsCurrent === 'function') window.handleSettingsCurrent(data);
        }
        else if (action === "storage.folderPicked") {
            if (typeof window.handleStorageFolderPicked === 'function') window.handleStorageFolderPicked(data);
        }
        else if (action === "storage.clearOffline.done") {
            if (typeof window.handleClearOfflineDone === 'function') window.handleClearOfflineDone(data);
        }
        else if (action === "settings.error") {
            if (typeof window.handleSettingsError === 'function') window.handleSettingsError(data);
        }
        else if (action === "spotify.login.success") {
            if (typeof window.handleSpotifyLoginSuccess === 'function') window.handleSpotifyLoginSuccess(data);
        }
        else if (action === "spotify.login.failed") {
            if (typeof window.handleSpotifyLoginFailed === 'function') window.handleSpotifyLoginFailed(data);
        }

        // 2. Gửi tin nhắn cho trang LIBRARY (nếu hàm tồn tại)
        else if (action === "local.musicAdded") {
            if (typeof window.handleLocalMusicAdded === 'function') window.handleLocalMusicAdded(data);
        }
        else if (action === "populateLibrary") {
            if (typeof window.populateLibrary === 'function') window.populateLibrary(data);
        }
        else if (action === "addNewPlaylistCard") { 
            if (typeof window.addNewPlaylistCard === 'function') window.addNewPlaylistCard(data);
        }
        else if (action === "playlistDeletedSuccess") {
            if (typeof window.playlistDeletedSuccess === 'function') window.playlistDeletedSuccess(data);
        }
    };

    // Đăng ký listener 1 lần duy nhất
    try {
        window.chrome?.webview?.addEventListener("message", (e) => {
            if (typeof window.__fromWpf === "function") {
                // e.data đã là object (vì C# dùng JsNotifyAsync)
                window.__fromWpf(e?.data);
            }
        });
        console.log("Interop listener registered.");
    } catch (e) {
        console.error("Failed to register interop listener.", e);
    }
})();