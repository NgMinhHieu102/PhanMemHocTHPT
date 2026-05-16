using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class ExerciseController : Controller
{
    private readonly DataContext _context;
    private readonly IExerciseService _exerciseService;

    public ExerciseController(DataContext context, IExerciseService exerciseService)
    {
        _context = context;
        _exerciseService = exerciseService;
    }

    public async Task<IActionResult> Index(int? topicId, string? difficulty, int? grade, string? search, int page = 1)
    {
        var data = await _exerciseService.GetAdminPagedAsync(topicId, difficulty, grade, search, page, 10);
        ViewBag.Topics = await _context.Topics.OrderBy(x => x.Name).ToListAsync();
        ViewBag.TopicId = topicId;
        ViewBag.Difficulty = difficulty;
        ViewBag.Grade = grade;
        ViewBag.Search = search;
        return View(data);
    }

    public async Task<IActionResult> CreateEdit(int? id)
    {
        ViewBag.Topics = await _context.Topics.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToListAsync();
        if (!id.HasValue) return View(new Exercise());
        var ex = await _context.Exercises.FindAsync(id.Value);
        return ex == null ? NotFound() : View(ex);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(Exercise model)
    {
        // Ignore navigation properties that are not part of this form post.
        ModelState.Remove(nameof(Exercise.Topic));
        ModelState.Remove(nameof(Exercise.Questions));
        ModelState.Remove(nameof(Exercise.Submissions));

        if (!ModelState.IsValid)
        {
            ViewBag.Topics = await _context.Topics.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToListAsync();
            return View(model);
        }

        try
        {
            if (model.Id == 0) _context.Exercises.Add(model); else _context.Exercises.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lưu bài tập thành công";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể lưu bài tập do dữ liệu bị trùng hoặc không hợp lệ.");
            ViewBag.Topics = await _context.Topics.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToListAsync();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Exercises.FindAsync(id);
        if (entity is null)
        {
            TempData["Error"] = "Không tìm thấy bài tập để xóa.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Exercises.Remove(entity);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa bài tập.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa bài tập do đang có câu hỏi/bài nộp liên quan.";
        }

        return RedirectToAction(nameof(Index));
    }
}
