using Spotifree.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spotifree.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "AIzaSyCvCIRj0BOtSGsgvgs8OuWNd-L46Pn3zkk";
        private readonly string _apiUrl;

        public GeminiService()
        {
            _httpClient = new HttpClient();
            _apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={ApiKey}";
        }

        public async Task<string> GetChatResponseAsync(string userPrompt)
        {
            try
            {
                var payload = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = userPrompt } } }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(_apiUrl, payload);

                if (!response.IsSuccessStatusCode)
                {
                    return "Lỗi: Không gọi được Gemini API. Check API key hoặc quota.";
                }

                var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var text = jsonDoc.RootElement
                                  .GetProperty("candidates")[0]
                                  .GetProperty("content")
                                  .GetProperty("parts")[0]
                                  .GetProperty("text")
                                  .GetString();

                return text ?? "Lỗi: Không parse được response.";
            }
            catch (Exception ex)
            {
                return $"Lỗi C#: {ex.Message}";
            }
        }
    }
}
