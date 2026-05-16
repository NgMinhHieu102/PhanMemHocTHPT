using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class StudentProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string StudentCode { get; set; } = string.Empty;

    public int? ClassId { get; set; }

    public int Grade { get; set; }
    public decimal StudentRating { get; set; } = 1500m;

    public string? Phone { get; set; }

    /// <summary>"Male" | "Female"</summary>
    public string? Gender { get; set; }

    public decimal TotalScore { get; set; }
    public int ExercisesDone { get; set; }
    public int TopicsStudied { get; set; }
    public int StreakDays { get; set; }
    public DateTime? LastActive { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Class? Class { get; set; }
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
