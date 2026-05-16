namespace TinHocTHPT.Services.Interfaces;

public class CodeExecutionResult
{
    public string Stdout { get; set; } = string.Empty;
    public string Stderr { get; set; } = string.Empty;
    public string? CompileOutput { get; set; }
    /// <summary>Judge0 status id: 3=AC, 5=TLE, 6=CE, 11+=RE, etc.</summary>
    public int StatusId { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public int TimeMs { get; set; }
    public int MemoryKb { get; set; }
}

public interface IJudge0Service
{
    /// <summary>
    /// Chạy source code với stdin trên Judge0 sandbox.
    /// languageId: 71 = Python 3, 54 = C++ (GCC 9.2.0)
    /// </summary>
    Task<CodeExecutionResult> ExecuteAsync(string sourceCode, string stdin, int languageId, int timeLimitMs, int memoryLimitKb);
}
