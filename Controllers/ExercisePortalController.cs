using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TinHocTHPT.Helpers;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Student;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Controllers;

[Authorize(Roles = "Student")]
public class ExercisePortalController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IExerciseService _exerciseService;
    private readonly ISubmissionService _submissionService;
    private readonly IStudentService _studentService;
    private readonly IRecommendService _recommendService;

    public ExercisePortalController(
        UserManager<ApplicationUser> userManager,
        IExerciseService exerciseService,
        ISubmissionService submissionService,
        IStudentService studentService,
        IRecommendService recommendService)
    {
        _userManager = userManager;
        _exerciseService = exerciseService;
        _submissionService = submissionService;
        _studentService = studentService;
        _recommendService = recommendService;
    }

    [HttpGet]
    public async Task<IActionResult> Do(int id)
    {
        var exercise = await _exerciseService.GetWithQuestionsAsync(id);
        if (exercise == null || exercise.Status != "Active")
        {
            return NotFound();
        }

        var vm = new ExerciseDoViewModel
        {
            ExerciseId = exercise.Id,
            ExerciseTitle = exercise.Title,
            TimeLimit = exercise.TimeLimit,
            Questions = exercise.Questions
                .OrderBy(q => q.OrderIndex)
                .Select(q => new QuestionDoItem
                {
                    Id = q.Id,
                    Content = q.Content,
                    Type = q.Type,
                    QuestionType = q.QuestionType,
                    ImageUrl = q.ImageUrl,
                    Difficulty = q.Difficulty,
                    DifficultyDisplay = q.Difficulty switch
                    {
                        "Easy" => "Dễ",
                        "Medium" => "Trung bình",
                        "Hard" => "Khó",
                        _ => q.Difficulty
                    },
                    Answers = q.Answers
                        .OrderBy(a => a.SubIndex ?? a.Label)
                        .Select(a => new AnswerDoItem
                        {
                            Id = a.Id,
                            Label = a.Label,
                            Content = a.Content,
                            SubIndex = a.SubIndex
                        }).ToList()
                }).ToList()
        };

        HttpContext.Session.SetString($"ExerciseStart_{id}", DateTime.UtcNow.ToString("O"));
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int exerciseId, List<QuestionAnswerDto>? answers, int timeSpent)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return RedirectToAction("Login", "Account", new { area = "" });

            var profile = await _studentService.GetByUserIdAsync(user.Id);
            if (profile == null) return Unauthorized();

            var submission = await _submissionService.SubmitExerciseAsync(
                profile.Id,
                exerciseId,
                answers ?? new List<QuestionAnswerDto>(),
                timeSpent);

            TempData["Success"] = $"Nộp bài thành công! Điểm của bạn: {submission.Score:F1}/10";
            return RedirectToAction("Result", "ExercisePortal", new { area = "", submissionId = submission.Id });
        }
        catch
        {
            TempData["Error"] = "Nộp bài thất bại. Vui lòng thử lại.";
            return RedirectToAction(nameof(Do), "ExercisePortal", new { area = "", id = exerciseId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Result(int submissionId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account", new { area = "" });

        var profile = await _studentService.GetByUserIdAsync(user.Id);
        var sub = await _submissionService.GetDetailAsync(submissionId);

        if (sub == null || sub.StudentId != profile?.Id)
        {
            return Forbid();
        }

        var groupedAnswers = sub.SubmissionAnswers
            .GroupBy(sa => sa.QuestionId)
            .Select(g => new
            {
                Question = g.First().Question,
                Rows = g.ToList(),
                IsCorrect = g.All(sa => sa.IsCorrect == true),
                IsSkipped = g.All(sa => !sa.SelectedAnswerId.HasValue && string.IsNullOrWhiteSpace(sa.EssayAnswer))
            })
            .OrderBy(g => g.Question.OrderIndex)
            .ToList();

        var correct = groupedAnswers.Count(g => g.IsCorrect);
        var wrong = groupedAnswers.Count(g => !g.IsCorrect && !g.IsSkipped);
        var skipped = groupedAnswers.Count(g => g.IsSkipped);
        var total = groupedAnswers.Count;

        var recommendations = profile != null
            ? await _recommendService.GetRecommendationsAsync(profile.Id, profile.Grade)
            : new List<RecommendedExercise>();

        var vm = new ResultViewModel
        {
            ExerciseId = sub.ExerciseId ?? 0,
            ExerciseTitle = sub.Exercise?.Title ?? sub.Exam?.Title ?? string.Empty,
            Score = sub.Score,
            MaxScore = sub.MaxScore,
            Percentage = total > 0 ? (int)Math.Round((double)correct / total * 100) : 0,
            CorrectCount = correct,
            WrongCount = wrong,
            SkippedCount = skipped,
            TimeSpent = sub.TimeSpent,
            AiFeedback = sub.AiFeedback ?? string.Empty,
            Grade = AiFeedbackHelper.GetGrade(sub.Score),
            GradeEmoji = AiFeedbackHelper.GetGradeEmoji(sub.Score),
            ScoreColorClass = AiFeedbackHelper.GetScoreCssClass(sub.Score),
            Recommendations = recommendations,
            QuestionResults = groupedAnswers
                .Select(g => new QuestionResultItem
                {
                    // Generate a concise Vietnamese explanation after students finish.
                    // This keeps feedback clear even when there is no dedicated explanation field.
                    OrderIndex = g.Question.OrderIndex,
                    Content = g.Question.Content,
                    Type = g.Question.Type,
                    QuestionType = g.Question.QuestionType,
                    IsCorrect = g.IsCorrect,
                    SelectedAnswerId = g.Rows.FirstOrDefault(r => r.SelectedAnswerId.HasValue)?.SelectedAnswerId,
                    EssayAnswer = g.Rows.FirstOrDefault()?.EssayAnswer,
                    Explanation = !string.IsNullOrWhiteSpace(g.Question.Explanation)
                        ? g.Question.Explanation.Trim()
                        : BuildExplanation(g.Question.QuestionType, g.Question.Answers, g.Rows),
                    Answers = g.Question.Answers.OrderBy(a => a.SubIndex ?? a.Label)
                        .Select(a => new AnswerResultItem
                        {
                            Id = a.Id,
                            Label = a.Label,
                            Content = a.Content,
                            SubIndex = a.SubIndex,
                            IsCorrect = a.IsCorrect
                        }).ToList()
                }).ToList()
        };

        return View(vm);
    }

    private static string BuildExplanation(
        string questionType,
        IEnumerable<Answer> answers,
        IEnumerable<SubmissionAnswer> rows)
    {
        if (questionType == QuestionTypes.MultipleTrue)
        {
            var correctParts = answers
                .Where(a => a.IsCorrect)
                .OrderBy(a => a.SubIndex ?? a.Label)
                .Select(a => $"{(a.SubIndex ?? a.Label)}: {a.Content}")
                .ToList();

            if (!correctParts.Any())
            {
                return "Đây là câu đúng/sai. Hãy đối chiếu từng mệnh đề với kiến thức lý thuyết để rút kinh nghiệm.";
            }

            return $"Các mệnh đề đúng: {string.Join("; ", correctParts)}.";
        }

        var correct = answers.FirstOrDefault(a => a.IsCorrect);
        if (correct is null)
        {
            return "Không tìm thấy đáp án chuẩn cho câu hỏi này.";
        }

        var selectedId = rows.FirstOrDefault(r => r.SelectedAnswerId.HasValue)?.SelectedAnswerId;
        var selected = selectedId.HasValue ? answers.FirstOrDefault(a => a.Id == selectedId.Value) : null;

        if (selected is null)
        {
            return $"Bạn chưa chọn đáp án. Đáp án đúng là {correct.Label}: {correct.Content}.";
        }

        if (selected.Id == correct.Id)
        {
            return $"Bạn chọn đúng. Đáp án chuẩn là {correct.Label}: {correct.Content}.";
        }

        return $"Bạn chọn {selected.Label}, nhưng đáp án đúng là {correct.Label}: {correct.Content}.";
    }
}
