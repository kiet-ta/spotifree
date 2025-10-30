// settings.js — bind-only, no-UI-change (with i18n + CSS zoom)
(() => {
    // ---------- tiny utils ----------
    const $ = (id) => document.getElementById(id);
    const qs = (sel) => document.querySelector(sel);
    const qsa = (sel) => Array.from(document.querySelectorAll(sel));
    const on = (el, ev, fn) => el && el.addEventListener(ev, fn, { passive: true });

    // ---------- i18n ----------
    const I18N = {
        "en-US": {
            "title.settings": "Settings",
            "section.account": "Account",
            "account.hint": "Edit login methods",
            "account.edit": "Edit",
            "section.language": "Language",
            "language.hint": "Choose language – Changes will be applied after restarting the app",
            "section.autoplay": "Autoplay",
            "autoplay.hint": "Enjoy nonstop listening. When your audio ends, we’ll play you something similar",
            "section.zoom": "Zoom level",
            "zoom.hint1": "Adjusting the zoom level can help you make the most of the app.",
            "zoom.hint2a": "You can also change it with",
            "zoom.hint2b": "or",
            "zoom.dense": "Dense",
            "zoom.default": "Default",
            "zoom.spacious": "Spacious",
            "zoom.reset": "Reset",
            "section.storage": "Storage",
            "storage.downloads": "Downloads",
            "storage.downloadsHint": "Content you have downloaded for offline use",
            "storage.removeAll": "Remove all downloads",
            "storage.location": "Offline storage location",
            "storage.change": "Change location",
            "nav.backHome": "Back to Home",
            "search.placeholder": "Search settings…"
        },
        "vi-VN": {
            "title.settings": "Cài đặt",
            "section.account": "Tài khoản",
            "account.hint": "Chỉnh phương thức đăng nhập",
            "account.edit": "Chỉnh sửa",
            "section.language": "Ngôn ngữ",
            "language.hint": "Chọn ngôn ngữ – Thay đổi sẽ áp dụng sau khi khởi động lại ứng dụng",
            "section.autoplay": "Tự động phát",
            "autoplay.hint": "Nghe liên tục. Khi âm thanh kết thúc, chúng tôi sẽ phát nội dung tương tự",
            "section.zoom": "Mức thu phóng",
            "zoom.hint1": "Điều chỉnh mức thu phóng giúp bạn tận dụng ứng dụng tốt hơn.",
            "zoom.hint2a": "Bạn cũng có thể đổi bằng",
            "zoom.hint2b": "hoặc",
            "zoom.dense": "Dày đặc",
            "zoom.default": "Mặc định",
            "zoom.spacious": "Thoáng",
            "zoom.reset": "Đặt lại",
            "section.storage": "Lưu trữ",
            "storage.downloads": "Tải xuống",
            "storage.downloadsHint": "Nội dung bạn đã tải để sử dụng ngoại tuyến",
            "storage.removeAll": "Xoá toàn bộ",
            "storage.location": "Thư mục lưu ngoại tuyến",
            "storage.change": "Đổi vị trí",
            "nav.backHome": "Về trang chủ",
            "search.placeholder": "Tìm trong cài đặt…"
        }
    };

    function t(lang, key) {
        return I18N[lang]?.[key] ?? I18N["en-US"][key] ?? "";
    }

    function applyI18n(lang) {
        const l = I18N[lang] ? lang : "en-US";

        // text nodes
        qsa("[data-i18n]").forEach(el => {
            const key = el.getAttribute("data-i18n");
            if (!key) return;
            const val = t(l, key);
            if (val) el.textContent = val;
        });

        // placeholders
        qsa("[data-i18n-placeholder]").forEach(el => {
            const key = el.getAttribute("data-i18n-placeholder");
            const val = t(l, key);
            if (val) el.setAttribute("placeholder", val);
        });
    }

    // ---------- Bridge: JS -> WPF ----------
    function send(action, data = {}) {
        try {
            window.chrome?.webview?.postMessage?.(JSON.stringify({ action, ...data }));
        } catch (e) {
            console.error("[Settings] postMessage fail:", e);
        }
    }

    // ---------- Apply zoom by CSS (only when user clicks) ----------
    function applyCssZoom(percent) {
        const pct = Math.max(50, Math.min(300, Number(percent) || 100));
        const host = document.getElementById("content-container") || document.body;
        host.style.zoom = pct + "%";

        const r = qs(`input[name="zoomRadio"][value="${pct}"]`);
        if (r) r.checked = true;

        // highlight 3 thumbnail bằng inline style (không đổi class)
        qsa(".s-tile .s-tile__img").forEach(img => img.style.boxShadow = "");
        if (pct <= 90) qs(".s-tile--dense")?.style && (qs(".s-tile--dense").style.boxShadow = "0 0 0 2px #fff inset");
        else if (pct === 100) qs(".s-tile--default")?.style && (qs(".s-tile--default").style.boxShadow = "0 0 0 2px #fff inset");
        else qs(".s-tile--spacious")?.style && (qs(".s-tile--spacious").style.boxShadow = "0 0 0 2px #fff inset");
    }

    // ---------- WPF -> JS ----------
    window.__fromWpf = (payload) => {
        try {
            const { action, data } = payload || {};
            if (action === "settings.current") {
                hydrate(data);                 // set trạng thái input
                applyI18n(data?.language || "en-US"); // dịch ngay theo state hiện tại
            } else if (action === "storage.folderPicked") {
                if (data?.ok) {
                    const p = $("storagePath");
                    if (p) p.textContent = data.path || "";
                }
            } else if (action === "storage.clearOffline.done") {
                const dl = $("storageDownloads");
                if (dl) dl.textContent = "—";
            } else if (action === "settings.error") {
                console.error("[Settings] backend error:", data?.message);
            }
        } catch (e) {
            console.error("[Settings] __fromWpf error:", e);
        }
    };

    // ---------- Fill UI từ AppSettings (NO visual change, chỉ value/checked) ----------
    function hydrate(s) {
        if (!s) return;
        const lang = $("language");
        if (lang) lang.value = s.language ?? "en-US";

        const ap = $("autoplay");
        if (ap) ap.checked = !!s.autoplay;

        const pct = Number(s.zoomPercent ?? 100);
        const r = qs(`input[name="zoomRadio"][value="${pct}"]`);
        if (r) r.checked = true;        // KHÔNG gọi applyCssZoom ở đây

        const pathEl = $("storagePath");
        if (pathEl) pathEl.textContent = s.offlineStoragePath || "";
    }

    // ---------- Wire events ----------
    function wire() {
        // Language
        on($("language"), "change", (e) => {
            const language = e.target.value || "en-US";
            applyI18n(language);                        // đổi text NGAY
            send("settings.setLanguage", { language }); // và lưu về WPF
        });

        // Autoplay
        on($("autoplay"), "change", (e) => {
            send("settings.setAutoplay", { enable: !!e.target.checked });
        });

        // Zoom radios
        qsa("input[name='zoomRadio']").forEach(r => {
            on(r, "change", (e) => {
                if (!e.target.checked) return;
                const percent = Number(e.target.value);
                applyCssZoom(percent);                   // phóng/thu ngay
                send("settings.applyZoom", { percent }); // lưu về WPF
            });
        });

        // Reset zoom
        on($("btnZoomReset"), "click", () => {
            applyCssZoom(100);
            send("settings.applyZoom", { percent: 100 });
        });

        // Storage
        on($("btnChangeLocation"), "click", () => send("storage.pickFolder"));
        on($("btnRemoveDownloads"), "click", () => {
            (async () => {  // clear cache phía front (best-effort)
                try {
                    if (window.caches) {
                        const keys = await caches.keys();
                        await Promise.all(keys.map(k => caches.delete(k)));
                    }
                } catch { }
            })();
            send("storage.clearOffline");
        });

        // Back Home (nếu có)
        on($("btnBackHome"), "click", () => {
            // gọi hàm có sẵn trong navigation
            if (typeof window.loadPage === "function") {
                window.loadPage("home");
            } else {
                // fallback phòng hờ (không bắt buộc)
                window.location.href = "/index.html#home";
            }
        });

        // Search (giữ nguyên bố cục, chỉ display: none)
        const search = $("settingsSearch");
        if (search) {
            const cards = qsa(".s-card");
            const keyOf = (el) => (el.getAttribute("data-search") || el.textContent || "").toLowerCase();
            on(search, "input", (e) => {
                const q = (e.target.value || "").trim().toLowerCase();
                cards.forEach(c => {
                    const show = !q || keyOf(c).includes(q);
                    c.style.display = show ? "" : "none";
                });
            });
        }
    }

    // ---------- Init ----------
    function init() {
        wire();
        send("settings.get"); // lấy state hiện tại -> __fromWpf sẽ hydrate + applyI18n
    }

    // Expose cho navigation.js
    window.initSettings = init;

    // Tự phát hiện khi settings.html đã được load
    const startIfSettingsVisible = () => {
        if (document.querySelector(".settings")) init();
    };
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", startIfSettingsVisible, { once: true });
    } else {
        startIfSettingsVisible();
    }
})();
