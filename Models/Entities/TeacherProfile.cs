using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class TeacherProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public string? Subject { get; set; }
    public string? Phone { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Class> Classes { get; set; } = new List<Class>();
}
