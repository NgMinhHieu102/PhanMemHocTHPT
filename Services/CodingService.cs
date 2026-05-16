using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Services;

public class CodingService : ICodingService
{
    private readonly DataContext _db;
    private readonly IJudge0Service _judge;

    // Judge0 CE language IDs
    private const int PythonLangId = 71;  // Python 3.8
    private const int CppLangId = 54;     // C++ (GCC 9.2.0)

    public CodingService(DataContext db, IJudge0Service judge)
    {
        _db = db;
        _judge = judge;
    }

    public async Task<List<CodingProblem>> GetProblemsAsync(int? grade)
    {
        var query = _db.CodingProblems
            .Where(p => p.Status == "Active")
            .Include(p => p.Topic)
            .AsQueryable();

        if (grade.HasValue)
            query = query.Where(p => p.Grade == grade.Value);

        return await query
            .OrderBy(p => p.Grade)
            .ThenBy(p => p.Difficulty == "Easy" ? 0 : p.Difficulty == "Medium" ? 1 : 2)
            .ThenBy(p => p.Title)
            .ToListAsync();
    }

    public Task<CodingProblem?> GetProblemDetailAsync(int problemId)
    {
        return _db.CodingProblems
            .Include(p => p.Topic)
            .Include(p => p.TestCases.Where(tc => tc.IsSample).OrderBy(tc => tc.OrderIndex))
            .FirstOrDefaultAsync(p => p.Id == problemId && p.Status == "Active");
    }

    public async Task<CodeSubmission> SubmitCodeAsync(int studentId, int problemId, string sourceCode, string language)
    {
        var problem = await _db.CodingProblems
            .Include(p => p.TestCases.OrderBy(tc => tc.OrderIndex))
            .FirstOrDefaultAsync(p => p.Id == problemId && p.Status == "Active")
            ?? throw new InvalidOperationException("Coding problem missing or inactive.");

        var langId = GetLanguageId(language);
        var tests = problem.TestCases.ToList();

        var submission = new CodeSubmission
        {
            StudentId = studentId,
            CodingProblemId = problemId,
            SourceCode = sourceCode,
            Language = language,
            TotalTests = tests.Count,
            SubmittedAt = DateTime.UtcNow
        };

        _db.CodeSubmissions.Add(submission);
        await _db.SaveChangesAsync();

        int passed = 0;
        int maxTimeMs = 0;
        int maxMemKb = 0;
        string? compileErr = null;
        string worstVerdict = Verdicts.Accepted;

        bool isFirstTest = true;
        foreach (var tc in tests)
        {
            if (!isFirstTest) await Task.Delay(1200); // Tránh limit 1 req/sec của RapidAPI Free
            isFirstTest = false;

            var result = await _judge.ExecuteAsync(
                sourceCode, tc.Input, langId, problem.TimeLimitMs, problem.MemoryLimitKb);

            maxTimeMs = Math.Max(maxTimeMs, result.TimeMs);
            maxMemKb = Math.Max(maxMemKb, result.MemoryKb);

            // Judge0 status: 3=Accepted, 5=TLE, 6=CE, 11+=RE
            if (result.StatusId == 6)
            {
                compileErr = result.CompileOutput;
                worstVerdict = Verdicts.CompilationError;
                break; // CE → dừng luôn
            }
            else if (result.StatusId == 5)
            {
                worstVerdict = Verdicts.TimeLimitExceeded;
            }
            else if (result.StatusId >= 7) // Runtime errors
            {
                if (worstVerdict == Verdicts.Accepted)
                    worstVerdict = Verdicts.RuntimeError;
            }
            else if (result.StatusId == 3)
            {
                // So sánh output (trim trailing whitespace)
                var expected = tc.ExpectedOutput.TrimEnd();
                var actual = result.Stdout.TrimEnd();
                if (string.Equals(expected, actual, StringComparison.Ordinal))
                {
                    passed++;
                }
                else
                {
                    if (worstVerdict == Verdicts.Accepted)
                        worstVerdict = Verdicts.WrongAnswer;
                }
            }
            else
            {
                if (worstVerdict == Verdicts.Accepted)
                    worstVerdict = Verdicts.WrongAnswer;
            }
        }

        submission.PassedTests = passed;
        submission.ExecutionTimeMs = maxTimeMs;
        submission.MemoryUsedKb = maxMemKb;
        submission.CompileOutput = compileErr;
        submission.Verdict = passed == tests.Count ? Verdicts.Accepted : worstVerdict;

        // Cập nhật điểm leaderboard khi AC lần đầu
        if (submission.Verdict == Verdicts.Accepted)
        {
            var alreadyAC = await _db.CodeSubmissions.AnyAsync(cs =>
                cs.StudentId == studentId &&
                cs.CodingProblemId == problemId &&
                cs.Id != submission.Id &&
                cs.Verdict == Verdicts.Accepted);

            if (!alreadyAC)
            {
                var student = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.Id == studentId);
                if (student is not null)
                {
                    // +10 điểm cho mỗi bài AC lần đầu
                    student.TotalScore += 10;
                    student.ExercisesDone += 1;
                    student.LastActive = DateTime.UtcNow;
                }
            }
        }

        await _db.SaveChangesAsync();
        return submission;
    }

    public async Task<CodeExecutionResult> RunSampleAsync(int problemId, string sourceCode, string language, string stdin)
    {
        var problem = await _db.CodingProblems
            .Include(p => p.TestCases.Where(tc => tc.IsSample).OrderBy(tc => tc.OrderIndex))
            .FirstOrDefaultAsync(p => p.Id == problemId && p.Status == "Active")
            ?? throw new InvalidOperationException("Coding problem missing or inactive.");

        var langId = GetLanguageId(language);
        // Nếu user cung cấp stdin, dùng nó; nếu không, chạy code với empty stdin (chứ không dùng sample input)
        var input = stdin ?? string.Empty;
        return await _judge.ExecuteAsync(sourceCode, input, langId, problem.TimeLimitMs, problem.MemoryLimitKb);
    }

    public Task<List<CodeSubmission>> GetSubmissionHistoryAsync(int studentId, int problemId)
    {
        return _db.CodeSubmissions
            .Where(cs => cs.StudentId == studentId && cs.CodingProblemId == problemId)
            .OrderByDescending(cs => cs.SubmittedAt)
            .Take(50)
            .ToListAsync();
    }

    public Task<bool> HasAcceptedAsync(int studentId, int problemId)
    {
        return _db.CodeSubmissions.AnyAsync(cs =>
            cs.StudentId == studentId &&
            cs.CodingProblemId == problemId &&
            cs.Verdict == Verdicts.Accepted);
    }

    public async Task<Dictionary<int, bool>> GetAcceptedMapAsync(int studentId, IEnumerable<int> problemIds)
    {
        var ids = problemIds.ToList();
        var accepted = await _db.CodeSubmissions
            .Where(cs => cs.StudentId == studentId && ids.Contains(cs.CodingProblemId) && cs.Verdict == Verdicts.Accepted)
            .Select(cs => cs.CodingProblemId)
            .Distinct()
            .ToListAsync();

        return ids.ToDictionary(id => id, id => accepted.Contains(id));
    }

    private static int GetLanguageId(string language) => language.ToLowerInvariant() switch
    {
        "cpp" or "c++" => CppLangId,
        _ => PythonLangId
    };
}
