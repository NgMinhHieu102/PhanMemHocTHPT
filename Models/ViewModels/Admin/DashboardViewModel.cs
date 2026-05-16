namespace TinHocTHPT.Models.ViewModels.Admin;

public class DashboardViewModel
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int TotalTopics { get; set; }
    public int TotalExercises { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalSubmissions { get; set; }
    public int TotalAttempts { get; set; }
    public int CompletionRate { get; set; }
    public List<string> ChartLabels { get; set; } = new();
    public List<int> SubmissionsPerDay { get; set; } = new();
}
