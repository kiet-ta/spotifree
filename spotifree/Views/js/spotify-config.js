// 🎵 Spotify API Configuration
// File cấu hình cho Spotify API integration

const SpotifyConfig = {
    // 🔑 API Credentials - THAY ĐỔI CÁC GIÁ TRỊ NÀY
    credentials: {
        // Lấy từ Spotify Developer Dashboard: https://developer.spotify.com/dashboard
        clientId: '8105bc07cf1a4611a714f641cf61cf2d',
        clientSecret: 'f9e2f2ba56144e67beb3e65fde494d21',
        redirectUri: 'http://localhost:3000/callback'
    },

    // 🌍 Market settings
    market: 'VN', // Thị trường Việt Nam
    language: 'vi', // Ngôn ngữ

    // 🎵 Search settings
    search: {
        defaultLimit: 10,
        maxLimit: 50,
        timeout: 10000 // 10 seconds
    },

    // 🎭 Mood-based search queries
    moodQueries: {
        happy: [
            'mood:happy',
            'energy:high',
            'valence:high',
            'danceability:high',
            'tempo:fast'
        ],
        sad: [
            'mood:sad',
            'energy:low',
            'valence:low',
            'tempo:slow'
        ],
        chill: [
            'mood:chill',
            'energy:medium',
            'valence:medium',
            'acoustic:high',
            'instrumentalness:high'
        ],
        energetic: [
            'energy:high',
            'danceability:high',
            'tempo:fast',
            'loudness:high'
        ],
        romantic: [
            'mood:romantic',
            'valence:high',
            'acoustic:high',
            'tempo:medium'
        ],
        party: [
            'energy:high',
            'danceability:high',
            'tempo:fast',
            'loudness:high'
        ]
    },

    // 🎵 Genre mappings
    genres: {
        'pop': 'pop',
        'rock': 'rock',
        'hip-hop': 'hip-hop',
        'r&b': 'r&b',
        'electronic': 'electronic',
        'jazz': 'jazz',
        'classical': 'classical',
        'country': 'country',
        'folk': 'folk',
        'blues': 'blues',
        'reggae': 'reggae',
        'latin': 'latin',
        'k-pop': 'k-pop',
        'indie': 'indie'
    },

    // 🎯 Popular artists by genre
    popularArtists: {
        'pop': ['Taylor Swift', 'Ariana Grande', 'Ed Sheeran', 'Dua Lipa', 'Billie Eilish'],
        'rock': ['Queen', 'AC/DC', 'Led Zeppelin', 'The Beatles', 'Pink Floyd'],
        'hip-hop': ['Drake', 'Kendrick Lamar', 'Travis Scott', 'Post Malone', 'Kanye West'],
        'r&b': ['The Weeknd', 'Frank Ocean', 'SZA', 'H.E.R.', 'Alicia Keys'],
        'electronic': ['Daft Punk', 'Skrillex', 'Deadmau5', 'Calvin Harris', 'Marshmello'],
        'k-pop': ['BTS', 'BLACKPINK', 'TWICE', 'Red Velvet', 'EXO']
    },

    // 🎵 Default playlists for moods
    defaultPlaylists: {
        happy: {
            name: 'Nhạc Vui Vẻ',
            description: 'Những bài hát sôi động, tích cực',
            tracks: [
                'Happy - Pharrell Williams',
                'Can\'t Stop The Feeling - Justin Timberlake',
                'Uptown Funk - Bruno Mars',
                'Good as Hell - Lizzo',
                'Walking on Sunshine - Katrina and the Waves'
            ]
        },
        sad: {
            name: 'Nhạc Buồn',
            description: 'Những bài hát sâu lắng, cảm động',
            tracks: [
                'Someone Like You - Adele',
                'Fix You - Coldplay',
                'Let Her Go - Passenger',
                'All Too Well - Taylor Swift',
                'Stay - Rihanna ft. Mikky Ekko'
            ]
        },
        chill: {
            name: 'Nhạc Chill',
            description: 'Nhạc thư giãn, nhẹ nhàng',
            tracks: [
                'Let Her Go - Passenger',
                'Ocean Eyes - Billie Eilish',
                'ILY - Surf Mesa',
                'Midnight City - M83',
                'The Night We Met - Lord Huron'
            ]
        }
    },

    // 🎵 Error messages
    errorMessages: {
        noResults: '😔 Không tìm thấy kết quả nào. Hãy thử từ khóa khác!',
        apiError: '❌ Có lỗi khi kết nối với Spotify. Hãy thử lại sau!',
        networkError: '🌐 Lỗi kết nối mạng. Vui lòng kiểm tra internet!',
        authError: '🔐 Lỗi xác thực. Vui lòng kiểm tra cấu hình API!',
        rateLimit: '⏰ Quá nhiều yêu cầu. Vui lòng chờ một chút!'
    },

    // 🎵 Success messages
    successMessages: {
        foundResults: '🎧 Tìm thấy {count} kết quả!',
        playingTrack: '▶️ Đang phát: {track} - {artist}',
        addedToPlaylist: '✅ Đã thêm vào playlist!',
        createdPlaylist: '📋 Đã tạo playlist mới!'
    },

    // 🎵 Feature flags
    features: {
        enablePreview: true,
        enableLyrics: false, // Cần API khác
        enableRecommendations: true,
        enableMoodDetection: true,
        enableSocialSharing: false,
        enableOfflineMode: false
    },

    // 🎵 Cache settings
    cache: {
        enabled: true,
        duration: 300000, // 5 minutes
        maxSize: 100 // items
    },

    // 🎵 UI settings
    ui: {
        showAlbumArt: true,
        showDuration: true,
        showPopularity: true,
        showGenres: true,
        maxResultsDisplay: 5
    }
};

// 🚀 Export configuration
window.SpotifyConfig = SpotifyConfig;

// 🎵 Helper functions
window.SpotifyConfigHelpers = {
    // Lấy query cho tâm trạng
    getMoodQuery: (mood) => {
        return SpotifyConfig.moodQueries[mood] || ['mood:neutral'];
    },

    // Lấy nghệ sĩ phổ biến theo thể loại
    getPopularArtists: (genre) => {
        return SpotifyConfig.popularArtists[genre] || [];
    },

    // Lấy thông báo lỗi
    getErrorMessage: (errorType) => {
        return SpotifyConfig.errorMessages[errorType] || '❌ Có lỗi xảy ra!';
    },

    // Lấy thông báo thành công
    getSuccessMessage: (messageType, data = {}) => {
        let message = SpotifyConfig.successMessages[messageType] || '✅ Thành công!';
        Object.keys(data).forEach(key => {
            message = message.replace(`{${key}}`, data[key]);
        });
        return message;
    },

    // Kiểm tra tính năng có được bật
    isFeatureEnabled: (feature) => {
        return SpotifyConfig.features[feature] || false;
    },

    // Lấy cấu hình UI
    getUIConfig: (key) => {
        return SpotifyConfig.ui[key];
    }
};

console.log('🎵 Spotify Configuration loaded successfully!');
