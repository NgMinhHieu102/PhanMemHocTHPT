using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class Topic
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int Grade { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
