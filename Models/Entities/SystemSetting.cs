using System.ComponentModel.DataAnnotations;

namespace TinHocTHPT.Models.Entities;

public class SystemSetting
{
    public int Id { get; set; }

    [Required]
    public string SystemName { get; set; } = "Hệ thống Tin học THPT";

    public string? Description { get; set; }
    public int MaxAttempts { get; set; } = 20;
    public bool EnableLogging { get; set; } = true;

    [Required]
    public string Language { get; set; } = "vi";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
