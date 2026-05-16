namespace TinHocTHPT.Models.ViewModels.Student;

public class ResultViewModel
{
    public int ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public int Percentage { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public int SkippedCount { get; set; }
    public int TimeSpent { get; set; }
    public string AiFeedback { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string GradeEmoji { get; set; } = string.Empty;
    public string ScoreColorClass { get; set; } = string.Empty;
    public List<RecommendedExercise> Recommendations { get; set; } = new();
    public List<QuestionResultItem> QuestionResults { get; set; } = new();
}

public class QuestionResultItem
{
    public int OrderIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int? SelectedAnswerId { get; set; }
    public string? EssayAnswer { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<AnswerResultItem> Answers { get; set; } = new();
}

public class AnswerResultItem
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? SubIndex { get; set; }
    public bool IsCorrect { get; set; }
}
