// ⚡ Tạo nút toggle mở/đóng chatbot
const chatbotToggle = document.createElement('div');
chatbotToggle.id = 'chatbot-toggle';
chatbotToggle.innerHTML = '💬';
document.body.appendChild(chatbotToggle);

const chatbotContainer = document.getElementById('chatbot-container');
const chatbotClose = document.getElementById('chatbot-close');

chatbotContainer.style.display = 'none';

document.addEventListener('DOMContentLoaded', () => {
    const welcomeTime = document.getElementById('welcome-time');
    if (welcomeTime) {
        welcomeTime.textContent = getCurrentTime();
    }
});

chatbotToggle.addEventListener('click', () => {
    chatbotContainer.style.display = 'flex';
    chatbotToggle.style.display = 'none';
    setTimeout(() => {
        document.getElementById('chat-input').focus();
    }, 300);
});

chatbotClose.addEventListener('click', () => {
    chatbotContainer.style.display = 'none';
    chatbotToggle.style.display = 'flex';
});


function sendMessage() {
    const input = document.getElementById('chat-input');
    const msg = input.value.trim();
    if (!msg) return;

    addMessage(msg, true);
    input.value = '';

    showTypingIndicator();

    setTimeout(() => {
        hideTypingIndicator();
    getBotReply(msg);
    }, 1000 + Math.random() * 1000);
}

function addMessage(text, isUser) {
    const log = document.getElementById('chatlog');
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message';

    const msg = document.createElement('div');
    msg.className = isUser ? 'user-msg' : 'bot-msg';
    msg.innerText = text;

    const timeDiv = document.createElement('div');
    timeDiv.className = 'message-time';
    timeDiv.textContent = getCurrentTime();

    messageDiv.appendChild(msg);
    messageDiv.appendChild(timeDiv);
    log.appendChild(messageDiv);

    log.scrollTo({
        top: log.scrollHeight,
        behavior: 'smooth'
    });
}

function getCurrentTime() {
    const now = new Date();
    return now.toLocaleTimeString('vi-VN', {
        hour: '2-digit',
        minute: '2-digit'
    });
}

function showTypingIndicator() {
    const log = document.getElementById('chatlog');
    const typingDiv = document.createElement('div');
    typingDiv.id = 'typing-indicator';
    typingDiv.className = 'typing-indicator';
    typingDiv.innerHTML = `
        <div class="typing-dot"></div>
        <div class="typing-dot"></div>
        <div class="typing-dot"></div>
    `;
    log.appendChild(typingDiv);
    log.scrollTo({
        top: log.scrollHeight,
        behavior: 'smooth'
    });
}

function hideTypingIndicator() {
    const typingIndicator = document.getElementById('typing-indicator');
    if (typingIndicator) {
        typingIndicator.remove();
    }
}

