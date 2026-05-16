namespace TinHocTHPT.Models.Entities;

public class ExamQuestion
{
    public int ExamId { get; set; }
    public int QuestionId { get; set; }
    public int OrderIndex { get; set; }

    public Exam Exam { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
