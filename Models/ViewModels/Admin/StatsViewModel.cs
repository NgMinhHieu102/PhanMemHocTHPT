namespace TinHocTHPT.Models.ViewModels.Admin;

public class StatsViewModel
{
    public StudentStatsViewModel StudentStats { get; set; } = new();
    public ExerciseStatsViewModel ExerciseStats { get; set; } = new();
}
