using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class Answer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }

    [Required]
    public string Label { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string? SubIndex { get; set; }
    public bool IsCorrect { get; set; }

    public Question Question { get; set; } = null!;
}
