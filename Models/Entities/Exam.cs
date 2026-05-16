using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class Exam
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int Grade { get; set; }
    public int Duration { get; set; }
    public int TotalQuestions { get; set; }
    public string Status { get; set; } = "Active";
    public string? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
