using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Helpers;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Student;
using TinHocTHPT.Services.Interfaces;
using X.PagedList;

namespace TinHocTHPT.Services;

public class SubmissionService : ISubmissionService
{
    private readonly DataContext _context;

    public SubmissionService(DataContext context)
    {
        _context = context;
    }

    public async Task<Submission> SubmitExerciseAsync(int studentId, int exerciseId, IEnumerable<QuestionAnswerDto> answers, int timeSpent)
    {
        var exercise = await _context.Exercises.FindAsync(exerciseId)
            ?? throw new InvalidOperationException("Exercise missing or inactive.");

        var questions = await _context.Questions
            .Where(q => q.ExerciseId == exerciseId)
            .Include(q => q.Answers)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();

        var topicMap = await _context.Topics.ToDictionaryAsync(t => t.Id, t => t.Name);

        var submission = new Submission
        {
            StudentId = studentId,
            ExerciseId = exerciseId,
            TimeSpent = timeSpent,
            SubmittedAt = DateTime.UtcNow,
            MaxScore = 10
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        var answerByQuestion = answers.ToDictionary(a => a.QuestionId);
        decimal earnedPoints = 0;
        decimal totalPoints = questions.Count;
        var topicWrong = new HashSet<string>();

        foreach (var question in questions)
        {
            answerByQuestion.TryGetValue(question.Id, out var submittedAnswer);

            if (question.QuestionType == QuestionTypes.MultipleTrue)
            {
                var subAnswers = question.Answers
                    .Where(a => !string.IsNullOrWhiteSpace(a.SubIndex))
                    .OrderBy(a => a.SubIndex)
                    .ToList();
                var correctItems = 0;

                foreach (var item in subAnswers)
                {
                    var selectedTrue = submittedAnswer?.TruefalseAnswers?.TryGetValue(item.SubIndex!, out var value) == true && value;
                    var itemCorrect = selectedTrue == item.IsCorrect;
                    if (itemCorrect)
                    {
                        correctItems++;
                    }

                    _context.SubmissionAnswers.Add(new SubmissionAnswer
                    {
                        SubmissionId = submission.Id,
                        QuestionId = question.Id,
                        SelectedAnswerId = item.Id,
                        IsCorrect = itemCorrect
                    });
                }

                earnedPoints += correctItems / 4m;
                if (correctItems < 4 && topicMap.TryGetValue(question.TopicId, out var topicName))
                {
                    topicWrong.Add(topicName);
                }
            }
            else if (question.QuestionType == QuestionTypes.Essay)
            {
                // Essay questions are not auto-graded
                _context.SubmissionAnswers.Add(new SubmissionAnswer
                {
                    SubmissionId = submission.Id,
                    QuestionId = question.Id,
                    EssayAnswer = submittedAnswer?.EssayAnswer,
                    IsCorrect = null // To be graded by teacher
                });
                // No points for essay, or add partial points if needed
            }
            else
            {
                var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == submittedAnswer?.SelectedAnswerId);
                var isCorrect = selectedAnswer?.IsCorrect == true;

                if (isCorrect)
                {
                    earnedPoints += 1;
                }
                else if (topicMap.TryGetValue(question.TopicId, out var topicName))
                {
                    topicWrong.Add(topicName);
                }

                _context.SubmissionAnswers.Add(new SubmissionAnswer
                {
                    SubmissionId = submission.Id,
                    QuestionId = question.Id,
                    SelectedAnswerId = selectedAnswer?.Id,
                    IsCorrect = isCorrect
                });
            }
        }

        submission.Score = totalPoints > 0 ? Math.Round(earnedPoints / totalPoints * 10, 1) : 0;
        submission.AiFeedback = AiFeedbackHelper.Generate(submission.Score, topicWrong.ToList());

        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == studentId);
        if (student is not null)
        {
            student.TotalScore += submission.Score;
            student.ExercisesDone += 1;
            student.LastActive = DateTime.UtcNow;

            const decimal defaultRating = 1500m;
            const decimal kFactor = 24m;

            var studentRating = student.StudentRating != 0 ? student.StudentRating : defaultRating;
            var exerciseRating = exercise.Rating != 0 ? exercise.Rating : defaultRating;
            var actualScore = submission.MaxScore > 0
                ? Math.Clamp(submission.Score / submission.MaxScore, 0m, 1m)
                : 0m;

            static decimal CalculateExpected(decimal ratingA, decimal ratingB)
            {
                return 1m / (1m + (decimal)Math.Pow(10, (double)((ratingB - ratingA) / 400m)));
            }

            var expectedScore = CalculateExpected(studentRating, exerciseRating);
            var ratingDelta = kFactor * (actualScore - expectedScore);

            student.StudentRating = Math.Clamp(studentRating + ratingDelta, 400m, 2600m);
            exercise.Rating = Math.Clamp(exerciseRating - ratingDelta, 400m, 2600m);
        }

        await _context.SaveChangesAsync();

        return submission;
    }

    public Task<Submission?> GetDetailAsync(int submissionId)
    {
        return _context.Submissions
            .Include(s => s.Student)
                .ThenInclude(p => p.User)
            .Include(s => s.Exercise)
            .Include(s => s.Exam)
            .Include(s => s.SubmissionAnswers)
                .ThenInclude(sa => sa.Question)
                    .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(s => s.Id == submissionId);
    }

    public async Task<IPagedList<Submission>> GetAdminPagedAsync(string? search, int? classId, int page, int pageSize)
    {
        var query = _context.Submissions
            .AsNoTracking()
            .Include(s => s.Student)
                .ThenInclude(p => p.User)
            .Include(s => s.Student)
                .ThenInclude(p => p.Class)
            .Include(s => s.Exercise)
            .Include(s => s.Exam)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(s =>
                s.Student.User.FullName.Contains(keyword) ||
                s.Student.StudentCode.Contains(keyword) ||
                (s.Exercise != null && s.Exercise.Title.Contains(keyword)) ||
                (s.Exam != null && s.Exam.Title.Contains(keyword)));
        }

        if (classId.HasValue)
        {
            query = query.Where(s => s.Student.ClassId == classId);
        }

        var ordered = query.OrderByDescending(s => s.SubmittedAt);
        var total = await ordered.CountAsync();
        var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new StaticPagedList<Submission>(items, page, pageSize, total);
    }
}
