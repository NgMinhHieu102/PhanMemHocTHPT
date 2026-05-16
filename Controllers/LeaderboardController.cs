using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;

namespace TinHocTHPT.Controllers;

public class LeaderboardController : Controller
{
    private readonly DataContext _context;

    public LeaderboardController(DataContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var students = await _context.StudentProfiles
            .Include(s => s.User)
            .Include(s => s.Class)
            .ToListAsync();

        var top = students
            .OrderByDescending(s => s.TotalScore)
            .Take(50)
            .ToList();

        return View(top);
    }
}
