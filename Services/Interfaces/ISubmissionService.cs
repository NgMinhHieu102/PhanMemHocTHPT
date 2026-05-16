using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Student;
using X.PagedList;

namespace TinHocTHPT.Services.Interfaces;

public interface ISubmissionService
{
    Task<Submission> SubmitExerciseAsync(int studentId, int exerciseId, IEnumerable<QuestionAnswerDto> answers, int timeSpent);
    Task<Submission?> GetDetailAsync(int submissionId);
    Task<IPagedList<Submission>> GetAdminPagedAsync(string? search, int? classId, int page, int pageSize);
}
