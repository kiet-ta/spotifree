// Home page logic using shared Spotify API (spotify-api.js)

(function(){
  function createCard({ title, subtitle, imageUrl }){
    const card = document.createElement('div');
    card.className = 'music-card';

    const thumb = document.createElement('div');
    thumb.className = 'music-thumb';
    thumb.style.background = `#1A1A1A url(${imageUrl || ''}) center/cover no-repeat`;
    if (!imageUrl) thumb.textContent = '🎵';

    const titleEl = document.createElement('div');
    titleEl.textContent = title;

    const subEl = document.createElement('div');
    subEl.className = 'subtitle';
    subEl.textContent = subtitle || '';

    card.appendChild(thumb);
    card.appendChild(titleEl);
    card.appendChild(subEl);
    return card;
  }

  function clearActiveFilters(){
    document.querySelectorAll('.filters .btn-pill').forEach(b => b.classList.remove('active'));
  }

  function setActiveFilter(btnId){
    clearActiveFilters();
    const el = document.getElementById(btnId);
    if (el) el.classList.add('active');
  }

  function ensureRow(rowId){
    const row = document.getElementById(rowId);
    if (row) row.innerHTML = '';
    return row;
  }

  async function loadNewReleases(){
    try{
      const row = ensureRow('new-music-row');
      if (!row) return;
      let albums = [];
      try {
        albums = await window.spotifyAPI.getNewReleases(15);
      } catch (_) { albums = []; }

      if (!albums || albums.length === 0){
        const sample = [
          { name:'New Wave VN', artist:'Various Artists', images:[{url:''}] },
          { name:'Chill Night', artist:'Indie VN', images:[{url:''}] },
          { name:'Hot 2025', artist:'Top VN', images:[{url:''}] }
        ];
        albums = sample;
      }

      albums.forEach(a => {
        const imageUrl = (a.images && a.images[0] && a.images[0].url) || '';
        row.appendChild(createCard({ title: a.name, subtitle: a.artist, imageUrl }));
      });
    }catch(err){
      console.error('[Home] loadNewReleases error', err);
    }
  }

  async function loadFeatured(){
    try{
      const row = ensureRow('made-for-you-row');
      if (!row) return;
      let playlists = [];
      try {
        playlists = await window.spotifyAPI.getFeaturedPlaylists(15);
      } catch (_) { playlists = []; }

      // Update hero from first playlist if available
      const heroTitle = document.getElementById('hero-title');
      const heroDesc = document.getElementById('hero-desc');
      const heroCover = document.getElementById('hero-cover');
      if (!playlists || playlists.length === 0){
        playlists = [
          { name:'Daily Mix 1', description:'Personalized for you', images:[{url:''}], owner:'Spotify' },
          { name:'Daily Mix 2', description:'Fresh tracks', images:[{url:''}], owner:'Spotify' },
          { name:'Focus Flow', description:'Beats to focus', images:[{url:''}], owner:'Spotify' }
        ];
      }

      if (playlists && playlists.length > 0){
        const first = playlists[0];
        if (heroTitle) heroTitle.textContent = first.name || 'Made For You';
        if (heroDesc) heroDesc.textContent = first.description || 'Featured playlist for you';
        const coverUrl = (first.images && first.images[0] && first.images[0].url) || '';
        if (heroCover){
          if (coverUrl){
            heroCover.style.background = `#333 url(${coverUrl}) center/cover no-repeat`;
            heroCover.textContent = '';
          } else {
            heroCover.style.background = '#FF9A9E';
            heroCover.textContent = '🎵';
          }
        }
      }

      playlists.forEach(p => {
        const imageUrl = (p.images && p.images[0] && p.images[0].url) || '';
        row.appendChild(createCard({ title: p.name, subtitle: p.owner || '', imageUrl }));
      });
    }catch(err){
      console.error('[Home] loadFeatured error', err);
    }
  }

  async function loadAll(){
    setActiveFilter('filter-all');
    const row = ensureRow('filter-row');
    if (!row) return;
    try{
      const res = await window.spotifyAPI.smartSearch('viet', 20);
      const items = [
        ...res.tracks.map(t => ({ title:t.name, subtitle:t.artist, imageUrl: (t.images && t.images[0] && t.images[0].url) || '' })),
        ...res.albums.map(a => ({ title:a.name, subtitle:a.artist, imageUrl: (a.images && a.images[0] && a.images[0].url) || '' })),
        ...res.playlists.map(p => ({ title:p.name, subtitle:p.owner, imageUrl: (p.images && p.images[0] && p.images[0].url) || '' }))
      ].slice(0,15);
      renderCollection(row, items, getSamples('all'));
    }catch(err){
      console.error('[Home] loadAll error', err);
      renderCollection(row, [], getSamples('all'));
    }
  }

  async function loadMusic(){
    setActiveFilter('filter-music');
    const row = ensureRow('filter-row');
    if (!row) return;
    try{
      const tracks = await window.spotifyAPI.searchTracks('viet hit', 15);
      const items = (tracks || []).map(t => ({ title:t.name, subtitle:t.artist, imageUrl:(t.images && t.images[0] && t.images[0].url) || '' }));
      renderCollection(row, items, getSamples('music'));
    }catch(err){
      console.error('[Home] loadMusic error', err);
      renderCollection(row, [], getSamples('music'));
    }
  }

  async function loadPodcasts(){
    setActiveFilter('filter-podcasts');
    const row = ensureRow('filter-row');
    if (!row) return;
    // API wrapper chưa có shows/podcasts -> hiển thị mẫu
    renderCollection(row, [], getSamples('podcasts'));
  }

  function renderCollection(row, items, sample){
    const list = (items && items.length ? items : sample);
    row.innerHTML = '';
    list.forEach(i => row.appendChild(createCard(i)));
  }

  function getSamples(kind){
    switch(kind){
      case 'music':
        return [
          { title:'Top Hits VN', subtitle:'Playlist', imageUrl:'' },
          { title:'Lofi Chill', subtitle:'Playlist', imageUrl:'' },
          { title:'Ballad 2025', subtitle:'Album', imageUrl:'' }
        ];
      case 'podcasts':
        return [
          { title:'The Gioi Podcast', subtitle:'Podcast Show', imageUrl:'' },
          { title:'Tech Talk VN', subtitle:'Podcast Show', imageUrl:'' },
          { title:'Kinh Tế Hôm Nay', subtitle:'Podcast Show', imageUrl:'' }
        ];
      case 'all':
      default:
        return [
          { title:'New Wave VN', subtitle:'Album', imageUrl:'' },
          { title:'Focus Flow', subtitle:'Playlist', imageUrl:'' },
          { title:'Daily Mix 1', subtitle:'Playlist', imageUrl:'' }
        ];
    }
  }

  async function initHome(){
    // Bind filter events
    const allBtn = document.getElementById('filter-all');
    const musicBtn = document.getElementById('filter-music');
    const podBtn = document.getElementById('filter-podcasts');
    if (allBtn) allBtn.onclick = loadAll;
    if (musicBtn) musicBtn.onclick = loadMusic;
    if (podBtn) podBtn.onclick = loadPodcasts;

    // Initial loads
    await Promise.all([loadNewReleases(), loadFeatured()]);
    await loadAll();
  }

  window.initHome = initHome;
})();


