using TinHocTHPT.Models.ViewModels.Admin;

namespace TinHocTHPT.Services.Interfaces;

public interface IStatsService
{
    Task<DashboardViewModel> GetDashboardStatsAsync();
    Task<StudentStatsViewModel> GetStudentStatsAsync(DateTime? from, DateTime? to, int? classId, int? grade);
    Task<ExerciseStatsViewModel> GetExerciseStatsAsync(DateTime? from, DateTime? to, int? topicId);
    Task<int> GetRankInClassAsync(int studentId, int classId);
    Task<int> GetRankInGradeAsync(int studentId, int grade);
}
