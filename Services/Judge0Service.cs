using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Services;

public class Judge0Service : IJudge0Service
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public Judge0Service(HttpClient http, IConfiguration config)
    {
        _http = http;
        _baseUrl = config["Judge0:BaseUrl"] ?? "https://judge0-ce.p.rapidapi.com";
        _apiKey = config["Judge0:ApiKey"] ?? "";
    }

    public async Task<CodeExecutionResult> ExecuteAsync(
        string sourceCode, string stdin, int languageId, int timeLimitMs, int memoryLimitKb)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return new CodeExecutionResult
            {
                StatusId = -1,
                StatusDescription = "Judge0 API key is missing",
                Stderr = "Thiếu cấu hình Judge0:ApiKey. Hãy đặt qua user-secrets hoặc biến môi trường Judge0__ApiKey."
            };
        }

        var payload = new
        {
            source_code = Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceCode)),
            stdin = Convert.ToBase64String(Encoding.UTF8.GetBytes(stdin)),
            language_id = languageId,
            cpu_time_limit = Math.Max(timeLimitMs / 1000.0, 1),
            memory_limit = memoryLimitKb,
            base64_encoded = true
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{_baseUrl}/submissions?base64_encoded=true&wait=true&fields=stdout,stderr,compile_output,status_id,status,time,memory");
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        request.Headers.Add("X-RapidAPI-Key", _apiKey);
        request.Headers.Add("X-RapidAPI-Host", "judge0-ce.p.rapidapi.com");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new CodeExecutionResult
            {
                StatusId = -1,
                StatusDescription = $"Judge0 API error: {response.StatusCode}",
                Stderr = body
            };
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var stdout = DecodeBase64(root, "stdout");
        var stderr = DecodeBase64(root, "stderr");
        var compileOutput = DecodeBase64(root, "compile_output");
        var statusId = root.TryGetProperty("status_id", out var sid) ? sid.GetInt32() : 0;
        var statusDesc = root.TryGetProperty("status", out var st)
            && st.TryGetProperty("description", out var desc)
                ? desc.GetString() ?? "" : "";
        var time = root.TryGetProperty("time", out var t) && t.ValueKind == JsonValueKind.String
            ? (int)(double.Parse(t.GetString()!) * 1000) : 0;
        var memory = root.TryGetProperty("memory", out var m) && m.ValueKind == JsonValueKind.Number
            ? m.GetInt32() : 0;

        return new CodeExecutionResult
        {
            Stdout = stdout,
            Stderr = stderr,
            CompileOutput = string.IsNullOrWhiteSpace(compileOutput) ? null : compileOutput,
            StatusId = statusId,
            StatusDescription = statusDesc,
            TimeMs = time,
            MemoryKb = memory
        };
    }

    private static string DecodeBase64(JsonElement root, string prop)
    {
        if (!root.TryGetProperty(prop, out var el) || el.ValueKind != JsonValueKind.String)
            return string.Empty;
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(el.GetString()!));
        }
        catch
        {
            return el.GetString() ?? string.Empty;
        }
    }
}
