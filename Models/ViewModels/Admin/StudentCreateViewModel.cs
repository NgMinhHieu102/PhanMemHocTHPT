using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TinHocTHPT.Models.ViewModels.Admin;

public class StudentCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(10, ErrorMessage = "Mật khẩu tối thiểu 10 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường, số và ký tự đặc biệt")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn khối lớp")]
    [Display(Name = "Khối lớp")]
    public int Grade { get; set; }

    [Display(Name = "Lớp học")]
    public int? ClassId { get; set; }

    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = "Active";

    public List<SelectListItem> GradeOptions { get; set; } = new();
    public List<SelectListItem> ClassOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new()
    {
        new("Hoạt động", "Active"),
        new("Không hoạt động", "Inactive")
    };
}
