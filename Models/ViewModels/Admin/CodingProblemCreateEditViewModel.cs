using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TinHocTHPT.Models.ViewModels.Admin;

public class CodingProblemCreateEditViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập nội dung đề bài.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn chủ đề.")]
    public int TopicId { get; set; }

    [Range(10, 12, ErrorMessage = "Khối phải từ 10 đến 12.")]
    public int Grade { get; set; } = 10;

    public string Difficulty { get; set; } = "Medium";
    public string SampleInput { get; set; } = string.Empty;
    public string SampleOutput { get; set; } = string.Empty;

    [Range(500, 10000)]
    public int TimeLimitMs { get; set; } = 2000;

    [Range(16384, 524288)]
    public int MemoryLimitKb { get; set; } = 131072;

    public string Status { get; set; } = "Active";

    public List<SelectListItem> TopicOptions { get; set; } = new();
    public List<CodingTestCaseInputViewModel> TestCases { get; set; } = new()
    {
        new() { IsSample = true, OrderIndex = 1 },
        new() { IsSample = false, OrderIndex = 2 },
        new() { IsSample = false, OrderIndex = 3 }
    };
}

public class CodingTestCaseInputViewModel
{
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public bool IsSample { get; set; }
    public int OrderIndex { get; set; }
}
