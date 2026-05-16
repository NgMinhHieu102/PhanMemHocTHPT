using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.ViewModels.Admin;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class SubmissionController : Controller
{
    private readonly ISubmissionService _submissionService;
    private readonly DataContext _context;

    public SubmissionController(ISubmissionService submissionService, DataContext context)
    {
        _submissionService = submissionService;
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int? classId, int page = 1)
    {
        var data = await _submissionService.GetAdminPagedAsync(search, classId, page, 10);
        ViewBag.Classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Search = search;
        ViewBag.ClassId = classId;
        return View(data);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var sub = await _submissionService.GetDetailAsync(id);
        if (sub == null) return NotFound();

        var answers = sub.SubmissionAnswers
            .OrderBy(sa => sa.Question.OrderIndex)
            .ThenBy(sa => sa.Id)
            .Select(sa =>
            {
                var selected = sa.SelectedAnswerId.HasValue
                    ? sa.Question.Answers.FirstOrDefault(a => a.Id == sa.SelectedAnswerId.Value)
                    : null;
                var correctList = sa.Question.Answers.Where(a => a.IsCorrect)
                    .Select(a => string.IsNullOrWhiteSpace(a.SubIndex) ? a.Label : $"{a.SubIndex}: {a.Content}")
                    .ToList();
                return new SubmissionAnswerDetail
                {
                    QuestionId = sa.QuestionId,
                    QuestionContent = sa.Question.Content,
                    QuestionType = sa.Question.QuestionType,
                    SubIndex = selected?.SubIndex,
                    SelectedLabel = selected?.Label,
                    SelectedContent = selected?.Content,
                    CorrectAnswers = string.Join(" | ", correctList),
                    IsCorrect = sa.IsCorrect ?? false
                };
            })
            .ToList();

        var vm = new SubmissionDetailViewModel
        {
            Id = sub.Id,
            StudentName = sub.Student.User.FullName,
            ExerciseTitle = sub.Exercise?.Title ?? sub.Exam?.Title ?? "Không xác định",
            Score = sub.Score,
            MaxScore = sub.MaxScore,
            TimeSpent = sub.TimeSpent,
            AiFeedback = sub.AiFeedback ?? string.Empty,
            SubmittedAt = sub.SubmittedAt,
            Answers = answers
        };

        return View(vm);
    }
}
