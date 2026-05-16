namespace TinHocTHPT.Models.ViewModels.Student;

public class RecommendedExercise
{
    public int ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string DifficultyDisplay { get; set; } = string.Empty;
    public string DifficultyClass { get; set; } = string.Empty;
    public string UrgencyLevel { get; set; } = string.Empty;
    public string UrgencyClass { get; set; } = string.Empty;
    public string FitLabel { get; set; } = string.Empty;
    public string FitClass { get; set; } = string.Empty;
}
