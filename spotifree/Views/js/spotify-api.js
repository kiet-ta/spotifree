class SpotifyAPI {
    constructor() {
        this.clientId = '8105bc07cf1a4611a714f641cf61cf2d';
        this.clientSecret = 'f9e2f2ba56144e67beb3e65fde494d21';
        this.redirectUri = 'https://oauth.pstmn.io/v1/callback';
        this.accessToken = null;
        this.tokenExpiry = null;
        this.baseURL = 'https://api.spotify.com/v1';
        this.initializeAuth();
    }

    async initializeAuth() {
        try {
            // PRIORITY 1: Check if token was injected by C# (from MainWindow)
            if (window.__ACCESS_TOKEN__) {
                this.accessToken = window.__ACCESS_TOKEN__;
                this.tokenExpiry = new Date(Date.now() + 3600 * 1000);
                this.tokenType = "Bearer";
                console.log('🎵 Token loaded from C# injection');
                console.log('   ✅ Token type: Bearer');
                console.log('   ⏰ Expires at:', this.tokenExpiry.toLocaleString('vi-VN'));
                
                // Save to localStorage
                localStorage.setItem('spotify_access_token', this.accessToken);
                localStorage.setItem('spotify_token_expiry', this.tokenExpiry.toISOString());
                return;
            }

            // PRIORITY 2: Check localStorage for existing valid token
            const savedToken = localStorage.getItem('spotify_access_token');
            const savedExpiry = localStorage.getItem('spotify_token_expiry');

            if (savedToken && savedExpiry && new Date() < new Date(savedExpiry)) {
                this.accessToken = savedToken;
                this.tokenExpiry = new Date(savedExpiry);
                this.tokenType = "Bearer";
                console.log('🎵 Token loaded from localStorage cache');
                console.log('   ⏰ Expires at:', this.tokenExpiry.toLocaleString('vi-VN'));
                return;
            }

            // PRIORITY 3: Try to get Client Credentials token (fallback)
            if (this.clientId && this.clientSecret) {
                console.log('🔄 Getting new token with Client Credentials...');
                await this.getClientCredentialsToken();
                return;
            }

            // No token available
            throw new Error('❌ No valid token available. Please login first.');
        } catch (error) {
            console.error('❌ Error initializing Spotify auth:', error);
            throw error;
        }
    }


    async getClientCredentialsToken() {
        try {
            if (!this.clientId || !this.clientSecret) {
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

            localStorage.setItem('spotify_access_token', this.accessToken);
            localStorage.setItem('spotify_token_expiry', this.tokenExpiry.toISOString());

            console.log('🎵 Spotify token obtained successfully');
        } catch (error) {
            console.error('❌ Error getting Spotify token:', error);
            this.showSpotifyError(error.message);
            throw error;
        }
    }

    showSpotifyError(message) {
        if (window.addMessage) {
            window.addMessage(`🚨 ${message}`, false);
        } else {
            console.error('Spotify Error:', message);
        }
    }

    async ensureValidToken() {
        if (!this.accessToken || new Date() >= this.tokenExpiry) {
            await this.getClientCredentialsToken();
        }
    }

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

    formatDuration(ms) {
        const minutes = Math.floor(ms / 60000);
        const seconds = Math.floor((ms % 60000) / 1000);
        return `${minutes}:${seconds.toString().padStart(2, '0')}`;
    }

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

    async getNewTracks(limit = 20) {
        try {
            await this.ensureValidToken();

            // Get current year for filtering
            const currentYear = new Date().getFullYear();
            
            // Search for new tracks with multiple strategies
            const params = new URLSearchParams({
                q: `year:${currentYear} tag:new`,
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
            
            if (!data.tracks || !data.tracks.items || data.tracks.items.length === 0) {
                // Fallback: search with just year if tag:new doesn't work
                console.log('🔄 Trying fallback search without tag:new...');
                return await this.searchTracks(`year:${currentYear}`, limit);
            }
            
            return this.formatTrackResults(data.tracks);
        } catch (error) {
            console.error('❌ Error getting new tracks:', error);
            throw error;
        }
    }

    async getSavedTracks(limit = 10, offset = 0) {
        try {
            await this.ensureValidToken();

            const params = new URLSearchParams({
                limit: limit.toString(),
                offset: offset.toString(),
                market: 'VN'
            });

            const response = await fetch(`${this.baseURL}/me/tracks?${params}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                if (response.status === 401) {
                    console.warn('⚠️ Unauthorized - Token có thể đã hết hạn hoặc không có quyền user-library-read');
                }
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            
            // Format saved tracks data
            const formattedTracks = data.items.map(item => ({
                id: item.track.id,
                name: item.track.name,
                artist: item.track.artists.map(artist => artist.name).join(', '),
                album: item.track.album.name,
                duration: this.formatDuration(item.track.duration_ms),
                popularity: item.track.popularity,
                preview_url: item.track.preview_url,
                external_urls: item.track.external_urls,
                images: item.track.album.images,
                release_date: item.track.album.release_date,
                added_at: item.added_at,
                genres: item.track.artists.flatMap(artist => artist.genres || [])
            }));

            return {
                items: formattedTracks,
                total: data.total,
                limit: data.limit,
                offset: data.offset
            };
        } catch (error) {
            console.error('❌ Error getting saved tracks:', error);
            throw error;
        }
    }

    async getPodcasts(limit = 5) {
        try {
            await this.ensureValidToken();

            const params = new URLSearchParams({
                q: 'genre:podcast',
                type: 'show',
                market: 'VN',
                limit: limit.toString()
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
            
            // Format podcast/show data
            if (!data.shows || !data.shows.items) {
                return [];
            }

            return data.shows.items.map(show => ({
                id: show.id,
                name: show.name,
                publisher: show.publisher,
                description: show.description,
                images: show.images,
                total_episodes: show.total_episodes,
                external_urls: show.external_urls
            }));
        } catch (error) {
            console.error('❌ Error getting podcasts:', error);
            throw error;
        }
    }

    async getCategories(limit = 5) {
        try {
            await this.ensureValidToken();

            const params = new URLSearchParams({
                country: 'VN',
                locale: 'vi_VN',
                limit: limit.toString()
            });

            const response = await fetch(`${this.baseURL}/browse/categories?${params}`, {
                headers: {
                    'Authorization': `Bearer ${this.accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            
            // Format categories data
            if (!data.categories || !data.categories.items) {
                return [];
            }

            return data.categories.items.map(category => ({
                id: category.id,
                name: category.name,
                icons: category.icons,
                href: category.href
            }));
        } catch (error) {
            console.error('❌ Error getting categories:', error);
            throw error;
        }
    }
}

window.spotifyAPI = new SpotifyAPI();

window.SpotifyHelpers = {
    searchSongs: async (query, limit = 10) => {
        try {
            return await window.spotifyAPI.searchTracks(query, limit);
        } catch (error) {
            console.error('Error searching songs:', error);
            return [];
        }
    },

    searchArtists: async (query, limit = 10) => {
        try {
            return await window.spotifyAPI.searchArtists(query, limit);
        } catch (error) {
            console.error('Error searching artists:', error);
            return [];
        }
    },

    searchByMood: async (mood, limit = 10) => {
        try {
            return await window.spotifyAPI.getMoodBasedTracks(mood, limit);
        } catch (error) {
            console.error('Error searching by mood:', error);
            return [];
        }
    },

    smartSearch: async (query, limit = 10) => {
        try {
            return await window.spotifyAPI.smartSearch(query, limit);
        } catch (error) {
            console.error('Error in smart search:', error);
            return { tracks: [], artists: [], albums: [], playlists: [], total: 0 };
        }
    },

    getSavedTracks: async (limit = 10, offset = 0) => {
        try {
            return await window.spotifyAPI.getSavedTracks(limit, offset);
        } catch (error) {
            console.error('Error getting saved tracks:', error);
            return { items: [], total: 0, limit: limit, offset: offset };
        }
    },

    getPodcasts: async (limit = 5) => {
        try {
            return await window.spotifyAPI.getPodcasts(limit);
        } catch (error) {
            console.error('Error getting podcasts:', error);
            return [];
        }
    },

    getCategories: async (limit = 5) => {
        try {
            return await window.spotifyAPI.getCategories(limit);
        } catch (error) {
            console.error('Error getting categories:', error);
            return [];
        }
    },

    getNewTracks: async (limit = 20) => {
        try {
            return await window.spotifyAPI.getNewTracks(limit);
        } catch (error) {
            console.error('Error getting new tracks:', error);
            return [];
        }
    }
};

console.log('🎵 Spotify API Integration loaded successfully!');


/*
/api/private-docs:1   Failed to load resource: the server responded with a status of 401 ()
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
Tracking Prevention blocked access to storage for <URL>.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/ot_close.svg.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/ot_close.svg.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/ot_close.svg.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/ot_close.svg.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/ot_company_logo.png.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/ot_company_logo.png.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/ot_company_logo.png.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/ot_company_logo.png.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/powered_by_logo.svg.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/powered_by_logo.svg.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/powered_by_logo.svg.
developer.spotify.com/:1  Tracking Prevention blocked access to storage for https://cdn.cookielaw.org/logos/static/powered_by_logo.svg.
[NEW] Explain Console errors by using Copilot in Edge: click
         
         to explain an error. 
        Learn more
        Don't show again

*/
