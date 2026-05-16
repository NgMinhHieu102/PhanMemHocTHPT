using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Admin;

namespace TinHocTHPT.Services.Interfaces;

public interface IExcelService
{
    Task<byte[]> ExportStudentsAsync(IEnumerable<StudentListItem> students);
    Task<byte[]> ExportSubmissionsAsync(IEnumerable<Submission> submissions);
}
