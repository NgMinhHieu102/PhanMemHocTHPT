using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Services;

public class AiQuestionGeneratorService : IAiQuestionGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly bool _useGemini;
    private readonly string _promptInstruction;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AiQuestionGeneratorService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var geminiApiKey = configuration["Gemini:ApiKey"] ?? configuration["ApiKey"];
        var grokApiKey = configuration["Grok:ApiKey"] ?? configuration["ApiKey"];

        _promptInstruction = configuration["AiQuestionGenerator:Instruction"]?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(geminiApiKey))
        {
            _useGemini = true;
            _apiKey = geminiApiKey;
            _model = configuration["Gemini:Model"] ?? "gemini-1.5-pro";
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            return;
        }

        if (!string.IsNullOrWhiteSpace(grokApiKey))
        {
            _useGemini = false;
            _apiKey = grokApiKey;
            _model = configuration["Grok:Model"] ?? "grok-2-latest";
            _httpClient.BaseAddress = new Uri("https://api.x.ai/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            return;
        }

        throw new InvalidOperationException("AI API key is not configured. Please add Gemini:ApiKey or Grok:ApiKey to configuration.");
    }

    public async Task<List<AiGeneratedQuestion>> GenerateAsync(AiQuestionPrompt prompt, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        string responseContent;
        bool isGemini = _useGemini;
        var promptText = BuildPrompt(prompt);

        // Log prompt được gửi
        Console.WriteLine($"=== AI PROMPT ===\n{promptText}\n=== END PROMPT ===");

        if (isGemini)
        {
            var geminiRequest = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptText }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.0,
                    maxOutputTokens = 2000
                }
            };

            response = await _httpClient.PostAsJsonAsync($"v1beta/models/{_model}:generateContent?key={_apiKey}", geminiRequest, cancellationToken);
            responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Gemini API returned status {(int)response.StatusCode}: {responseContent}");
            }

            var geminiResponse = JsonSerializer.Deserialize<GeminiContentResponse>(responseContent, JsonOptions);
            var geminiText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrWhiteSpace(geminiText))
            {
                throw new InvalidOperationException($"Gemini không trả về nội dung hợp lệ. Full response: {responseContent}");
            }

            // Log text từ Gemini
            Console.WriteLine($"=== GEMINI TEXT ===\n{geminiText}\n=== END GEMINI TEXT ===");

            var geminiPayload = ExtractJson(geminiText.Trim());
            if (string.IsNullOrWhiteSpace(geminiPayload))
            {
                throw new InvalidOperationException($"Không thể phân tích nội dung trả về từ Gemini. Text: '{geminiText}'. Vui lòng kiểm tra cấu hình hoặc nội dung yêu cầu.");
            }

            // Log JSON được extract
            Console.WriteLine($"=== EXTRACTED JSON ===\n{geminiPayload}\n=== END EXTRACTED JSON ===");

            var geminiQuestions = JsonSerializer.Deserialize<List<AiGeneratedQuestion>>(geminiPayload, JsonOptions);
            if (geminiQuestions == null || geminiQuestions.Count == 0)
            {
                throw new InvalidOperationException($"AI tạo ra định dạng JSON nhưng không tìm thấy câu hỏi hợp lệ. JSON: '{geminiPayload}'");
            }

            // Log các câu hỏi được parse
            Console.WriteLine($"=== PARSED QUESTIONS ===");
            foreach (var q in geminiQuestions)
            {
                Console.WriteLine($"Question: '{q.Content}' | Answers: {q.Answers?.Count ?? 0}");
            }
            Console.WriteLine("=== END PARSED QUESTIONS ===");

            return geminiQuestions;
        }

        var requestBody = new
        {
            model = _model,
            temperature = 0.2,
            max_tokens = 1200,
            messages = new[]
            {
                new { role = "system", content = "Bạn là một trợ lý tạo câu hỏi trắc nghiệm và tự luận cho giáo viên Tin học THPT. Hãy trả về dữ liệu ở định dạng JSON thuần, không kèm giải thích ngoài JSON." },
                new { role = "user", content = BuildPrompt(prompt) }
            }
        };

        response = await _httpClient.PostAsJsonAsync("v1/chat/completions", requestBody, cancellationToken);
        responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
            {
                throw new InvalidOperationException("Đã vượt quá giới hạn sử dụng Grok API. Vui lòng kiểm tra tài khoản xAI tại https://console.x.ai/");
            }
            throw new InvalidOperationException($"Grok API returned status {(int)response.StatusCode}: {responseContent}");
        }

        var openAiResponse = JsonSerializer.Deserialize<OpenAiChatResponse>(responseContent, JsonOptions);
        var aiText = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(aiText))
        {
            throw new InvalidOperationException("Grok không trả về nội dung hợp lệ.");
        }

        var payload = ExtractJson(aiText.Trim());
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidOperationException("Không thể phân tích nội dung trả về từ AI. Vui lòng kiểm tra cấu hình hoặc nội dung yêu cầu.");
        }

        var generated = JsonSerializer.Deserialize<List<AiGeneratedQuestion>>(payload, JsonOptions);
        if (generated == null || generated.Count == 0)
        {
            throw new InvalidOperationException("AI tạo ra định dạng JSON nhưng không tìm thấy câu hỏi hợp lệ.");
        }

        return generated;
    }

    private string BuildPrompt(AiQuestionPrompt prompt)
    {
        var prefix = string.IsNullOrWhiteSpace(_promptInstruction)
            ? "Bạn là trợ lý tạo câu hỏi Tin học THPT. Hãy tạo câu hỏi theo định dạng JSON thuần, không kèm giải thích ngoài JSON."
            : _promptInstruction;
        var typeLabel = prompt.QuestionType switch
        {
            QuestionTypes.Essay => "tự luận",
            QuestionTypes.MultipleTrue => "đúng/sai",
            _ => "trắc nghiệm"
        };

        return prefix + "\n\n" +
               $"Tạo {prompt.Count} câu hỏi {typeLabel} từ nội dung sau:\n\n{prompt.Content}\n\n" +
               "QUAN TRỌNG: Trả về CHỈ JSON array, không có text nào khác.\n\n" +
               "Format JSON:\n" +
               "[\n" +
               "  {\n" +
               "    \"question\": \"Nội dung câu hỏi đầy đủ\",\n" +
               "    \"explanation\": \"Giải thích đáp án\",\n" +
               "    \"answers\": [\n" +
               "      {\"label\": \"A\", \"content\": \"Đáp án A\", \"isCorrect\": false},\n" +
               "      {\"label\": \"B\", \"content\": \"Đáp án B\", \"isCorrect\": true}\n" +
               "    ]\n" +
               "  }\n" +
               "]\n\n" +
               "Lưu ý:\n" +
               "- question PHẢI có nội dung đầy đủ, không được trống\n" +
               "- Với trắc nghiệm: 4 đáp án A/B/C/D, chỉ 1 đáp án đúng\n" +
               "- Với đúng/sai: ít nhất 3 mệnh đề với true/false\n" +
               "- Với tự luận: answers là array rỗng\n" +
               "- Mức độ: " + prompt.Difficulty;
    }

    private static string? ExtractJson(string text)
    {
        text = RemoveCodeFence(text).Trim();

        // Thử extract JSON hoàn chỉnh trước
        var json = TryExtractJsonArray(text) ?? TryExtractJsonObject(text);
        if (json != null) return json;

        // Nếu không tìm thấy JSON hoàn chỉnh, thử parse trực tiếp
        if (text.StartsWith('[') || text.StartsWith('{'))
        {
            // Thử thêm closing bracket nếu thiếu
            var fixedText = FixIncompleteJson(text);
            if (IsValidJson(fixedText)) return fixedText;
        }

        return null;
    }

    private static string FixIncompleteJson(string text)
    {
        var result = text;
        var openBrackets = 0;
        var openBraces = 0;
        var inString = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '"')
            {
                bool escaped = i > 0 && text[i - 1] == '\\';
                if (!escaped) inString = !inString;
            }
            if (inString) continue;

            if (c == '[') openBrackets++;
            else if (c == ']') openBrackets--;
            else if (c == '{') openBraces++;
            else if (c == '}') openBraces--;
        }

        // Thêm closing brackets nếu thiếu
        while (openBrackets > 0) { result += "]"; openBrackets--; }
        while (openBraces > 0) { result += "}"; openBraces--; }

        return result;
    }

    private static string RemoveCodeFence(string text)
    {
        if (text.StartsWith("```"))
        {
            var endFence = text.IndexOf("```", 3, StringComparison.Ordinal);
            if (endFence > 0)
            {
                return text[3..endFence].Trim();
            }
        }

        return text;
    }

    private static string? TryExtractJsonArray(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != '[') continue;
            var end = FindMatchingBracket(text, i, '[', ']');
            if (end <= i) continue;
            var candidate = text[i..(end + 1)];
            if (IsValidJson(candidate)) return candidate;
        }

        return null;
    }

    private static string? TryExtractJsonObject(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != '{') continue;
            var end = FindMatchingBracket(text, i, '{', '}');
            if (end <= i) continue;
            var candidate = text[i..(end + 1)];
            if (IsValidJson(candidate)) return candidate;
        }

        return null;
    }

    private static int FindMatchingBracket(string text, int start, char open, char close)
    {
        int depth = 0;
        bool inString = false;
        for (int i = start; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '"')
            {
                bool escaped = i > 0 && text[i - 1] == '\\';
                if (!escaped) inString = !inString;
            }
            if (inString) continue;
            if (c == open) depth++;
            else if (c == close)
            {
                depth--;
                if (depth == 0) return i;
            }
        }

        return -1;
    }

    private static bool IsValidJson(string candidate)
    {
        try
        {
            JsonDocument.Parse(candidate);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class OpenAiChatResponse
    {
        public List<OpenAiChoice>? Choices { get; set; }
    }

    private sealed class OpenAiChoice
    {
        public OpenAiMessage? Message { get; set; }
    }

    private sealed class OpenAiMessage
    {
        public string? Content { get; set; }
    }

    private sealed class GeminiContentResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        public string? Text { get; set; }
    }
}

public sealed record AiQuestionPrompt(string Content, string QuestionType, int Count, string Difficulty, string TopicName);
public sealed record AiGeneratedQuestion
{
    public string Content { get; init; } = string.Empty;
    public string? Explanation { get; init; }
    public List<AiGeneratedAnswer> Answers { get; init; } = new();
}

public sealed record AiGeneratedAnswer
{
    public string Label { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
    public string? SubIndex { get; init; }
}
