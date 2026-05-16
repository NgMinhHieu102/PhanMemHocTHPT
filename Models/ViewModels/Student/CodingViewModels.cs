namespace TinHocTHPT.Models.ViewModels.Student;

public class CodingIndexViewModel
{
    public int Grade { get; set; }
    public bool BrowseAllGrades { get; set; }
    public List<CodingProblemListItem> Problems { get; set; } = new();
}

public class CodingProblemListItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Grade { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
    public int TestCaseCount { get; set; }
    public bool IsAccepted { get; set; }
}

public class CodingProblemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
    public string TopicName { get; set; } = string.Empty;
    public int Grade { get; set; }
    public string SampleInput { get; set; } = string.Empty;
    public string SampleOutput { get; set; } = string.Empty;
    public int TimeLimitMs { get; set; }
    public int MemoryLimitKb { get; set; }
    public bool IsAccepted { get; set; }
    public List<SampleTestItem> SampleTests { get; set; } = new();
}

public class SampleTestItem
{
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
}

public class CodingSubmissionItem
{
    public int Id { get; set; }
    public string Verdict { get; set; } = string.Empty;
    public int PassedTests { get; set; }
    public int TotalTests { get; set; }
    public string Language { get; set; } = string.Empty;
    public int ExecutionTimeMs { get; set; }
    public int MemoryUsedKb { get; set; }
    public string? CompileOutput { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string SourceCode { get; set; } = string.Empty;
}
