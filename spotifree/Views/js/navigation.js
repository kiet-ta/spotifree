// js/navigation.js
async function loadPage(pageName) {
    try {
        const url = `/pages/${pageName}.html`;
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Page not found: ${url}`);
        }

        const htmlContent = await response.text();
        document.getElementById('content-container').innerHTML = htmlContent;

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
            else if (p === 'music_detail') {
                if (typeof initMusicDetailPage === 'function') {
                    initMusicDetailPage();
                } else {
                    console.error('initMusicDetailPage() not found. Did you include js/music_detail.js in index.html?');
                }
            }
            else if (p === 'album') {
                if (typeof window.initAlbum === 'function') {
                    window.initAlbum();
                } else {
                    console.error('initAlbum() not found. Did you include js/album.js?');
                }
            }
        });

    } catch (error) {
        console.error("Error loading page:", error);
        document.getElementById('content-container').innerHTML = "<h1>Error!</h1>";
    }
}

// onload
window.onload = () => {
    loadPage('home');
};
