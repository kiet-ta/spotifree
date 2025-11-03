// Player state
let isPlaying = false
let currentTime = 0
const duration = 246 // 4:06 in seconds

// DOM elements
const playBtn = document.querySelector(".play-btn")
const prevBtn = document.querySelector(".prev-btn")
const nextBtn = document.querySelector(".next-btn")
const shuffleBtn = document.querySelector(".shuffle-btn")
const repeatBtn = document.querySelector(".repeat-btn")
const progressSlider = document.querySelector(".progress-slider")
const progressFill = document.querySelector(".progress-fill")
const currentTimeEl = document.querySelector(".current-time")
const durationTimeEl = document.querySelector(".duration-time")
const volumeSlider = document.querySelector(".volume-slider")

// Format time helper
function formatTime(seconds) {
  const mins = Math.floor(seconds / 60)
  const secs = Math.floor(seconds % 60)
  return `${mins}:${secs.toString().padStart(2, "0")}`
}

// Play/Pause toggle
playBtn.addEventListener("click", () => {
  isPlaying = !isPlaying
  playBtn.textContent = isPlaying ? "⏸️" : "▶️"
  playBtn.classList.toggle("active")
})

// Progress bar update
progressSlider.addEventListener("input", (e) => {
  currentTime = (e.target.value / 100) * duration
  updateProgress()
})

function updateProgress() {
  const percentage = (currentTime / duration) * 100
  progressFill.style.width = percentage + "%"
  progressSlider.value = percentage
  currentTimeEl.textContent = formatTime(currentTime)
}

// Simulate playback
setInterval(() => {
  if (isPlaying && currentTime < duration) {
    currentTime += 0.1
    updateProgress()
  }
}, 100)

// Next button
nextBtn.addEventListener("click", () => {
  currentTime = 0
  updateProgress()
})

// Previous button
prevBtn.addEventListener("click", () => {
  currentTime = 0
  updateProgress()
})

// Shuffle button
shuffleBtn.addEventListener("click", () => {
  shuffleBtn.style.opacity = shuffleBtn.style.opacity === "0.5" ? "1" : "0.5"
})

// Repeat button
repeatBtn.addEventListener("click", () => {
  repeatBtn.style.opacity = repeatBtn.style.opacity === "0.5" ? "1" : "0.5"
})

// Volume control
volumeSlider.addEventListener("input", (e) => {
  console.log("[v0] Volume:", e.target.value)
})

// Initialize
durationTimeEl.textContent = formatTime(duration)
updateProgress()
