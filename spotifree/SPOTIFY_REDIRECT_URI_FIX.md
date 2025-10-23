# 🔒 Sửa Lỗi "This redirect URI is not secure"

## 🚨 **Nguyên Nhân Lỗi**

Spotify yêu cầu **HTTPS** cho redirect URI trong production, nhưng cho phép **HTTP** cho development.

## 🔧 **Giải Pháp**

### **Giải Pháp 1: Sử Dụng HTTP cho Development (Khuyến nghị)**

#### 1.1 Thay Đổi Redirect URI
Trong Spotify Dashboard, thay đổi:

```
❌ TRƯỚC:
Redirect URI: http://localhost:3000/callback

✅ SAU:  
Redirect URI: http://localhost:3000
```

**Lưu ý**: Bỏ `/callback` ở cuối!

#### 1.2 Cập Nhật Code
Mở file `spotifree/Views/js/spotify-config.js`:

```javascript
// ❌ TRƯỚC
credentials: {
    clientId: 'YOUR_CLIENT_ID',
    clientSecret: 'YOUR_CLIENT_SECRET',
    redirectUri: 'http://localhost:3000/callback'  // ❌ Có /callback
}

// ✅ SAU
credentials: {
    clientId: 'YOUR_CLIENT_ID', 
    clientSecret: 'YOUR_CLIENT_SECRET',
    redirectUri: 'http://localhost:3000'  // ✅ Bỏ /callback
}
```

### **Giải Pháp 2: Sử Dụng HTTPS (Nếu cần)**

#### 2.1 Sử Dụng HTTPS Localhost
```
Redirect URI: https://localhost:3000
```

#### 2.2 Hoặc Sử Dụng Ngrok (Tạm thời)
```bash
# Cài đặt ngrok
npm install -g ngrok

# Tạo HTTPS tunnel
ngrok http 3000

# Sử dụng URL từ ngrok
Redirect URI: https://abc123.ngrok.io
```

### **Giải Pháp 3: Bỏ Qua Redirect URI (Cho Client Credentials)**

#### 3.1 Sử Dụng Client Credentials Flow
Vì chúng ta chỉ cần **tìm kiếm nhạc** (không cần user login), có thể bỏ qua redirect URI:

```
Redirect URI: (để trống hoặc không cần thiết)
```

#### 3.2 Cập Nhật Code
```javascript
credentials: {
    clientId: 'YOUR_CLIENT_ID',
    clientSecret: 'YOUR_CLIENT_SECRET', 
    redirectUri: null  // ✅ Không cần redirect URI
}
```

## 🎯 **Hướng Dẫn Chi Tiết**

### **Bước 1: Sửa Trong Dashboard**
1. Vào [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Click vào app của bạn
3. Click **"Edit Settings"**
4. Trong **Redirect URIs**, thay đổi:
   ```
   ❌ http://localhost:3000/callback
   ✅ http://localhost:3000
   ```
5. Click **"Save"**

### **Bước 2: Cập Nhật Code**
Mở file `spotifree/Views/js/spotify-config.js`:

```javascript
const SpotifyConfig = {
    credentials: {
        clientId: 'YOUR_ACTUAL_CLIENT_ID',
        clientSecret: 'YOUR_ACTUAL_CLIENT_SECRET',
        redirectUri: 'http://localhost:3000'  // ✅ Không có /callback
    },
    // ... rest of config
};
```

### **Bước 3: Test**
1. Lưu file
2. Refresh ứng dụng
3. Mở Developer Console
4. Kiểm tra logs:
   ```
   🎵 Spotify token obtained successfully
   ```

## 🔍 **Các Redirect URI Được Chấp Nhận**

### ✅ **HTTP (Development)**
```
http://localhost:3000
http://localhost:8080
http://127.0.0.1:3000
```

### ✅ **HTTPS (Production)**
```
https://yourdomain.com
https://yourdomain.com/callback
https://localhost:3000
```

### ❌ **Không Được Chấp Nhận**
```
http://localhost:3000/callback  // ❌ Có /callback với HTTP
http://example.com              // ❌ Không phải localhost
```

## 🚨 **Troubleshooting**

### **Lỗi vẫn còn:**
1. **Kiểm tra lại Dashboard**: Đảm bảo đã save settings
2. **Clear cache**: Refresh browser hoặc hard refresh (Ctrl+F5)
3. **Kiểm tra URL**: Không có typo trong redirect URI

### **Lỗi "Invalid redirect URI":**
1. **Kiểm tra chính tả**: `localhost` không phải `localhst`
2. **Kiểm tra port**: Đảm bảo port 3000 đúng
3. **Kiểm tra protocol**: `http://` không phải `https://`

### **Lỗi "App not found":**
1. **Kiểm tra Client ID**: Copy chính xác từ Dashboard
2. **Kiểm tra app status**: Đảm bảo app đã được tạo thành công

## 📋 **Checklist Hoàn Thành**

- [ ] ✅ Đã thay đổi Redirect URI trong Dashboard
- [ ] ✅ Đã cập nhật `redirectUri` trong code
- [ ] ✅ Không còn lỗi "This redirect URI is not secure"
- [ ] ✅ App được tạo thành công
- [ ] ✅ Có thể lấy Client ID và Client Secret
- [ ] ✅ Console hiển thị "Spotify token obtained successfully"

## 🎯 **Kết Quả Cuối Cùng**

Sau khi sửa lỗi redirect URI:

```
✅ App created successfully in Spotify Dashboard
✅ Client ID: [your-client-id]
✅ Client Secret: [your-client-secret]  
✅ Redirect URI: http://localhost:3000
✅ No more "This redirect URI is not secure" error
```

## 💡 **Tips Bổ Sung**

### **1. Sử Dụng Environment Variables**
```javascript
// Thay vì hardcode
redirectUri: process.env.SPOTIFY_REDIRECT_URI || 'http://localhost:3000'
```

### **2. Multiple Redirect URIs**
Trong Dashboard, bạn có thể thêm nhiều redirect URIs:
```
http://localhost:3000
http://localhost:8080  
https://yourdomain.com
```

### **3. Development vs Production**
```javascript
// Development
redirectUri: 'http://localhost:3000'

// Production  
redirectUri: 'https://yourdomain.com'
```

## 🎉 **Kết Luận**

Sau khi sửa lỗi redirect URI:
- ✅ Có thể tạo app thành công trong Spotify Dashboard
- ✅ Lấy được Client ID và Client Secret
- ✅ Cấu hình chatbot với Spotify API
- ✅ Tìm kiếm nhạc thực từ Spotify database

**🎵 Bây giờ bạn có thể tiếp tục setup Spotify API cho chatbot! 🎵**
