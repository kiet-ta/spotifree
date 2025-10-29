// album.js
< !--album.html head-- >
  <script defer src="./album.js"></script>

(() => {
  // =======================
  // Helpers & Token
  // =======================
  const qs = (s, r = document) => r.querySelector(s);
  const qsa = (s, r = document) => [...r.querySelectorAll(s)];
  const fmtTime = (ms) => {
    const total = Math.floor(ms / 1000);
    const m = Math.floor(total / 60);
    const s = total % 60;
    return `${m}:${String(s).padStart(2, "0")}`;
  };
  const fmtDate = (iso) => {
    try {
      const d = new Date(iso);
      return d.toLocaleDateString(undefined, { month: "short", day: "2-digit", year: "numeric" });
    } catch { return ""; }
  };
  const fmtTotalDuration = (ms) => {
    const totalSec = Math.floor(ms / 1000);
    const h = Math.floor(totalSec / 3600);
    const m = Math.floor((totalSec % 3600) / 60);
    return h ? `${h} hr ${m} min` : `${m} min`;
  };

  const getQuery = (key) => new URLSearchParams(location.search).get(key);

  async function getAccessToken() {
    // 1) Nếu WPF đã inject
    if (window.__ACCESS_TOKEN__) return window.__ACCESS_TOKEN__;
    // 2) Nếu shell WPF expose hàm async
    if (window.Shell?.getAccessToken) {
      try { return await window.Shell.getAccessToken(); } catch { }
    }
    // 3) Nếu token lưu trong localStorage
    const t = localStorage.getItem("spotify_access_token");
    if (t) return t;
    // 4) Nếu WPF gửi qua postMessage
    return new Promise((resolve) => {
      const timer = setTimeout(() => resolve(null), 1200);
      window.addEventListener("message", (e) => {
        if (e?.data?.type === "SPOTIFY_TOKEN") {
          clearTimeout(timer);
          resolve(e.data.token || null);
        }
      }, { once: true });
      // (WPF có thể chủ động postMessage ngay khi load)
    });
  }

  function authHeader(token) {
    if (!token) throw new Error("No Spotify access token.");
    return { Authorization: `Bearer ${token}` };
  }

  // =======================
  // Spotify API
  // =======================
  async function fetchPlaylistMeta(id, token) {
    const res = await fetch(`https://api.spotify.com/v1/playlists/${id}`, {
      headers: { ...authHeader(token) }
    });
    if (!res.ok) throw new Error(`Get playlist meta failed: ${res.status}`);
    return res.json();
  }

  async function fetchPlaylistTracksPaged(id, token, limit = 100, offset = 0) {
    const url = new URL(`https://api.spotify.com/v1/playlists/${id}/tracks`);
    url.searchParams.set("limit", limit);
    url.searchParams.set("offset", offset);
    // Lấy cả added_at và track full
    const res = await fetch(url.toString(), {
      headers: { ...authHeader(token) }
    });
    if (!res.ok) throw new Error(`Get playlist items failed: ${res.status}`);
    return res.json(); // { items, next, total, limit, offset }
  }

  async function checkSavedTracks(ids, token) {
    // GET /v1/me/tracks/contains?ids=...
    const url = new URL("https://api.spotify.com/v1/me/tracks/contains");
    url.searchParams.set("ids", ids.join(","));
    const res = await fetch(url, { headers: { ...authHeader(token) } });
    if (!res.ok) return ids.map(() => false);
    return res.json(); // [bool]
  }

  async function saveTrack(id, token, save = true) {
    const url = new URL("https://api.spotify.com/v1/me/tracks");
    url.searchParams.set("ids", id);
    const res = await fetch(url, {
      method: save ? "PUT" : "DELETE",
      headers: { ...authHeader(token) }
    });
    return res.ok;
  }

  async function followPlaylist(playlistId, token, follow = true) {
    // PUT/DELETE /v1/playlists/{playlist_id}/followers  (no body changes)
    const res = await fetch(`https://api.spotify.com/v1/playlists/${playlistId}/followers`, {
      method: follow ? "PUT" : "DELETE",
      headers: { ...authHeader(token), "Content-Type": "application/json" },
      body: follow ? JSON.stringify({}) : undefined
    });
    return res.ok;
  }

  // =======================
  // Player bridge (Web Playback SDK hoặc WPF host)
  // =======================
  async function playUris(uris, token, offset = 0) {
    // Ưu tiên WPF host
    if (window.Shell?.playUris) {
      try { return await window.Shell.playUris(uris, offset); } catch { }
    }
    // Fallback: call Web API /v1/me/player/play
    const res = await fetch("https://api.spotify.com/v1/me/player/play", {
      method: "PUT",
      headers: { ...authHeader(token), "Content-Type": "application/json" },
      body: JSON.stringify({
        uris,
        offset: { position: offset }
      })
    });
    return res.ok;
  }

  function gotoArtist(artistId) {
    if (window.Shell?.navigateToArtist) window.Shell.navigateToArtist(artistId);
    else location.href = `artist.html?id=${artistId}`;
  }
  function gotoAlbum(albumId) {
    if (window.Shell?.navigateToAlbum) window.Shell.navigateToAlbum(albumId);
    else location.href = `album.html?id=${albumId}`;
  }

  // =======================
  // Render
  // =======================
  const state = {
    token: null,
    playlistId: null,
    playlist: null,
    items: [],          // normalized items
    total: 0,
    offset: 0,
    limit: 100,
    loading: false,
    reachedEnd: false,
    observer: null
  };

  function mapItem(spItem) {
    const t = spItem.track;
    const artists = (t?.artists || []).map(a => ({ id: a.id, name: a.name }));
    return {
      id: t?.id,
      uri: t?.uri,
      name: t?.name || "",
      duration_ms: t?.duration_ms || 0,
      album: t?.album ? { id: t.album.id, name: t.album.name, image: getImage(t.album?.images) } : null,
      artists,
      added_at: spItem?.added_at || null,
      explicit: !!t?.explicit
    };
  }

  function getImage(images) {
    if (!images || !images.length) return null;
    // pick mid
    return (images.find(i => i.width >= 160) || images[0])?.url || images[0]?.url || null;
  }

  function renderHeader() {
    const cover = getImage(state.playlist?.images);
    const name = state.playlist?.name || "";
    const owner = state.playlist?.owner?.display_name || state.playlist?.owner?.id || "";
    const totalTracks = state.playlist?.tracks?.total ?? state.total;
    const totalDuration = fmtTotalDuration(state.items.reduce((a, b) => a + b.duration_ms, 0));
    const $cover = qs("#pl-cover");
    const $title = qs("#pl-title");
    const $subtitle = qs("#pl-subtitle");

    if ($cover) {
      $cover.src = cover || "";
      $cover.alt = name;
      if (!cover) $cover.style.background = "var(--ring)";
    }
    if ($title) $title.textContent = name;
    if ($subtitle) {
      $subtitle.textContent = `${owner} • ${totalTracks} songs • ${totalDuration}`;
    }
  }

  function renderRows(items) {
    const $tbody = qs("#tracks-tbody");
    if (!$tbody) return;
    const frag = document.createDocumentFragment();
    items.forEach((it, idx) => {
      const tr = document.createElement("tr");
      tr.className = "track-row";
      tr.dataset.uri = it.uri || "";
      tr.dataset.index = String(state.items.indexOf(it));

      tr.innerHTML = `
        <td class="col-idx">${state.items.indexOf(it) + 1}</td>
        <td class="col-title">
          <div class="tcell">
            <img class="thumb" src="${it.album?.image || ""}" alt="" />
            <div class="meta">
              <div class="name">${it.name}</div>
              <div class="sub">
                ${it.artists.map(a => `<a class="artist" data-artist="${a.id}">${a.name}</a>`).join(", ")}
              </div>
            </div>
          </div>
        </td>
        <td class="col-album"><a class="album" data-album="${it.album?.id || ""}">${it.album?.name || ""}</a></td>
        <td class="col-added">${it.added_at ? fmtDate(it.added_at) : ""}</td>
        <td class="col-time">
          <button class="heart" data-id="${it.id || ""}" aria-label="Like">${"♡"}</button>
          <span class="dur">${fmtTime(it.duration_ms)}</span>
        </td>
      `;
      frag.appendChild(tr);
    });
    $tbody.appendChild(frag);
  }

  function wireRowEvents(root = document) {
    // Play on row click
    root.addEventListener("click", async (e) => {
      const row = e.target.closest?.("tr.track-row");
      if (row && !e.target.closest(".heart, .artist, .album")) {
        const idx = Number(row.dataset.index || 0);
        const uris = state.items.map(x => x.uri).filter(Boolean);
        try { await playUris(uris, state.token, idx); } catch { }
      }
    });
    // Artist click
    root.addEventListener("click", (e) => {
      const a = e.target.closest?.("a.artist");
      if (a && a.dataset.artist) gotoArtist(a.dataset.artist);
    });
    // Album click
    root.addEventListener("click", (e) => {
      const a = e.target.closest?.("a.album");
      if (a && a.dataset.album) gotoAlbum(a.dataset.album);
    });
    // Heart toggle
    root.addEventListener("click", async (e) => {
      const btn = e.target.closest?.("button.heart");
      if (!btn) return;
      e.stopPropagation();
      const id = btn.dataset.id;
      if (!id) return;
      const liked = btn.classList.toggle("on");
      const ok = await saveTrack(id, state.token, liked);
      if (!ok) btn.classList.toggle("on"); // revert if fail
      btn.textContent = btn.classList.contains("on") ? "❤" : "♡";
    });
  }

  async function hydrateHearts(batchStart = 0) {
    // Gọi theo lô 50 id/lần
    const ids = state.items.map(x => x.id).filter(Boolean);
    for (let i = batchStart; i < ids.length; i += 50) {
      const slice = ids.slice(i, i + 50);
      const flags = await checkSavedTracks(slice, state.token);
      flags.forEach((v, j) => {
        const realIdx = i + j;
        const id = ids[realIdx];
        const btn = qs(`button.heart[data-id="${id}"]`);
        if (btn) {
          if (v) btn.classList.add("on");
          btn.textContent = v ? "❤" : "♡";
        }
      });
    }
  }

  function setupInfiniteScroll() {
    const sentinel = qs("#scroll-sentinel");
    if (!sentinel) return;
    state.observer?.disconnect?.();
    state.observer = new IntersectionObserver(async (entries) => {
      const first = entries[0];
      if (!first.isIntersecting || state.loading || state.reachedEnd) return;
      await loadMore();
    }, { root: null, threshold: 0.1 });
    state.observer.observe(sentinel);
  }

  async function loadMore() {
    if (state.loading || state.reachedEnd) return;
    state.loading = true;
    try {
      const page = await fetchPlaylistTracksPaged(state.playlistId, state.token, state.limit, state.offset);
      const mapped = (page.items || []).map(mapItem);
      state.items.push(...mapped);
      state.total = page.total ?? state.total;
      state.offset = (page.offset || 0) + (page.limit || mapped.length);
      if (!page.next || mapped.length === 0) state.reachedEnd = true;

      renderRows(mapped);
      await hydrateHearts(Math.max(0, state.items.length - mapped.length)); // hydrate lô mới
    } catch (err) {
      console.error(err);
    } finally {
      state.loading = false;
    }
  }

  // =======================
  // Header actions
  // =======================
  function wireHeaderActions() {
    const playBtn = qs("#btn-play");
    const likeBtn = qs("#btn-like-pl");
    if (playBtn) {
      playBtn.addEventListener("click", async () => {
        const uris = state.items.map(x => x.uri).filter(Boolean);
        if (!uris.length) return;
        try { await playUris(uris, state.token, 0); } catch { }
      });
    }
    if (likeBtn) {
      likeBtn.addEventListener("click", async () => {
        const on = likeBtn.classList.toggle("on");
        const ok = await followPlaylist(state.playlistId, state.token, on);
        if (!ok) likeBtn.classList.toggle("on");
        likeBtn.textContent = likeBtn.classList.contains("on") ? "Following" : "Follow";
      });
    }
  }

  // =======================
  // Boot
  // =======================
  document.addEventListener("DOMContentLoaded", async () => {
    state.playlistId = getQuery("playlist") || getQuery("id");
    if (!state.playlistId) {
      console.warn("Missing playlist id");
      return;
    }
    state.token = await getAccessToken();
    if (!state.token) {
      console.error("No Spotify access token found.");
      qs("#error-box")?.classList.remove("hidden");
      return;
    }

    try {
      // meta (để hiện header sớm)
      state.playlist = await fetchPlaylistMeta(state.playlistId, state.token);
      renderHeader();
      wireHeaderActions();

      // trang đầu
      await loadMore();

      // sự kiện hàng
      wireRowEvents(document);

      // cuộn vô hạn
      setupInfiniteScroll();
    } catch (err) {
      console.error(err);
      qs("#error-box")?.classList.remove("hidden");
    }
  });
})();
