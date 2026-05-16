using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Services.Interfaces;
using X.PagedList;

namespace TinHocTHPT.Services;

public class ExerciseService : IExerciseService
{
    private readonly DataContext _context;

    public ExerciseService(DataContext context)
    {
        _context = context;
    }

    public Task<List<Exercise>> GetByTopicAsync(int topicId)
    {
        return _context.Exercises
            .Where(e => e.TopicId == topicId && e.Status == "Active")
            .Include(e => e.Topic)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public Task<Exercise?> GetWithQuestionsAsync(int exerciseId)
    {
        return _context.Exercises
            .Include(e => e.Topic)
            .Include(e => e.Questions)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
    }

    public Task<List<Exam>> GetExamsAsync(int grade)
    {
        return _context.Exams
            .Where(e => e.Grade == grade && e.Status == "Active")
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public Task<Exam?> GetExamWithQuestionsAsync(int examId)
    {
        return _context.Exams
            .Include(e => e.ExamQuestions.OrderBy(eq => eq.OrderIndex))
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(e => e.Id == examId);
    }

    public async Task<IPagedList<Exercise>> GetAdminPagedAsync(int? topicId, string? difficulty, int? grade, string? search, int page, int pageSize)
    {
        var query = _context.Exercises
            .AsNoTracking()
            .Include(e => e.Topic)
            .AsQueryable();

        if (topicId.HasValue)
        {
            query = query.Where(e => e.TopicId == topicId);
        }

        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            query = query.Where(e => e.Difficulty == difficulty);
        }

        if (grade.HasValue)
        {
            query = query.Where(e => e.Grade == grade);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(e => e.Title.Contains(search));
        }

        var ordered = query.OrderByDescending(e => e.CreatedAt);
        var total = await ordered.CountAsync();
        var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new StaticPagedList<Exercise>(items, page, pageSize, total);
    }
}
