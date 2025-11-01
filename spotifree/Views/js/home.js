// Home page logic using shared Spotify API (spotify-api.js)

(function(){
  // Helper function để đảm bảo text hiển thị đúng UTF-8
  function sanitizeText(text) {
    if (!text) return '';
    // Decode HTML entities nếu có
    const textarea = document.createElement('textarea');
    textarea.innerHTML = text;
    return textarea.value;
  }

  function createCard({ title, subtitle, imageUrl }){
    const card = document.createElement('div');
    card.className = 'music-card';

    const thumb = document.createElement('div');
    thumb.className = 'music-thumb';
    thumb.style.background = `#1A1A1A url(${imageUrl || ''}) center/cover no-repeat`;
    if (!imageUrl) thumb.textContent = '🎵';

    const titleEl = document.createElement('div');
    titleEl.className = 'card-title';
    titleEl.textContent = sanitizeText(title);
    titleEl.title = sanitizeText(title); // Tooltip cho text dài

    const subEl = document.createElement('div');
    subEl.className = 'subtitle';
    subEl.textContent = sanitizeText(subtitle);
    subEl.title = sanitizeText(subtitle); // Tooltip

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

  function showLoading(rowId, message = 'Đang tải...'){
    const row = document.getElementById(rowId);
    if (!row) return;
    row.innerHTML = `<div style="color:#ccc;padding:24px;text-align:center;">
      <div style="font-size:24px;margin-bottom:8px;">⏳</div>
      <div>${message}</div>
    </div>`;
  }

  async function loadNewReleases(){
    const rowId = 'new-music-row';
    showLoading(rowId, 'Đang tải bài hát mới...');
    
    try{
      console.log('🎵 Loading new tracks released in 2025...');
      const tracks = await window.spotifyAPI.getNewTracks(20);
      
      const row = ensureRow(rowId);
      if (!row) return;
      
      if (!tracks || tracks.length === 0){
        row.innerHTML = '<div style="color:#ccc;padding:24px;">Không có bài hát mới</div>';
        return;
      }

      console.log(`✅ Loaded ${tracks.length} new tracks`);
      
      tracks.forEach(t => {
        const imageUrl = (t.images && t.images[0] && t.images[0].url) || '';
        const card = createCard({ 
          title: t.name, 
          subtitle: `${t.artist} • ${t.album}`, 
          imageUrl 
        });
        
        // Add click to preview
        card.style.cursor = 'pointer';
        card.onclick = () => {
          console.log('🎵 New track:', t.name, 'by', t.artist);
          if (t.preview_url) {
            console.log('Preview URL:', t.preview_url);
          }
        };
        
        row.appendChild(card);
      });
    }catch(err){
      console.error('[Home] loadNewReleases error', err);
      const row = ensureRow('new-music-row');
      if (row) row.innerHTML = '<div style="color:#f44;padding:24px;">⚠️ Lỗi tải bài hát mới</div>';
    }
  }

  async function loadFeatured(){
    const rowId = 'made-for-you-row';
    showLoading(rowId, 'Đang tải bài hát yêu thích...');
    
    try{
      console.log('🎵 Loading YOUR saved tracks for Made For You section...');
      const result = await window.spotifyAPI.getSavedTracks(10, 0);
      
      const row = ensureRow(rowId);
      if (!row) return;
      const tracks = result.items || [];
      
      console.log(`✅ Loaded ${tracks.length} saved tracks from /me/tracks`);

      // Update hero with saved tracks info
      const heroTitle = document.getElementById('hero-title');
      const heroDesc = document.getElementById('hero-desc');
      const heroCover = document.getElementById('hero-cover');
      
      if (tracks && tracks.length > 0){
        const first = tracks[0];
        if (heroTitle) heroTitle.textContent = 'Bài Hát Đã Lưu';
        if (heroDesc) heroDesc.textContent = `${result.total} bài hát yêu thích của bạn • Cập nhật liên tục`;
        const coverUrl = (first.images && first.images[0] && first.images[0].url) || '';
        if (heroCover){
          if (coverUrl){
            heroCover.style.background = `#333 url(${coverUrl}) center/cover no-repeat`;
            heroCover.textContent = '';
          } else {
            heroCover.style.background = 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
            heroCover.textContent = '🎵';
          }
        }
      } else {
        if (heroTitle) heroTitle.textContent = 'Made For You';
        if (heroDesc) heroDesc.textContent = 'Bạn chưa có bài hát đã lưu';
        if (heroCover){
            heroCover.style.background = '#FF9A9E';
            heroCover.textContent = '🎵';
          }
        }

      // Display saved tracks in the row
      if (tracks.length === 0){
        const emptyMsg = document.createElement('div');
        emptyMsg.style.cssText = 'color:#ccc; padding:24px; text-align:center;';
        emptyMsg.textContent = 'Bạn chưa lưu bài hát nào. Hãy thêm bài hát yêu thích vào thư viện!';
        row.appendChild(emptyMsg);
        return;
      }

      tracks.forEach(t => {
        const imageUrl = (t.images && t.images[0] && t.images[0].url) || '';
        const card = createCard({ 
          title: t.name, 
          subtitle: `${t.artist} • ${t.duration}`, 
          imageUrl 
        });
        
        // Add click handler
        card.style.cursor = 'pointer';
        card.onclick = () => {
          console.log('🎵 Playing:', t.name, 'by', t.artist);
          if (t.preview_url) {
            console.log('Preview URL:', t.preview_url);
            // TODO: Integrate with audio player
          } else {
            console.log('No preview available for this track');
          }
        };
        
        row.appendChild(card);
      });

      // Add "Show all" button if there are more tracks
      if (result.total > tracks.length) {
        const moreCard = document.createElement('div');
        moreCard.className = 'music-card';
        moreCard.style.cssText = 'display:flex; align-items:center; justify-content:center; cursor:pointer; min-height:220px; background:#1A1A1A;';
        moreCard.innerHTML = `<div style="text-align:center;"><div style="font-size:32px; margin-bottom:8px;">➕</div><div>Xem tất cả ${result.total} bài</div></div>`;
        moreCard.onclick = () => {
          console.log('📚 Show all saved tracks - navigate to library...');
          // TODO: Navigate to library page
        };
        row.appendChild(moreCard);
      }

    }catch(err){
      console.error('[Home] loadFeatured error', err);
      const row = ensureRow('made-for-you-row');
      if (row) row.innerHTML = '<div style="color:#f44;padding:24px;">⚠️ Lỗi tải bài hát đã lưu</div>';
    }
  }

  async function loadAll(){
    setActiveFilter('filter-all');
    const rowId = 'filter-row';
    showLoading(rowId, 'Đang tải tất cả...');
    
    try{
      console.log('📦 Loading All - combining Music Categories + Podcasts...');
      
      // Load both categories and podcasts in parallel
      const [categories, podcasts] = await Promise.all([
        window.spotifyAPI.getCategories(10),
        window.spotifyAPI.getPodcasts(10)
      ]);
      
      // Combine both into one list
      const items = [
        ...categories.map(cat => ({ 
          title: cat.name, 
          subtitle: '🎵 Music Category', 
          imageUrl: (cat.icons && cat.icons[0] && cat.icons[0].url) || '' 
        })),
        ...podcasts.map(pod => ({ 
          title: pod.name, 
          subtitle: `🎙️ ${pod.publisher || 'Podcast'}`, 
          imageUrl: (pod.images && pod.images[0] && pod.images[0].url) || '' 
        }))
      ];
      
      console.log(`✅ Loaded ${categories.length} categories + ${podcasts.length} podcasts = ${items.length} total`);
      
      const row = ensureRow(rowId);
      if (!row) return;
      
      if (items.length === 0) {
        row.innerHTML = '<div style="color:#ccc;padding:24px;">Không có nội dung</div>';
        return;
      }
      
      items.forEach(i => row.appendChild(createCard(i)));
    }catch(err){
      console.error('[Home] loadAll error', err);
      const row = document.getElementById(rowId);
      if (row) row.innerHTML = '<div style="color:#f44;padding:24px;">⚠️ Lỗi tải dữ liệu</div>';
    }
  }

  async function loadMusic(){
    setActiveFilter('filter-music');
    const rowId = 'filter-row';
    showLoading(rowId, 'Đang tải danh mục nhạc...');
    
    try{
      console.log('🎵 Loading music categories from /browse/categories...');
      const categories = await window.spotifyAPI.getCategories(15);
      
      const row = ensureRow(rowId);
      if (!row) return;
      
      if (!categories || categories.length === 0) {
        row.innerHTML = '<div style="color:#ccc;padding:24px;">Không có danh mục nhạc</div>';
        return;
      }
      
      console.log(`✅ Loaded ${categories.length} categories`);
      categories.forEach(cat => {
        const imageUrl = (cat.icons && cat.icons[0] && cat.icons[0].url) || '';
        row.appendChild(createCard({ title: cat.name, subtitle: 'Category', imageUrl }));
      });
    }catch(err){
      console.error('[Home] loadMusic error', err);
      const row = document.getElementById(rowId);
      if (row) row.innerHTML = '<div style="color:#f44;padding:24px;">⚠️ Lỗi tải danh mục nhạc</div>';
    }
  }

  async function loadPodcasts(){
    setActiveFilter('filter-podcasts');
    const rowId = 'filter-row';
    showLoading(rowId, 'Đang tải podcasts...');
    
    try{
      console.log('🎙️ Loading podcasts from /search?type=show...');
      const podcasts = await window.spotifyAPI.getPodcasts(15);
      
      const row = ensureRow(rowId);
      if (!row) return;
      
      if (!podcasts || podcasts.length === 0) {
        row.innerHTML = '<div style="color:#ccc;padding:24px;">Không tìm thấy podcast</div>';
        return;
      }
      
      console.log(`✅ Loaded ${podcasts.length} podcasts`);
      podcasts.forEach(pod => {
        const imageUrl = (pod.images && pod.images[0] && pod.images[0].url) || '';
        row.appendChild(createCard({ title: pod.name, subtitle: pod.publisher || 'Podcast', imageUrl }));
      });
    }catch(err){
      console.error('[Home] loadPodcasts error', err);
      const row = document.getElementById(rowId);
      if (row) row.innerHTML = '<div style="color:#f44;padding:24px;">⚠️ Lỗi tải podcast</div>';
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

    // Initial loads - loadFeatured now loads saved tracks from /me/tracks
    await Promise.all([
      loadNewReleases(), 
      loadFeatured()  // This now loads saved tracks into "Made For You" section
    ]);
    await loadAll();
  }

  window.initHome = initHome;
})();


