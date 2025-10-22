function initLibrary() {
    console.log("initLibrary() is running!"); 
    const createBtn = document.getElementById('create-playlist-btn');

    if (createBtn) {
        console.log("Button #create-playlist-btn was found.");
        
        createBtn.addEventListener('click', () => {
            console.log('Create playlist button clicked!'); 

            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    action: 'createPlaylist',
                    name: 'My New Playlist' 
                });
            } else {
                console.error("Cannot communicate with C# backend");
            }
        });

    } else {
        console.warn("#create-playlist-btn not found. Did you add the id in library.html?");
    }

}

// create ui card for playlist
window.addNewPlaylistCard = function (playlist) {
    console.log("addNewPlaylistCard called from C# with:", playlist); 
    
    const grid = document.querySelector('.playlists-grid');
    if (!grid) {
        console.error("Playlist grid not found");
        return;
    }

    const card = document.createElement('div');
    card.className = 'playlist-card';

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
          <div class="playlist-type">${playlist.type}</div>
        </div>
    `;

    grid.appendChild(card);
}