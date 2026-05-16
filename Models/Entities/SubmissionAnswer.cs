namespace TinHocTHPT.Models.Entities;

public class SubmissionAnswer
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public int QuestionId { get; set; }
    public int? SelectedAnswerId { get; set; }
    public bool? IsCorrect { get; set; }
    public string? EssayAnswer { get; set; }

    public Submission Submission { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public Answer? SelectedAnswer { get; set; }
}
