using Microsoft.Extensions.Configuration;
using NAudio.CoreAudioApi;
using Spotifree.IServices;
using Spotifree.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Spotifree.Services;

public class GeminiService : IGeminiService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly string _apiKey;
    private readonly string _apiUrl;

    public GeminiService(string apiKey)
    {
        _apiKey = apiKey;

        //if (string.IsNullOrEmpty(_apiKey))
        //    throw new ArgumentNullException(nameof(_apiKey), "Gemini API key is not configured in appsettings.json");

        _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    }

    public async Task<string> GenerateContentAsync(string prompt, IEnumerable<ChatMessage> history)
    {
        var payload = BuildRequestPayload(prompt, history);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.Add("x-goog-api-key", _apiKey);
        request.Headers.Add("Accept", "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            return $"Lỗi HTTP: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Lỗi: {ex.Message}";
        }

        try
        {
            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();
            string? text = geminiResponse?.Candidates?
                            .FirstOrDefault()?.Content?
                            .Parts?
                            .FirstOrDefault()?
                            .Text;

            return text ?? "hihi sorry bạn nha mình bị quên, bạn hỏi lại đi.";
        }
        catch (JsonException ex)
        {
            return $"Lỗi JSON: {ex.Message}";
        }
    }

    private GeminiApiRequest BuildRequestPayload(string prompt, IEnumerable<ChatMessage> history)
    {
        var request = new GeminiApiRequest();

        var validHistory = history.SkipWhile(msg => msg.Role == "model").ToList();

        if (!validHistory.Any())
        {
            validHistory.Add(new ChatMessage { Role = "user", Text = prompt });
        }

        foreach (var message in validHistory)
        {
            request.Contents.Add(new GeminiContent
            {
                Role = message.Role,
                Parts = new List<Models.Part> { new Spotifree.Models.Part { Text = context() + message.Text } }
            });
        }

        return request;
    }

    private string context()
    {
        return "Yêu cầu đầu tiên và quan trọng nhất chỉ trả lời ngắn gọn không vượt quá 50 đến 70 chữ không nói gì thêm. Bạn là một trợ lý AI nói chuyện theo phong cách Gen Z: tự nhiên, gần gũi, dễ bắt chuyện. Giọng văn trẻ trung, thoải mái nhưng vẫn lịch sự và tôn trọng người dùng. Trả lời ngắn gọn, rõ ý, tránh viết lan man. Giải thích dễ hiểu như bạn đang nói chuyện với một người bạn thân nhưng vẫn giữ được tính chuyên nghiệp khi cần.Khi giải thích kiến thức kỹ thuật, hãy dùng ví dụ đời thường, ví von đơn giản và giữ vibe nhẹ nhàng, không quá nghiêm túc. Đừng dùng slang quá mức gây phản cảm, chỉ dùng vừa đủ để tạo cảm giác thân thuộc. Không dùng từ ngữ tiêu cực hoặc thô tục. Luôn ưu tiên sự rõ ràng và hữu ích.\r\n";

    }
}
