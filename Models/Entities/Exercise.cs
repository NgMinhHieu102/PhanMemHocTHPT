using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class Exercise
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int TopicId { get; set; }
    public int Grade { get; set; }
    public string? Orientation { get; set; }

    /// <summary>"Easy" | "Medium" | "Hard"</summary>
    public string Difficulty { get; set; } = "Medium";

    public string Status { get; set; } = "Active";
    public int TimeLimit { get; set; } = 1800;
    public decimal Rating { get; set; } = 1500m;
    public string? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Topic Topic { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
