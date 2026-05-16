namespace TinHocTHPT.Models.ViewModels.Student;

public class StudentDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
    public int ExercisesDone { get; set; }
    public int TopicsStudied { get; set; }
    public int StreakDays { get; set; }
    public int RankInClass { get; set; }
    public int RankInGrade { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public List<RecommendedExercise> Recommendations { get; set; } = new();
}
