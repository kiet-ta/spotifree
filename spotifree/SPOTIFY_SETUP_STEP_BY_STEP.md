# 🎵 Hướng Dẫn Setup Spotify API - Từng Bước Chi Tiết

## 🎯 **Bước 1: Tạo App Trong Dashboard**

### 1.1 Truy Cập Dashboard
1. Đăng nhập vào [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Bạn sẽ thấy giao diện dashboard

### 1.2 Tạo App Mới
1. Click nút **"Create App"** (màu xanh lá)
2. Điền thông tin:
   ```
   App Name: Spotifree Chatbot
   App Description: Music chatbot for Spotifree application
   Website: http://localhost:3000
   Redirect URI: http://localhost:3000/callback
   ```
3. Tích vào **"Terms of Service"**
4. Click **"Save"**

### 1.3 Xác Nhận Tạo App
- Bạn sẽ thấy app mới trong danh sách
- Click vào tên app để vào trang chi tiết

## 🔑 **Bước 2: Lấy Credentials**

### 2.1 Lấy Client ID
1. Trong trang app, bạn sẽ thấy:
   ```
   Client ID: [Một chuỗi dài các ký tự]
   ```
2. **Copy Client ID** này (giữ nguyên)

### 2.2 Lấy Client Secret
1. Click nút **"Show"** bên cạnh Client Secret
2. Bạn sẽ thấy:
   ```
   Client Secret: [Một chuỗi dài các ký tự]
   ```
3. **Copy Client Secret** này (giữ nguyên)

### 2.3 Lưu Credentials An Toàn
- Lưu Client ID và Client Secret vào file text tạm thời
- **KHÔNG** chia sẻ Client Secret với ai khác

## 🔧 **Bước 3: Cấu Hình Code**

### 3.1 Mở File Config
Mở file: `spotifree/Views/js/spotify-config.js`

### 3.2 Cập Nhật Credentials
Tìm dòng 8-9 và thay thế:

```javascript
// ❌ TRƯỚC (placeholder)
credentials: {
    clientId: 'YOUR_SPOTIFY_CLIENT_ID_HERE',
    clientSecret: 'YOUR_SPOTIFY_CLIENT_SECRET_HERE',
    redirectUri: 'http://localhost:3000/callback'
}

// ✅ SAU (credentials thực)
credentials: {
    clientId: 'YOUR_ACTUAL_CLIENT_ID_FROM_DASHBOARD',
    clientSecret: 'YOUR_ACTUAL_CLIENT_SECRET_FROM_DASHBOARD', 
    redirectUri: 'http://localhost:3000/callback'
}
```

### 3.3 Ví Dụ Thực Tế
```javascript
credentials: {
    clientId: 'a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6',
    clientSecret: 'x9y8z7w6v5u4t3s2r1q0p9o8n7m6l5k4',
    redirectUri: 'http://localhost:3000/callback'
}
```

## 🎯 **Bước 4: Test API**

### 4.1 Mở Developer Console
1. Mở ứng dụng Spotifree
2. Mở Developer Tools (F12)
3. Vào tab **Console**

### 4.2 Kiểm Tra Logs
Bạn sẽ thấy một trong các thông báo:

#### ✅ **Thành Công:**
```
🎵 Spotify token obtained successfully
🎵 Spotify API Integration loaded successfully!
```

#### ⚠️ **Cần Kiểm Tra:**
```
❌ Spotify credentials chưa được cấu hình!
⚠️ Spotify credentials chưa được cấu hình. Chế độ fallback sẽ được sử dụng.
```

### 4.3 Test Tìm Kiếm
Trong chatbot, thử gõ:
```
"Tìm nhạc Ed Sheeran"
```

**Kết quả mong đợi:**
```
🎵 Đang tìm kiếm "Ed Sheeran" trên Spotify...
🎧 Tìm thấy 10 bài hát:
1. Shape of You - Ed Sheeran
2. Perfect - Ed Sheeran
...
```

## 🚨 **Bước 5: Xử Lý Lỗi (Nếu Có)**

### 5.1 Lỗi "Invalid client credentials"
**Nguyên nhân:** Client ID hoặc Client Secret sai
**Giải pháp:**
1. Kiểm tra lại credentials trong Dashboard
2. Copy lại chính xác (không có khoảng trắng thừa)
3. Lưu file và refresh trang

### 5.2 Lỗi "CORS error"
**Nguyên nhân:** Domain không được phép
**Giải pháp:**
1. Vào Dashboard > App Settings
2. Thêm domain vào **Allowed Origins**
3. Thêm: `http://localhost:3000`

### 5.3 Lỗi "Rate limit exceeded"
**Nguyên nhân:** Quá nhiều requests
**Giải pháp:**
1. Chờ 1-2 phút
2. Thử lại
3. Kiểm tra code có gọi API quá nhiều không

## 📊 **Bước 6: Verify Setup**

### 6.1 Checklist Hoàn Thành
- [ ] ✅ Đã tạo app trong Spotify Dashboard
- [ ] ✅ Đã lấy Client ID và Client Secret
- [ ] ✅ Đã cập nhật credentials trong code
- [ ] ✅ Console hiển thị "Spotify token obtained successfully"
- [ ] ✅ Chatbot tìm kiếm được nhạc thực từ Spotify
- [ ] ✅ Không còn lỗi "Có lỗi khi tìm kiếm trên Spotify"

### 6.2 Test Các Tính Năng
Thử các lệnh sau trong chatbot:

```
"Tìm nhạc Taylor Swift"
"Tôi đang vui"
"Tôi đang buồn" 
"Tìm nhạc pop"
"Phát nhạc Ed Sheeran"
```

## 🎉 **Kết Quả Cuối Cùng**

### **Trước Setup:**
```
User: "Tìm nhạc Ed Sheeran"
Bot: "❌ Có lỗi khi tìm kiếm trên Spotify. Hãy thử lại sau!"
```

### **Sau Setup:**
```
User: "Tìm nhạc Ed Sheeran"
Bot: "🎵 Đang tìm kiếm 'Ed Sheeran' trên Spotify...
     🎧 Tìm thấy 10 bài hát:
     1. Shape of You - Ed Sheeran
     2. Perfect - Ed Sheeran
     3. Thinking Out Loud - Ed Sheeran
     ..."
```

## 🔧 **Troubleshooting Nâng Cao**

### **Nếu vẫn có lỗi:**

#### 1. Kiểm tra Network Tab
- Mở Developer Tools > Network
- Tìm requests đến `api.spotify.com`
- Kiểm tra status code và response

#### 2. Test với Postman
```bash
curl -X POST \
  https://accounts.spotify.com/api/token \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -H 'Authorization: Basic [BASE64_ENCODED_CREDENTIALS]' \
  -d 'grant_type=client_credentials'
```

#### 3. Debug Mode
```javascript
// Trong browser console
console.log('Spotify Config:', window.SpotifyConfig);
console.log('Spotify API:', window.spotifyAPI);
```

## 📚 **Tài Liệu Tham Khảo**

- [Spotify Web API Documentation](https://developer.spotify.com/documentation/web-api/)
- [Authorization Guide](https://developer.spotify.com/documentation/general/guides/authorization/)
- [Rate Limits](https://developer.spotify.com/documentation/web-api/concepts/rate-limits)

## ✅ **Kết Luận**

Sau khi hoàn thành các bước trên:
- ✅ Chatbot sẽ tìm kiếm nhạc thực từ Spotify
- ✅ Không còn lỗi "Có lỗi khi tìm kiếm trên Spotify"
- ✅ Trải nghiệm người dùng hoàn hảo
- ✅ Có thể sử dụng tất cả tính năng âm nhạc

**🎵 Chúc bạn thành công với Spotifree Chatbot! 🎵**
