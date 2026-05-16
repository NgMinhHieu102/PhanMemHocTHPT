namespace TinHocTHPT.Models.ViewModels.Admin;

public class SubmissionDetailViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ExerciseTitle { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public int TimeSpent { get; set; }
    public string AiFeedback { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public List<SubmissionAnswerDetail> Answers { get; set; } = new();
}

public class SubmissionAnswerDetail
{
    public int QuestionId { get; set; }
    public string QuestionContent { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string? SubIndex { get; set; }
    public string? SelectedLabel { get; set; }
    public string? SelectedContent { get; set; }
    public string CorrectAnswers { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
