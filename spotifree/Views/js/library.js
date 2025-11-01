let contextMenu = null;
let currentPlaylistId = null;

function initLibrary() {
    console.log("initLibrary() is running!");
    const createBtn = document.getElementById('create-playlist-btn');
    const scanLocalBtn = document.getElementById('scan-local-btn');
    const grid = document.querySelector('.playlists-grid');

    const homeBtn = document.getElementById('library-home-button');
    if (homeBtn) {
        homeBtn.addEventListener('click', () => {
            if (typeof window.loadPage === 'function') {
                window.loadPage('home'); // Gọi lại hàm navigation
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

    if (scanLocalBtn) {
        scanLocalBtn.addEventListener('click', () => {
            console.log('Scan Local Music button clicked!');
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    action: 'scanLocalLibrary'
                });
            } else {
                console.error("Cannot communicate with C# backend");
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

                // playlist contain the keyword, it will show
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
        // request backend to get playlists and local library
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

window.populateLibrary = function(playlistsData) {

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

    card.innerHTML = `
        <div class="playlist-image">
          <svg viewBox="0 0 100 100"> 
            <circle cx="25" cy="25" r="8" />
            <path d="M 20 80 L 50 40 L 80 70" />
            <rect x="10" y="10" width="80" height="80" rx="8" />
          </svg>
        </div>
        <div class="playlist-info">
          <div class="playlist-name">${playlist.name}</div>
          <div class="playlist-type">${playlist.type || 'Playlist'}</div> 
        </div>
    `;

    grid.appendChild(card);
}

window.addNewPlaylistCard = function (playlist) {
    console.log("[JS] addNewPlaylistCard was calling by C# backend!", playlist);
    renderPlaylistCard(playlist);
    console.log("[JS] card already in UI!");
}

// Function to populate local library (called from C#)
window.populateLocalLibrary = function(localTracksData) {
    console.log("[JS] populateLocalLibrary called with:", localTracksData);

    let localTracks;
    if (typeof localTracksData === 'string') {
        try {
            localTracks = JSON.parse(localTracksData);
        } catch (e) {
            console.error("[JS] LỖI PARSE JSON cho local library!", e);
            return;
        }
    } else {
        localTracks = localTracksData;
    }

    const grid = document.querySelector('.playlists-grid');
    if (!grid) {
        console.error("[JS] Cannot find .playlists-grid");
        return;
    }

    if (!Array.isArray(localTracks)) {
        console.error("[JS] localTracks is not an array:", localTracks);
        return;
    }

    if (localTracks.length === 0) {
        console.log("[JS] Local library is empty.");
        return;
    }

    console.log(`[JS] Rendering ${localTracks.length} local tracks...`);
    localTracks.forEach((track, index) => {
        try {
            if (!track || !track.id || !track.name) {
                console.warn(`[JS] Local track #${index} missing required fields, skip.`);
                return;
            }
            renderLocalMusicCard(track);
        } catch (renderError) {
            console.error(`[JS] LỖI KHI RENDER local track #${index}:`, renderError, track);
        }
    });
}

function renderLocalMusicCard(track) {
    const grid = document.querySelector('.playlists-grid');
    if (!grid) return;

    const card = document.createElement('div');
    card.className = 'playlist-card local-music-card';
    card.dataset.playlistId = track.id;
    card.dataset.filePath = track.filePath || '';
    card.style.cursor = 'pointer';

    // Create a visual indicator for local music
    const artistDisplay = track.artist ? ` • ${track.artist}` : '';
    const albumDisplay = track.album ? ` • ${track.album}` : '';
    
    card.innerHTML = `
        <div class="playlist-image" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);">
          <svg viewBox="0 0 100 100" style="fill: white; opacity: 0.8;"> 
            <circle cx="50" cy="30" r="8" />
            <path d="M 30 70 L 50 50 L 70 70 L 50 50 L 30 70" stroke="white" stroke-width="3" fill="none"/>
          </svg>
        </div>
        <div class="playlist-info">
          <div class="playlist-name">${escapeHtml(track.name)}</div>
          <div class="playlist-type">${escapeHtml(track.type || 'Local Music')}${escapeHtml(artistDisplay)}${escapeHtml(albumDisplay)}</div> 
        </div>
    `;

    // Add click handler to play local music
    card.addEventListener('click', (e) => {
        if (!e.target.closest('.context-menu')) {
            console.log(`[JS] Clicked local track: ${track.name}`, track.filePath);
            // TODO: Implement play local music functionality
            if (window.playLocalMusic) {
                window.playLocalMusic(track.filePath, track);
            } else {
                console.warn("[JS] playLocalMusic function not available");
            }
        }
    });

    grid.appendChild(card);
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}