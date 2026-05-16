using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Student;

namespace TinHocTHPT.Controllers;

[Authorize(Roles = "Student,Admin")]
public class TopicsController : Controller
{
    private readonly DataContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TopicsController(DataContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account", new { area = "" });

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var topics = await _context.Topics
                .Where(t => t.Status == "Active")
                .Select(t => new TopicItemViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description ?? string.Empty,
                    Grade = t.Grade,
                    Icon = t.Icon,
                    ExerciseCount = t.Exercises.Count(e => e.Status == "Active")
                })
                .OrderBy(t => t.Grade)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(new TopicsViewModel { Grade = 10, BrowseAllGrades = true, Topics = topics });
        }

        var profile = await _context.StudentProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (profile is null) return RedirectToAction("Dashboard", "StudentPortal", new { area = "" });

        var studentTopics = await _context.Topics
            .Where(t => t.Status == "Active")
            .Select(t => new TopicItemViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description ?? string.Empty,
                Grade = t.Grade,
                Icon = t.Icon,
                ExerciseCount = t.Exercises.Count(e => e.Status == "Active")
            })
            .OrderBy(t => t.Grade)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return View(new TopicsViewModel { Grade = profile.Grade, Topics = studentTopics });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id && t.Status == "Active");
        if (topic == null) return NotFound();

        var exercises = await _context.Exercises
            .Where(e => e.TopicId == id && e.Status == "Active")
            .OrderBy(e => e.Difficulty)
            .ToListAsync();

        var completedExerciseIds = new HashSet<int>();
        var user = await _userManager.GetUserAsync(User);
        if (user is not null && await _userManager.IsInRoleAsync(user, "Student"))
        {
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (profile is not null)
            {
                var completedIds = await _context.Submissions
                    .Where(s => s.StudentId == profile.Id && s.ExerciseId.HasValue)
                    .Join(
                        _context.Exercises.Where(e => e.TopicId == id && e.Status == "Active"),
                        s => s.ExerciseId!.Value,
                        e => e.Id,
                        (s, e) => e.Id)
                    .Distinct()
                    .ToListAsync();
                completedExerciseIds = completedIds.ToHashSet();
            }
        }

        ViewBag.Exercises = exercises;
        ViewBag.CompletedExerciseIds = completedExerciseIds;
        return View(topic);
    }
}
