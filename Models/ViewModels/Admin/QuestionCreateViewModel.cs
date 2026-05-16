using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TinHocTHPT.Models.ViewModels.Admin;

public class QuestionCreateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn bài tập")]
    [Display(Name = "Bài tập")]
    public int ExerciseId { get; set; }

    [Display(Name = "Đề thi")]
    public int? ExamId { get; set; }

    [Display(Name = "Bài tập/Đề thi")]
    public string SelectedItem { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn chủ đề")]
    [Display(Name = "Chủ đề")]
    public int TopicId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn loại câu hỏi")]
    [Display(Name = "Loại câu hỏi")]
    public string Type { get; set; } = "MultipleChoice";

    [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
    [Display(Name = "Nội dung câu hỏi")]
    public string Content { get; set; } = string.Empty;

    [MaxLength(8000)]
    [Display(Name = "Giải thích (hiện sau khi nộp bài)")]
    [DataType(DataType.MultilineText)]
    public string? Explanation { get; set; }

    [Display(Name = "Mức độ")]
    public string Difficulty { get; set; } = "Medium";

    [Display(Name = "Ảnh minh họa")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImageUrl { get; set; }

    public List<AnswerInputModel> Answers { get; set; } = new()
    {
        new() { Label = "A" },
        new() { Label = "B" },
        new() { Label = "C" },
        new() { Label = "D" }
    };

    public List<AnswerInputModel> TrueFalseAnswers { get; set; } = new()
    {
        new() { Label = "TF", SubIndex = "a" },
        new() { Label = "TF", SubIndex = "b" },
        new() { Label = "TF", SubIndex = "c" },
        new() { Label = "TF", SubIndex = "d" }
    };

    public List<SelectListItem> TopicOptions { get; set; } = new();
    public List<SelectListItem> ExerciseOptions { get; set; } = new();
    public List<SelectListItem> ExamOptions { get; set; } = new();
    public List<SelectListItem> TypeOptions { get; set; } =
    [
        new("Trắc nghiệm nhiều đáp án", "MultipleChoice"),
        new("Câu hỏi đúng/sai", "MultipleTrue"),
        new("Tự luận", "Essay")
    ];

    public List<SelectListItem> DifficultyOptions { get; set; } =
    [
        new("Dễ", "Easy"),
        new("Trung bình", "Medium"),
        new("Khó", "Hard")
    ];
}

public class AnswerInputModel
{
    public string Label { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? SubIndex { get; set; }
}
