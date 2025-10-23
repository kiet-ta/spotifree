// 🎧 Tích hợp Chatbot với WPF Application
// File này chứa các hàm hỗ trợ tích hợp chatbot với WPF WebView2

// 📡 Gửi message đến WPF application
function sendToWPF(action, data = {}) {
    if (window.chrome && window.chrome.webview) {
        const message = {
            action: action,
            timestamp: new Date().toISOString(),
            ...data
        };

        try {
            window.chrome.webview.postMessage(JSON.stringify(message));
            console.log('📤 Sent to WPF:', message);
        } catch (error) {
            console.error('❌ Error sending to WPF:', error);
        }
    } else {
        console.warn('⚠️ WebView2 not available - running in browser mode');
    }
}

// 🎵 Các lệnh điều khiển nhạc
const MusicCommands = {
    // Tìm kiếm và phát nhạc
    searchAndPlay: (query) => {
        sendToWPF('searchAndPlay', { query: query });
    },

    // Phát nhạc
    play: () => {
        sendToWPF('playMusic');
    },

    // Tạm dừng
    pause: () => {
        sendToWPF('pauseMusic');
    },

    // Dừng
    stop: () => {
        sendToWPF('stopMusic');
    },

    // Phát tiếp
    resume: () => {
        sendToWPF('resumeMusic');
    },

    // Phát bài tiếp theo
    next: () => {
        sendToWPF('nextTrack');
    },

    // Phát bài trước
    previous: () => {
        sendToWPF('previousTrack');
    },

    // Phát ngẫu nhiên
    shuffle: () => {
        sendToWPF('shufflePlaylist');
    },

    // Lặp lại
    repeat: (mode = 'one') => {
        sendToWPF('repeatMode', { mode: mode });
    },

    // Điều chỉnh âm lượng
    setVolume: (volume) => {
        sendToWPF('setVolume', { volume: Math.max(0, Math.min(100, volume)) });
    },

    // Tăng âm lượng
    volumeUp: () => {
        sendToWPF('volumeUp');
    },

    // Giảm âm lượng
    volumeDown: () => {
        sendToWPF('volumeDown');
    }
};

// 📚 Quản lý thư viện nhạc
const LibraryCommands = {
    // Tìm kiếm trong thư viện
    searchLibrary: (query, type = 'all') => {
        sendToWPF('searchLibrary', {
            query: query,
            type: type // 'all', 'song', 'artist', 'album', 'playlist'
        });
    },

    // Lấy danh sách bài hát
    getSongs: (limit = 50) => {
        sendToWPF('getSongs', { limit: limit });
    },

    // Lấy danh sách playlist
    getPlaylists: () => {
        sendToWPF('getPlaylists');
    },

    // Tạo playlist mới
    createPlaylist: (name, description = '') => {
        sendToWPF('createPlaylist', {
            name: name,
            description: description
        });
    },

    // Thêm bài hát vào playlist
    addToPlaylist: (playlistId, songId) => {
        sendToWPF('addToPlaylist', {
            playlistId: playlistId,
            songId: songId
        });
    },

    // Xóa bài hát khỏi playlist
    removeFromPlaylist: (playlistId, songId) => {
        sendToWPF('removeFromPlaylist', {
            playlistId: playlistId,
            songId: songId
        });
    }
};

// 📊 Thống kê và phân tích
const AnalyticsCommands = {
    // Lấy thống kê nghe nhạc
    getListeningStats: (period = 'week') => {
        sendToWPF('getListeningStats', { period: period });
    },

    // Lấy bài hát được nghe nhiều nhất
    getTopSongs: (limit = 10) => {
        sendToWPF('getTopSongs', { limit: limit });
    },

    // Lấy nghệ sĩ được nghe nhiều nhất
    getTopArtists: (limit = 10) => {
        sendToWPF('getTopArtists', { limit: limit });
    },

    // Lấy thể loại nhạc yêu thích
    getTopGenres: (limit = 5) => {
        sendToWPF('getTopGenres', { limit: limit });
    }
};

