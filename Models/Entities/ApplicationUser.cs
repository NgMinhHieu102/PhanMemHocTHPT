using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TinHocTHPT.Models.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>"Admin" | "Student"</summary>
    public string Role { get; set; } = "Student";

    /// <summary>"Active" | "Inactive"</summary>
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StudentProfile? StudentProfile { get; set; }
    public TeacherProfile? TeacherProfile { get; set; }
}
