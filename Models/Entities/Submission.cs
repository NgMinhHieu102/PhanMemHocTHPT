using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class Submission
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int? ExerciseId { get; set; }
    public int? ExamId { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; } = 10;
    public int TimeSpent { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string? AiFeedback { get; set; }

    public StudentProfile Student { get; set; } = null!;
    public Exercise? Exercise { get; set; }
    public Exam? Exam { get; set; }
    public ICollection<SubmissionAnswer> SubmissionAnswers { get; set; } = new List<SubmissionAnswer>();
}
