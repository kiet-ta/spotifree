using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Spotifree.Models;
public class GeminiApiRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; set; } = new List<GeminiContent>();
}

public class GeminiContent
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("parts")]
    public List<Part> Parts { get; set; }
}

public class Part
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class GeminiApiResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate> Candidates { get; set; }
}

public class Candidate
{
    [JsonPropertyName("content")]
    public GeminiContent Content { get; set; }
}