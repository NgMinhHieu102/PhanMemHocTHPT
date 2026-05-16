using Microsoft.AspNetCore.Mvc.Rendering;

namespace TinHocTHPT.Models.ViewModels.Admin;

public class StudentListItem
{
    public int Id { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Grade { get; set; }
    public string ClassName { get; set; } = "Chưa xếp lớp";
    public string Status { get; set; } = "Active";
    public decimal TotalScore { get; set; }
    public int ExercisesDone { get; set; }
    public DateTime? LastActive { get; set; }
}
