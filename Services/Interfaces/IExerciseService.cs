using TinHocTHPT.Models.Entities;
using X.PagedList;

namespace TinHocTHPT.Services.Interfaces;

public interface IExerciseService
{
    Task<List<Exercise>> GetByTopicAsync(int topicId);
    Task<Exercise?> GetWithQuestionsAsync(int exerciseId);
    Task<List<Exam>> GetExamsAsync(int grade);
    Task<Exam?> GetExamWithQuestionsAsync(int examId);
    Task<IPagedList<Exercise>> GetAdminPagedAsync(int? topicId, string? difficulty, int? grade, string? search, int page, int pageSize);
}
