using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Admin;
using X.PagedList;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class QuestionController : Controller
{
    private static readonly HashSet<string> AllowedImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    ];

    private readonly DataContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public QuestionController(DataContext context, IWebHostEnvironment env, IConfiguration configuration)
    {
        _context = context;
        _env = env;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(string? search, int? topicId, string? difficulty, int page = 1)
    {
        var query = _context.Questions
            .Include(q => q.Exercise)
            .Include(q => q.Topic)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(q => q.Content.Contains(keyword) || q.Exercise.Title.Contains(keyword));
        }

        if (topicId.HasValue)
        {
            query = query.Where(q => q.TopicId == topicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            query = query.Where(q => q.Difficulty == difficulty);
        }

        var ordered = query.OrderByDescending(q => q.CreatedAt);
        const int pageSize = 15;
        var total = await ordered.CountAsync();
        var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var paged = new StaticPagedList<Question>(items, page, pageSize, total);

        ViewBag.Topics = await _context.Topics.OrderBy(t => t.Grade).ThenBy(t => t.Name).ToListAsync();
        ViewBag.Search = search;
        ViewBag.TopicId = topicId;
        ViewBag.Difficulty = difficulty;
        return View(paged);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new QuestionCreateViewModel();
        await Populate(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuestionCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await Populate(vm);
            return View(vm);
        }

        // Parse SelectedItem
        if (string.IsNullOrWhiteSpace(vm.SelectedItem))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn bài tập hoặc đề thi.");
            await Populate(vm);
            return View(vm);
        }

        var parts = vm.SelectedItem.Split('-', 2);
        if (parts.Length != 2)
        {
            ModelState.AddModelError(string.Empty, "Dữ liệu không hợp lệ.");
            await Populate(vm);
            return View(vm);
        }

        var type = parts[0];
        var id = int.Parse(parts[1]);

        if (type == "exercise")
        {
            vm.ExerciseId = id;
            vm.ExamId = null;
        }
        else if (type == "exam")
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                ModelState.AddModelError(string.Empty, "Đề thi không tồn tại.");
                await Populate(vm);
                return View(vm);
            }

            // Check if exercise already exists for this exam
            var existingExercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Title == exam.Title && e.TopicId == vm.TopicId);
            if (existingExercise == null)
            {
                existingExercise = new Exercise
                {
                    Title = exam.Title,
                    TopicId = vm.TopicId,
                    Grade = exam.Grade,
                    Difficulty = "Medium",
                    TimeLimit = exam.Duration,
                    Orientation = "Exam",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Exercises.Add(existingExercise);
                await _context.SaveChangesAsync();
            }

            vm.ExerciseId = existingExercise.Id;
            vm.ExamId = id;
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Loại không hợp lệ.");
            await Populate(vm);
            return View(vm);
        }

        var (imageUrl, imageError) = await TrySaveValidatedImageAsync(vm.ImageFile);
        if (imageError is not null)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), imageError);
            await Populate(vm);
            return View(vm);
        }

        var question = new Question
        {
            ExerciseId = vm.ExerciseId,
            ExamId = vm.ExamId,
            TopicId = vm.TopicId,
            Type = vm.Type,
            QuestionType = vm.Type,
            Content = vm.Content,
            Explanation = string.IsNullOrWhiteSpace(vm.Explanation) ? null : vm.Explanation.Trim(),
            ImageUrl = imageUrl,
            Difficulty = vm.Difficulty,
            OrderIndex = await _context.Questions.CountAsync(q => q.ExerciseId == vm.ExerciseId) + 1,
            CreatedAt = DateTime.UtcNow
        };

        var normalizedAnswers = NormalizeAnswers(vm);
        if (normalizedAnswers.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Vui lòng nhập đáp án hợp lệ cho loại câu hỏi đã chọn.");
            await Populate(vm);
            return View(vm);
        }

        try
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            foreach (var ans in normalizedAnswers)
            {
                _context.Answers.Add(new Answer { QuestionId = question.Id, Label = ans.Label, SubIndex = ans.SubIndex, Content = ans.Content, IsCorrect = ans.IsCorrect });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Th\u00EAm c\u00E2u h\u1ECFi th\u00E0nh c\u00F4ng";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể lưu câu hỏi do dữ liệu bị trùng hoặc không hợp lệ.");
            await Populate(vm);
            return View(vm);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var q = await _context.Questions.Include(x => x.Answers).FirstOrDefaultAsync(x => x.Id == id);
        if (q == null) return NotFound();

        var vm = new QuestionCreateViewModel
        {
            Id = q.Id,
            ExerciseId = q.ExerciseId,
            ExamId = q.ExamId ?? 0,
            TopicId = q.TopicId,
            Type = string.IsNullOrWhiteSpace(q.QuestionType) ? q.Type : q.QuestionType,
            Content = q.Content,
            Explanation = q.Explanation,
            Difficulty = q.Difficulty,
            ExistingImageUrl = q.ImageUrl
        };

        // Set SelectedItem based on ExamId or ExerciseId
        if (vm.ExamId > 0)
        {
            vm.SelectedItem = $"exam-{vm.ExamId}";
        }
        else
        {
            vm.SelectedItem = $"exercise-{vm.ExerciseId}";
        }

        if (vm.Type == QuestionTypes.MultipleTrue)
        {
            var existing = q.Answers
                .Where(a => !string.IsNullOrWhiteSpace(a.SubIndex))
                .OrderBy(a => a.SubIndex)
                .ToDictionary(a => a.SubIndex!.ToLowerInvariant(), a => a);
            vm.TrueFalseAnswers = new[] { "a", "b", "c", "d" }
                .Select(idx => existing.TryGetValue(idx, out var ans)
                    ? new AnswerInputModel { Label = "TF", SubIndex = idx, Content = ans.Content, IsCorrect = ans.IsCorrect }
                    : new AnswerInputModel { Label = "TF", SubIndex = idx })
                .ToList();
        }
        else
        {
            vm.Answers = q.Answers
                .Where(a => string.IsNullOrWhiteSpace(a.SubIndex))
                .OrderBy(a => a.Label)
                .Select(a => new AnswerInputModel { Label = a.Label, Content = a.Content, IsCorrect = a.IsCorrect })
                .ToList();
            if (vm.Answers.Count == 0)
            {
                vm.Answers = new()
                {
                    new() { Label = "A" }, new() { Label = "B" }, new() { Label = "C" }, new() { Label = "D" }
                };
            }
        }

        await Populate(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(QuestionCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await Populate(vm);
            return View(vm);
        }

        var q = await _context.Questions.Include(x => x.Answers).FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (q == null) return NotFound();

        // Parse SelectedItem
        if (string.IsNullOrWhiteSpace(vm.SelectedItem))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn bài tập hoặc đề thi.");
            await Populate(vm);
            return View(vm);
        }

        var parts = vm.SelectedItem.Split('-', 2);
        if (parts.Length != 2)
        {
            ModelState.AddModelError(string.Empty, "Dữ liệu không hợp lệ.");
            await Populate(vm);
            return View(vm);
        }

        var type = parts[0];
        var id = int.Parse(parts[1]);

        if (type == "exercise")
        {
            vm.ExerciseId = id;
            vm.ExamId = null;
        }
        else if (type == "exam")
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                ModelState.AddModelError(string.Empty, "Đề thi không tồn tại.");
                await Populate(vm);
                return View(vm);
            }

            // Check if exercise already exists for this exam
            var existingExercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Title == exam.Title && e.TopicId == vm.TopicId);
            if (existingExercise == null)
            {
                existingExercise = new Exercise
                {
                    Title = exam.Title,
                    TopicId = vm.TopicId,
                    Grade = exam.Grade,
                    Difficulty = "Medium",
                    TimeLimit = exam.Duration,
                    Orientation = "Exam",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Exercises.Add(existingExercise);
                await _context.SaveChangesAsync();
            }

            vm.ExerciseId = existingExercise.Id;
            vm.ExamId = id;
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Loại không hợp lệ.");
            await Populate(vm);
            return View(vm);
        }

        q.ExerciseId = vm.ExerciseId;
        q.ExamId = vm.ExamId;
        q.TopicId = vm.TopicId;
        q.Type = vm.Type;
        q.QuestionType = vm.Type;
        q.Content = vm.Content;
        q.Explanation = string.IsNullOrWhiteSpace(vm.Explanation) ? null : vm.Explanation.Trim();
        q.Difficulty = vm.Difficulty;

        var (newImageUrl, imageError) = await TrySaveValidatedImageAsync(vm.ImageFile);
        if (imageError is not null)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), imageError);
            await Populate(vm);
            return View(vm);
        }

        if (!string.IsNullOrWhiteSpace(newImageUrl)) q.ImageUrl = newImageUrl;

        var normalizedAnswers = NormalizeAnswers(vm);
        if (normalizedAnswers.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Vui lòng nhập đáp án hợp lệ cho loại câu hỏi đã chọn.");
            await Populate(vm);
            return View(vm);
        }

        try
        {
            _context.Answers.RemoveRange(q.Answers);
            foreach (var ans in normalizedAnswers)
            {
                _context.Answers.Add(new Answer { QuestionId = q.Id, Label = ans.Label, SubIndex = ans.SubIndex, Content = ans.Content, IsCorrect = ans.IsCorrect });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "C\u1EADp nh\u1EADt c\u00E2u h\u1ECFi th\u00E0nh c\u00F4ng";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể cập nhật câu hỏi do dữ liệu bị trùng hoặc không hợp lệ.");
            await Populate(vm);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _context.Questions.Include(q => q.Answers).FirstOrDefaultAsync(q => q.Id == id);
        if (question is null)
        {
            TempData["Error"] = "Không tìm thấy câu hỏi để xóa.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Answers.RemoveRange(question.Answers);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa câu hỏi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa câu hỏi do đang có dữ liệu bài nộp liên quan.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task Populate(QuestionCreateViewModel vm)
    {
        vm.TopicOptions = await _context.Topics.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToListAsync();
        var exerciseOptions = await _context.Exercises.Select(e => new SelectListItem($"Bài tập: {e.Title}", $"exercise-{e.Id}")).ToListAsync();
        var examOptions = await _context.Exams.Select(e => new SelectListItem($"Đề thi: {e.Title}", $"exam-{e.Id}")).ToListAsync();
        vm.ExerciseOptions = exerciseOptions.Concat(examOptions).ToList();
        vm.ExamOptions = await _context.Exams.Select(e => new SelectListItem(e.Title, e.Id.ToString())).ToListAsync();
    }

    private static List<AnswerInputModel> NormalizeAnswers(QuestionCreateViewModel vm)
    {
        if (vm.Type == "Essay")
        {
            // Essay questions don't have predefined answers
            return [];
        }

        if (vm.Type == QuestionTypes.MultipleTrue)
        {
            var items = vm.TrueFalseAnswers
                .Select(a => new AnswerInputModel
                {
                    Label = "TF",
                    SubIndex = string.IsNullOrWhiteSpace(a.SubIndex) ? string.Empty : a.SubIndex.Trim().ToLowerInvariant(),
                    Content = string.IsNullOrWhiteSpace(a.Content) ? string.Empty : a.Content.Trim(),
                    IsCorrect = a.IsCorrect
                })
                .Where(a => !string.IsNullOrWhiteSpace(a.SubIndex) && !string.IsNullOrWhiteSpace(a.Content))
                .ToList();

            if (items.Count < 2)
            {
                return [];
            }

            return items;
        }

        var answers = vm.Answers
            .Select(a => new AnswerInputModel
            {
                Label = string.IsNullOrWhiteSpace(a.Label) ? string.Empty : a.Label.Trim(),
                Content = string.IsNullOrWhiteSpace(a.Content) ? string.Empty : a.Content.Trim(),
                IsCorrect = a.IsCorrect
            })
            .Where(a => !string.IsNullOrWhiteSpace(a.Label))
            .ToList();

        if (answers.Count < 2 || answers.Any(a => string.IsNullOrWhiteSpace(a.Content)) || answers.All(a => !a.IsCorrect))
        {
            return [];
        }

        return answers;
    }

    private async Task<(string? Url, string? Error)> TrySaveValidatedImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return (null, null);

        var maxBytes = _configuration.GetValue<long?>("AppSettings:MaxFileSizeBytes") ?? 5_242_880;
        if (file.Length > maxBytes)
            return (null, $"Ảnh vượt quá giới hạn {maxBytes / 1024} KB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedImageExtensions.Contains(ext))
            return (null, "Chỉ chấp nhận ảnh JPG, PNG, GIF hoặc WebP.");

        var folder = Path.Combine(_env.WebRootPath, "uploads", "questions");
        Directory.CreateDirectory(folder);
        var safeExt = ext is ".jpeg" ? ".jpg" : ext;
        var fileName = $"{Guid.NewGuid():N}{safeExt}";
        var fullPath = Path.Combine(folder, fileName);

        await using (var input = file.OpenReadStream())
        {
            var header = new byte[12];
            var read = await input.ReadAsync(header.AsMemory(0, 12));
            if (!HasImageSignature(header.AsSpan(0, read), ext))
                return (null, "Nội dung tệp không khớp định dạng ảnh cho phép.");

            await using var output = System.IO.File.Create(fullPath);
            await output.WriteAsync(header.AsMemory(0, read));
            await input.CopyToAsync(output);
        }

        return ($"/uploads/questions/{fileName}", null);
    }

    private static bool HasImageSignature(ReadOnlySpan<byte> head, string ext)
    {
        return ext switch
        {
            ".jpg" or ".jpeg" => head.Length >= 3 && head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF,
            ".png" => head.Length >= 8 && head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47,
            ".gif" => head.Length >= 6 && head[0] == 0x47 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x38,
            ".webp" => head.Length >= 12
                      && head[0] == 0x52 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x46
                      && head[8] == 0x57 && head[9] == 0x45 && head[10] == 0x42 && head[11] == 0x50,
            _ => false
        };
    }
}
