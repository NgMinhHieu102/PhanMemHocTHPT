namespace TinHocTHPT.Models.ViewModels.Admin;

public class ExerciseStatsViewModel
{
    public int TotalExercises { get; set; }
    public int TotalAttempts { get; set; }
    public decimal AverageScore { get; set; }
    public List<string> Labels { get; set; } = new();
    public List<int> Attempts { get; set; } = new();
}
