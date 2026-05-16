using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.ViewModels.Admin;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Services;

public class StatsService : IStatsService
{
    private readonly DataContext _context;

    public StatsService(DataContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardStatsAsync()
    {
        var vm = new DashboardViewModel
        {
            TotalStudents = await _context.StudentProfiles.CountAsync(),
            TotalTeachers = await _context.TeacherProfiles.CountAsync(),
            TotalClasses = await _context.Classes.CountAsync(),
            TotalTopics = await _context.Topics.CountAsync(),
            TotalExercises = await _context.Exercises.CountAsync(),
            TotalQuestions = await _context.Questions.CountAsync(),
            TotalSubmissions = await _context.Submissions.CountAsync(),
            TotalAttempts = await _context.Submissions.CountAsync()
        };

        vm.CompletionRate = vm.TotalStudents > 0
            ? (int)Math.Round((double)vm.TotalSubmissions / vm.TotalStudents * 100)
            : 0;

        var from = DateTime.UtcNow.Date.AddDays(-6);
        var grouped = await _context.Submissions
            .Where(s => s.SubmittedAt >= from)
            .GroupBy(s => s.SubmittedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        for (var i = 0; i < 7; i++)
        {
            var day = from.AddDays(i);
            vm.ChartLabels.Add(day.ToString("dd/MM"));
            vm.SubmissionsPerDay.Add(grouped.FirstOrDefault(x => x.Date == day)?.Count ?? 0);
        }

        return vm;
    }

    public async Task<StudentStatsViewModel> GetStudentStatsAsync(DateTime? from, DateTime? to, int? classId, int? grade)
    {
        var query = _context.StudentProfiles
            .AsNoTracking()
            .Include(s => s.User)
            .AsQueryable();

        if (classId.HasValue)
        {
            query = query.Where(s => s.ClassId == classId);
        }

        if (grade.HasValue)
        {
            query = query.Where(s => s.Grade == grade);
        }

        var data = await query.ToListAsync();
        var result = new StudentStatsViewModel
        {
            TotalStudents = data.Count,
            ActiveStudents = data.Count(x => x.User.Status == "Active"),
            AverageScore = data.Count == 0 ? 0 : Math.Round(data.Average(x => x.TotalScore), 2)
        };

        result.Labels = data.OrderByDescending(x => x.TotalScore).Take(10).Select(x => x.User.FullName).ToList();
        result.Scores = data.OrderByDescending(x => x.TotalScore).Take(10).Select(x => x.TotalScore).ToList();

        return result;
    }

    public async Task<ExerciseStatsViewModel> GetExerciseStatsAsync(DateTime? from, DateTime? to, int? topicId)
    {
        var query = _context.Submissions
            .AsNoTracking()
            .Include(s => s.Exercise)
            .Where(s => s.ExerciseId.HasValue)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(s => s.SubmittedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(s => s.SubmittedAt <= to.Value);
        }

        if (topicId.HasValue)
        {
            query = query.Where(s => s.Exercise != null && s.Exercise.TopicId == topicId);
        }

        var list = await query.ToListAsync();
        var vm = new ExerciseStatsViewModel
        {
            TotalAttempts = list.Count,
            AverageScore = list.Count == 0 ? 0 : Math.Round(list.Average(x => x.Score), 2),
            TotalExercises = await _context.Exercises.CountAsync(e => !topicId.HasValue || e.TopicId == topicId)
        };

        var grouped = list
            .GroupBy(s => s.Exercise?.Title ?? "Không xác định")
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToList();

        vm.Labels = grouped.Select(g => g.Key).ToList();
        vm.Attempts = grouped.Select(g => g.Count()).ToList();
        return vm;
    }

    public async Task<int> GetRankInClassAsync(int studentId, int classId)
    {
        var students = await _context.StudentProfiles
            .Where(s => s.ClassId == classId)
            .Select(s => new { s.Id, s.TotalScore })
            .ToListAsync();

        var ids = students
            .OrderByDescending(s => s.TotalScore)
            .Select(s => s.Id)
            .ToList();

        var index = ids.FindIndex(id => id == studentId);
        return index >= 0 ? index + 1 : 0;
    }

    public async Task<int> GetRankInGradeAsync(int studentId, int grade)
    {
        var students = await _context.StudentProfiles
            .Where(s => s.Grade == grade)
            .Select(s => new { s.Id, s.TotalScore })
            .ToListAsync();

        var ids = students
            .OrderByDescending(s => s.TotalScore)
            .Select(s => s.Id)
            .ToList();

        var index = ids.FindIndex(id => id == studentId);
        return index >= 0 ? index + 1 : 0;
    }
}
