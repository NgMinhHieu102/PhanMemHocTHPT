namespace TinHocTHPT.Models.ViewModels.Student;

public class ExerciseDoViewModel
{
    public int ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public int TimeLimit { get; set; }
    public List<QuestionDoItem> Questions { get; set; } = new();
}

public class QuestionDoItem
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string DifficultyDisplay { get; set; } = string.Empty;
    public List<AnswerDoItem> Answers { get; set; } = new();
}

public class AnswerDoItem
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? SubIndex { get; set; }
}

public class QuestionAnswerDto
{
    public int QuestionId { get; set; }
    public int? SelectedAnswerId { get; set; }
    public Dictionary<string, bool>? TruefalseAnswers { get; set; }
    public string? EssayAnswer { get; set; }
}
