// 🎵 Spotify API Integration
// File này tích hợp với Spotify Web API để tìm kiếm và lấy thông tin nhạc thực tế

class SpotifyAPI {
    constructor() {
        // 🔑 Sử dụng credentials từ config hoặc fallback
        this.clientId = window.SpotifyConfig?.credentials?.clientId || 'YOUR_SPOTIFY_CLIENT_ID';
        this.clientSecret = window.SpotifyConfig?.credentials?.clientSecret || 'YOUR_SPOTIFY_CLIENT_SECRET';
        this.redirectUri = window.SpotifyConfig?.credentials?.redirectUri || 'http://localhost:3000/callback';
        this.accessToken = null;
        this.tokenExpiry = null;
        this.baseURL = 'https://api.spotify.com/v1';

        // Khởi tạo
        this.initializeAuth();
    }

    // 🔐 Khởi tạo xác thực
    async initializeAuth() {
        try {
            // Kiểm tra credentials trước
            if (this.clientId === 'YOUR_SPOTIFY_CLIENT_ID' || this.clientSecret === 'YOUR_SPOTIFY_CLIENT_SECRET') {
                console.warn('⚠️ Spotify credentials chưa được cấu hình. Chế độ fallback sẽ được sử dụng.');
                this.enableFallbackMode();
                return;
            }

            // Thử lấy token từ localStorage
            const savedToken = localStorage.getItem('spotify_access_token');
            const savedExpiry = localStorage.getItem('spotify_token_expiry');

            if (savedToken && savedExpiry && new Date() < new Date(savedExpiry)) {
                this.accessToken = savedToken;
                this.tokenExpiry = new Date(savedExpiry);
                console.log('🎵 Spotify token loaded from cache');
                return;
            }

            // Nếu không có token hoặc đã hết hạn, lấy token mới
            await this.getClientCredentialsToken();
        } catch (error) {
            console.error('❌ Error initializing Spotify auth:', error);
            console.warn('⚠️ Chuyển sang chế độ fallback');
            this.enableFallbackMode();
        }
    }

    // 🔄 Bật chế độ fallback (không cần Spotify API)
    enableFallbackMode() {
        this.fallbackMode = true;
        console.log('🔄 Spotify fallback mode enabled - sử dụng dữ liệu mẫu');
    }

    // 🎵 Dữ liệu mẫu cho chế độ fallback
    getFallbackData() {
        return {
            tracks: [
                {
                    id: 'fallback-1',
                    name: 'Shape of You',
                    artist: 'Ed Sheeran',
                    album: '÷ (Divide)',
                    duration: '3:53',
                    popularity: 95,
                    preview_url: null,
                    external_urls: { spotify: 'https://open.spotify.com/track/fallback-1' },
                    images: [{ url: 'https://via.placeholder.com/300x300?text=Ed+Sheeran' }],
                    release_date: '2017-01-06'
                },
                {
                    id: 'fallback-2',
                    name: 'Perfect',
                    artist: 'Ed Sheeran',
                    album: '÷ (Divide)',
                    duration: '4:23',
                    popularity: 92,
                    preview_url: null,
                    external_urls: { spotify: 'https://open.spotify.com/track/fallback-2' },
                    images: [{ url: 'https://via.placeholder.com/300x300?text=Perfect' }],
                    release_date: '2017-01-06'
                },
                {
                    id: 'fallback-3',
                    name: 'Thinking Out Loud',
                    artist: 'Ed Sheeran',
                    album: 'x (Multiply)',
                    duration: '4:41',
                    popularity: 88,
                    preview_url: null,
                    external_urls: { spotify: 'https://open.spotify.com/track/fallback-3' },
                    images: [{ url: 'https://via.placeholder.com/300x300?text=Thinking+Out+Loud' }],
                    release_date: '2014-06-20'
                }
            ],
            artists: [
                {
                    id: 'fallback-artist-1',
                    name: 'Ed Sheeran',
                    popularity: 95,
                    genres: ['pop', 'acoustic', 'folk'],
                    followers: 50000000,
                    images: [{ url: 'https://via.placeholder.com/300x300?text=Ed+Sheeran' }],
                    external_urls: { spotify: 'https://open.spotify.com/artist/fallback-artist-1' }
                }
            ],
            albums: [
                {
                    id: 'fallback-album-1',
                    name: '÷ (Divide)',
                    artist: 'Ed Sheeran',
                    release_date: '2017-03-03',
                    total_tracks: 16,
                    images: [{ url: 'https://via.placeholder.com/300x300?text=Divide' }],
                    external_urls: { spotify: 'https://open.spotify.com/album/fallback-album-1' }
                }
            ],
            playlists: [
                {
                    id: 'fallback-playlist-1',
                    name: 'Today\'s Top Hits',
                    description: 'The most played songs right now',
                    tracks: 50,
                    images: [{ url: 'https://via.placeholder.com/300x300?text=Top+Hits' }],
                    external_urls: { spotify: 'https://open.spotify.com/playlist/fallback-playlist-1' },
                    owner: 'Spotify'
                }
            ]
        };
    }

