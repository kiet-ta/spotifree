// ⚡ Tạo nút toggle mở/đóng chatbot
const chatbotToggle = document.createElement('div');
chatbotToggle.id = 'chatbot-toggle';
chatbotToggle.innerHTML = '💬';
document.body.appendChild(chatbotToggle);

const chatbotContainer = document.getElementById('chatbot-container');
const chatbotClose = document.createElement('span');
chatbotClose.id = 'chatbot-close';
chatbotClose.innerHTML = '✖';
document.getElementById('chatbot-header').appendChild(chatbotClose);

// Ẩn chatbot khi khởi chạy
chatbotContainer.style.display = 'none';

chatbotToggle.addEventListener('click', () => {
    chatbotContainer.style.display = 'flex';
    chatbotToggle.style.display = 'none';   // ✅ Ẩn icon toggle khi mở khung chat
});

chatbotClose.addEventListener('click', () => {
    chatbotContainer.style.display = 'none';
    chatbotToggle.style.display = 'flex';   // ✅ Hiện lại icon khi đóng khung chat
});


// 📨 Gửi tin nhắn người dùng
function sendMessage() {
    const input = document.getElementById('chat-input');
    const msg = input.value.trim();
    if (!msg) return;

    addMessage(msg, true);
    getBotReply(msg);
    input.value = '';
}

// 💬 Thêm tin nhắn vào khung chat
function addMessage(text, isUser) {
    const log = document.getElementById('chatlog');
    const msg = document.createElement('div');
    msg.className = isUser ? 'user-msg' : 'bot-msg';
    msg.innerText = text;
    log.appendChild(msg);
    log.scrollTop = log.scrollHeight;
}

// 🧠 Bot thông minh hơn — random phản hồi
function getBotReply(input) {
    input = input.toLowerCase();

    const moodReplies = {
        'vui': ['🎉 Happy - Pharrell Williams', '🌞 Can’t Stop The Feeling', '🕺 Uptown Funk'],
        'buồn': ['😢 Someone Like You', '💔 Fix You', '🎧 Let Her Go'],
        'chill': ['☕ Let Her Go', '🌊 Ocean Eyes', '🎶 ILY - Surf Mesa'],
    };

    let response = "Tôi chưa hiểu câu hỏi này 😅";

    // 🎧 Phản hồi theo mood
    for (const mood in moodReplies) {
        if (input.includes(mood)) {
            const songs = moodReplies[mood];
            const randomSong = songs[Math.floor(Math.random() * songs.length)];
            response = `Gợi ý bài hát: ${randomSong}`;
            break;
        }
    }

    // 🚀 Gửi yêu cầu phát nhạc
    if (input.includes('mở nhạc')) {
        if (window.chrome.webview) {
            window.chrome.webview.postMessage(
                JSON.stringify({ action: 'playMusic', song: 'Let Her Go' })
            );
        }
        response = '🎵 Đang mở bài hát “Let Her Go”...';
    }

    // 📜 Lệnh trợ giúp
    if (input.includes('help') || input.includes('giúp')) {
        response = "✨ Bạn có thể nhập:\n- 'Tôi đang vui/buồn/chill'\n- 'Mở nhạc'\n- Hoặc gõ tên bài hát.";
    }

    addMessage(response, false);
}
