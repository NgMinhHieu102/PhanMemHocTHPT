using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Student;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Controllers;

[Authorize(Roles = "Student")]
public class StudentPortalController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentService _studentService;
    private readonly IRecommendService _recommendService;
    private readonly IStatsService _statsService;
    private readonly DataContext _context;

    public StudentPortalController(
        UserManager<ApplicationUser> userManager,
        IStudentService studentService,
        IRecommendService recommendService,
        IStatsService statsService,
        DataContext context)
    {
        _userManager = userManager;
        _studentService = studentService;
        _recommendService = recommendService;
        _statsService = statsService;
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        var profile = await _studentService.GetByUserIdAsync(user.Id);
        if (profile == null)
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        var recommendations = await _recommendService.GetRecommendationsAsync(profile.Id, profile.Grade);

        var rankInClass = profile.ClassId.HasValue
            ? await _statsService.GetRankInClassAsync(profile.Id, profile.ClassId.Value)
            : 0;
        var rankInGrade = await _statsService.GetRankInGradeAsync(profile.Id, profile.Grade);

        var vm = new StudentDashboardViewModel
        {
            FullName = user.FullName,
            TotalScore = profile.TotalScore,
            ExercisesDone = profile.ExercisesDone,
            TopicsStudied = profile.TopicsStudied,
            StreakDays = profile.StreakDays,
            RankInClass = rankInClass,
            RankInGrade = rankInGrade,
            ClassName = profile.Class?.Name ?? "Chưa xếp lớp",
            Recommendations = recommendations
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        var profile = await _studentService.GetByUserIdAsync(user.Id);
        if (profile is null)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        var vm = new StudentProfileEditViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = profile.Phone,
            Gender = profile.Gender
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(StudentProfileEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        var profile = await _studentService.GetByUserIdAsync(user.Id);
        if (profile is null)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        var duplicateEmail = await _context.Users.AnyAsync(u => u.Id != user.Id && u.Email == vm.Email);
        if (duplicateEmail)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email đã được sử dụng.");
            return View(vm);
        }

        var emailChanged = !string.Equals(user.Email?.Trim(), vm.Email.Trim(), StringComparison.OrdinalIgnoreCase);
        if (emailChanged)
        {
            if (string.IsNullOrWhiteSpace(vm.CurrentPassword))
            {
                ModelState.AddModelError(nameof(vm.CurrentPassword), "Vui lòng nhập mật khẩu hiện tại để đổi email.");
                return View(vm);
            }

            var passwordOk = await _userManager.CheckPasswordAsync(user, vm.CurrentPassword);
            if (!passwordOk)
            {
                ModelState.AddModelError(nameof(vm.CurrentPassword), "Mật khẩu hiện tại không đúng.");
                return View(vm);
            }
        }

        user.FullName = vm.FullName.Trim();
        if (emailChanged)
        {
            await _userManager.SetEmailAsync(user, vm.Email.Trim());
        }
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var err in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(vm);
        }

        profile.Phone = string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone.Trim();
        profile.Gender = string.IsNullOrWhiteSpace(vm.Gender) ? null : vm.Gender.Trim();

        await _context.SaveChangesAsync();
        TempData["Success"] = "Cập nhật thông tin thành công.";
        return RedirectToAction(nameof(Dashboard));
    }
}