async function getBotReply(input) {
    const originalInput = input;
    input = input.toLowerCase().trim();

    if (!window.chatContext) {
        window.chatContext = {
            lastMood: null,
            conversationHistory: [],
            userPreferences: {},
            isFirstTime: true
        };
    }

    window.chatContext.conversationHistory.push({
        user: originalInput,
        timestamp: new Date()
    });

    let response = "";
    let quickReplies = [];
    let spotifyResults = null;

    if (window.ChatbotContext && window.ContextHelpers) {
        if (window.chatContext.isFirstTime) {
            response = window.ChatbotContext.contextualResponses.firstTime;
            quickReplies = ['Tìm nhạc', 'Tôi đang vui', 'Tôi đang buồn', 'Nhạc chill'];
            window.chatContext.isFirstTime = false;
            addMessage(response, false);
            if (quickReplies.length > 0) {
                setTimeout(() => showQuickReplies(quickReplies), 500);
            }
            return;
        }
    }

    if (input.includes('phát') || input.includes('mở') || input.includes('nghe') || input.includes('tìm')) {
        const songMatch = input.match(/(?:phát|mở|nghe|tìm)\s+(.+)/);
        if (songMatch) {
            const songName = songMatch[1].trim();
            response = `🎵 Đang tìm kiếm "${songName}" trên Spotify...`;
            addMessage(response, false);

            try {
                spotifyResults = await window.SpotifyHelpers.smartSearch(songName, 10);

                if (spotifyResults.tracks && spotifyResults.tracks.length > 0) {
                    const tracks = spotifyResults.tracks.slice(0, 5);
                    let trackList = `🎧 Tìm thấy ${spotifyResults.tracks.length} bài hát:\n\n`;

                    tracks.forEach((track, index) => {
                        trackList += `${index + 1}. **${track.name}** - ${track.artist}\n`;
                        if (track.album) trackList += `   📀 Album: ${track.album}\n`;
                        if (track.duration) trackList += `   ⏱️ ${track.duration}\n`;
                        trackList += `   ⭐ ${track.popularity}/100\n\n`;
                    });

                    addMessage(trackList, false);
                    quickReplies = [
                        `Phát "${tracks[0].name}"`,
                        `Phát "${tracks[1]?.name || 'bài khác'}"`,
                        'Tìm khác',
                        'Xem tất cả'
                    ];

                    if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(
                            JSON.stringify({
                                action: 'searchAndPlay',
                                query: songName,
                                spotifyResults: spotifyResults
                            })
                        );
                    }
                } else {
                    response = "😔 Không tìm thấy kết quả nào cho '" + songName + "'. Hãy thử từ khóa khác!";
                    quickReplies = ['Tìm khác', 'Nhạc vui', 'Nhạc buồn', 'Nhạc chill'];
                }
            } catch (error) {
                console.error('Error searching Spotify:', error);
                response = "❌ Có lỗi khi tìm kiếm trên Spotify. Hãy thử lại sau!";
                quickReplies = ['Thử lại', 'Tìm khác', 'Trợ giúp'];
            }
        } else {
            response = "🎵 Bạn muốn nghe bài gì? Tôi có thể tìm kiếm và phát nhạc cho bạn!";
            quickReplies = ['Nhạc vui', 'Nhạc buồn', 'Nhạc chill', 'Top hits'];
        }
    }
    else if (input.includes('vui') || input.includes('happy') || input.includes('hạnh phúc')) {
        window.chatContext.lastMood = 'happy';

        if (window.ContextHelpers) {
            response = window.ContextHelpers.getMoodResponse('happy') || "Tuyệt vời! Tâm trạng vui vẻ của bạn rất đáng yêu! 🎉";
        } else {
            response = "Tuyệt vời! Tâm trạng vui vẻ của bạn rất đáng yêu! 🎉";
    }

    addMessage(response, false);

        try {
            const happyTracks = await window.SpotifyHelpers.searchByMood('happy', 5);

            if (happyTracks && happyTracks.length > 0) {
                let trackList = `🎉 Gợi ý nhạc vui vẻ:\n\n`;
                happyTracks.forEach((track, index) => {
                    trackList += `${index + 1}. **${track.name}** - ${track.artist}\n`;
                    if (track.popularity) trackList += `   ⭐ ${track.popularity}/100\n`;
                });

                addMessage(trackList, false);
                quickReplies = [
                    `Phát "${happyTracks[0].name}"`,
                    `Phát "${happyTracks[1]?.name || 'bài khác'}"`,
                    'Tôi muốn nhạc buồn',
                    'Tìm khác'
                ];
            } else {
                const fallbackSongs = [
                    'Happy - Pharrell Williams',
                    'Can\'t Stop The Feeling - Justin Timberlake',
                    'Uptown Funk - Bruno Mars',
                    'Good as Hell - Lizzo',
                    'Walking on Sunshine - Katrina and the Waves'
                ];
                const randomSong = fallbackSongs[Math.floor(Math.random() * fallbackSongs.length)];
                addMessage(`🎵 Gợi ý bài hát: ${randomSong}`, false);
                quickReplies = ['Phát bài này', 'Bài khác', 'Tôi muốn nhạc buồn'];
            }
        } catch (error) {
            console.error('Error getting happy tracks:', error);
            addMessage("🎵 Tôi sẽ tìm nhạc vui vẻ cho bạn!", false);
            quickReplies = ['Tìm nhạc vui', 'Tôi muốn nhạc buồn', 'Nhạc chill'];
        }
    }
    else if (input.includes('buồn') || input.includes('sad') || input.includes('khóc')) {
        window.chatContext.lastMood = 'sad';
        const sadSongs = [
            '😢 Someone Like You - Adele',
            '💔 Fix You - Coldplay',
            '🎧 Let Her Go - Passenger',
            '🌧️ All Too Well - Taylor Swift',
            '💙 Stay - Rihanna ft. Mikky Ekko'
        ];
        const randomSong = sadSongs[Math.floor(Math.random() * sadSongs.length)];
        response = `Tôi hiểu bạn đang buồn... 💙 Nhạc có thể giúp chúng ta cảm thấy tốt hơn.\n\nGợi ý bài hát: ${randomSong}`;
        quickReplies = ['Phát bài này', 'Bài khác', 'Tôi muốn nhạc vui'];
    }
    else if (input.includes('chill') || input.includes('thư giãn') || input.includes('relax')) {
        window.chatContext.lastMood = 'chill';
        const chillSongs = [
            '☕ Let Her Go - Passenger',
            '🌊 Ocean Eyes - Billie Eilish',
            '🎶 ILY - Surf Mesa',
            '🌙 Midnight City - M83',
            '🍃 The Night We Met - Lord Huron'
        ];
        const randomSong = chillSongs[Math.floor(Math.random() * chillSongs.length)];
        response = `Thời gian thư giãn tuyệt vời! 🌙\n\nGợi ý bài hát: ${randomSong}`;
        quickReplies = ['Phát bài này', 'Bài khác', 'Tôi muốn nhạc năng động'];
    }
    else if (input.includes('dừng') || input.includes('stop')) {
        response = "⏹️ Đã dừng phát nhạc!";
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(
                JSON.stringify({ action: 'stopMusic' })
            );
        }
        quickReplies = ['Phát tiếp', 'Bài khác', 'Tìm nhạc mới'];
    }
    else if (input.includes('tạm dừng') || input.includes('pause')) {
        response = "⏸️ Đã tạm dừng!";
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(
                JSON.stringify({ action: 'pauseMusic' })
            );
        }
        quickReplies = ['Phát tiếp', 'Bài khác', 'Dừng hẳn'];
    }
    else if (input.includes('tiếp') || input.includes('resume') || input.includes('play')) {
        response = "▶️ Đang phát tiếp!";
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(
                JSON.stringify({ action: 'resumeMusic' })
            );
        }
        quickReplies = ['Tạm dừng', 'Bài khác', 'Dừng hẳn'];
    }
    else if (input.includes('tìm') || input.includes('search')) {
        const searchMatch = input.match(/(?:tìm|search)\s+(.+)/);
        if (searchMatch) {
            const query = searchMatch[1].trim();
            response = `🔍 Đang tìm kiếm "${query}" trong thư viện nhạc của bạn...`;
            quickReplies = ['Phát kết quả', 'Tìm khác', 'Xem tất cả'];
        } else {
            response = "🔍 Bạn muốn tìm gì? Tôi có thể tìm kiếm bài hát, nghệ sĩ, hoặc album!";
            quickReplies = ['Tìm theo tên bài', 'Tìm theo nghệ sĩ', 'Tìm theo thể loại'];
        }
    }
    else if (input.includes('help') || input.includes('giúp') || input.includes('hướng dẫn')) {
        response = `🎧 **Trợ lý âm nhạc Spotifree**\n\nTôi có thể giúp bạn:\n• 🎵 Tìm kiếm và phát nhạc\n• 🎭 Gợi ý nhạc theo tâm trạng\n• ⏯️ Điều khiển phát nhạc\n• 🔍 Tìm kiếm trong thư viện\n• 📱 Quản lý playlist\n\nHãy thử nói: "Tôi đang vui" hoặc "Phát nhạc pop"!`;
        quickReplies = ['Tìm nhạc', 'Tâm trạng vui', 'Tâm trạng buồn', 'Nhạc chill'];
    }
    else if (input.includes('xin chào') || input.includes('hello') || input.includes('hi')) {
        response = "👋 Xin chào! Tôi rất vui được gặp bạn! Bạn muốn nghe nhạc gì hôm nay?";
        quickReplies = ['Tìm nhạc mới', 'Nhạc theo tâm trạng', 'Xem playlist', 'Trợ giúp'];
    }
    else if (input.includes('cảm ơn') || input.includes('thank')) {
        response = "😊 Không có gì! Tôi rất vui được giúp bạn. Còn gì khác tôi có thể làm không?";
        quickReplies = ['Tìm nhạc khác', 'Tạo playlist', 'Xem thống kê', 'Cài đặt'];
    }
    else if (input.includes('bạn là ai') || input.includes('who are you')) {
        response = "🤖 Tôi là trợ lý âm nhạc AI của Spotifree! Tôi được thiết kế để giúp bạn khám phá và thưởng thức âm nhạc một cách thông minh và thú vị nhất.";
        quickReplies = ['Tìm nhạc', 'Hướng dẫn sử dụng', 'Tính năng mới', 'Liên hệ hỗ trợ'];
    }
    else {
        const fallbackResponses = [
            "🤔 Tôi chưa hiểu rõ ý bạn. Bạn có thể thử nói 'Tìm nhạc' hoặc 'Tôi đang vui' không?",
            "😅 Xin lỗi, tôi chưa hiểu. Hãy thử hỏi tôi về nhạc nhé!",
            "🎵 Tôi chuyên về âm nhạc! Bạn muốn tìm bài gì hay nghe nhạc theo tâm trạng?",
            "💭 Hmm, tôi chưa hiểu. Bạn có thể nói rõ hơn về việc tìm nhạc không?"
        ];
        response = fallbackResponses[Math.floor(Math.random() * fallbackResponses.length)];
        quickReplies = ['Tìm nhạc', 'Tôi đang vui', 'Tôi đang buồn', 'Trợ giúp'];
    }

    addMessage(response, false);

    if (quickReplies.length > 0) {
        setTimeout(() => {
            showQuickReplies(quickReplies);
        }, 500);
    }
}

function showQuickReplies(replies) {
    const log = document.getElementById('chatlog');
    const quickRepliesDiv = document.createElement('div');
    quickRepliesDiv.className = 'quick-replies';

    replies.forEach(reply => {
        const btn = document.createElement('button');
        btn.className = 'quick-reply-btn';
        btn.textContent = reply;
        btn.onclick = () => {
            quickRepliesDiv.remove();
            addMessage(reply, true);
            showTypingIndicator();
            setTimeout(() => {
                hideTypingIndicator();
                getBotReply(reply);
            }, 500);
        };
        quickRepliesDiv.appendChild(btn);
    });

    log.appendChild(quickRepliesDiv);
    log.scrollTo({
        top: log.scrollHeight,
        behavior: 'smooth'
    });
}

document.addEventListener('DOMContentLoaded', () => {
    const chatInput = document.getElementById('chat-input');
    if (chatInput) {
        chatInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });
    }
});
