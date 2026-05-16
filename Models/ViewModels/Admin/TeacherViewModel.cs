using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TinHocTHPT.Models.ViewModels.Admin;

public class TeacherViewModel
{
    public int? Id { get; set; }
    public string? UserId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = "Active";
}
