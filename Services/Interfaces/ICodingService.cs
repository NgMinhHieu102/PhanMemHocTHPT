using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Services.Interfaces;

public interface ICodingService
{
    Task<List<CodingProblem>> GetProblemsAsync(int? grade);
    Task<CodingProblem?> GetProblemDetailAsync(int problemId);
    Task<CodeSubmission> SubmitCodeAsync(int studentId, int problemId, string sourceCode, string language);
    Task<CodeExecutionResult> RunSampleAsync(int problemId, string sourceCode, string language, string stdin);
    Task<List<CodeSubmission>> GetSubmissionHistoryAsync(int studentId, int problemId);
    Task<bool> HasAcceptedAsync(int studentId, int problemId);
    Task<Dictionary<int, bool>> GetAcceptedMapAsync(int studentId, IEnumerable<int> problemIds);
}
