using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class StatsController : Controller
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    public async Task<IActionResult> Students(DateTime? from, DateTime? to, int? classId, int? grade)
    {
        var vm = await _statsService.GetStudentStatsAsync(from, to, classId, grade);
        return View(vm);
    }

    public async Task<IActionResult> Exercises(DateTime? from, DateTime? to, int? topicId)
    {
        var vm = await _statsService.GetExerciseStatsAsync(from, to, topicId);
        return View(vm);
    }
}
