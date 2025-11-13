// js/library.js

// ==== GLOBAL LOCAL QUEUE DÙNG CHUNG GIỮA LIBRARY & ALBUM ====
window.__localQueue = window.__localQueue || [];

let contextMenu = null;
let currentPlaylistId = null;

window.appendLibrary = function (newTracks) {
    if (!newTracks || !Array.isArray(newTracks)) return;

    console.log(`[JS] Appending ${newTracks.length} new tracks to library.`);

    const grid = document.querySelector('.playlists-grid');
    if (grid && grid.innerHTML.includes("empty")) {
        grid.innerHTML = '';
    }

    newTracks.forEach(track => {
        renderPlaylistCard(track);
    });
};

function initLibrary() {
    console.log("initLibrary() is running!");
    const createBtn = document.getElementById('create-playlist-btn');
    const grid = document.querySelector('.playlists-grid');
    const homeBtn = document.getElementById('library-home-button');

    const scanLocalBtn = document.getElementById('scan-local-btn');
    if (scanLocalBtn) {
        scanLocalBtn.addEventListener('click', () => {
            console.log('Scan local library button clicked!');
            if (window.chrome && window.chrome.webview) {

                const grid = document.querySelector('.playlists-grid');
                if (grid) {
                    grid.innerHTML = '<p style="color: #ccc; grid-column: 1 / -1; text-align: center;">Scanning folder, please wait...</p>';
                }

                window.chrome.webview.postMessage({
                    action: 'scanLocalLibrary'
                });
            }
        });
    }

    const addLocalBtn = document.getElementById('add-local-music-btn');
    if (addLocalBtn) {
        addLocalBtn.addEventListener('click', () => {
            console.log('Add local music button clicked!');
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    action: 'local.addMusic'
                });
            } else {
                console.error("Cannot communicate with C# backend");
                alert("This feature is only available in the app.");
            }
        });
    }

    if (homeBtn) {
        homeBtn.addEventListener('click', () => {
            if (typeof window.loadPage === 'function') {
                window.loadPage('home');
            } else {
                console.error('loadPage function is not defined!');
            }
        });
    }

    if (createBtn) {
        createBtn.addEventListener('click', () => {
            console.log('Create playlist button clicked!');

            const playlistName = prompt("Name of the new playlist?", "My New Playlist");

            if (playlistName) {
                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: 'createPlaylist',
                        name: playlistName
                    });
                } else {
                    console.error("Cannot communicate with C# backend");
                }
            }
        });
    }

    if (grid) {
        grid.addEventListener('contextmenu', (e) => {
            const card = e.target.closest('.playlist-card');

            if (card) {
                e.preventDefault();
                currentPlaylistId = card.dataset.playlistId;
                showContextMenu(e.clientX, e.clientY);
            }
        });
    }

    document.addEventListener('click', () => {
        if (contextMenu) {
            contextMenu.classList.remove('active');
        }
    });

    createContextMenu();

    const searchInput = document.getElementById('library-search-input');
    const playlistGrid = document.querySelector('.playlists-grid');

    if (searchInput && playlistGrid) {
        searchInput.addEventListener('input', (e) => {
            const searchTerm = e.target.value.toLowerCase().trim();
            const cards = playlistGrid.querySelectorAll('.playlist-card');

            cards.forEach(card => {
                const nameElement = card.querySelector('.playlist-name');
                if (!nameElement) return;

                const playlistName = nameElement.textContent.toLowerCase();

                if (playlistName.includes(searchTerm)) {
                    card.style.display = '';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    }

    if (window.chrome && window.chrome.webview) {
        if (grid) {
            grid.innerHTML = '';
        }
        console.log("[JS] requesting backend get playlists and local library!");
        window.chrome.webview.postMessage({
            action: 'getLibraryPlaylists'
        });
        window.chrome.webview.postMessage({
            action: 'getLocalLibrary'
        });
    } else {
        console.error("Cannot communicate with C# backend to get playlists");
    }
}

function createContextMenu() {
    contextMenu = document.createElement('div');
    contextMenu.className = 'context-menu';
    contextMenu.innerHTML = `
        <div class="menu-item" id="menu-rename">Rename</div>
        <div class="menu-item" id="menu-delete">Delete</div>
    `;
    document.body.appendChild(contextMenu);

    document.getElementById('menu-rename').addEventListener('click', () => {
        if (currentPlaylistId) {
            handleRename(currentPlaylistId);
        }
        contextMenu.classList.remove('active');
    });

    document.getElementById('menu-delete').addEventListener('click', () => {
        if (currentPlaylistId) {
            handleDelete(currentPlaylistId);
        }
        contextMenu.classList.remove('active');
    });

}

function showContextMenu(x, y) {
    if (contextMenu) {
        contextMenu.style.left = `${x}px`;
        contextMenu.style.top = `${y}px`;
        contextMenu.classList.add('active');
    }
}

function handleRename(playlistId) {
    const card = document.querySelector(`.playlist-card[data-playlist-id="${playlistId}"]`);
    const nameElement = card.querySelector('.playlist-name');

    const oldName = nameElement.textContent;
    const newName = prompt("Rename playlist to:", oldName);

    if (newName && newName !== oldName) {
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage({
                action: 'renamePlaylist',
                id: playlistId,
                name: newName
            });

            nameElement.textContent = newName;
        }

    }
}
function handleDelete(playlistId) {
    if (window.chrome && window.chrome.webview) {
        const confirmed = confirm(`Are you sure you want to delete this playlist ?`);
        if (confirmed) {
            window.chrome.webview.postMessage({
                action: 'deletePlaylist',
                id: playlistId
            });
        }
    } else {
        console.error("Cannot communicate with C# backend");
    }
}

window.playlistDeletedSuccess = function (playlistId) {
    const card = document.querySelector(`.playlist-card[data-playlist-id="${playlistId}"]`);
    if (card) {
        card.remove();
        console.log(`[JS] Playlist ${playlistId} deleted successfully!`);
    } else {
        console.error(`[JS] Playlist ${playlistId} not found!`);
    }
}
window.showNotification = function (message, type = 'info') {
    console.log(`[${type}] ${message}`);
    alert(`[${type}] ${message}`);
}

window.populateLibrary = function (playlistsData) {

    let playlists;
    if (typeof playlistsData === 'string') {
        try {
            playlists = JSON.parse(playlistsData);
            console.log("[JS] Đã parse JSON string thành Array:", playlists);
        } catch (e) {
            console.error("[JS] LỖI PARSE JSON!", e, playlistsData);
            return;
        }
    } else {
        playlists = playlistsData;
    }

    const grid = document.querySelector('.playlists-grid');
    if (!grid) {
        return;
    }

    grid.innerHTML = '';
    window.__localQueue = [];

    if (!Array.isArray(playlists)) {
        console.error("[JS] ERROR: After processing, 'playlist' is still not an Array!", playlists);
        return;
    }
    if (playlists.length === 0) {
        console.log("[JS] Empty library.");
        grid.innerHTML = '<p style="color: #ccc; grid-column: 1 / -1;">Your library is empty.</p>';
        return;
    }

    console.log(`[JS] Start looping through the ${playlists.length} playlist to...`);
    playlists.forEach((playlist, index) => {
        try {
            console.log(`[JS] Rendering playlist #${index}: ${playlist ? playlist.name : 'null/undefined'}`);
            if (!playlist || typeof playlist.id === 'undefined' || typeof playlist.name === 'undefined') {
                console.warn(`[JS] Playlist #${index} Missing ID or Name, skip.`);
                return;
            }
            renderPlaylistCard(playlist);
        } catch (renderError) {
            console.error(`[JS] LỖI KHI RENDER playlist #${index}:`, renderError, playlist);
        }
    });

    console.log("[JS] Đã hoàn thành vòng lặp render.");
}

function renderPlaylistCard(playlist) {
    const grid = document.querySelector('.playlists-grid');
    if (!grid) return;

    const card = document.createElement('div');
    card.className = 'playlist-card';
    card.dataset.playlistId = playlist.id;

    let cardName = "";
    let cardType = "";

    if (playlist.type === 'Local Music') {
        cardName = playlist.name || playlist.Title || 'Unknown Title';
        cardType = playlist.Artist || playlist.artist || 'Unknown Artist';

        const normalized = {
            Id: playlist.id ?? playlist.Id ?? null,
            Title: playlist.Title || playlist.name || 'Unknown Title',
            Artist: playlist.Artist || playlist.artist || 'Unknown Artist',
            FilePath: playlist.FilePath || playlist.filePath || '',
            Duration: playlist.Duration || playlist.duration || 0,
            CoverArtUrl: playlist.CoverArtUrl || playlist.coverArtUrl || null,
            Album: playlist.Album || playlist.album || 'Local File',
            type: 'Local Music'
        };

        if (normalized.FilePath) {
            const exists = window.__localQueue.some(s => s.FilePath === normalized.FilePath);
            if (!exists) window.__localQueue.push(normalized);
        }
    }
    else {
        cardName = playlist.name;
        cardType = playlist.type || 'Playlist';
    }

    card.innerHTML = `
        <div class="playlist-image">
          <svg viewBox="0 0 100 100"> 
            <circle cx="50" cy="40" r="15" />
            <path d="M 65 40 V 80 H 75" />
            <path d="M 35 40 V 80 H 25" />
            <rect x="10" y="10" width="80" height="80" rx="8" />
          </svg>
        </div>
        <div class="playlist-info">
          <div class="playlist-name">${cardName}</div>
          <div class="playlist-type">${cardType}</div> 
        </div>
    `;

    card.addEventListener('click', () => {
        handleLibraryPlay(playlist);
    });

    grid.appendChild(card);
}

window.addNewPlaylistCard = function (playlist) {
    console.log("[JS] addNewPlaylistCard was calling by C# backend!", playlist);
    renderPlaylistCard(playlist);
    console.log("[JS] card already in UI!");
}

function handleLibraryPlay(songData) {
    console.log(`[JS] Yêu cầu phát: ${songData.name}`);

    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage({
            action: 'playSongFromJsLibrary',
            song: songData
        });

        if (typeof window.loadPage === 'function') {
            window.loadPage('music_detail');
        }
    } else {
        console.error("Cannot communicate with C# backend to play song");
    }
}

window.handleLocalMusicAdded = (data) => {
    if (data && data.length > 0) {
        console.log("[JS] Files added from C#. Sending to C# to save persistence...", data);
        alert(`Added ${data.length} local files! Check console for paths.`);

        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage({
                action: 'local.addAndSaveTracks',
                filePaths: data
            });
        }
    } else {
        console.log("[JS] User cancelled adding local files.");
    }
};

window.initLibrary = initLibrary;
