using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinHocTHPT.Models.ViewModels.Admin;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class DashboardController : Controller
{
    private readonly IStatsService _statsService;

    public DashboardController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    public async Task<IActionResult> Index()
    {
        DashboardViewModel vm = await _statsService.GetDashboardStatsAsync();
        return View(vm);
    }
}