    // 🔑 Lấy token bằng Client Credentials Flow
    async getClientCredentialsToken() {
        try {
            // Kiểm tra credentials
            if (this.clientId === 'YOUR_SPOTIFY_CLIENT_ID' || this.clientSecret === 'YOUR_SPOTIFY_CLIENT_SECRET') {
                throw new Error('❌ Spotify credentials chưa được cấu hình! Vui lòng cập nhật Client ID và Client Secret trong spotify-config.js');
            }

            const response = await fetch('https://accounts.spotify.com/api/token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'Authorization': 'Basic ' + btoa(this.clientId + ':' + this.clientSecret)
                },
                body: 'grant_type=client_credentials'
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(`❌ Spotify API Error: ${response.status} - ${errorData.error_description || errorData.error}`);
            }

            const data = await response.json();
            this.accessToken = data.access_token;
            this.tokenExpiry = new Date(Date.now() + (data.expires_in * 1000));

            // Lưu vào localStorage
            localStorage.setItem('spotify_access_token', this.accessToken);
            localStorage.setItem('spotify_token_expiry', this.tokenExpiry.toISOString());

            console.log('🎵 Spotify token obtained successfully');
        } catch (error) {
            console.error('❌ Error getting Spotify token:', error);
            // Hiển thị lỗi thân thiện cho user
            this.showSpotifyError(error.message);
            throw error;
        }
    }

    // 🚨 Hiển thị lỗi Spotify cho user
    showSpotifyError(message) {
        // Tạo notification hoặc alert
        if (window.addMessage) {
            window.addMessage(`🚨 ${message}`, false);
        } else {
            console.error('Spotify Error:', message);
        }
    }

    // 🔄 Kiểm tra và làm mới token nếu cần
    async ensureValidToken() {
        if (!this.accessToken || new Date() >= this.tokenExpiry) {
            await this.getClientCredentialsToken();
        }
    }

    // 🎵 Tìm kiếm bài hát
    async searchTracks(query, limit = 10, offset = 0) {
        try {
            // Nếu đang ở chế độ fallback, trả về dữ liệu mẫu
            if (this.fallbackMode) {
                console.log('🔄 Using fallback data for track search');
                const fallbackData = this.getFallbackData();
                return fallbackData.tracks.slice(0, limit);
            }

            await this.ensureValidToken();

            const params = new URLSearchParams({
                q: query,
                type: 'track',
                limit: limit.toString(),
                offset: offset.toString(),
                market: 'VN' // Thị trường Việt Nam
            });

            const response = await fetch(`${this.baseURL}/search?${params}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatTrackResults(data.tracks);
        } catch (error) {
            console.error('❌ Error searching tracks:', error);
            // Fallback về dữ liệu mẫu nếu có lỗi
            if (!this.fallbackMode) {
                console.log('🔄 Falling back to sample data');
                this.enableFallbackMode();
                const fallbackData = this.getFallbackData();
                return fallbackData.tracks.slice(0, limit);
            }
            throw error;
        }
    }

    // 🎤 Tìm kiếm nghệ sĩ
    async searchArtists(query, limit = 10) {
        try {
            await this.ensureValidToken();

            const params = new URLSearchParams({
                q: query,
                type: 'artist',
                limit: limit.toString(),
                market: 'VN'
            });

            const response = await fetch(`${this.baseURL}/search?${params}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatArtistResults(data.artists);
        } catch (error) {
            console.error('❌ Error searching artists:', error);
            throw error;
        }
    }

    // 🎧 Tìm kiếm album
    async searchAlbums(query, limit = 10) {
        try {
            await this.ensureValidToken();

            const params = new URLSearchParams({
                q: query,
                type: 'album',
                limit: limit.toString(),
                market: 'VN'
            });

            const response = await fetch(`${this.baseURL}/search?${params}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatAlbumResults(data.albums);
        } catch (error) {
            console.error('❌ Error searching albums:', error);
            throw error;
        }
    }

    // 🎵 Tìm kiếm playlist
    async searchPlaylists(query, limit = 10) {
        try {
            await this.ensureValidToken();

            const params = new URLSearchParams({
                q: query,
                type: 'playlist',
                limit: limit.toString(),
                market: 'VN'
            });

            const response = await fetch(`${this.baseURL}/search?${params}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatPlaylistResults(data.playlists);
        } catch (error) {
            console.error('❌ Error searching playlists:', error);
            throw error;
        }
    }

    // 🎯 Tìm kiếm theo thể loại
    async searchByGenre(genre, limit = 20) {
        try {
            await this.ensureValidToken();

            const params = new URLSearchParams({
                q: `genre:${genre}`,
                type: 'track',
                limit: limit.toString(),
                market: 'VN'
            });

            const response = await fetch(`${this.baseURL}/search?${params}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatTrackResults(data.tracks);
        } catch (error) {
            console.error('❌ Error searching by genre:', error);
            throw error;
        }
    }

    // 🎵 Lấy thông tin chi tiết bài hát
    async getTrackDetails(trackId) {
        try {
            await this.ensureValidToken();

            const response = await fetch(`${this.baseURL}/tracks/${trackId}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatTrackDetails(data);
        } catch (error) {
            console.error('❌ Error getting track details:', error);
            throw error;
        }
    }

    // 🎤 Lấy thông tin nghệ sĩ
    async getArtistDetails(artistId) {
        try {
            await this.ensureValidToken();

            const response = await fetch(`${this.baseURL}/artists/${artistId}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatArtistDetails(data);
        } catch (error) {
            console.error('❌ Error getting artist details:', error);
            throw error;
        }
    }

    // 🎧 Lấy top tracks của nghệ sĩ
    async getArtistTopTracks(artistId, market = 'VN') {
        try {
            await this.ensureValidToken();

            const response = await fetch(`${this.baseURL}/artists/${artistId}/top-tracks?market=${market}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatTrackResults({ items: data.tracks });
        } catch (error) {
            console.error('❌ Error getting artist top tracks:', error);
            throw error;
        }
    }

    // 🎵 Lấy bài hát theo tâm trạng
    async getMoodBasedTracks(mood, limit = 10) {
        const moodQueries = {
            happy: 'mood:happy OR energy:high OR valence:high',
            sad: 'mood:sad OR energy:low OR valence:low',
            chill: 'mood:chill OR energy:medium OR valence:medium',
            energetic: 'energy:high OR tempo:fast',
            romantic: 'mood:romantic OR valence:high',
            party: 'energy:high OR tempo:fast OR danceability:high'
        };

        const query = moodQueries[mood] || mood;
        return await this.searchTracks(query, limit);
    }

    // 🎯 Tìm kiếm thông minh (tất cả loại)
    async smartSearch(query, limit = 10) {
        try {
            // Nếu đang ở chế độ fallback, trả về dữ liệu mẫu
            if (this.fallbackMode) {
                console.log('🔄 Using fallback data for smart search');
                const fallbackData = this.getFallbackData();
                return {
                    tracks: fallbackData.tracks.slice(0, Math.ceil(limit * 0.4)),
                    artists: fallbackData.artists.slice(0, Math.ceil(limit * 0.2)),
                    albums: fallbackData.albums.slice(0, Math.ceil(limit * 0.2)),
                    playlists: fallbackData.playlists.slice(0, Math.ceil(limit * 0.2)),
                    total: fallbackData.tracks.length + fallbackData.artists.length + fallbackData.albums.length + fallbackData.playlists.length
                };
            }

            const [tracks, artists, albums, playlists] = await Promise.all([
                this.searchTracks(query, Math.ceil(limit * 0.4)),
                this.searchArtists(query, Math.ceil(limit * 0.2)),
                this.searchAlbums(query, Math.ceil(limit * 0.2)),
                this.searchPlaylists(query, Math.ceil(limit * 0.2))
            ]);

            return {
                tracks,
                artists,
                albums,
                playlists,
                total: tracks.length + artists.length + albums.length + playlists.length
            };
        } catch (error) {
            console.error('❌ Error in smart search:', error);
            // Fallback về dữ liệu mẫu nếu có lỗi
            if (!this.fallbackMode) {
                console.log('🔄 Falling back to sample data for smart search');
                this.enableFallbackMode();
                const fallbackData = this.getFallbackData();
                return {
                    tracks: fallbackData.tracks.slice(0, Math.ceil(limit * 0.4)),
                    artists: fallbackData.artists.slice(0, Math.ceil(limit * 0.2)),
                    albums: fallbackData.albums.slice(0, Math.ceil(limit * 0.2)),
                    playlists: fallbackData.playlists.slice(0, Math.ceil(limit * 0.2)),
                    total: fallbackData.tracks.length + fallbackData.artists.length + fallbackData.albums.length + fallbackData.playlists.length
                };
            }
            throw error;
        }
    }

    // 🎵 Format kết quả bài hát
    formatTrackResults(tracksData) {
        if (!tracksData || !tracksData.items) return [];

        return tracksData.items.map(track => ({
            id: track.id,
            name: track.name,
            artist: track.artists.map(artist => artist.name).join(', '),
            album: track.album.name,
            duration: this.formatDuration(track.duration_ms),
            popularity: track.popularity,
            preview_url: track.preview_url,
            external_urls: track.external_urls,
            images: track.album.images,
            release_date: track.album.release_date,
            genres: track.artists.flatMap(artist => artist.genres || [])
        }));
    }

    // 🎤 Format kết quả nghệ sĩ
    formatArtistResults(artistsData) {
        if (!artistsData || !artistsData.items) return [];

        return artistsData.items.map(artist => ({
            id: artist.id,
            name: artist.name,
            popularity: artist.popularity,
            genres: artist.genres,
            followers: artist.followers.total,
            images: artist.images,
            external_urls: artist.external_urls
        }));
    }

    // 🎧 Format kết quả album
    formatAlbumResults(albumsData) {
        if (!albumsData || !albumsData.items) return [];

        return albumsData.items.map(album => ({
            id: album.id,
            name: album.name,
            artist: album.artists.map(artist => artist.name).join(', '),
            release_date: album.release_date,
            total_tracks: album.total_tracks,
            images: album.images,
            external_urls: album.external_urls
        }));
    }

    // 🎵 Format kết quả playlist
    formatPlaylistResults(playlistsData) {
        if (!playlistsData || !playlistsData.items) return [];

        return playlistsData.items.map(playlist => ({
            id: playlist.id,
            name: playlist.name,
            description: playlist.description,
            tracks: playlist.tracks.total,
            images: playlist.images,
            external_urls: playlist.external_urls,
            owner: playlist.owner.display_name
        }));
    }

    // 🎵 Format chi tiết bài hát
    formatTrackDetails(track) {
        return {
            id: track.id,
            name: track.name,
            artist: track.artists.map(artist => artist.name).join(', '),
            album: track.album.name,
            duration: this.formatDuration(track.duration_ms),
            popularity: track.popularity,
            preview_url: track.preview_url,
            external_urls: track.external_urls,
            images: track.album.images,
            release_date: track.album.release_date,
            genres: track.artists.flatMap(artist => artist.genres || []),
            explicit: track.explicit,
            available_markets: track.available_markets
        };
    }

    // 🎤 Format chi tiết nghệ sĩ
    formatArtistDetails(artist) {
        return {
            id: artist.id,
            name: artist.name,
            popularity: artist.popularity,
            genres: artist.genres,
            followers: artist.followers.total,
            images: artist.images,
            external_urls: artist.external_urls
        };
    }

    // ⏱️ Format thời lượng
    formatDuration(ms) {
        const minutes = Math.floor(ms / 60000);
        const seconds = Math.floor((ms % 60000) / 1000);
        return `${minutes}:${seconds.toString().padStart(2, '0')}`;
    }

    // 🎵 Lấy featured playlists
    async getFeaturedPlaylists(limit = 10) {
        try {
            await this.ensureValidToken();

            const response = await fetch(`${this.baseURL}/browse/featured-playlists?limit=${limit}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatPlaylistResults(data.playlists);
        } catch (error) {
            console.error('❌ Error getting featured playlists:', error);
            throw error;
        }
    }

    // 🎯 Lấy new releases
    async getNewReleases(limit = 20) {
        try {
            await this.ensureValidToken();

            const response = await fetch(`${this.baseURL}/browse/new-releases?limit=${limit}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return this.formatAlbumResults(data.albums);
        } catch (error) {
            console.error('❌ Error getting new releases:', error);
            throw error;
        }
    }
}

// 🚀 Tạo instance global
window.spotifyAPI = new SpotifyAPI();

// 🎵 Helper functions để sử dụng dễ dàng
window.SpotifyHelpers = {
    // Tìm kiếm bài hát
    searchSongs: async (query, limit = 10) => {
        try {
            return await window.spotifyAPI.searchTracks(query, limit);
        } catch (error) {
            console.error('Error searching songs:', error);
            return [];
        }
    },

    // Tìm kiếm nghệ sĩ
    searchArtists: async (query, limit = 10) => {
        try {
            return await window.spotifyAPI.searchArtists(query, limit);
        } catch (error) {
            console.error('Error searching artists:', error);
            return [];
        }
    },

    // Tìm kiếm theo tâm trạng
    searchByMood: async (mood, limit = 10) => {
        try {
            return await window.spotifyAPI.getMoodBasedTracks(mood, limit);
        } catch (error) {
            console.error('Error searching by mood:', error);
            return [];
        }
    },

    // Tìm kiếm thông minh
    smartSearch: async (query, limit = 10) => {
        try {
            return await window.spotifyAPI.smartSearch(query, limit);
        } catch (error) {
            console.error('Error in smart search:', error);
            return { tracks: [], artists: [], albums: [], playlists: [], total: 0 };
        }
    }
};

console.log('🎵 Spotify API Integration loaded successfully!');
