document.addEventListener("DOMContentLoaded", () => {
  // 💬 Create toggle button
  const chatbotToggle = document.createElement("div");
  chatbotToggle.id = "chatbot-toggle";
  chatbotToggle.innerHTML = "💬";
  document.body.appendChild(chatbotToggle);

  const chatbotContainer = document.getElementById("chatbot-container");
  const chatbotHeader = document.getElementById("chatbot-header");

  // ✖ Close button
  if (chatbotHeader) {
    const chatbotClose = document.createElement("span");
    chatbotClose.id = "chatbot-close";
    chatbotClose.innerHTML = "✖";
    chatbotHeader.appendChild(chatbotClose);

    chatbotContainer.style.display = "none";

    chatbotToggle.addEventListener("click", () => {
      chatbotContainer.style.display = "flex";
      chatbotToggle.style.display = "none";
    });

    chatbotClose.addEventListener("click", () => {
      chatbotContainer.style.display = "none";
      chatbotToggle.style.display = "flex";
    });
  }

  const input = document.getElementById("chat-input");
  const sendButton = chatbotContainer.querySelector("button");

  if (input && sendButton) {
    sendButton.addEventListener("click", sendMessage);
    input.addEventListener("keypress", (e) => {
      if (e.key === "Enter") sendMessage();
    });
  }

  // 💡 Suggestion bar ABOVE input
  const suggestionBar = document.createElement("div");
  suggestionBar.id = "chat-suggestions";
  suggestionBar.innerHTML = `
    <div class="suggestion">I'm happy</div>
    <div class="suggestion">I'm sad</div>
    <div class="suggestion">I'm chill</div>
    <div class="suggestion">Play a song</div>
    <div class="suggestion">Help</div>
  `;
  chatbotContainer.insertBefore(suggestionBar, document.getElementById("chat-input-area"));

  // Suggestion click handler
  suggestionBar.querySelectorAll(".suggestion").forEach((btn) => {
    btn.addEventListener("click", () => {
      input.value = btn.innerText;
      sendMessage();
    });
  });

  // ===============================
  // FUNCTIONS
  // ===============================

  function sendMessage() {
    const msg = input.value.trim();
    if (!msg) return;
    addMessage(msg, true);
    input.value = "";
    showTyping(() => getBotReply(msg));
  }

  function addMessage(text, isUser) {
    const log = document.getElementById("chatlog");
    const msg = document.createElement("div");
    msg.className = isUser ? "user-msg" : "bot-msg";
    msg.innerText = text;
    log.appendChild(msg);
    log.scrollTop = log.scrollHeight;
  }

  function showTyping(callback) {
    const log = document.getElementById("chatlog");
    const typing = document.createElement("div");
    typing.className = "typing";
    typing.innerText = "Bot is typing...";
    log.appendChild(typing);
    log.scrollTop = log.scrollHeight;
    setTimeout(() => {
      typing.remove();
      callback();
    }, 1000);
  }

  function getBotReply(userInput) {
    const input = userInput.toLowerCase();

    const moodReplies = {
      happy: ["🎉 Happy - Pharrell Williams", "🌞 Can’t Stop The Feeling", "🕺 Uptown Funk"],
      sad: ["😢 Someone Like You", "💔 Fix You", "🎧 Let Her Go"],
      chill: ["☕ Let Her Go", "🌊 Ocean Eyes", "🎶 ILY - Surf Mesa"]
    };

    let response = "I'm not sure what you mean 😅";

    // mood detection
    for (const mood in moodReplies) {
      if (input.includes(mood)) {
        const songs = moodReplies[mood];
        const randomSong = songs[Math.floor(Math.random() * songs.length)];
        response = `🎵 Suggested song: ${randomSong}`;
        break;
      }
    }

    // play song command
    if (input.includes("play")) {
      if (window.chrome?.webview) {
        window.chrome.webview.postMessage(
          JSON.stringify({ action: "playMusic", song: "Let Her Go" })
        );
      }
      response = "🎶 Playing 'Let Her Go'...";
    }

    // help command
    if (input.includes("help")) {
      response =
        "✨ You can type:\n- 'I'm happy/sad/chill'\n- 'Play a song'\n- Or type any artist or mood.";
    }

    addMessage(response, false);
  }
});
