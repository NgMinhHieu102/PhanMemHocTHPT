using TinHocTHPT.Models.ViewModels.Student;

namespace TinHocTHPT.Services.Interfaces;

public interface IRecommendService
{
    Task<List<RecommendedExercise>> GetRecommendationsAsync(int studentId, int grade);
}
