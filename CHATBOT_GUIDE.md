# 🎧 Hướng Dẫn Sử Dụng Chatbot Spotifree

## 🌟 Tổng Quan
Chatbot Spotifree là trợ lý âm nhạc AI thông minh được tích hợp vào ứng dụng WPF, giúp bạn tương tác với âm nhạc một cách tự nhiên và thú vị.

## ✨ Tính Năng Chính

### 🎵 Điều Khiển Nhạc
- **Phát nhạc**: "Phát [tên bài hát]", "Mở nhạc", "Nghe [nghệ sĩ]"
- **Điều khiển**: "Dừng", "Tạm dừng", "Phát tiếp", "Bài tiếp theo", "Bài trước"
- **Âm lượng**: "Tăng âm lượng", "Giảm âm lượng", "Âm lượng 50%"

### 🎭 Gợi Ý Theo Tâm Trạng
- **Vui vẻ**: "Tôi đang vui", "Happy", "Hạnh phúc"
- **Buồn bã**: "Tôi đang buồn", "Sad", "Khóc"
- **Thư giãn**: "Tôi muốn chill", "Thư giãn", "Relax"

### 🔍 Tìm Kiếm Thông Minh
- **Tìm bài hát**: "Tìm [tên bài]", "Search [nghệ sĩ]"
- **Tìm theo thể loại**: "Tìm nhạc pop", "Nhạc rock", "Nhạc EDM"
- **Tìm trong thư viện**: "Tìm trong thư viện của tôi"

### 📱 Quản Lý Playlist
- **Tạo playlist**: "Tạo playlist mới", "Tạo danh sách [tên]"
- **Thêm bài hát**: "Thêm bài này vào playlist"
- **Xem playlist**: "Xem playlist của tôi"

## 🎨 Giao Diện Mới

### 🎨 Thiết Kế Hiện Đại
- **Gradient đẹp mắt**: Sử dụng gradient tím-xanh chuyên nghiệp
- **Animation mượt mà**: Hiệu ứng chuyển động tự nhiên
- **Glass morphism**: Hiệu ứng kính mờ hiện đại
- **Responsive**: Tối ưu cho mọi kích thước màn hình

### 💬 Trải Nghiệm Chat
- **Typing indicator**: Hiệu ứng bot đang gõ
- **Message timestamps**: Hiển thị thời gian tin nhắn
- **Quick replies**: Nút trả lời nhanh thông minh
- **Smooth scrolling**: Cuộn mượt mà

### 🌙 Dark Mode
- **Tự động**: Phát hiện theme hệ thống
- **Tùy chỉnh**: Có thể bật/tắt thủ công
- **Màu sắc hài hòa**: Palette màu tối chuyên nghiệp

## 🚀 Tích Hợp WPF

### 📡 Communication Protocol
```javascript
// Gửi lệnh đến WPF
window.chrome.webview.postMessage(JSON.stringify({
    action: 'searchAndPlay',
    query: 'tên bài hát'
}));
```

### 🎵 Music Commands
- `searchAndPlay(query)` - Tìm và phát nhạc
- `playMusic()` - Phát nhạc
- `pauseMusic()` - Tạm dừng
- `stopMusic()` - Dừng
- `nextTrack()` - Bài tiếp theo
- `previousTrack()` - Bài trước

### 📚 Library Commands
- `searchLibrary(query, type)` - Tìm trong thư viện
- `getSongs(limit)` - Lấy danh sách bài hát
- `getPlaylists()` - Lấy danh sách playlist
- `createPlaylist(name, description)` - Tạo playlist mới

### 📊 Analytics Commands
- `getListeningStats(period)` - Thống kê nghe nhạc
- `getTopSongs(limit)` - Bài hát được nghe nhiều nhất
- `getTopArtists(limit)` - Nghệ sĩ được nghe nhiều nhất

## 🎯 Cách Sử Dụng

### 1. Mở Chatbot
- Click vào nút 💬 ở góc dưới bên phải
- Hoặc nhấn phím tắt (nếu có)

### 2. Trò Chuyện Tự Nhiên
```
Bạn: "Tôi đang vui"
Bot: "Tuyệt vời! Tâm trạng vui vẻ của bạn rất đáng yêu! 🎉
     Gợi ý bài hát: Happy - Pharrell Williams"
```

### 3. Điều Khiển Nhạc
```
Bạn: "Phát nhạc Ed Sheeran"
Bot: "🎵 Đang tìm kiếm 'Ed Sheeran'..."
     [Tự động phát nhạc]
```

### 4. Sử Dụng Quick Replies
- Bot sẽ hiển thị các nút trả lời nhanh
- Click để chọn thay vì gõ lại

## 🔧 Cấu Hình

### 🎨 Customization
```css
/* Thay đổi màu chủ đạo */
:root {
    --primary-color: #667eea;
    --secondary-color: #764ba2;
}
```

### ⚙️ Settings
- **Theme**: Light/Dark/Auto
- **Language**: Vietnamese/English
- **Animation**: Enable/Disable
- **Sound**: Notification sounds

## 🐛 Troubleshooting

### ❌ Chatbot không phản hồi
1. Kiểm tra console log
2. Đảm bảo WebView2 được load
3. Restart ứng dụng

### 🎵 Không phát được nhạc
1. Kiểm tra kết nối WPF
2. Verify file nhạc tồn tại
3. Check permissions

### 🔍 Tìm kiếm không hoạt động
1. Kiểm tra thư viện nhạc
2. Verify search query
3. Check network connection

## 📈 Roadmap

### 🚀 Tính Năng Sắp Tới
- **Voice commands**: Điều khiển bằng giọng nói
- **Smart recommendations**: Gợi ý thông minh hơn
- **Mood detection**: Phát hiện tâm trạng qua nhạc
- **Social features**: Chia sẻ playlist với bạn bè
- **AI learning**: Học từ sở thích người dùng

### 🔄 Cập Nhật
- **Version 2.0**: Voice integration
- **Version 2.1**: Advanced AI
- **Version 2.2**: Social features
- **Version 3.0**: Full AI assistant

## 📞 Hỗ Trợ

### 💬 Liên Hệ
- **Email**: support@spotifree.com
- **GitHub**: github.com/spotifree/chatbot
- **Discord**: discord.gg/spotifree

### 📚 Tài Liệu
- **API Docs**: docs.spotifree.com/api
- **Tutorials**: docs.spotifree.com/tutorials
- **Examples**: docs.spotifree.com/examples

---

**🎧 Enjoy your music with Spotifree Chatbot! 🎧**
