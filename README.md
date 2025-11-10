# Spotifree

Spotifree is a hybrid desktop music player, combining your local music library (using C\#) with the power of the Spotify API in a modern web UI.

This project is built to solve the challenge of managing scattered local music files by aggregating them into a single, cohesive, and modern application.

## Table of Contents

  - [About The Project](https://www.google.com/search?q=%23about-the-project)
  - [Key Features](https://www.google.com/search?q=%23key-features)
  - [Tech Stack](https://www.google.com/search?q=%23tech-stack)
  - [Getting Started](https://www.google.com/search?q=%23getting-started)
  - [Screenshots](https://www.google.com/search?q=%23screenshots)
  - [License](https://www.google.com/search?q=%23license)

## About The Project

The primary goal of this project is to provide a robust solution for local music management, moving away from the need to manually search for individual files. It combines the performance and system-level access of a .NET 8 WPF backend with the flexibility and modern aesthetics of a web-based frontend.

The architecture is intentionally hybrid:

  * A C\# backend (running in WPF) manages all heavy-lifting: local audio playback (via NAudio), file metadata scanning (via TagLib-Sharp), and secure communication with the Spotify Web API.
  * A WebView2 component hosts the entire user interface, which is built with HTML, CSS, and JavaScript.
  * A two-way communication "bridge" is established. The C\# `MainViewModel` can execute JavaScript in the frontend, while the frontend can call C\# methods directly using an exposed `PlayerBridge` host object.

## Key Features

  * **Hybrid Playback:** Seamlessly plays both local audio files (`.mp3`, `.flac`, `.wav`, etc.) and integrates with Spotify's streaming ecosystem.
  * **Modern Web UI:** The entire user experience (Home, Library, Player, Settings) is rendered via HTML/JS/CSS hosted within WebView2, allowing for rapid UI development.
  * **Spotify API Integration:**
      * Fetches and displays new releases, featured playlists, podcasts, and categories.
      * Synchronizes and displays the user's "Saved Tracks".
      * Full playlist management: create, rename, and delete Spotify playlists.
  * **Local Library Management:**
      * Scans specified directories on the user's machine for local music files.
      * Parses metadata (Title, Artist) using TagLib-Sharp.
      * Bridges local file paths to the web UI for playback.
  * **Advanced UI/UX:**
      * Includes a separate **Mini Player** window (also a WebView2 instance) for lightweight controls.
      * Features an AI **Chatbot Assistant** (powered by `ChatbotBridge.cs`) for music suggestions.

## Tech Stack

The project leverages a modern hybrid stack:

**Backend (C\# / .NET 8)**

  * **Framework:** .NET 8 / WPF (Windows Presentation Foundation)
  * **Host:** Microsoft WebView2
  * **Local Audio:** NAudio for robust audio file playback.
  * **Metadata:** TagLib-Sharp for reading tags from local music files.
  * **API Client:** `HttpClient` for all Spotify Web API communication.
  * **Architecture:**
      * Model-View-ViewModel (MVVM) for the WPF components (`MainViewModel`, `RelayCommand`).
      * Dependency Injection (using `Microsoft.Extensions.DependencyInjection`).

**Frontend (Web)**

  * **UI:** Vanilla JavaScript (ES6+), HTML5, CSS3
  * **API Client:** A dedicated `spotify-api.js` client for frontend-driven API calls.
  * **Architecture:**
      * SPA-like navigation (using `navigation.js` to load HTML partials).
      * Direct C\# interop via `window.chrome.webview.postMessage` and the injected `player` host object.

## Getting Started

To get a local copy up and running, follow these simple steps.

### Prerequisites

  * **.NET 8 SDK:** [Download .NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
  * **Visual Studio 2022:** [Download Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (Community Edition is sufficient).
      * Make sure to include the ".NET desktop development" workload during installation.

### Installation

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/your-username/spotifree.git
    ```
2.  **Build and Run the Project:**
      * Open the `spotifree.sln` file in Visual Studio 2022.
      * Wait for Visual Studio to restore the NuGet packages (NAudio, WebView2, etc.).
      * Press `F5` or click the "Start" button to build and run the application.

## Screenshots

*(Update Later.)*

## License

Distributed under the MIT License. See `LICENSE` file for more information.

## Author

*(Update Later.)*
