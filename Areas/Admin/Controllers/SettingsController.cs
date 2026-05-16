using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly DataContext _context;

    public SettingsController(DataContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var setting = await _context.SystemSettings.FirstOrDefaultAsync() ?? new SystemSetting();
        return View(setting);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SystemSetting model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var entity = await _context.SystemSettings.FirstOrDefaultAsync();
            if (entity == null)
            {
                model.UpdatedAt = DateTime.UtcNow;
                _context.SystemSettings.Add(model);
            }
            else
            {
                entity.SystemName = model.SystemName;
                entity.Description = model.Description;
                entity.MaxAttempts = model.MaxAttempts;
                entity.EnableLogging = model.EnableLogging;
                entity.Language = model.Language;
                entity.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật cấu hình thành công";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể lưu cấu hình do dữ liệu không hợp lệ.");
            return View(model);
        }
    }
}
