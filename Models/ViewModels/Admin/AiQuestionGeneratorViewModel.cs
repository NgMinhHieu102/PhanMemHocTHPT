using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Models.ViewModels.Admin;

public class AiQuestionGeneratorViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn chủ đề.")]
    [Display(Name = "Chủ đề")]
    public int TopicId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung bài học.")]
    [Display(Name = "Nội dung bài học")]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại câu hỏi.")]
    [Display(Name = "Loại câu hỏi")]
    public string QuestionType { get; set; } = QuestionTypes.MultipleChoice;

    [Display(Name = "Số lượng câu hỏi")]
    [Range(1, 10, ErrorMessage = "Số lượng câu hỏi phải từ 1 đến 10.")]
    public int Count { get; set; } = 5;

    [Display(Name = "Mức độ")]
    public string Difficulty { get; set; } = "Medium";

    public List<SelectListItem> TopicOptions { get; set; } = new();
    public List<SelectListItem> QuestionTypeOptions { get; set; } = new()
    {
        new SelectListItem("Trắc nghiệm", QuestionTypes.MultipleChoice),
        new SelectListItem("Đúng/Sai", QuestionTypes.MultipleTrue),
        new SelectListItem("Tự luận", QuestionTypes.Essay)
    };
    public List<SelectListItem> DifficultyOptions { get; set; } = new()
    {
        new SelectListItem("Dễ", "Easy"),
        new SelectListItem("Trung bình", "Medium"),
        new SelectListItem("Khó", "Hard")
    };

    public bool IsSuccess { get; set; }
    public string? StatusMessage { get; set; }
}

public class ExamLibraryGeneratorViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề đề thi.")]
    [MaxLength(200)]
    [Display(Name = "Tiêu đề đề thi")]
    public string Title { get; set; } = "Đề thi từ thư viện câu hỏi";

    [Required(ErrorMessage = "Vui lòng chọn lớp.")]
    [Display(Name = "Khối")]
    public int Grade { get; set; } = 10;

    [Display(Name = "Chủ đề (tùy chọn)")]
    public int? TopicId { get; set; }

    [Display(Name = "Mức độ (tùy chọn)")]
    public string? Difficulty { get; set; }

    [Display(Name = "Số lượng câu hỏi")]
    [Range(1, 50, ErrorMessage = "Số lượng câu hỏi phải từ 1 đến 50.")]
    public int TotalQuestions { get; set; } = 10;

    public List<SelectListItem> TopicOptions { get; set; } = new();
    public List<SelectListItem> DifficultyOptions { get; set; } = new()
    {
        new SelectListItem("Tất cả", string.Empty),
        new SelectListItem("Dễ", "Easy"),
        new SelectListItem("Trung bình", "Medium"),
        new SelectListItem("Khó", "Hard")
    };

    public bool IsSuccess { get; set; }
    public string? StatusMessage { get; set; }
    public int AvailableQuestions { get; set; }
}
