using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class Question
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public int? ExamId { get; set; }
    public int TopicId { get; set; }

    /// <summary>Legacy display/admin type. New scoring uses QuestionType.</summary>
    public string Type { get; set; } = "MultipleChoice";
    public string QuestionType { get; set; } = QuestionTypes.MultipleChoice;

    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>Optional explanation shown to students on the result page; when set, overrides auto-generated text.</summary>
    public string? Explanation { get; set; }

    public string? ImageUrl { get; set; }
    public string Difficulty { get; set; } = "Medium";
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Exercise Exercise { get; set; } = null!;
    public Exam? Exam { get; set; }
    public Topic Topic { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<SubmissionAnswer> SubmissionAnswers { get; set; } = new List<SubmissionAnswer>();
}

public static class QuestionTypes
{
    public const string MultipleChoice = "MultipleChoice";
    public const string MultipleTrue = "MultipleTrue";
    public const string Essay = "Essay";
}
