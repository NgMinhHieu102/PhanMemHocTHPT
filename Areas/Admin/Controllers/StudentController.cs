using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.ViewModels.Admin;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class StudentController : Controller
{
    private readonly IStudentService _studentService;
    private readonly DataContext _context;

    public StudentController(IStudentService studentService, DataContext context)
    {
        _studentService = studentService;
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int? grade, int? classId, string? status, int page = 1)
    {
        var data = await _studentService.GetPagedAsync(search, grade, classId, status, page, 10);
        ViewBag.Classes = await _context.Classes.OrderBy(x => x.Name).ToListAsync();
        return View(data);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var student = await _studentService.GetByIdAsync(id);
        if (student == null) return NotFound();
        return View(student);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new StudentCreateViewModel();
        await PopulateOptions(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentCreateViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await PopulateOptions(vm);
                return View(vm);
            }

            var createResult = await _studentService.CreateAsync(vm);
            if (!createResult.Success)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                await PopulateOptions(vm);
                return View(vm);
            }

            TempData["Success"] = "T\u1EA1o h\u1ECDc sinh th\u00E0nh c\u00F4ng";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            TempData["Error"] = "C\u00F3 l\u1ED7i khi t\u1EA1o h\u1ECDc sinh.";
            await PopulateOptions(vm);
            return View(vm);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var student = await _studentService.GetByIdAsync(id);
        if (student == null) return NotFound();

        var vm = new StudentEditViewModel
        {
            Id = student.Id,
            UserId = student.UserId,
            StudentCode = student.StudentCode,
            FullName = student.User.FullName,
            Email = student.User.Email ?? string.Empty,
            Username = student.User.UserName ?? string.Empty,
            Phone = student.Phone,
            Gender = student.Gender,
            Grade = student.Grade,
            ClassId = student.ClassId,
            Status = student.User.Status
        };

        await PopulateOptions(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StudentEditViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await PopulateOptions(vm);
                return View(vm);
            }

            var updateResult = await _studentService.UpdateAsync(vm);
            if (!updateResult.Success)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                await PopulateOptions(vm);
                return View(vm);
            }

            TempData["Success"] = "C\u1EADp nh\u1EADt h\u1ECDc sinh th\u00E0nh c\u00F4ng";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            TempData["Error"] = "C\u00F3 l\u1ED7i khi c\u1EADp nh\u1EADt h\u1ECDc sinh.";
            await PopulateOptions(vm);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _studentService.DeleteAsync(id);
            TempData["Success"] = "\u0110\u00E3 x\u00F3a h\u1ECDc sinh";
        }
        catch
        {
            TempData["Error"] = "X\u00F3a h\u1ECDc sinh th\u1EA5t b\u1EA1i.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptions(StudentCreateViewModel vm)
    {
        vm.GradeOptions =
        [
            new SelectListItem("Khối 10", "10"),
            new SelectListItem("Khối 11", "11"),
            new SelectListItem("Khối 12", "12")
        ];

        vm.ClassOptions = await _context.Classes
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();
    }

    private async Task PopulateOptions(StudentEditViewModel vm)
    {
        vm.GradeOptions =
        [
            new SelectListItem("Khối 10", "10"),
            new SelectListItem("Khối 11", "11"),
            new SelectListItem("Khối 12", "12")
        ];

        vm.ClassOptions = await _context.Classes
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();
    }
}
