// storage usage & tools for UI
window.StorageUtil = (function () {
    const usageEl = () => document.getElementById('storageUsage');
    const quotaEl = () => document.getElementById('storageQuota');
    const keysViewer = () => document.getElementById('keysViewer');

    async function estimate() {
        if (navigator.storage?.estimate) {
            const { usage = 0, quota = 0 } = await navigator.storage.estimate();
            return { usage, quota };
        }
        // fallback: đếm chuỗi localStorage
        let total = 0;
        for (let i = 0; i < localStorage.length; i++) {
            const k = localStorage.key(i);
            const v = localStorage.getItem(k);
            total += (k?.length || 0) + (v?.length || 0);
        }
        return { usage: total, quota: 5 * 1024 * 1024 };
    }
    const fmtBytes = (x) => { if (!x) return '0 B'; const u = ['B', 'KB', 'MB', 'GB']; let i = 0; while (x >= 1024 && i < u.length - 1) { x /= 1024; i++; } return x.toFixed(1) + ' ' + u[i]; };

    async function refreshUsageUI() {
        const { usage, quota } = await estimate();
        usageEl().textContent = `${fmtBytes(usage)} used`;
        quotaEl().textContent = `Quota ~ ${fmtBytes(quota)}`;
    }

    function viewKeys() {
        const out = [];
        for (let i = 0; i < localStorage.length; i++) {
            const k = localStorage.key(i);
            const v = localStorage.getItem(k);
            out.push(`• ${k} = ${String(v).slice(0, 140)}${v && v.length > 140 ? '…' : ''}`);
        }
        keysViewer().textContent = out.join('\n');
        keysViewer().classList.remove('hidden');
    }

    function clearAll() {
        localStorage.clear();
        refreshUsageUI();
        keysViewer().classList.add('hidden');
        alert('Storage cleared');
    }

    // wire buttons
    window.addEventListener('click', (e) => {
        const t = e.target; if (!(t instanceof HTMLElement)) return;
        if (t.id === 'btnViewKeys') { viewKeys(); }
        if (t.id === 'btnClearStorage') { clearAll(); }
    }, true);

    return { refreshUsageUI };
})();
