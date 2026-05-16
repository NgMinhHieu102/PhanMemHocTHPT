namespace TinHocTHPT.Models.ViewModels.Student;

public class TopicsViewModel
{
    /// <summary>Học sinh: khối đang học. Quản trị: chỉ là placeholder khi BrowseAllGrades.</summary>
    public int Grade { get; set; }

    /// <summary>Giáo viên / Admin xem chủ đề mọi khối và dùng bộ lọc theo khối.</summary>
    public bool BrowseAllGrades { get; set; }

    public List<TopicItemViewModel> Topics { get; set; } = new();
}

public class TopicItemViewModel
{
    public int Id { get; set; }

    /// <summary>Khối của chủ đề (để lọc nhanh trên trang).</summary>
    public int Grade { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int ExerciseCount { get; set; }
}
