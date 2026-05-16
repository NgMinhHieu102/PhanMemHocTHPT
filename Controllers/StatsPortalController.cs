using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Controllers;

[Authorize(Roles = "Student")]
public class StatsPortalController : Controller
{
    private readonly DataContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StatsPortalController(DataContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account", new { area = "" });

        var profile = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (profile is null) return RedirectToAction("Dashboard", "StudentPortal", new { area = "" });

        var recent = await _context.Submissions
            .Where(s => s.StudentId == profile.Id)
            .Include(s => s.Exercise)
            .Include(s => s.Exam)
            .OrderByDescending(s => s.SubmittedAt)
            .Take(10)
            .ToListAsync();

        ViewBag.Recent = recent;
        return View(profile);
    }
}
