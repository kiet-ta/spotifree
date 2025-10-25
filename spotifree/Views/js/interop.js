// WebView2 ↔ WPF host interop (no-op nếu host chưa cắm)
window.Interop = {
    setZoom: (percent) => {
        try { window.chrome?.webview?.postMessage?.({ type: 'setZoom', value: percent }); } catch { }
    },
    setLaunchOnLogin: (flag) => {
        try { window.chrome?.webview?.postMessage?.({ type: 'setLaunchOnLogin', value: !!flag }); } catch { }
    },
    setStartMinimized: (flag) => {
        try { window.chrome?.webview?.postMessage?.({ type: 'setStartMinimized', value: !!flag }); } catch { }
    },
    setCloseToTray: (flag) => {
        try { window.chrome?.webview?.postMessage?.({ type: 'setCloseToTray', value: !!flag }); } catch { }
    }
};
