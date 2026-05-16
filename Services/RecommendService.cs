using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Data;
using TinHocTHPT.Models.ViewModels.Student;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Services;

public class RecommendService : IRecommendService
{
    private const string UrgentReview = "__urgent_review__";
    private const string UrgentFit = "__fit__";
    private const string UrgentAdvance = "__advance__";

    private readonly DataContext _context;

    public RecommendService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<RecommendedExercise>> GetRecommendationsAsync(int studentId, int grade)
    {
        var since = DateTime.UtcNow.AddDays(-30);
        var submissions = await _context.Submissions
            .Where(s => s.StudentId == studentId && s.SubmittedAt >= since)
            .Include(s => s.Exercise)
            .ToListAsync();

        var doneExerciseIds = submissions
            .Where(s => s.ExerciseId.HasValue)
            .Select(s => s.ExerciseId!.Value)
            .Distinct()
            .ToHashSet();

        var topicAvg = submissions
            .Where(s => s.ExerciseId.HasValue && s.Exercise != null)
            .GroupBy(s => s.Exercise!.TopicId)
            .ToDictionary(g => g.Key, g => g.Average(s => (double)s.Score));

        var studentProfile = await _context.StudentProfiles.FindAsync(studentId);
        var studentRating = studentProfile?.StudentRating ?? 1500m;

        var available = await _context.Exercises
            .Where(e => e.Grade == grade && e.Status == "Active" && !doneExerciseIds.Contains(e.Id))
            .Include(e => e.Topic)
            .ToListAsync();

        var scored = available.Select(e =>
        {
            topicAvg.TryGetValue(e.TopicId, out var avg);
            if (avg == 0)
            {
                avg = 5.0;
            }

            var diffScore = e.Difficulty switch
            {
                "Easy" => avg < 5 ? 3 : avg < 7 ? 1 : 0,
                "Medium" => avg >= 5 && avg < 7 ? 3 : avg < 5 ? 1 : 2,
                "Hard" => avg >= 7 ? 3 : 0,
                _ => 1
            };

            var urgencyCode = avg < 5 ? UrgentReview : avg < 7 ? UrgentFit : UrgentAdvance;
            var matchGap = Math.Abs(e.Rating - studentRating);
            var ratingMatchScore = Math.Max(0m, 600m - matchGap) / 60m;
            var totalScore = diffScore * 10m + ratingMatchScore;
            return new { Exercise = e, Score = totalScore, AvgScore = avg, UrgencyCode = urgencyCode, MatchGap = matchGap };
        })
        .OrderByDescending(x => x.Score)
        .ThenBy(x => x.MatchGap)
        .ThenBy(x => x.AvgScore)
        .Take(3)
        .ToList();

        return scored.Select(x => new RecommendedExercise
        {
            ExerciseId = x.Exercise.Id,
            ExerciseTitle = x.Exercise.Title,
            TopicName = x.Exercise.Topic?.Name ?? string.Empty,
            Difficulty = x.Exercise.Difficulty,
            DifficultyDisplay = x.Exercise.Difficulty switch
            {
                "Easy" => "\u0111\u1EC5",
                "Medium" => "Trung b\u00ECnh",
                "Hard" => "Kh\u00F3",
                _ => x.Exercise.Difficulty
            },
            DifficultyClass = x.Exercise.Difficulty switch
            {
                "Easy" => "bg-success",
                "Medium" => "bg-warning",
                "Hard" => "bg-danger",
                _ => "bg-secondary"
            },
            UrgencyLevel = x.UrgencyCode switch
            {
                UrgentReview => "C\u1EA7n \u00F4n g\u1EA5p",
                UrgentFit => "Ph\u00F9 h\u1EE3p",
                _ => "N\u00E2ng cao"
            },
            UrgencyClass = x.UrgencyCode switch
            {
                UrgentReview => "border-danger text-danger",
                UrgentFit => "border-warning text-warning",
                _ => "border-primary text-primary"
            },
            FitLabel = x.MatchGap switch
            {
                < 150 => "Rất phù hợp",
                < 300 => "Phù hợp",
                < 450 => "Gần mức phù hợp",
                _ => "Cần ôn thêm"
            },
            FitClass = x.MatchGap switch
            {
                < 150 => "text-success",
                < 300 => "text-info",
                < 450 => "text-warning",
                _ => "text-muted"
            }
        }).ToList();
    }
}
