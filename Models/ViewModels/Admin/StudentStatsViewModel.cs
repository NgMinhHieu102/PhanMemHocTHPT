namespace TinHocTHPT.Models.ViewModels.Admin;

public class StudentStatsViewModel
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public decimal AverageScore { get; set; }
    public List<string> Labels { get; set; } = new();
    public List<decimal> Scores { get; set; } = new();
}
