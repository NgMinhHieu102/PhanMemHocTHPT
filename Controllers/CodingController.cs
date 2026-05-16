using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Student;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Controllers;

[Authorize(Roles = "Student,Admin")]
public class CodingController : Controller
{
    private const string SubmitErrorMessage = "Không thể nộp bài. Vui lòng thử lại sau.";
    private const string RunSampleErrorMessage = "Không thể chạy thử. Vui lòng thử lại sau.";

    private readonly ICodingService _codingService;
    private readonly IStudentService _studentService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CodingController> _logger;

    public CodingController(
        ICodingService codingService,
        IStudentService studentService,
        UserManager<ApplicationUser> userManager,
        ILogger<CodingController> logger)
    {
        _codingService = codingService;
        _studentService = studentService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var profile = isAdmin ? null : await _studentService.GetByUserIdAsync(user.Id);

        var problems = await _codingService.GetProblemsAsync(null);
        var studentId = profile?.Id ?? 0;

        var problemIds = problems.Select(p => p.Id).ToList();
        var acceptedMap = studentId > 0
            ? await _codingService.GetAcceptedMapAsync(studentId, problemIds)
            : problemIds.ToDictionary(id => id, _ => false);

        var vm = new CodingIndexViewModel
        {
            Grade = profile?.Grade ?? 10,
            BrowseAllGrades = isAdmin,
            Problems = problems.Select(p => new CodingProblemListItem
            {
                Id = p.Id,
                Title = p.Title,
                Grade = p.Grade,
                TopicName = p.Topic?.Name ?? "",
                Difficulty = p.Difficulty,
                TestCaseCount = p.TestCases.Count,
                IsAccepted = acceptedMap.GetValueOrDefault(p.Id, false)
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Problem(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        var problem = await _codingService.GetProblemDetailAsync(id);
        if (problem is null) return NotFound();

        var profile = await _studentService.GetByUserIdAsync(user.Id);
        var isAccepted = profile != null && await _codingService.HasAcceptedAsync(profile.Id, id);

        var vm = new CodingProblemViewModel
        {
            Id = problem.Id,
            Title = problem.Title,
            Description = problem.Description,
            Difficulty = problem.Difficulty,
            TopicName = problem.Topic?.Name ?? "",
            Grade = problem.Grade,
            SampleInput = problem.SampleInput,
            SampleOutput = problem.SampleOutput,
            TimeLimitMs = problem.TimeLimitMs,
            MemoryLimitKb = problem.MemoryLimitKb,
            IsAccepted = isAccepted,
            SampleTests = problem.TestCases
                .Where(tc => tc.IsSample)
                .Select(tc => new SampleTestItem
                {
                    Input = tc.Input,
                    ExpectedOutput = tc.ExpectedOutput
                }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("code-run")]
    public async Task<IActionResult> Submit([FromBody] CodeSubmitRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var profile = await _studentService.GetByUserIdAsync(user.Id);
        if (profile is null) return Unauthorized();

        try
        {
            var submission = await _codingService.SubmitCodeAsync(
                profile.Id, request.ProblemId, request.SourceCode, request.Language);

            return Json(new
            {
                success = true,
                verdict = submission.Verdict,
                passedTests = submission.PassedTests,
                totalTests = submission.TotalTests,
                executionTimeMs = submission.ExecutionTimeMs,
                memoryUsedKb = submission.MemoryUsedKb,
                compileOutput = submission.CompileOutput
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Submit failed for user {UserId}, problem {ProblemId}", user?.Id, request?.ProblemId);
            return Json(new { success = false, error = SubmitErrorMessage });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("code-run")]
    public async Task<IActionResult> RunSample([FromBody] CodeSubmitRequest request)
    {
        try
        {
            var result = await _codingService.RunSampleAsync(
                request.ProblemId, request.SourceCode, request.Language, request.Stdin);

            return Json(new
            {
                success = true,
                stdout = result.Stdout,
                stderr = result.Stderr,
                compileOutput = result.CompileOutput,
                statusDescription = result.StatusDescription,
                timeMs = result.TimeMs,
                memoryKb = result.MemoryKb
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RunSample failed for problem {ProblemId}", request?.ProblemId);
            return Json(new { success = false, error = RunSampleErrorMessage });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Submissions(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        var profile = await _studentService.GetByUserIdAsync(user.Id);
        if (profile is null) return RedirectToAction("Dashboard", "StudentPortal");

        var problem = await _codingService.GetProblemDetailAsync(id);
        if (problem is null) return NotFound();

        var history = await _codingService.GetSubmissionHistoryAsync(profile.Id, id);
        ViewBag.ProblemTitle = problem.Title;
        ViewBag.ProblemId = id;

        var vm = history.Select(s => new CodingSubmissionItem
        {
            Id = s.Id,
            Verdict = s.Verdict,
            PassedTests = s.PassedTests,
            TotalTests = s.TotalTests,
            Language = s.Language,
            ExecutionTimeMs = s.ExecutionTimeMs,
            MemoryUsedKb = s.MemoryUsedKb,
            CompileOutput = s.CompileOutput,
            SubmittedAt = s.SubmittedAt,
            SourceCode = s.SourceCode
        }).ToList();

        return View(vm);
    }
}

public class CodeSubmitRequest
{
    public int ProblemId { get; set; }
    public string SourceCode { get; set; } = string.Empty;
    public string Language { get; set; } = "python";
    public string Stdin { get; set; } = string.Empty;
}
