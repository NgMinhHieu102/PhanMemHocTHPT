using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Controllers;

public class SetupController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SetupController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> CreateAdmin()
    {
        // Tạo roles nếu chưa có
        var roles = new[] { "Admin", "Teacher", "Student" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Xóa admin cũ nếu có
        var existingAdmin = await _userManager.FindByNameAsync("admin");
        if (existingAdmin != null)
        {
            await _userManager.DeleteAsync(existingAdmin);
        }

        // Tạo admin mới
        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@tinhoc.edu.vn",
            FullName = "Quản trị viên",
            Role = "Admin",
            Status = "Active",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(admin, "admin123");
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
            return Json(new { success = true, message = "✅ Tạo admin thành công! Username: admin, Password: admin123" });
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message = $"❌ Lỗi: {errors}" });
        }
    }
}