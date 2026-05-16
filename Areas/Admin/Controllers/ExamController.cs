using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class ExamController : Controller
{
    private readonly DataContext _context;

    public ExamController(DataContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int? grade, string? status)
    {
        var query = _context.Exams.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.Title.Contains(keyword));
        }

        if (grade.HasValue)
        {
            query = query.Where(x => x.Grade == grade.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        ViewBag.Search = search;
        ViewBag.Grade = grade;
        ViewBag.Status = status;

        var exams = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(exams);
    }

    public async Task<IActionResult> CreateEdit(int? id)
    {
        if (!id.HasValue) return View(new Exam());
        var exam = await _context.Exams.FindAsync(id.Value);
        return exam == null ? NotFound() : View(exam);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(Exam model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            if (model.Id == 0) _context.Exams.Add(model); else _context.Exams.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lưu đề thi thành công";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể lưu đề thi do dữ liệu bị trùng hoặc không hợp lệ.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Exams.FindAsync(id);
        if (entity is null)
        {
            TempData["Error"] = "Không tìm thấy đề thi để xóa.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Exams.Remove(entity);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa đề thi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa đề thi do đang có bài nộp/liên kết dữ liệu.";
        }

        return RedirectToAction(nameof(Index));
    }
}
