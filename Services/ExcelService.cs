using ClosedXML.Excel;
using TinHocTHPT.Models.Entities;
using TinHocTHPT.Models.ViewModels.Admin;
using TinHocTHPT.Services.Interfaces;

namespace TinHocTHPT.Services;

public class ExcelService : IExcelService
{
    public Task<byte[]> ExportStudentsAsync(IEnumerable<StudentListItem> students)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("HocSinh");

        ws.Cell(1, 1).Value = "Mã HS";
        ws.Cell(1, 2).Value = "Họ và tên";
        ws.Cell(1, 3).Value = "Email";
        ws.Cell(1, 4).Value = "Khối";
        ws.Cell(1, 5).Value = "Lớp";
        ws.Cell(1, 6).Value = "Điểm";
        ws.Cell(1, 7).Value = "Trạng thái";

        var row = 2;
        foreach (var item in students)
        {
            ws.Cell(row, 1).Value = item.StudentCode;
            ws.Cell(row, 2).Value = item.FullName;
            ws.Cell(row, 3).Value = item.Email;
            ws.Cell(row, 4).Value = item.Grade;
            ws.Cell(row, 5).Value = item.ClassName;
            ws.Cell(row, 6).Value = item.TotalScore;
            ws.Cell(row, 7).Value = item.Status == "Active" ? "Hoạt động" : "Không hoạt động";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportSubmissionsAsync(IEnumerable<Submission> submissions)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("BaiNop");

        ws.Cell(1, 1).Value = "Học sinh";
        ws.Cell(1, 2).Value = "Bài tập";
        ws.Cell(1, 3).Value = "Điểm";
        ws.Cell(1, 4).Value = "Thời gian làm (giây)";
        ws.Cell(1, 5).Value = "Thời điểm nộp";

        var row = 2;
        foreach (var item in submissions)
        {
            ws.Cell(row, 1).Value = item.Student.User.FullName;
            ws.Cell(row, 2).Value = item.Exercise?.Title ?? item.Exam?.Title ?? "-";
            ws.Cell(row, 3).Value = item.Score;
            ws.Cell(row, 4).Value = item.TimeSpent;
            ws.Cell(row, 5).Value = item.SubmittedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
