# 🧪 Test Spotify API Integration

## ✅ **Credentials Đã Được Cập Nhật**

### **File: spotify-config.js**
```javascript
credentials: {
    clientId: '8105bc07cf1a4611a714f641cf61cf2d',
    clientSecret: 'f9e2f2ba56144e67beb3e65fde494d21',
    redirectUri: 'https://localhost:3000'
}
```

### **File: spotify-api.js**
```javascript
this.clientId = '8105bc07cf1a4611a714f641cf61cf2d';
this.clientSecret = 'f9e2f2ba56144e67beb3e65fde494d21';
this.redirectUri = 'https://localhost:3000';
```

## 🧪 **Test Ngay**

### **Bước 1: Clear Cache**
```javascript
// Trong browser console
localStorage.clear();
sessionStorage.clear();
location.reload();
```

### **Bước 2: Kiểm Tra Console Logs**
Mở Developer Console (F12) và xem:
```
🎵 Spotify token obtained successfully
🎵 Spotify API Integration loaded successfully!
```

### **Bước 3: Test API Trực Tiếp**
```javascript
// Trong browser console
fetch('https://accounts.spotify.com/api/token', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'Authorization': 'Basic ' + btoa('8105bc07cf1a4611a714f641cf61cf2d:f9e2f2ba56144e67beb3e65fde494d21')
    },
    body: 'grant_type=client_credentials'
})
.then(response => response.json())
.then(data => {
    console.log('✅ Token:', data.access_token ? 'Success!' : 'Failed');
    console.log('Data:', data);
})
.catch(error => console.error('❌ Error:', error));
```

### **Bước 4: Test Chatbot**
Gõ trong chatbot:
```
"Tìm nhạc Sơn Tùng"
"Tìm nhạc BTS"
"Tìm nhạc Taylor Swift"
```

## 🎯 **Kết Quả Mong Đợi**

### **Console Logs:**
```
🎵 Spotify token obtained successfully
🎵 Spotify API Integration loaded successfully!
```

### **Chatbot Response:**
```
User: "Tìm nhạc Sơn Tùng"
Bot: "🎵 Đang tìm kiếm 'Sơn Tùng' trên Spotify...
     🎧 Tìm thấy 10 bài hát:
     1. Muộn Rồi Mà Sao Còn - Sơn Tùng M-TP
     2. Nơi Này Có Anh - Sơn Tùng M-TP
     3. Chúng Ta Của Hiện Tại - Sơn Tùng M-TP
     ..."
```

## 🚨 **Nếu Vẫn Có Lỗi**

### **Lỗi 401 Unauthorized:**
- Kiểm tra credentials có đúng không
- Kiểm tra redirect URI trong Dashboard

### **Lỗi CORS:**
- Thêm domain vào CORS settings trong Dashboard
- Thêm `http://localhost:3000` vào Allowed Origins

### **Vẫn hiển thị Ed Sheeran:**
- Clear cache hoàn toàn
- Hard refresh (Ctrl+Shift+R)
- Kiểm tra Console logs

## 📋 **Checklist Hoàn Thành**

- [ ] ✅ Đã cập nhật credentials trong spotify-config.js
- [ ] ✅ Đã cập nhật credentials trong spotify-api.js
- [ ] ✅ Đã clear cache và refresh
- [ ] ✅ Console hiển thị "Spotify token obtained successfully"
- [ ] ✅ Test API trực tiếp thành công
- [ ] ✅ Chatbot tìm kiếm được nhạc thực từ Spotify
- [ ] ✅ Không còn hiển thị Ed Sheeran khi tìm Sơn Tùng

## 🎵 **Test Các Tìm Kiếm Khác**

```
"Tìm nhạc Sơn Tùng"
"Tìm nhạc BTS"
"Tìm nhạc Taylor Swift"
"Tìm nhạc pop"
"Tôi đang vui"
"Tôi đang buồn"
```

**🎧 Bây giờ chatbot sẽ tìm kiếm đúng bài hát từ Spotify database! 🎧**
