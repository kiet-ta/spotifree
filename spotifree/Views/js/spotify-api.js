// 🎵 Spotify API Integration
// File này tích hợp với Spotify Web API để tìm kiếm và lấy thông tin nhạc thực tế

class SpotifyAPI {
    constructor() {
        this.clientId = 'YOUR_SPOTIFY_CLIENT_ID'; // Thay bằng Client ID thực của bạn
        this.clientSecret = 'YOUR_SPOTIFY_CLIENT_SECRET'; // Thay bằng Client Secret thực của bạn
        this.redirectUri = 'http://localhost:3000/callback'; // URL callback
        this.accessToken = null;
        this.tokenExpiry = null;
        this.baseURL = 'https://api.spotify.com/v1';

        // Khởi tạo
        this.initializeAuth();
    }

    // 🔐 Khởi tạo xác thực
    async initializeAuth() {
        try {
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
        }
    }

    // 🔑 Lấy token bằng Client Credentials Flow
    async getClientCredentialsToken() {
        try {
            const response = await fetch('https://accounts.spotify.com/api/token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'Authorization': 'Basic ' + btoa(this.clientId + ':' + this.clientSecret)
                },
                body: 'grant_type=client_credentials'
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
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
            throw error;
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
