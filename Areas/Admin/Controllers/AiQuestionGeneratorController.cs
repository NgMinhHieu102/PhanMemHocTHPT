using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Admin;
using TinHocTHPT.Services;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class AiQuestionGeneratorController : Controller
{
    private readonly DataContext _context;
    private readonly IAiQuestionGeneratorService _aiService;
    private readonly IQuestionLibraryService _libraryService;

    public AiQuestionGeneratorController(DataContext context, IAiQuestionGeneratorService aiService, IQuestionLibraryService libraryService)
    {
        _context = context;
        _aiService = aiService;
        _libraryService = libraryService;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new AiQuestionGeneratorViewModel();
        await PopulateTopics(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AiQuestionGeneratorViewModel vm)
    {
        await PopulateTopics(vm);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (string.IsNullOrWhiteSpace(vm.Content) || vm.Content.Length < 80)
        {
            ModelState.AddModelError(nameof(vm.Content), "Nội dung bài học cần ít nhất 80 ký tự để AI tạo câu hỏi chính xác.");
            return View(vm);
        }

        var topic = await _context.Topics.FindAsync(vm.TopicId);
        if (topic == null)
        {
            ModelState.AddModelError(nameof(vm.TopicId), "Chủ đề không tồn tại.");
            return View(vm);
        }

        List<AiGeneratedQuestion> questions;
        try
        {
            questions = await _aiService.GenerateAsync(new AiQuestionPrompt(vm.Content.Trim(), vm.QuestionType, vm.Count, vm.Difficulty, topic.Name));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Lỗi khi gọi dịch vụ AI: {ex.Message}");
            return View(vm);
        }

        if (questions.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "AI chưa tạo được câu hỏi hợp lệ. Vui lòng thử lại với nội dung khác.");
            return View(vm);
        }

        var exercise = await GetOrCreateAiExerciseAsync(topic, vm.Difficulty);
        var createdCount = await SaveGeneratedQuestionsAsync(exercise, questions, vm.QuestionType, vm.Difficulty);

        vm.IsSuccess = true;
        vm.StatusMessage = createdCount > 0
            ? $"Đã lưu {createdCount} câu hỏi vào bài tập '{exercise.Title}'."
            : "AI đã trả về câu hỏi nhưng không có câu hỏi hợp lệ để lưu.";

        return View(vm);
    }

    public async Task<IActionResult> LibraryExam()
    {
        var vm = new ExamLibraryGeneratorViewModel();
        await PopulateTopics(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LibraryExam(ExamLibraryGeneratorViewModel vm)
    {
        await PopulateTopics(vm);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var (exam, questionCount) = await _libraryService.CreateExamFromLibraryAsync(
                vm.Title.Trim(), vm.Grade, vm.TotalQuestions, vm.TopicId, vm.Difficulty);

            vm.IsSuccess = true;
            vm.StatusMessage = $"Đã tạo đề thi '{exam.Title}' với {questionCount} câu hỏi từ thư viện.";
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Lỗi khi tạo đề thi: {ex.Message}");
        }

        return View(vm);
    }

    private async Task PopulateTopics(AiQuestionGeneratorViewModel vm)
    {
        vm.TopicOptions = await _context.Topics
            .OrderBy(t => t.Grade)
            .ThenBy(t => t.Name)
            .Select(t => new SelectListItem($"Khối {t.Grade} - {t.Name}", t.Id.ToString()))
            .ToListAsync();
    }

    private async Task PopulateTopics(ExamLibraryGeneratorViewModel vm)
    {
        var topics = await _context.Topics
            .OrderBy(t => t.Grade)
            .ThenBy(t => t.Name)
            .Select(t => new SelectListItem($"Khối {t.Grade} - {t.Name}", t.Id.ToString()))
            .ToListAsync();

        topics.Insert(0, new SelectListItem("Tất cả chủ đề", string.Empty));
        vm.TopicOptions = topics;
    }

    private async Task<Exercise> GetOrCreateAiExerciseAsync(Topic topic, string difficulty)
    {
        var exerciseTitlePrefix = "AI tạo câu hỏi";
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.TopicId == topic.Id && e.Title.StartsWith(exerciseTitlePrefix));

        if (exercise != null)
        {
            return exercise;
        }

        exercise = new Exercise
        {
            Title = $"AI tạo câu hỏi - {topic.Name} {DateTime.UtcNow:yyyyMMddHHmm}",
            TopicId = topic.Id,
            Grade = topic.Grade,
            Difficulty = difficulty,
            TimeLimit = 15,
            Orientation = "AI",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        return exercise;
    }

    private async Task<int> SaveGeneratedQuestionsAsync(Exercise exercise, List<AiGeneratedQuestion> questions, string questionType, string difficulty)
    {
        var createdCount = 0;
        var orderBase = await _context.Questions.CountAsync(q => q.ExerciseId == exercise.Id);

        foreach (var generated in questions)
        {
            if (string.IsNullOrWhiteSpace(generated.Content))
            {
                continue;
            }

            var question = new Question
            {
                ExerciseId = exercise.Id,
                TopicId = exercise.TopicId,
                Type = questionType,
                QuestionType = questionType,
                Content = generated.Content.Trim(),
                Explanation = string.IsNullOrWhiteSpace(generated.Explanation) ? null : generated.Explanation.Trim(),
                Difficulty = difficulty,
                OrderIndex = ++orderBase,
                CreatedAt = DateTime.UtcNow
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            if (questionType != QuestionTypes.Essay)
            {
                var answers = generated.Answers
                    .Where(a => !string.IsNullOrWhiteSpace(a.Content))
                    .ToList();

                if (questionType == QuestionTypes.MultipleChoice && !answers.Any(a => a.IsCorrect))
                {
                    continue;
                }

                if (questionType == QuestionTypes.MultipleTrue && answers.Count < 2)
                {
                    continue;
                }

                foreach (var answer in answers)
                {
                    var newAnswer = new Answer
                    {
                        QuestionId = question.Id,
                        Label = questionType == QuestionTypes.MultipleTrue ? "TF" : answer.Label,
                        SubIndex = questionType == QuestionTypes.MultipleTrue ? (answer.SubIndex ?? answer.Label)?.ToLowerInvariant() : null,
                        Content = answer.Content.Trim(),
                        IsCorrect = answer.IsCorrect
                    };
                    _context.Answers.Add(newAnswer);
                }

                await _context.SaveChangesAsync();
            }

            createdCount++;
        }

        return createdCount;
    }
}
