using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Admin;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class CodingProblemController : Controller
{
    private readonly DataContext _context;

    public CodingProblemController(DataContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int? grade, string? status)
    {
        var query = _context.CodingProblems
            .Include(p => p.Topic)
            .Include(p => p.TestCases)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(p => p.Title.Contains(keyword) || p.Topic.Name.Contains(keyword));
        }

        if (grade.HasValue)
        {
            query = query.Where(p => p.Grade == grade.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        ViewBag.Search = search;
        ViewBag.Grade = grade;
        ViewBag.Status = status;

        var items = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new CodingProblemCreateEditViewModel();
        await PopulateOptions(vm);
        return View("CreateEdit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CodingProblemCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptions(vm);
            return View("CreateEdit", vm);
        }

        var problem = new CodingProblem
        {
            Title = vm.Title.Trim(),
            Description = vm.Description.Trim(),
            TopicId = vm.TopicId,
            Grade = vm.Grade,
            Difficulty = vm.Difficulty,
            SampleInput = vm.SampleInput ?? string.Empty,
            SampleOutput = vm.SampleOutput ?? string.Empty,
            TimeLimitMs = vm.TimeLimitMs,
            MemoryLimitKb = vm.MemoryLimitKb,
            Status = vm.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.CodingProblems.Add(problem);
        await _context.SaveChangesAsync();
        await SaveTestsAsync(problem.Id, vm);
        TempData["Success"] = "Thêm bài luyện code thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var problem = await _context.CodingProblems
            .Include(p => p.TestCases)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (problem is null) return NotFound();

        var vm = new CodingProblemCreateEditViewModel
        {
            Id = problem.Id,
            Title = problem.Title,
            Description = problem.Description,
            TopicId = problem.TopicId,
            Grade = problem.Grade,
            Difficulty = problem.Difficulty,
            SampleInput = problem.SampleInput,
            SampleOutput = problem.SampleOutput,
            TimeLimitMs = problem.TimeLimitMs,
            MemoryLimitKb = problem.MemoryLimitKb,
            Status = problem.Status,
            TestCases = problem.TestCases
                .OrderBy(t => t.OrderIndex)
                .Select(t => new CodingTestCaseInputViewModel
                {
                    Input = t.Input,
                    ExpectedOutput = t.ExpectedOutput,
                    IsSample = t.IsSample,
                    OrderIndex = t.OrderIndex
                }).ToList()
        };

        if (!vm.TestCases.Any())
        {
            vm.TestCases =
            [
                new() { Input = problem.SampleInput, ExpectedOutput = problem.SampleOutput, IsSample = true, OrderIndex = 1 }
            ];
        }

        await PopulateOptions(vm);
        return View("CreateEdit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CodingProblemCreateEditViewModel vm)
    {
        if (!ModelState.IsValid || vm.Id is null)
        {
            await PopulateOptions(vm);
            return View("CreateEdit", vm);
        }

        var problem = await _context.CodingProblems
            .Include(p => p.TestCases)
            .FirstOrDefaultAsync(p => p.Id == vm.Id.Value);
        if (problem is null) return NotFound();

        problem.Title = vm.Title.Trim();
        problem.Description = vm.Description.Trim();
        problem.TopicId = vm.TopicId;
        problem.Grade = vm.Grade;
        problem.Difficulty = vm.Difficulty;
        problem.SampleInput = vm.SampleInput ?? string.Empty;
        problem.SampleOutput = vm.SampleOutput ?? string.Empty;
        problem.TimeLimitMs = vm.TimeLimitMs;
        problem.MemoryLimitKb = vm.MemoryLimitKb;
        problem.Status = vm.Status;

        _context.TestCases.RemoveRange(problem.TestCases);
        await _context.SaveChangesAsync();
        await SaveTestsAsync(problem.Id, vm);
        TempData["Success"] = "Cập nhật bài luyện code thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var problem = await _context.CodingProblems
            .Include(p => p.TestCases)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (problem is null)
        {
            TempData["Error"] = "Không tìm thấy bài luyện code để xóa.";
            return RedirectToAction(nameof(Index));
        }

        _context.TestCases.RemoveRange(problem.TestCases);
        _context.CodingProblems.Remove(problem);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã xóa bài luyện code.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptions(CodingProblemCreateEditViewModel vm)
    {
        vm.TopicOptions = await _context.Topics
            .OrderBy(t => t.Grade)
            .ThenBy(t => t.Name)
            .Select(t => new SelectListItem($"{t.Name} (Khối {t.Grade})", t.Id.ToString()))
            .ToListAsync();
    }

    private async Task SaveTestsAsync(int problemId, CodingProblemCreateEditViewModel vm)
    {
        var tests = vm.TestCases
            .Where(t => !string.IsNullOrWhiteSpace(t.Input) || !string.IsNullOrWhiteSpace(t.ExpectedOutput))
            .OrderBy(t => t.OrderIndex)
            .Select((t, idx) => new TestCase
            {
                CodingProblemId = problemId,
                Input = t.Input ?? string.Empty,
                ExpectedOutput = t.ExpectedOutput ?? string.Empty,
                IsSample = t.IsSample,
                OrderIndex = idx + 1
            })
            .ToList();

        if (!tests.Any())
        {
            tests.Add(new TestCase
            {
                CodingProblemId = problemId,
                Input = vm.SampleInput ?? string.Empty,
                ExpectedOutput = vm.SampleOutput ?? string.Empty,
                IsSample = true,
                OrderIndex = 1
            });
        }

        var firstSample = tests.FirstOrDefault(t => t.IsSample) ?? tests.First();
        var problem = await _context.CodingProblems.FirstAsync(p => p.Id == problemId);
        problem.SampleInput = firstSample.Input;
        problem.SampleOutput = firstSample.ExpectedOutput;

        _context.TestCases.AddRange(tests);
        await _context.SaveChangesAsync();
    }
}
