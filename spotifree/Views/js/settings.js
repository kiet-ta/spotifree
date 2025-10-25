// Spotifree Settings — UI-first (LocalStorage) + WebView2 interop hooks
(() => {
    const LS_KEY = "spotifree.settings";
    const $ = (id) => document.getElementById(id);
    const on = (el, ev, fn) => el && el.addEventListener(ev, fn, { passive: true });

    const DEFAULTS = Object.freeze({
        language: "en-US",
        autoplay: true,
        launchOnLogin: false,
        startMinimized: false,
        closeToTray: false,
        startupMode: "off",   // off | normal | minimized (đồng bộ 2 cờ trên)
        zoomPercent: 100
    });

    const State = {
        load() {
            try { const raw = localStorage.getItem(LS_KEY); return raw ? { ...DEFAULTS, ...JSON.parse(raw) } : { ...DEFAULTS }; }
            catch { return { ...DEFAULTS }; }
        },
        save(s) { localStorage.setItem(LS_KEY, JSON.stringify(s)); }
    };

    const setZoom = (pct) => {
        document.documentElement.style.zoom = pct / 100;
        window.Interop?.setZoom?.(pct);
    };
    const fmtMB = (bytes) => {
        if (!bytes) return "0 MB";
        const mb = bytes / (1024 * 1024);
        return (mb < 1 ? mb.toFixed(2) : mb.toFixed(0)) + " MB";
    };

    // dropdown ↔ flags
    const applyStartupToFlags = (mode, s) => {
        if (mode === "off") { s.launchOnLogin = false; s.startMinimized = false; }
        else if (mode === "normal") { s.launchOnLogin = true; s.startMinimized = false; }
        else { s.launchOnLogin = true; s.startMinimized = true; }
        s.startupMode = mode;
        window.Interop?.setLaunchOnLogin?.(s.launchOnLogin);
        window.Interop?.setStartMinimized?.(s.startMinimized);
    };

    function init() {
        const s = State.load();

        // Account
        on($("btnEditAccount"), "click", () => {
            window.Interop?.openAccount?.() || alert("Open account settings (stub)");
        });

        // Language
        $("language").value = s.language;
        on($("language"), "change", e => { s.language = e.target.value; State.save(s); });

        // Autoplay
        $("autoplay").checked = !!s.autoplay;
        on($("autoplay"), "change", e => { s.autoplay = !!e.target.checked; State.save(s); });

        // Zoom radios + Reset
        document.querySelectorAll("input[name='zoomRadio']").forEach(r => {
            r.checked = (+r.value === +s.zoomPercent);
            on(r, "change", e => {
                if (!e.target.checked) return;
                s.zoomPercent = +e.target.value; setZoom(s.zoomPercent); State.save(s);
                // tile highlight
                document.querySelectorAll(".s-tile").forEach(t => t.classList.remove("s-tile--active"));
                if (s.zoomPercent <= 90) document.querySelector(".s-tile--dense").parentElement.classList.add("s-tile--active");
                else if (s.zoomPercent === 100) document.querySelector(".s-tile--default").parentElement.classList.add("s-tile--active");
                else document.querySelector(".s-tile--spacious").parentElement.classList.add("s-tile--active");
            });
        });
        on($("btnZoomReset"), "click", () => {
            s.zoomPercent = 100; setZoom(100); State.save(s);
            document.querySelector("input[name='zoomRadio'][value='100']").checked = true;
            document.querySelectorAll(".s-tile").forEach(t => t.classList.remove("s-tile--active"));
            document.querySelector(".s-tile--default").parentElement.classList.add("s-tile--active");
        });
        setZoom(s.zoomPercent);

        // Startup & window
        $("startupMode").value = s.startupMode || (s.launchOnLogin ? (s.startMinimized ? "minimized" : "normal") : "off");
        on($("startupMode"), "change", e => { applyStartupToFlags(e.target.value, s); State.save(s); });

        $("closeToTray").checked = !!s.closeToTray;
        on($("closeToTray"), "change", e => { s.closeToTray = !!e.target.checked; window.Interop?.setCloseToTray?.(s.closeToTray); State.save(s); });

        // Storage (estimate Cache)
        if (navigator.storage?.estimate) {
            navigator.storage.estimate().then(({ usage = 0 }) => { $("storageCache").textContent = fmtMB(usage); });
        }
        $("storageDownloads").textContent = "0 MB"; // demo
        on($("btnClearCache"), "click", () => {
            localStorage.clear();
            $("storageCache").textContent = "0 MB";
            alert("Cache cleared");
        });
        on($("btnChangeLocation"), "click", () => {
            window.Interop?.changeStorageLocation?.() || alert("Change location (stub)");
        });
    }

    window.PageInits = window.PageInits || {};
    window.PageInits.settings = init;
})();
