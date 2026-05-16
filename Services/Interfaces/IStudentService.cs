using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Admin;
using X.PagedList;

namespace TinHocTHPT.Services.Interfaces;

public interface IStudentService
{
    Task<IPagedList<StudentListItem>> GetPagedAsync(string? search, int? grade, int? classId, string? status, int page, int pageSize);
    Task<StudentProfile?> GetByIdAsync(int id);
    Task<StudentProfile?> GetByUserIdAsync(string userId);
    Task<(bool Success, List<string> Errors)> CreateAsync(StudentCreateViewModel vm);
    Task<(bool Success, List<string> Errors)> UpdateAsync(StudentEditViewModel vm);
    Task<bool> DeleteAsync(int id);
    Task<byte[]> ExportToExcelAsync(string? search, int? grade, int? classId);
    Task UpdateStatsAsync(int studentId, decimal score, int topicId);
}
