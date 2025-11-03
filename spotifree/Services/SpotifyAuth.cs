using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace spotifree.Services
{
    public class SpotifyAuth
    {
        private const string TokenFile = "spotify_token.json";
        private readonly string _clientId;
        private readonly string _redirectUri;
        private readonly string _scope;
        private string? _codeVerifier;
        private string? _codeChallenge;

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }

        public string ClientId => _clientId;
        public string RedirectUri => _redirectUri;
        public string Scope => _scope;

        public SpotifyAuth(string clientId, string redirectUri, string scope)
        {
            _clientId = clientId;
            _redirectUri = redirectUri;
            _scope = scope;

            // HARDCODED TOKEN FOR LOCAL TESTING
            //AccessToken = "BQC4Q518LIAWx_MOVHiirytiqDvob_rzX8Ml5_zSb2szXaRO_n76L0xNPyYrxFPW_rFw8wbpuaiHgK2HtfhIjMOZV2eHneVF0pZTuzXL8h-lmmQ3uHvtLm-Uc21K9tSGYg_igK6kmkI955NOLQBY5JV79C3kWIv4bTimD3XO6i-5Hdx8eAkWhMD3V-lup8AxlnwX8CLTihh5zWiOtv3QlkRxG_ZieJ2xXzHIQQpRAbhTQmUyMDCHlC6G1mWe3g0hOgAG26GUqhRC2fRq_DJvPkOwWVY";
            //RefreshToken = "AQBsHjf3wRmW9xJaUj9HqSM1ffhujjful-29OAlM8PmuS_61sDagJjZ_fnLHFDA1-fEZBIZGmpqQ2zSMJ88_82AtU3s9Kp7p2QmsxGqY0s0X0-NNAeGLSzmqMBvE5cDKQbs";
            //ExpiresAtUtc = DateTime.UtcNow.AddHours(1); // expires_in = 3600 seconds = 1 hour

            // auto load token cache if yet (will be overridden by hardcoded values above)
            // LoadToken();
        }

        private string AppDataFilePath()
        {
            var app = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(app, "Sleptify");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, TokenFile);
        }

        private void LoadToken()
        {
            try
            {
                var file = AppDataFilePath();
                if (!System.IO.File.Exists(file)) return;
                var json = System.IO.File.ReadAllText(file);
                var doc = JsonDocument.Parse(json);
                AccessToken = doc.RootElement.GetProperty("access_token").GetString();
                RefreshToken = doc.RootElement.GetProperty("refresh_token").GetString();
                if (doc.RootElement.TryGetProperty("expires_at", out var ea))
                    ExpiresAtUtc = ea.GetDateTime();
            }
            catch { }
        }

        private void SaveToken()
        {
            try
            {
                var file = AppDataFilePath();
                var json = JsonSerializer.Serialize(new
                {
                    access_token = AccessToken,
                    refresh_token = RefreshToken,
                    expires_at = ExpiresAtUtc
                });
                System.IO.File.WriteAllText(file, json);
            }
            catch { }
        }

        private void ClearToken()
        {
            AccessToken = null;
            RefreshToken = null;
            ExpiresAtUtc = DateTime.MinValue;
            try
            {
                var file = AppDataFilePath();
                if (System.IO.File.Exists(file)) System.IO.File.Delete(file);
            }
            catch { }
        }

        public bool IsValid()
        {
            // For testing: always return true if token is set (ignore expiry)
            if (!string.IsNullOrEmpty(AccessToken))
            {
                // Auto-extend expiry if near expiration for testing
                if (DateTime.UtcNow.AddMinutes(10) >= ExpiresAtUtc)
                {
                    ExpiresAtUtc = DateTime.UtcNow.AddHours(1);
                }
                return true;
            }
            return false;
        }

        /* ===========================================================
         *   Main Function: EnsureTokenAsync (2 overload)
         * =========================================================== */

        public async Task<bool> EnsureTokenAsync()
        {
            // For testing: token is hardcoded, so always return true
            if (IsValid()) return true;

            // If somehow token is missing, try refresh or re-auth
            if (!string.IsNullOrEmpty(RefreshToken))
            {
                if (await RefreshAsync()) return true;
            }

            // For testing: if hardcoded token exists, use it
            if (!string.IsNullOrEmpty(AccessToken))
            {
                return true;
            }

            return await FullAuthAsync(showDialog: false);
        }

        public async Task<bool> EnsureTokenValidAsync_NoPopup()
        {
            if (IsValid()) return true;

            if (!string.IsNullOrEmpty(RefreshToken))
            {
                if (await RefreshAsync()) return true;
            }

            // Không gọi FullAuthAsync()
            return IsValid();
        }

        /* ===========================================================
         *   AUTH FLOW – PKCE
         * =========================================================== */

        private async Task<bool> FullAuthAsync(bool showDialog)
        {
            try
            {
                // Tạo PKCE
                (_codeVerifier, _codeChallenge) = Pkce.GenerateCodes();

                var authUrl =
                    "https://accounts.spotify.com/authorize" +
                    "?client_id=" + Uri.EscapeDataString(_clientId) +
                    "&response_type=code" +
                    "&redirect_uri=" + Uri.EscapeDataString(_redirectUri) +
                    "&scope=" + Uri.EscapeDataString(_scope) +
                    "&code_challenge_method=S256" +
                    "&code_challenge=" + _codeChallenge +
                    (showDialog ? "&show_dialog=true" : "");

                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });

                // wait callback code
                var code = await WaitForAuthCodeAsync();
                if (string.IsNullOrEmpty(code))
                    return false;

                // Exchange code lấy token
                return await ExchangeCodeAsync(code);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Auth error: " + ex.Message);
                return false;
            }
        }

        private async Task<bool> ExchangeCodeAsync(string code)
        {
            using var http = new HttpClient();
            var body = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = _redirectUri,
                ["code_verifier"] = _codeVerifier ?? ""
            };
            var res = await http.PostAsync("https://accounts.spotify.com/api/token",
                new FormUrlEncodedContent(body));
            if (!res.IsSuccessStatusCode) return false;

            var json = await res.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            AccessToken = doc.RootElement.GetProperty("access_token").GetString();
            RefreshToken = doc.RootElement.TryGetProperty("refresh_token", out var rt)
                ? rt.GetString()
                : RefreshToken;
            var expires = doc.RootElement.GetProperty("expires_in").GetInt32();
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expires - 30);
            SaveToken();
            return true;
        }

        public async Task<bool> RefreshAsync()
        {
            if (string.IsNullOrEmpty(RefreshToken)) return false;
            try
            {
                using var http = new HttpClient();
                var body = new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = RefreshToken
                };
                var res = await http.PostAsync("https://accounts.spotify.com/api/token",
                    new FormUrlEncodedContent(body));
                if (!res.IsSuccessStatusCode) return false;

                var json = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                AccessToken = doc.RootElement.GetProperty("access_token").GetString();
                if (doc.RootElement.TryGetProperty("refresh_token", out var rt))
                    RefreshToken = rt.GetString();
                var expires = doc.RootElement.GetProperty("expires_in").GetInt32();
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expires - 30);
                SaveToken();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Refresh token failed: " + ex.Message);
                return false;
            }
        }

        /* ===========================================================
         *   LISTENER – get code via RedirectUri (http://127.0.0.1:5173/callback)
         * =========================================================== */

        private async Task<string?> WaitForAuthCodeAsync()
        {
            var listener = new System.Net.HttpListener();
            listener.Prefixes.Add(_redirectUri.EndsWith("/") ? _redirectUri : _redirectUri + "/");
            listener.Start();

            try
            {
                var ctx = await listener.GetContextAsync();
                var req = ctx.Request;
                var resp = ctx.Response;

                var code = req.QueryString["code"];
                var html = "<html><body style='font-family:sans-serif;text-align:center;padding-top:40px;'>Sleptify connected.<br/>You can close this window.</body></html>";
                var buf = Encoding.UTF8.GetBytes(html);
                resp.ContentLength64 = buf.Length;
                await resp.OutputStream.WriteAsync(buf, 0, buf.Length);
                resp.OutputStream.Close();
                listener.Stop();
                return code;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WaitAuth error: " + ex.Message);
                return null;
            }
            finally
            {
                try { listener.Stop(); } catch { }
            }
        }
    }

    /* ===========================================================
     *   PKCE Utilities
     * =========================================================== */

    internal static class Pkce
    {
        public static (string verifier, string challenge) GenerateCodes()
        {
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            var verifier = Base64UrlEncode(bytes);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var challenge = Base64UrlEncode(sha256.ComputeHash(Encoding.ASCII.GetBytes(verifier)));
            return (verifier, challenge);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}