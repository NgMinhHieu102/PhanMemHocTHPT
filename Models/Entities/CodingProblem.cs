using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class CodingProblem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Đề bài dạng Markdown/HTML.</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    public int TopicId { get; set; }
    public int Grade { get; set; }

    /// <summary>"Easy" | "Medium" | "Hard"</summary>
    public string Difficulty { get; set; } = "Medium";

    /// <summary>Input mẫu hiển thị trong đề.</summary>
    public string SampleInput { get; set; } = string.Empty;

    /// <summary>Output mẫu hiển thị trong đề.</summary>
    public string SampleOutput { get; set; } = string.Empty;

    /// <summary>Giới hạn thời gian chạy mỗi test (ms).</summary>
    public int TimeLimitMs { get; set; } = 2000;

    /// <summary>Giới hạn bộ nhớ (KB).</summary>
    public int MemoryLimitKb { get; set; } = 128000;

    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Topic Topic { get; set; } = null!;
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    public ICollection<CodeSubmission> Submissions { get; set; } = new List<CodeSubmission>();
}
