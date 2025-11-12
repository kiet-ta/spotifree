async function loadPage(pageName) {
    try {
        const url = `/pages/${pageName}.html`;
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Page not found: ${url}`);
        }

        const htmlContent = await response.text();
        document.getElementById('content-container').innerHTML = htmlContent;

        // Gộp tất cả các lệnh gọi 'init' vào một nơi
        // và dùng 'queueMicrotask' cho tất cả để đảm bảo DOM sẵn sàng.
        const p = String(pageName || '').toLowerCase();

        setTimeout(() => {
            if (p === 'home') {
                if (typeof initHome === 'function') {
                    initHome();
                } else {
                    console.error("initHome function not found");
                }
            }
            else if (p === 'library') {
                if (typeof initLibrary === 'function') {
                    initLibrary();
                } else {
                    console.error("initLibrary function not found");
                }
            }
            else if (p === 'setting' || p === 'settings') {
                if (typeof window.initSettings === 'function') {
                    window.initSettings();
                } else {
                    console.error('initSettings() not found.');
                }
            }
            // --- PHẦN THÊM MỚI ---
            else if (p === 'music_detail') {
                if (typeof initMusicDetailPage === 'function') {
                    initMusicDetailPage(); // <-- Gọi hàm mới từ js/music_detail.js
                } else {
                    console.error('initMusicDetailPage() not found. Did you include js/music-detail.js in index.html?');
                }
            }
            // (Bạn có thể thêm 'else if' cho 'playlist', 'album' ở đây nếu cần)
        });

    } catch (error) {
        console.error("Error loading page:", error);
        document.getElementById('content-container').innerHTML = "<h1>Error!</h1>";
    }
}

// --- XÓA BỎ HÀM NÀY ---
/* Hàm này không còn cần thiết nữa vì bạn đã thay đổi
    nút 'Music Details' để gọi loadPage('music_detail').

function openMusicDetailWindow() {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage({ action: 'nav.openMusicDetail' });
    } else {
        console.warn('Không thể giao tiếp với C# host.');
    }
}
*/

// Giữ nguyên hàm onload
window.onload = () => {
    loadPage('home');
};