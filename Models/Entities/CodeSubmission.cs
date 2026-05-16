namespace TinHocTHPT.Models.Entities;

/// <summary>Verdict codes cho Online Judge.</summary>
public static class Verdicts
{
    public const string Accepted = "AC";
    public const string WrongAnswer = "WA";
    public const string TimeLimitExceeded = "TLE";
    public const string RuntimeError = "RE";
    public const string CompilationError = "CE";
    public const string Pending = "PENDING";
}

public class CodeSubmission
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CodingProblemId { get; set; }

    /// <summary>Source code học sinh nộp.</summary>
    public string SourceCode { get; set; } = string.Empty;

    /// <summary>"python" | "cpp"</summary>
    public string Language { get; set; } = "python";

    /// <summary>AC | WA | TLE | RE | CE | PENDING</summary>
    public string Verdict { get; set; } = Verdicts.Pending;

    public int PassedTests { get; set; }
    public int TotalTests { get; set; }

    /// <summary>Compile error output (nếu CE).</summary>
    public string? CompileOutput { get; set; }

    /// <summary>Thời gian thực thi max (ms).</summary>
    public int ExecutionTimeMs { get; set; }

    /// <summary>Bộ nhớ sử dụng max (KB).</summary>
    public int MemoryUsedKb { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public StudentProfile Student { get; set; } = null!;
    public CodingProblem Problem { get; set; } = null!;
}
