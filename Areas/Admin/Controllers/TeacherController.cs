using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Admin;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TeacherController : Controller
{
    private readonly DataContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherController(DataContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _context.TeacherProfiles.Include(t => t.User).OrderBy(t => t.User.FullName).ToListAsync();
        return View(data);
    }

    public IActionResult Create() => View(new TeacherViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeacherViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _userManager.FindByNameAsync(vm.Username) is not null)
        {
            ModelState.AddModelError(nameof(vm.Username), "Tên đăng nhập đã tồn tại.");
            return View(vm);
        }

        if (await _userManager.FindByEmailAsync(vm.Email) is not null)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email đã được sử dụng.");
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Username,
            Email = vm.Email,
            FullName = vm.FullName,
            Role = "Teacher",
            Status = vm.Status,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, string.IsNullOrWhiteSpace(vm.Password) ? "Teacher@123" : vm.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(vm);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Teacher");
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await _userManager.DeleteAsync(user);
            return View(vm);
        }

        try
        {
            _context.TeacherProfiles.Add(new TeacherProfile { UserId = user.Id, Subject = vm.Subject, Phone = vm.Phone });
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể lưu hồ sơ giáo viên do dữ liệu bị trùng hoặc không hợp lệ.");
            await _userManager.RemoveFromRoleAsync(user, "Teacher");
            await _userManager.DeleteAsync(user);
            return View(vm);
        }

        TempData["Success"] = "Th\u00EAm gi\u00E1o vi\u00EAn th\u00E0nh c\u00F4ng";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var teacher = await _context.TeacherProfiles.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
        if (teacher == null) return NotFound();

        return View(new TeacherViewModel
        {
            Id = teacher.Id,
            UserId = teacher.UserId,
            FullName = teacher.User.FullName,
            Email = teacher.User.Email ?? string.Empty,
            Username = teacher.User.UserName ?? string.Empty,
            Subject = teacher.Subject,
            Phone = teacher.Phone,
            Status = teacher.User.Status
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TeacherViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var teacher = await _context.TeacherProfiles.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == vm.Id);
        if (teacher == null) return NotFound();

        var hasDuplicateUsername = await _context.Users
            .AnyAsync(u => u.Id != teacher.UserId && u.UserName == vm.Username);
        if (hasDuplicateUsername)
        {
            ModelState.AddModelError(nameof(vm.Username), "Tên đăng nhập đã tồn tại.");
            return View(vm);
        }

        var hasDuplicateEmail = await _context.Users
            .AnyAsync(u => u.Id != teacher.UserId && u.Email == vm.Email);
        if (hasDuplicateEmail)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email đã được sử dụng.");
            return View(vm);
        }

        teacher.Subject = vm.Subject;
        teacher.Phone = vm.Phone;
        teacher.User.FullName = vm.FullName;
        teacher.User.Status = vm.Status;
        if (!string.Equals(teacher.User.UserName, vm.Username, StringComparison.OrdinalIgnoreCase))
        {
            var setName = await _userManager.SetUserNameAsync(teacher.User, vm.Username);
            if (!setName.Succeeded)
            {
                foreach (var err in setName.Errors) ModelState.AddModelError(string.Empty, err.Description);
                return View(vm);
            }
        }
        if (!string.Equals(teacher.User.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
        {
            var setEmail = await _userManager.SetEmailAsync(teacher.User, vm.Email);
            if (!setEmail.Succeeded)
            {
                foreach (var err in setEmail.Errors) ModelState.AddModelError(string.Empty, err.Description);
                return View(vm);
            }
        }
        var updateUser = await _userManager.UpdateAsync(teacher.User);
        if (!updateUser.Succeeded)
        {
            foreach (var err in updateUser.Errors) ModelState.AddModelError(string.Empty, err.Description);
            return View(vm);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể cập nhật giáo viên do dữ liệu bị trùng hoặc không hợp lệ.");
            return View(vm);
        }
        TempData["Success"] = "C\u1EADp nh\u1EADt gi\u00E1o vi\u00EAn th\u00E0nh c\u00F4ng";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var teacher = await _context.TeacherProfiles.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
        if (teacher is null)
        {
            TempData["Error"] = "Không tìm thấy giáo viên để xóa.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.TeacherProfiles.Remove(teacher);
            await _context.SaveChangesAsync();
            await _userManager.DeleteAsync(teacher.User);
            TempData["Success"] = "Đã xóa giáo viên.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa giáo viên do đang được tham chiếu dữ liệu khác.";
        }

        return RedirectToAction(nameof(Index));
    }
}
