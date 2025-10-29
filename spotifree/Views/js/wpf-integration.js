
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

const MusicCommands = {
    searchAndPlay: (query) => {
        sendToWPF('searchAndPlay', { query: query });
    },

    play: () => {
        sendToWPF('playMusic');
    },

    pause: () => {
        sendToWPF('pauseMusic');
    },

    stop: () => {
        sendToWPF('stopMusic');
    },

    resume: () => {
        sendToWPF('resumeMusic');
    },

    next: () => {
        sendToWPF('nextTrack');
    },

    previous: () => {
        sendToWPF('previousTrack');
    },

    shuffle: () => {
        sendToWPF('shufflePlaylist');
    },

    repeat: (mode = 'one') => {
        sendToWPF('repeatMode', { mode: mode });
    },

    setVolume: (volume) => {
        sendToWPF('setVolume', { volume: Math.max(0, Math.min(100, volume)) });
    },

    volumeUp: () => {
        sendToWPF('volumeUp');
    },

    volumeDown: () => {
        sendToWPF('volumeDown');
    }
};

const LibraryCommands = {
    searchLibrary: (query, type = 'all') => {
        sendToWPF('searchLibrary', {
            query: query,
            type: type // 'all', 'song', 'artist', 'album', 'playlist'
        });
    },

    getSongs: (limit = 50) => {
        sendToWPF('getSongs', { limit: limit });
    },

    getPlaylists: () => {
        sendToWPF('getPlaylists');
    },

    createPlaylist: (name, description = '') => {
        sendToWPF('createPlaylist', {
            name: name,
            description: description
        });
    },

    addToPlaylist: (playlistId, songId) => {
        sendToWPF('addToPlaylist', {
            playlistId: playlistId,
            songId: songId
        });
    },

    removeFromPlaylist: (playlistId, songId) => {
        sendToWPF('removeFromPlaylist', {
            playlistId: playlistId,
            songId: songId
        });
    }
};

const AnalyticsCommands = {
    getListeningStats: (period = 'week') => {
        sendToWPF('getListeningStats', { period: period });
    },

    getTopSongs: (limit = 10) => {
        sendToWPF('getTopSongs', { limit: limit });
    },

    getTopArtists: (limit = 10) => {
        sendToWPF('getTopArtists', { limit: limit });
    },

    getTopGenres: (limit = 5) => {
        sendToWPF('getTopGenres', { limit: limit });
    }
};

const SettingsCommands = {
    getSettings: () => {
        sendToWPF('getSettings');
    },

    updateSettings: (settings) => {
        sendToWPF('updateSettings', { settings: settings });
    },

    setTheme: (theme) => {
        sendToWPF('setTheme', { theme: theme });
    },

    setLanguage: (language) => {
        sendToWPF('setLanguage', { language: language });
    }
};

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

function handleSearchResults(results) {
    if (results && results.length > 0) {
        const message = `🔍 Tìm thấy ${results.length} kết quả:\n\n`;
        const songList = results.slice(0, 5).map((song, index) =>
            `${index + 1}. ${song.title} - ${song.artist}`
        ).join('\n');

        addMessage(message + songList, false);

        const quickReplies = results.slice(0, 3).map(song => `Phát "${song.title}"`);
        quickReplies.push('Tìm khác', 'Xem tất cả');
        showQuickReplies(quickReplies);
    } else {
        addMessage('😔 Không tìm thấy kết quả nào. Hãy thử từ khóa khác!', false);
        showQuickReplies(['Tìm khác', 'Nhạc vui', 'Nhạc buồn', 'Nhạc chill']);
    }
}

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

function handleLibraryData(data) {
    if (data.type === 'songs') {
        const message = `📚 Thư viện của bạn có ${data.songs.length} bài hát`;
        addMessage(message, false);
    } else if (data.type === 'playlists') {
        const message = `📋 Bạn có ${data.playlists.length} playlist`;
        addMessage(message, false);
    }
}

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

function initializeWPFIntegration() {
    setupWPFListener();
    console.log('🎧 WPF Integration initialized');
}

window.MusicCommands = MusicCommands;
window.LibraryCommands = LibraryCommands;
window.AnalyticsCommands = AnalyticsCommands;
window.SettingsCommands = SettingsCommands;

document.addEventListener('DOMContentLoaded', initializeWPFIntegration);
