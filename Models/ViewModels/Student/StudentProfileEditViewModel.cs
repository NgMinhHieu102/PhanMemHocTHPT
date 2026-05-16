using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.ViewModels.Student;

public class StudentProfileEditViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Gender { get; set; }

    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }
}
