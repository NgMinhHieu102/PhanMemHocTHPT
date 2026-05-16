using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Services;

public class QuestionLibraryService : IQuestionLibraryService
{
    private readonly DataContext _context;

    public QuestionLibraryService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<Question>> GetRandomQuestionsAsync(int count, int? topicId = null, string? difficulty = null)
    {
        var query = _context.Questions
            .Include(q => q.Answers)
            .Include(q => q.Topic)
            .Where(q => q.ExamId == null); // Chỉ lấy câu hỏi chưa được dùng trong đề thi

        if (topicId.HasValue && topicId.Value > 0)
        {
            query = query.Where(q => q.TopicId == topicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            query = query.Where(q => q.Difficulty == difficulty);
        }

        var questions = await query.ToListAsync();

        // Randomize và lấy số lượng cần thiết
        var random = new Random();
        return questions.OrderBy(_ => random.Next()).Take(count).ToList();
    }

    public async Task<(Exam exam, int questionCount)> CreateExamFromLibraryAsync(
        string title, int grade, int totalQuestions, int? topicId = null, string? difficulty = null)
    {
        var questions = await GetRandomQuestionsAsync(totalQuestions, topicId, difficulty);

        if (questions.Count == 0)
        {
            throw new InvalidOperationException("Không tìm thấy câu hỏi phù hợp trong thư viện.");
        }

        var exam = new Exam
        {
            Title = title,
            Grade = grade,
            Duration = Math.Max(questions.Count * 3, 15), // 3 phút mỗi câu, tối thiểu 15 phút
            TotalQuestions = questions.Count,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        // Thêm câu hỏi vào đề thi
        for (int i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            question.ExamId = exam.Id;

            _context.ExamQuestions.Add(new ExamQuestion
            {
                ExamId = exam.Id,
                QuestionId = question.Id,
                OrderIndex = i + 1
            });
        }

        await _context.SaveChangesAsync();

        return (exam, questions.Count);
    }
}