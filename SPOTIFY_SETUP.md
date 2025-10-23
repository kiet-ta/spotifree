# 🎵 Hướng Dẫn Setup Spotify API

## 📋 Tổng Quan
Để chatbot có thể tìm kiếm và lấy thông tin nhạc thực tế từ Spotify, bạn cần thiết lập Spotify Web API.

## 🚀 Bước 1: Tạo Spotify App

### 1.1 Đăng ký Spotify Developer Account
1. Truy cập [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Đăng nhập bằng tài khoản Spotify của bạn
3. Nếu chưa có tài khoản, đăng ký miễn phí tại [spotify.com](https://spotify.com)

### 1.2 Tạo App Mới
1. Click **"Create App"** trên Dashboard
2. Điền thông tin:
   - **App Name**: `Spotifree Chatbot` (hoặc tên bạn muốn)
   - **App Description**: `Music chatbot for Spotifree application`
   - **Website**: `http://localhost:3000` (hoặc URL của bạn)
   - **Redirect URI**: `http://localhost:3000/callback`
3. Chấp nhận Terms of Service
4. Click **"Save"**

### 1.3 Lấy Credentials
Sau khi tạo app, bạn sẽ thấy:
- **Client ID**: Copy giá trị này
- **Client Secret**: Click "Show" để hiển thị và copy

## 🔧 Bước 2: Cấu Hình Code

### 2.1 Cập nhật Spotify Config
Mở file `spotifree/Views/js/spotify-config.js` và thay đổi:

```javascript
credentials: {
    clientId: 'YOUR_SPOTIFY_CLIENT_ID_HERE', // Thay bằng Client ID thực
    clientSecret: 'YOUR_SPOTIFY_CLIENT_SECRET_HERE', // Thay bằng Client Secret thực
    redirectUri: 'http://localhost:3000/callback'
}
```

### 2.2 Cập nhật Spotify API
Mở file `spotifree/Views/js/spotify-api.js` và thay đổi:

```javascript
constructor() {
    this.clientId = 'YOUR_SPOTIFY_CLIENT_ID'; // Thay bằng Client ID thực
    this.clientSecret = 'YOUR_SPOTIFY_CLIENT_SECRET'; // Thay bằng Client Secret thực
    this.redirectUri = 'http://localhost:3000/callback';
    // ...
}
```

## 🔐 Bước 3: Cấu Hình Bảo Mật

### 3.1 Environment Variables (Khuyến nghị)
Thay vì hardcode credentials, hãy sử dụng environment variables:

```javascript
// Trong spotify-config.js
credentials: {
    clientId: process.env.SPOTIFY_CLIENT_ID || 'YOUR_CLIENT_ID',
    clientSecret: process.env.SPOTIFY_CLIENT_SECRET || 'YOUR_CLIENT_SECRET',
    redirectUri: process.env.SPOTIFY_REDIRECT_URI || 'http://localhost:3000/callback'
}
```

### 3.2 CORS Configuration
Đảm bảo rằng domain của bạn được thêm vào CORS settings trong Spotify Dashboard.

## 🎵 Bước 4: Test API

### 4.1 Kiểm tra kết nối
Mở Developer Console và kiểm tra:
```javascript
// Test trong browser console
console.log('Spotify API Status:', window.spotifyAPI ? 'Loaded' : 'Not loaded');
```

### 4.2 Test tìm kiếm
```javascript
// Test tìm kiếm bài hát
window.SpotifyHelpers.searchSongs('Ed Sheeran', 5)
    .then(results => console.log('Search results:', results))
    .catch(error => console.error('Search error:', error));
```

## 🚨 Troubleshooting

### ❌ Lỗi thường gặp

#### 1. "Invalid client credentials"
- **Nguyên nhân**: Client ID hoặc Client Secret sai
- **Giải pháp**: Kiểm tra lại credentials trong Dashboard

#### 2. "CORS error"
- **Nguyên nhân**: Domain không được phép
- **Giải pháp**: Thêm domain vào CORS settings

#### 3. "Rate limit exceeded"
- **Nguyên nhân**: Quá nhiều requests
- **Giải pháp**: Thêm delay giữa các requests

#### 4. "Token expired"
- **Nguyên nhân**: Access token hết hạn
- **Giải pháp**: API sẽ tự động refresh token

### 🔧 Debug Tips

#### 1. Kiểm tra Network Tab
- Mở Developer Tools > Network
- Xem requests đến Spotify API
- Kiểm tra status codes và responses

#### 2. Console Logs
```javascript
// Bật debug mode
localStorage.setItem('spotify_debug', 'true');
```

#### 3. Test với Postman
```bash
# Test Client Credentials Flow
curl -X POST \
  https://accounts.spotify.com/api/token \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -H 'Authorization: Basic YOUR_BASE64_ENCODED_CREDENTIALS' \
  -d 'grant_type=client_credentials'
```

## 📊 API Limits

### 🎵 Spotify API Limits
- **Rate Limit**: 10,000 requests per hour
- **Search Limit**: 50 results per request
- **Token Expiry**: 1 hour (tự động refresh)

### 💡 Optimization Tips
1. **Cache results**: Lưu kết quả tìm kiếm
2. **Debounce search**: Tránh quá nhiều requests
3. **Batch requests**: Gộp nhiều requests
4. **Error handling**: Xử lý lỗi gracefully

## 🎯 Advanced Features

### 🎵 Các tính năng nâng cao có thể thêm:

#### 1. User Authentication
```javascript
// Authorization Code Flow cho user data
const authUrl = `https://accounts.spotify.com/authorize?client_id=${clientId}&response_type=code&redirect_uri=${redirectUri}&scope=user-read-private user-read-email`;
```

#### 2. Playlist Management
```javascript
// Tạo playlist cho user
const createPlaylist = async (userId, name, description) => {
    // Implementation
};
```

#### 3. Audio Features
```javascript
// Lấy audio features của bài hát
const getAudioFeatures = async (trackId) => {
    // Implementation
};
```

#### 4. Recommendations
```javascript
// Gợi ý bài hát dựa trên seed tracks
const getRecommendations = async (seedTracks, limit = 20) => {
    // Implementation
};
```

## 📚 Tài Liệu Tham Khảo

### 🔗 Links Hữu Ích
- [Spotify Web API Documentation](https://developer.spotify.com/documentation/web-api/)
- [Authorization Guide](https://developer.spotify.com/documentation/general/guides/authorization/)
- [Web API Reference](https://developer.spotify.com/documentation/web-api/reference/)
- [SDK Examples](https://github.com/spotify/web-api-examples)

### 📖 API Endpoints Quan Trọng
- **Search**: `GET /v1/search`
- **Tracks**: `GET /v1/tracks/{id}`
- **Artists**: `GET /v1/artists/{id}`
- **Albums**: `GET /v1/albums/{id}`
- **Playlists**: `GET /v1/playlists/{id}`

## ✅ Checklist

- [ ] Tạo Spotify Developer Account
- [ ] Tạo App trong Dashboard
- [ ] Lấy Client ID và Client Secret
- [ ] Cập nhật credentials trong code
- [ ] Test API connection
- [ ] Test search functionality
- [ ] Test mood-based search
- [ ] Test error handling
- [ ] Optimize performance
- [ ] Add caching
- [ ] Test với WPF integration

## 🎉 Kết Luận

Sau khi hoàn thành setup, chatbot của bạn sẽ có thể:
- ✅ Tìm kiếm bài hát thực tế từ Spotify
- ✅ Gợi ý nhạc theo tâm trạng
- ✅ Lấy thông tin chi tiết về bài hát, nghệ sĩ
- ✅ Tích hợp với WPF application
- ✅ Cung cấp trải nghiệm âm nhạc phong phú

**🎵 Chúc bạn thành công với Spotifree Chatbot! 🎵**
