using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class ClassController : Controller
{
    private readonly DataContext _context;

    public ClassController(DataContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _context.Classes.Include(c => c.Teacher).ThenInclude(t => t!.User).OrderBy(c => c.Name).ToListAsync();
        return View(data);
    }

    public async Task<IActionResult> CreateEdit(int? id)
    {
        ViewBag.Teachers = await _context.TeacherProfiles.Include(x => x.User).ToListAsync();
        if (!id.HasValue) return View(new Class());
        var item = await _context.Classes.FindAsync(id.Value);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(Class model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Teachers = await _context.TeacherProfiles.Include(x => x.User).ToListAsync();
            return View(model);
        }

        try
        {
            if (model.Id == 0) _context.Classes.Add(model); else _context.Classes.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lưu lớp học thành công";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể lưu lớp học do dữ liệu bị trùng hoặc không hợp lệ.");
            ViewBag.Teachers = await _context.TeacherProfiles.Include(x => x.User).ToListAsync();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Classes.FindAsync(id);
        if (entity is null)
        {
            TempData["Error"] = "Không tìm thấy lớp để xóa.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Classes.Remove(entity);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa lớp học.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa lớp do đang có dữ liệu liên quan.";
        }

        return RedirectToAction(nameof(Index));
    }
}
