namespace TinHocTHPT.Models.Entities;

public class TestCase
{
    public int Id { get; set; }
    public int CodingProblemId { get; set; }

    /// <summary>Dữ liệu đầu vào gửi qua stdin.</summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>Đầu ra mong đợi (so sánh sau khi trim).</summary>
    public string ExpectedOutput { get; set; } = string.Empty;

    /// <summary>true = hiện cho học sinh xem (sample test).</summary>
    public bool IsSample { get; set; }

    public int OrderIndex { get; set; }

    public CodingProblem Problem { get; set; } = null!;
}
