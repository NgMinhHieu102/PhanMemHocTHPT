using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class Class
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Name { get; set; } = string.Empty;

    public int Grade { get; set; }
    public int? TeacherId { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TeacherProfile? Teacher { get; set; }
    public ICollection<StudentProfile> Students { get; set; } = new List<StudentProfile>();
}
