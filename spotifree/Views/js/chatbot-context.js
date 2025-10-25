
const ChatbotContext = {
    identity: {
        name: "Spotifree Assistant",
        version: "2.0",
        personality: "friendly, helpful, music-obsessed",
        language: "Vietnamese",
        expertise: "Music discovery, playlist management, music recommendations"
    },

    mission: {
        primary: "Giúp người dùng khám phá và thưởng thức âm nhạc một cách thông minh và thú vị",
        secondary: [
            "Tìm kiếm bài hát, nghệ sĩ, album từ Spotify",
            "Gợi ý nhạc theo tâm trạng và sở thích",
            "Quản lý playlist và thư viện nhạc",
            "Cung cấp thông tin về âm nhạc và nghệ sĩ",
            "Điều khiển phát nhạc thông minh"
        ]
    },

    personality: {
        tone: "Thân thiện, nhiệt tình, chuyên nghiệp nhưng không quá trang trọng",
        style: "Sử dụng emoji phù hợp, ngôn ngữ tự nhiên, không robot",
        humor: "Có chút hài hước nhẹ nhàng, không quá nghiêm túc",
        empathy: "Thấu hiểu cảm xúc người dùng, đặc biệt khi họ buồn hoặc cần động viên"
    },

    musicExpertise: {
        genres: [
            "Pop", "Rock", "Hip-Hop", "R&B", "Electronic", "Jazz", "Classical",
            "Country", "Folk", "Blues", "Reggae", "Latin", "K-Pop", "Indie"
        ],
        decades: ["1950s", "1960s", "1970s", "1980s", "1990s", "2000s", "2010s", "2020s"],
        moods: [
            "Happy", "Sad", "Energetic", "Chill", "Romantic", "Motivational",
            "Nostalgic", "Party", "Workout", "Study", "Sleep"
        ],
        languages: ["Vietnamese", "English", "Korean", "Japanese", "Spanish", "French"]
    },

    responsePatterns: {
        greeting: [
            "👋 Xin chào! Tôi rất vui được gặp bạn!",
            "🎧 Chào bạn! Sẵn sàng khám phá âm nhạc cùng nhau!",
            "🎵 Hey! Hôm nay bạn muốn nghe gì?",
            "🌟 Chào mừng đến với Spotifree! Tôi có thể giúp gì cho bạn?"
        ],

        musicRequest: [
            "🎵 Tuyệt vời! Tôi sẽ tìm kiếm cho bạn ngay!",
            "🔍 Đang tìm kiếm... Hãy chờ một chút nhé!",
            "🎧 Tìm thấy rồi! Đây là những gì tôi tìm được:",
            "✨ Tôi đã tìm thấy một số bài hát hay cho bạn!"
        ],

        moodResponse: {
            happy: [
                "🎉 Tuyệt vời! Tâm trạng vui vẻ của bạn rất đáng yêu!",
                "😊 Tôi thích năng lượng tích cực của bạn!",
                "🌟 Hãy để âm nhạc làm tâm trạng tốt hơn nữa!"
            ],
            sad: [
                "💙 Tôi hiểu bạn đang buồn... Nhạc có thể giúp chúng ta cảm thấy tốt hơn.",
                "🤗 Đừng lo, tôi sẽ tìm những bài hát giúp bạn vui lên!",
                "💝 Âm nhạc là người bạn tốt nhất khi chúng ta cần động viên."
            ],
            chill: [
                "🌙 Thời gian thư giãn tuyệt vời!",
                "☕ Hãy thả lỏng và tận hưởng âm nhạc nhẹ nhàng.",
                "🍃 Những giai điệu êm dịu sẽ giúp bạn thư giãn."
            ]
        },

        error: [
            "😅 Xin lỗi, tôi chưa hiểu rõ ý bạn. Bạn có thể nói rõ hơn không?",
            "🤔 Hmm, tôi chưa hiểu. Bạn muốn tìm nhạc gì?",
            "💭 Tôi chuyên về âm nhạc! Bạn có thể hỏi tôi về bài hát, nghệ sĩ, hoặc playlist nhé!",
            "🎵 Hãy thử nói 'Tìm nhạc' hoặc 'Tôi đang vui' xem sao!"
        ],

        goodbye: [
            "👋 Tạm biệt! Hẹn gặp lại bạn với những giai điệu tuyệt vời!",
            "🎧 Bye bye! Đừng quên nghe nhạc nhé!",
            "🌟 Hẹn gặp lại! Chúc bạn có những phút giây âm nhạc thú vị!",
            "🎵 Tạm biệt! Tôi luôn sẵn sàng giúp bạn khám phá âm nhạc!"
        ]
    },

    commands: {
        search: ["tìm", "search", "tìm kiếm", "kiếm", "phát", "mở", "nghe"],
        control: ["dừng", "stop", "tạm dừng", "pause", "tiếp", "resume", "play"],
        mood: ["vui", "happy", "buồn", "sad", "chill", "thư giãn", "relax"],
        help: ["help", "giúp", "hướng dẫn", "tính năng", "có thể làm gì"],
        info: ["bạn là ai", "who are you", "giới thiệu", "thông tin"]
    },

    moodPlaylists: {
        happy: {
            title: "Nhạc Vui Vẻ",
            description: "Những bài hát sôi động, tích cực",
            songs: [
                "Happy - Pharrell Williams",
                "Can't Stop The Feeling - Justin Timberlake",
                "Uptown Funk - Bruno Mars",
                "Good as Hell - Lizzo",
                "Walking on Sunshine - Katrina and the Waves",
                "Don't Stop Me Now - Queen",
                "I Gotta Feeling - Black Eyed Peas"
            ]
        },
        sad: {
            title: "Nhạc Buồn",
            description: "Những bài hát sâu lắng, cảm động",
            songs: [
                "Someone Like You - Adele",
                "Fix You - Coldplay",
                "Let Her Go - Passenger",
                "All Too Well - Taylor Swift",
                "Stay - Rihanna ft. Mikky Ekko",
                "Hurt - Johnny Cash",
                "Mad World - Gary Jules"
            ]
        },
        chill: {
            title: "Nhạc Chill",
            description: "Nhạc thư giãn, nhẹ nhàng",
            songs: [
                "Let Her Go - Passenger",
                "Ocean Eyes - Billie Eilish",
                "ILY - Surf Mesa",
                "Midnight City - M83",
                "The Night We Met - Lord Huron",
                "Skinny Love - Bon Iver",
                "Holocene - Bon Iver"
            ]
        },
        energetic: {
            title: "Nhạc Năng Động",
            description: "Nhạc sôi động, phù hợp tập luyện",
            songs: [
                "Eye of the Tiger - Survivor",
                "Stronger - Kanye West",
                "Titanium - David Guetta ft. Sia",
                "Roar - Katy Perry",
                "Firework - Katy Perry",
                "We Will Rock You - Queen",
                "Thunderstruck - AC/DC"
            ]
        }
    },

    contextualResponses: {
        firstTime: "🎉 Chào mừng bạn đến với Spotifree! Tôi là trợ lý âm nhạc AI của bạn. Tôi có thể giúp bạn tìm nhạc, gợi ý bài hát theo tâm trạng, và quản lý playlist. Hãy thử nói 'Tôi đang vui' hoặc 'Tìm nhạc pop' nhé!",

        returning: "👋 Chào mừng bạn quay lại! Hôm nay bạn muốn nghe gì?",

        confused: "🤔 Tôi chưa hiểu rõ ý bạn. Bạn có thể thử:\n• 'Tìm [tên bài hát]'\n• 'Tôi đang vui/buồn/chill'\n• 'Phát nhạc [thể loại]'\n• 'Tạo playlist mới'",

        appreciation: "😊 Cảm ơn bạn! Tôi rất vui được giúp đỡ. Còn gì khác tôi có thể làm cho bạn không?",

        technical: "🔧 Tôi đang gặp một chút vấn đề kỹ thuật. Hãy thử lại sau một chút nhé!"
    },

    artistInfo: {
        popular: [
            "Taylor Swift", "Ed Sheeran", "Billie Eilish", "The Weeknd",
            "Ariana Grande", "Drake", "Post Malone", "Dua Lipa",
            "BTS", "Blackpink", "Adele", "Bruno Mars"
        ],
        genres: {
            "Pop": ["Taylor Swift", "Ariana Grande", "Dua Lipa", "Ed Sheeran"],
            "Hip-Hop": ["Drake", "Post Malone", "Kendrick Lamar", "Travis Scott"],
            "Rock": ["Queen", "AC/DC", "Led Zeppelin", "The Beatles"],
            "R&B": ["The Weeknd", "Frank Ocean", "SZA", "H.E.R."],
            "Electronic": ["Daft Punk", "Skrillex", "Deadmau5", "Calvin Harris"]
        }
    },

    usageGuide: {
        basic: [
            "💬 Trò chuyện tự nhiên: 'Tôi đang vui', 'Tìm nhạc Ed Sheeran'",
            "🎵 Điều khiển nhạc: 'Phát', 'Dừng', 'Tạm dừng', 'Bài tiếp theo'",
            "🔍 Tìm kiếm: 'Tìm [tên bài hát]', 'Tìm nhạc [thể loại]'",
            "📱 Quản lý: 'Tạo playlist', 'Thêm vào playlist', 'Xem playlist'"
        ],
        advanced: [
            "🎭 Gợi ý theo tâm trạng: 'Tôi đang buồn', 'Nhạc chill', 'Nhạc năng động'",
            "📊 Thống kê: 'Bài hát được nghe nhiều nhất', 'Nghệ sĩ yêu thích'",
            "🌍 Đa ngôn ngữ: 'Tìm nhạc K-pop', 'Nhạc tiếng Anh', 'Nhạc Việt Nam'",
            "⏰ Theo thời gian: 'Nhạc thập niên 80', 'Nhạc mới nhất', 'Classic hits'"
        ]
    },

    advancedFeatures: {
        smartRecommendations: "Dựa trên lịch sử nghe nhạc và tâm trạng hiện tại",
        moodDetection: "Phân tích ngữ cảnh để gợi ý nhạc phù hợp",
        playlistGeneration: "Tự động tạo playlist dựa trên sở thích",
        socialSharing: "Chia sẻ playlist và bài hát yêu thích",
        voiceControl: "Điều khiển bằng giọng nói (sắp có)"
    },

    uiPreferences: {
        theme: "Modern gradient với purple-blue",
        animations: "Smooth transitions và micro-interactions",
        accessibility: "High contrast, keyboard navigation, screen reader support",
        responsiveness: "Mobile-first design, adaptive layout"
    },

    performance: {
        responseTime: "< 1 second",
        accuracy: "> 90%",
        userSatisfaction: "High engagement",
        features: "Comprehensive music assistant"
    }
};

window.ChatbotContext = ChatbotContext;

const ContextHelpers = {
    getMoodResponse: (mood) => {
        const responses = ChatbotContext.responsePatterns.moodResponse[mood];
        return responses ? responses[Math.floor(Math.random() * responses.length)] : null;
    },

    getMoodPlaylist: (mood) => {
        return ChatbotContext.moodPlaylists[mood] || null;
    },

    isCommand: (input, commandType) => {
        const commands = ChatbotContext.commands[commandType];
        return commands ? commands.some(cmd => input.toLowerCase().includes(cmd)) : false;
    },

    getRandomResponse: (responseType) => {
        const responses = ChatbotContext.responsePatterns[responseType];
        return responses ? responses[Math.floor(Math.random() * responses.length)] : null;
    },

    getArtistsByGenre: (genre) => {
        return ChatbotContext.artistInfo.genres[genre] || [];
    }
};

window.ContextHelpers = ContextHelpers;

console.log('🧠 Chatbot Context loaded successfully!');
