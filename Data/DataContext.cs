using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Data;

public class DataContext : IdentityDbContext<ApplicationUser>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<StudentProfile> StudentProfiles { get; set; } = null!;
    public DbSet<TeacherProfile> TeacherProfiles { get; set; } = null!;
    public DbSet<Class> Classes { get; set; } = null!;
    public DbSet<Topic> Topics { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Answer> Answers { get; set; } = null!;
    public DbSet<Exam> Exams { get; set; } = null!;
    public DbSet<ExamQuestion> ExamQuestions { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;
    public DbSet<SubmissionAnswer> SubmissionAnswers { get; set; } = null!;
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
    public DbSet<CodingProblem> CodingProblems { get; set; } = null!;
    public DbSet<TestCase> TestCases { get; set; } = null!;
    public DbSet<CodeSubmission> CodeSubmissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ExamQuestion>()
            .HasKey(eq => new { eq.ExamId, eq.QuestionId });

        builder.Entity<StudentProfile>()
            .HasOne(s => s.User)
            .WithOne(u => u.StudentProfile)
            .HasForeignKey<StudentProfile>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TeacherProfile>()
            .HasOne(t => t.User)
            .WithOne(u => u.TeacherProfile)
            .HasForeignKey<TeacherProfile>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Class>()
            .HasOne(c => c.Teacher)
            .WithMany(t => t.Classes)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Question>()
            .HasOne(q => q.Topic)
            .WithMany(t => t.Questions)
            .HasForeignKey(q => q.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Question>()
            .HasOne(q => q.Exam)
            .WithMany(e => e.Questions)
            .HasForeignKey(q => q.ExamId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Exercise>()
            .Property(e => e.Orientation)
            .HasMaxLength(10);
        builder.Entity<Exercise>()
            .Property(e => e.Rating)
            .HasColumnType("decimal(8,2)")
            .HasDefaultValue(1500m);
        builder.Entity<StudentProfile>()
            .Property(s => s.StudentRating)
            .HasColumnType("decimal(8,2)")
            .HasDefaultValue(1500m);
        builder.Entity<Question>()
            .Property(q => q.QuestionType)
            .HasMaxLength(20)
            .HasDefaultValue(QuestionTypes.MultipleChoice);
        builder.Entity<Question>()
            .Property(q => q.Explanation)
            .HasMaxLength(8000);
        builder.Entity<Answer>()
            .Property(a => a.SubIndex)
            .HasMaxLength(1);

        builder.Entity<Submission>()
            .Property(s => s.Score)
            .HasColumnType("decimal(5,2)");
        builder.Entity<Submission>()
            .Property(s => s.MaxScore)
            .HasColumnType("decimal(5,2)");
        builder.Entity<StudentProfile>()
            .Property(s => s.TotalScore)
            .HasColumnType("decimal(10,2)");

        // ── Online Judge ──
        builder.Entity<CodingProblem>()
            .HasOne(p => p.Topic)
            .WithMany()
            .HasForeignKey(p => p.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TestCase>()
            .HasOne(tc => tc.Problem)
            .WithMany(p => p.TestCases)
            .HasForeignKey(tc => tc.CodingProblemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CodeSubmission>()
            .HasOne(cs => cs.Problem)
            .WithMany(p => p.Submissions)
            .HasForeignKey(cs => cs.CodingProblemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CodeSubmission>()
            .HasOne(cs => cs.Student)
            .WithMany()
            .HasForeignKey(cs => cs.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CodeSubmission>()
            .HasIndex(cs => new { cs.StudentId, cs.CodingProblemId });

        builder.Entity<CodingProblem>()
            .HasIndex(p => new { p.Grade, p.Status });

        builder.Entity<Class>()
            .HasIndex(c => c.Name)
            .IsUnique();

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "role-admin-001",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "1"
            },
            new IdentityRole
            {
                Id = "role-student-001",
                Name = "Student",
                NormalizedName = "STUDENT",
                ConcurrencyStamp = "2"
            });

        const string adminId = "admin-user-id-00001";
        var admin = new ApplicationUser
        {
            Id = adminId,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@tinhoc.edu.vn",
            NormalizedEmail = "ADMIN@TINHOC.EDU.VN",
            FullName = "Quản trị viên",
            Role = "Admin",
            Status = "Active",
            EmailConfirmed = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            SecurityStamp = "static-security-stamp-admin-001",
            ConcurrencyStamp = "static-concurrency-stamp-admin-001"
        };
        admin.PasswordHash = "AQAAAAIAAYagAAAAEL2UUunglclDaj2tIasgAvdh0fCL8VaSFU2MTrZeav+LIxvXCgdU91ow50sbQYQyPA==";
        builder.Entity<ApplicationUser>().HasData(admin);

        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                UserId = adminId,
                RoleId = "role-admin-001"
            });

        builder.Entity<SystemSetting>().HasData(new SystemSetting
        {
            Id = 1,
            SystemName = "Hệ thống Tin học THPT",
            Description = "Hệ thống gợi ý bài tập và luyện thi Tin học THPT",
            MaxAttempts = 20,
            EnableLogging = true,
            Language = "vi",
            UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        builder.Entity<Topic>().HasData(
            new Topic { Id = 1, Name = "Python cơ bản", Grade = 10, Description = "Giới thiệu Python, cú pháp cơ bản", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 2, Name = "Biến và kiểu dữ liệu", Grade = 10, Description = "int, float, str, bool, type()", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 3, Name = "Cấu lệnh điều kiện", Grade = 10, Description = "if, elif, else, toán tử logic", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 4, Name = "Vòng lặp", Grade = 10, Description = "for, while, range, break, continue", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 5, Name = "Mảng một chiều", Grade = 10, Description = "List, append, sort, slice", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 6, Name = "Hàm trong Python", Grade = 10, Description = "def, return, scope, default params", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 7, Name = "Xâu ký tự (String)", Grade = 10, Description = "Xử lý chuỗi, len, split, join", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 8, Name = "Hệ điều hành", Grade = 11, Description = "OS, Plug&Play, phần mềm nguồn mở", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 9, Name = "Cơ sở dữ liệu", Grade = 11, Description = "CSDL quan hệ, bảng, khóa chính", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 10, Name = "SQL cơ bản", Grade = 11, Description = "SELECT, WHERE, ORDER BY, GROUP BY", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 11, Name = "Mảng 2 chiều & Thuật toán", Grade = 11, Description = "Ma trận, tìm kiếm, sắp xếp", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 12, Name = "HTML cơ bản", Grade = 12, Description = "Thẻ HTML, thuộc tính, liên kết", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Topic { Id = 13, Name = "CSS cơ bản", Grade = 12, Description = "Selector, box model, flexbox", Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

        builder.Entity<Class>().HasData(
            new Class { Id = 1, Name = "10A1", Grade = 10, Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Class { Id = 2, Name = "10A2", Grade = 10, Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Class { Id = 3, Name = "11A1", Grade = 11, Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Class { Id = 4, Name = "11A2", Grade = 11, Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Class { Id = 5, Name = "12A1", Grade = 12, Status = "Active", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
    }
}
