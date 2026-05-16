using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class TopicController : Controller
{
    private readonly DataContext _context;

    public TopicController(DataContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? grade, string? search, string? status)
    {
        var query = _context.Topics.AsQueryable();

        if (grade.HasValue)
        {
            query = query.Where(x => x.Grade == grade.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.Name.Contains(search) || x.Description.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        ViewBag.Grade = grade;
        ViewBag.Search = search;
        ViewBag.Status = status;

        return View(await query.OrderBy(x => x.Grade).ThenBy(x => x.Name).ToListAsync());
    }

    public async Task<IActionResult> CreateEdit(int? id)
    {
        if (!id.HasValue) return View(new Topic());
        var topic = await _context.Topics.FindAsync(id.Value);
        return topic == null ? NotFound() : View(topic);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(Topic model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            if (model.Id == 0) _context.Topics.Add(model); else _context.Topics.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lưu chủ đề thành công";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể lưu chủ đề do dữ liệu bị trùng hoặc không hợp lệ.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Topics.FindAsync(id);
        if (entity is null)
        {
            TempData["Error"] = "Không tìm thấy chủ đề để xóa.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Topics.Remove(entity);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa chủ đề.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa chủ đề do đang được bài tập/câu hỏi sử dụng.";
        }

        return RedirectToAction(nameof(Index));
    }
}
