using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Admin;
using TinHocTHPT.Services.Interfaces;
using X.PagedList;

namespace TinHocTHPT.Services;

public class StudentService : IStudentService
{
    private readonly DataContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IExcelService _excelService;

    public StudentService(DataContext context, UserManager<ApplicationUser> userManager, IExcelService excelService)
    {
        _context = context;
        _userManager = userManager;
        _excelService = excelService;
    }

    public async Task<IPagedList<StudentListItem>> GetPagedAsync(string? search, int? grade, int? classId, string? status, int page, int pageSize)
    {
        var query = _context.StudentProfiles
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Class)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(s =>
                s.StudentCode.Contains(keyword) ||
                s.User.FullName.Contains(keyword) ||
                s.User.UserName!.Contains(keyword) ||
                s.User.Email!.Contains(keyword));
        }

        if (grade.HasValue)
        {
            query = query.Where(s => s.Grade == grade.Value);
        }

        if (classId.HasValue)
        {
            query = query.Where(s => s.ClassId == classId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.User.Status == status);
        }

        var projected = query
            .OrderByDescending(s => s.StudentCode)
            .Select(s => new StudentListItem
            {
                Id = s.Id,
                StudentCode = s.StudentCode,
                FullName = s.User.FullName,
                Username = s.User.UserName ?? string.Empty,
                Email = s.User.Email ?? string.Empty,
                Grade = s.Grade,
                ClassName = s.Class != null ? s.Class.Name : "Chưa xếp lớp",
                Status = s.User.Status,
                TotalScore = s.TotalScore,
                ExercisesDone = s.ExercisesDone,
                LastActive = s.LastActive
            });

        var total = await projected.CountAsync();
        var items = await projected.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new StaticPagedList<StudentListItem>(items, page, pageSize, total);
    }

    public Task<StudentProfile?> GetByIdAsync(int id)
    {
        return _context.StudentProfiles
            .Include(s => s.User)
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public Task<StudentProfile?> GetByUserIdAsync(string userId)
    {
        return _context.StudentProfiles
            .Include(s => s.User)
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<(bool Success, List<string> Errors)> CreateAsync(StudentCreateViewModel vm)
    {
        if (await _userManager.FindByNameAsync(vm.Username) is not null)
        {
            return (false, ["Tên đăng nhập đã tồn tại."]);
        }

        if (await _userManager.FindByEmailAsync(vm.Email) is not null)
        {
            return (false, ["Email đã được sử dụng."]);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Username,
            Email = vm.Email,
            FullName = vm.FullName,
            Role = "Student",
            Status = vm.Status,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description).ToList());
        }

        if (!await _userManager.IsInRoleAsync(user, "Student"))
        {
            await _userManager.AddToRoleAsync(user, "Student");
        }

        var nextId = await _context.StudentProfiles.CountAsync() + 1;
        _context.StudentProfiles.Add(new StudentProfile
        {
            UserId = user.Id,
            StudentCode = $"HS{nextId:D4}",
            ClassId = vm.ClassId,
            Grade = vm.Grade,
            Phone = vm.Phone,
            Gender = vm.Gender,
            LastActive = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, []);
    }

    public async Task<(bool Success, List<string> Errors)> UpdateAsync(StudentEditViewModel vm)
    {
        var student = await _context.StudentProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == vm.Id);

        if (student is null)
        {
            return (false, ["Không tìm thấy học sinh cần cập nhật."]);
        }

        var hasDuplicateUsername = await _context.Users
            .AnyAsync(u => u.Id != student.UserId && u.UserName == vm.Username);
        if (hasDuplicateUsername)
        {
            return (false, ["Tên đăng nhập đã tồn tại."]);
        }

        var hasDuplicateEmail = await _context.Users
            .AnyAsync(u => u.Id != student.UserId && u.Email == vm.Email);
        if (hasDuplicateEmail)
        {
            return (false, ["Email đã được sử dụng."]);
        }

        student.ClassId = vm.ClassId;
        student.Grade = vm.Grade;
        student.Phone = vm.Phone;
        student.Gender = vm.Gender;

        student.User.FullName = vm.FullName;
        student.User.Status = vm.Status;
        if (!string.Equals(student.User.UserName, vm.Username, StringComparison.OrdinalIgnoreCase))
        {
            var setName = await _userManager.SetUserNameAsync(student.User, vm.Username);
            if (!setName.Succeeded) return (false, setName.Errors.Select(e => e.Description).ToList());
        }
        if (!string.Equals(student.User.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
        {
            var setEmail = await _userManager.SetEmailAsync(student.User, vm.Email);
            if (!setEmail.Succeeded) return (false, setEmail.Errors.Select(e => e.Description).ToList());
        }

        var updateUser = await _userManager.UpdateAsync(student.User);
        if (!updateUser.Succeeded)
        {
            return (false, updateUser.Errors.Select(e => e.Description).ToList());
        }

        try
        {
            await _context.SaveChangesAsync();
            return (true, []);
        }
        catch (DbUpdateException)
        {
            return (false, ["Không thể cập nhật học sinh do dữ liệu bị trùng hoặc không hợp lệ."]);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.StudentProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student is null)
        {
            return false;
        }

        var result = await _userManager.DeleteAsync(student.User);
        return result.Succeeded;
    }

    public async Task<byte[]> ExportToExcelAsync(string? search, int? grade, int? classId)
    {
        var data = await GetPagedAsync(search, grade, classId, null, 1, int.MaxValue / 2);
        return await _excelService.ExportStudentsAsync(data);
    }

    public async Task UpdateStatsAsync(int studentId, decimal score, int topicId)
    {
        var student = await _context.StudentProfiles.FirstOrDefaultAsync(x => x.Id == studentId);
        if (student is null)
        {
            return;
        }

        student.TotalScore += score;
        student.ExercisesDone += 1;

        var topicsDone = await _context.Submissions
            .Where(s => s.StudentId == studentId && s.ExerciseId.HasValue)
            .Join(_context.Exercises, s => s.ExerciseId!.Value, e => e.Id, (s, e) => e.TopicId)
            .Distinct()
            .CountAsync();

        student.TopicsStudied = Math.Max(student.TopicsStudied, topicsDone);
        student.LastActive = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
