namespace TinHocTHPT.Helpers;

public static class AiFeedbackHelper
{
    public static string Generate(decimal score, List<string> wrongTopics)
    {
        var topicsText = wrongTopics.Any()
            ? $" B\u1EA1n c\u1EA7n \u00F4n th\u00EAm: {string.Join(", ", wrongTopics)}."
            : string.Empty;

        return score switch
        {
            >= 9.0m =>
                "Xuất sắc! Bạn nắm vững kiến thức rất tốt. Hãy thử thêm bài khó hơn để duy trì đà tiến bộ.",
            >= 7.0m =>
                $"Rất tốt!{topicsText} Tiếp tục luyện tập để đạt điểm cao hơn.",
            >= 5.0m =>
                $"Đạt mức trung bình.{topicsText} Hãy xem lại lý thuyết và làm thêm bài tập cùng chủ đề.",
            _ =>
                $"Cần cố gắng hơn. Hãy bắt đầu lại từ bài cơ bản và làm từng câu chậm, chắc.{topicsText}"
        };
    }

    public static string GetGrade(decimal score) => score switch
    {
        >= 9.0m => "Xu\u1EA5t s\u1EAFc",
        >= 7.0m => "T\u1ED1t",
        >= 5.0m => "Trung b\u00ECnh",
        _ => "C\u1EA7n c\u1ED1 g\u1EAFng"
    };

    public static string GetGradeEmoji(decimal score) => score switch
    {
        >= 9.0m => "\u2B50",
        >= 7.0m => "\u2713",
        >= 5.0m => "\u26AA",
        _ => "\u203C"
    };

    public static string GetScoreCssClass(decimal score) => score switch
    {
        >= 7.0m => "score-high",
        >= 5.0m => "score-mid",
        _ => "score-low"
    };
}