// ⚙️ Cài đặt ứng dụng
const SettingsCommands = {
    // Lấy cài đặt hiện tại
    getSettings: () => {
        sendToWPF('getSettings');
    },

    // Cập nhật cài đặt
    updateSettings: (settings) => {
        sendToWPF('updateSettings', { settings: settings });
    },

    // Đặt theme
    setTheme: (theme) => {
        sendToWPF('setTheme', { theme: theme });
    },

    // Đặt ngôn ngữ
    setLanguage: (language) => {
        sendToWPF('setLanguage', { language: language });
    }
};

// 🔄 Lắng nghe phản hồi từ WPF
function setupWPFListener() {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.addEventListener('message', (event) => {
            try {
                const data = JSON.parse(event.data);
                handleWPFResponse(data);
            } catch (error) {
                console.error('❌ Error parsing WPF response:', error);
            }
        });
    }
}

// 📨 Xử lý phản hồi từ WPF
function handleWPFResponse(data) {
    console.log('📥 Received from WPF:', data);

    switch (data.action) {
        case 'searchResults':
            handleSearchResults(data.results);
            break;
        case 'playbackStatus':
            handlePlaybackStatus(data.status);
            break;
        case 'libraryData':
            handleLibraryData(data.data);
            break;
        case 'error':
            handleError(data.error);
            break;
        default:
            console.log('🤷 Unknown action:', data.action);
    }
}

// 🎵 Xử lý kết quả tìm kiếm
function handleSearchResults(results) {
    if (results && results.length > 0) {
        const message = `🔍 Tìm thấy ${results.length} kết quả:\n\n`;
        const songList = results.slice(0, 5).map((song, index) =>
            `${index + 1}. ${song.title} - ${song.artist}`
        ).join('\n');

        addMessage(message + songList, false);

        // Hiển thị quick replies để phát nhạc
        const quickReplies = results.slice(0, 3).map(song => `Phát "${song.title}"`);
        quickReplies.push('Tìm khác', 'Xem tất cả');
        showQuickReplies(quickReplies);
    } else {
        addMessage('😔 Không tìm thấy kết quả nào. Hãy thử từ khóa khác!', false);
        showQuickReplies(['Tìm khác', 'Nhạc vui', 'Nhạc buồn', 'Nhạc chill']);
    }
}

// ▶️ Xử lý trạng thái phát nhạc
function handlePlaybackStatus(status) {
    const statusMessages = {
        'playing': '▶️ Đang phát nhạc',
        'paused': '⏸️ Đã tạm dừng',
        'stopped': '⏹️ Đã dừng',
        'loading': '⏳ Đang tải...',
        'error': '❌ Có lỗi xảy ra'
    };

    const message = statusMessages[status.state] || '🎵 Trạng thái không xác định';
    addMessage(message, false);

    if (status.currentSong) {
        addMessage(`🎵 Hiện tại: ${status.currentSong.title} - ${status.currentSong.artist}`, false);
    }
}

// 📚 Xử lý dữ liệu thư viện
function handleLibraryData(data) {
    if (data.type === 'songs') {
        const message = `📚 Thư viện của bạn có ${data.songs.length} bài hát`;
        addMessage(message, false);
    } else if (data.type === 'playlists') {
        const message = `📋 Bạn có ${data.playlists.length} playlist`;
        addMessage(message, false);
    }
}

// ❌ Xử lý lỗi
function handleError(error) {
    const errorMessages = {
        'file_not_found': '❌ Không tìm thấy file nhạc',
        'playback_error': '❌ Lỗi phát nhạc',
        'search_error': '❌ Lỗi tìm kiếm',
        'library_error': '❌ Lỗi truy cập thư viện',
        'network_error': '❌ Lỗi kết nối mạng'
    };

    const message = errorMessages[error.code] || '❌ Có lỗi xảy ra: ' + error.message;
    addMessage(message, false);
}

// 🚀 Khởi tạo tích hợp WPF
function initializeWPFIntegration() {
    setupWPFListener();
    console.log('🎧 WPF Integration initialized');
}

// Export các commands để sử dụng trong chatbot
window.MusicCommands = MusicCommands;
window.LibraryCommands = LibraryCommands;
window.AnalyticsCommands = AnalyticsCommands;
window.SettingsCommands = SettingsCommands;

// Khởi tạo khi DOM loaded
document.addEventListener('DOMContentLoaded', initializeWPFIntegration);
