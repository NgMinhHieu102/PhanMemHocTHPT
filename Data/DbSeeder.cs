using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TinHocTHPT.Helpers;
using TinHocTHPT.Models.Entities;

namespace TinHocTHPT.Data;

public static class DbSeeder
{
    private const string Practice = "Practice";
    private const string TopicExam = "TopicExam";
    private const string TrialExam = "TrialExam";

    private static readonly DateTime SeedDate = new(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc);

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<DataContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var env = sp.GetRequiredService<IHostEnvironment>();

        await db.Database.MigrateAsync();
        await EnsureRolesAsync(roleManager);
        if (env.IsDevelopment())
        {
            await SeedDemoUsersAsync(db, userManager);
        }

        await SeedCurriculumAsync(db);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "Teacher", "Student" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedCurriculumAsync(DataContext db)
    {
        await SeedTopicsAsync(db);
        await SeedKhoi10Async(db);
        await SeedKhoi11CsAsync(db);
        await SeedKhoi11IctAsync(db);
        await SeedKhoi12CsAsync(db);
        await SeedKhoi12IctAsync(db);
        await SeedTrialExamsAsync(db);
        await SeedCodingProblemsAsync(db);
    }

    private static async Task SeedDemoUsersAsync(DataContext db, UserManager<ApplicationUser> userManager)
    {
        if (!await db.Classes.AnyAsync())
        {
            await db.Classes.AddRangeAsync(
                new Class { Name = "10A1", Grade = 10, Status = "Active", CreatedAt = SeedDate },
                new Class { Name = "11A1", Grade = 11, Status = "Active", CreatedAt = SeedDate },
                new Class { Name = "12A1", Grade = 12, Status = "Active", CreatedAt = SeedDate });
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync(u => u.UserName == "admin"))
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@tinhoc.edu.vn",
                FullName = "Quản trị viên",
                Role = "Admin",
                Status = "Active",
                EmailConfirmed = true,
                CreatedAt = SeedDate
            };

            var result = await userManager.CreateAsync(admin, "admin123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        if (await db.StudentProfiles.AnyAsync())
        {
            return;
        }

        var classes = await db.Classes.OrderBy(c => c.Grade).ToListAsync();
        var names = new[]
        {
            "Nguyễn Minh An", "Trần Hà My", "Lê Đức Anh", "Phạm Gia Hân", "Hoàng Nam Khánh",
            "Vũ Mai Linh", "Đỗ Tuấn Minh", "Bùi Thanh Trúc", "Ngô Phương Nhi", "Đinh Quốc Việt"
        };

        var index = 0;
        foreach (var cls in classes)
        {
            for (var i = 0; i < 3; i++)
            {
                var username = $"hs{index + 1:D3}";
                var student = new ApplicationUser
                {
                    UserName = username,
                    Email = $"{username}@tinhoc.edu.vn",
                    FullName = names[index % names.Length],
                    Role = "Student",
                    Status = "Active",
                    EmailConfirmed = true,
                    CreatedAt = SeedDate
                };

                var result = await userManager.CreateAsync(student, "student123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(student, "Student");
                    db.StudentProfiles.Add(new StudentProfile
                    {
                        UserId = student.Id,
                        StudentCode = $"HS{index + 1:D4}",
                        ClassId = cls.Id,
                        Grade = cls.Grade,
                        Gender = index % 2 == 0 ? "Male" : "Female",
                        TotalScore = 0,
                        ExercisesDone = 0,
                        TopicsStudied = 0,
                        StreakDays = 0,
                        LastActive = SeedDate
                    });
                }

                index++;
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedTopicsAsync(DataContext db)
    {
        var topics = new[]
        {
            (10, "Thông tin và dữ liệu", "Phân biệt thông tin, dữ liệu; biểu diễn dữ liệu số, văn bản và đơn vị lưu trữ."),
            (10, "Mạng máy tính và An toàn số", "Mạng LAN/WAN/Internet, giao thức, mã hóa, tường lửa, bản quyền và giấy phép."),
            (10, "Python cơ bản", "Biến, kiểu dữ liệu, rẽ nhánh, vòng lặp, hàm nhập xuất và đọc hiểu chương trình Python."),
            (11, "Lập trình nâng cao", "Mảng, hàm, module, tìm kiếm, sắp xếp, kiểm thử và độ phức tạp thuật toán."),
            (11, "Cơ sở dữ liệu", "CSDL, hệ quản trị CSDL, mô hình quan hệ, khóa, truy vấn SQL cơ bản."),
            (12, "Trí tuệ nhân tạo và Học máy", "AI, học máy, dữ liệu huấn luyện, ứng dụng, đạo đức AI và dữ liệu cá nhân."),
            (12, "Thiết kế Web", "HTML, CSS, cấu trúc trang, bộ chọn, box model và flexbox cơ bản.")
        };

        foreach (var (grade, name, description) in topics)
        {
            var topic = await db.Topics.FirstOrDefaultAsync(t => t.Grade == grade && t.Name == name);
            if (topic is null)
            {
                db.Topics.Add(new Topic
                {
                    Grade = grade,
                    Name = name,
                    Description = description,
                    Status = "Active",
                    CreatedAt = SeedDate
                });
            }
            else
            {
                topic.Description = description;
                topic.Status = "Active";
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedKhoi10Async(DataContext db)
    {
        await SeedExerciseAsync(db, "Thông tin và dữ liệu - Luyện tập cơ bản", 10, null, Practice, "Easy", 900, "Thông tin và dữ liệu",
        [
            D(MakeMCQ(0, "Trong tin học, phát biểu nào sau đây phân biệt đúng giữa dữ liệu và thông tin?", 1, 1, "Dữ liệu là thông tin đã được xử lí.", "Thông tin là dữ liệu đã được xử lí, có ý nghĩa trong một ngữ cảnh.", "Dữ liệu luôn có ý nghĩa đầy đủ với người nhận.", "Thông tin chỉ tồn tại dưới dạng số." , 2), "Easy"),
            D(MakeMCQ(0, "Số nhị phân 1011 có giá trị thập phân là bao nhiêu?", 2, 1, "9", "10", "11", "12", 3), "Easy"),
            D(MakeMCQ(0, "1 byte bằng bao nhiêu bit?", 3, 1, "4 bit", "8 bit", "16 bit", "1024 bit", 2), "Easy"),
            D(MakeMCQ(0, "Theo cách tính thường dùng trong tin học, 1 KB bằng bao nhiêu byte?", 4, 1, "1000 byte", "1024 byte", "2048 byte", "8 byte", 2), "Easy"),
            D(MakeMCQ(0, "Unicode được sử dụng rộng rãi vì lí do nào sau đây?", 5, 1, "Chỉ mã hóa được chữ cái tiếng Anh.", "Giảm toàn bộ dung lượng tệp về 1 byte.", "Biểu diễn được kí tự của nhiều hệ chữ khác nhau.", "Không cần bảng mã khi lưu văn bản.", 3), "Medium"),
            D(MakeMCQ(0, "Dãy bit 01000001 trong bảng mã ASCII biểu diễn kí tự nào?", 6, 1, "A", "a", "0", "1", 1), "Medium"),
            D(MakeMCQ(0, "Phát biểu nào đúng về biểu diễn số nguyên trong máy tính?", 7, 1, "Máy tính lưu số nguyên dưới dạng dãy bit.", "Số nguyên luôn được lưu bằng mã ASCII.", "Số nguyên âm không thể biểu diễn bằng bit.", "Mọi số nguyên đều chiếm đúng 1 byte.", 1), "Medium"),
            D(MakeMCQ(0, "Vì sao số thực trong máy tính có thể có sai số khi tính toán?", 8, 1, "Vì máy tính không xử lí được phép cộng.", "Vì số thực thường được biểu diễn xấp xỉ bằng số bit hữu hạn.", "Vì số thực luôn được lưu bằng văn bản.", "Vì CPU chỉ nhận dữ liệu âm thanh.", 2), "Medium")
        ]);

        await SeedExerciseAsync(db, "Mạng máy tính và An toàn số - Luyện tập cơ bản", 10, null, Practice, "Easy", 900, "Mạng máy tính và An toàn số",
        [
            D(MakeMCQ(0, "Mạng LAN thường được hiểu là loại mạng nào?", 1, 1, "Mạng toàn cầu", "Mạng cục bộ trong phạm vi nhỏ", "Mạng chỉ dùng vệ tinh", "Mạng không có máy chủ", 2), "Easy"),
            D(MakeMCQ(0, "Internet là gì?", 2, 1, "Một mạng máy tính toàn cầu liên kết nhiều mạng khác nhau.", "Một phần mềm soạn thảo văn bản.", "Một thiết bị lưu trữ dữ liệu.", "Một giao thức chỉ dùng trong LAN.", 1), "Easy"),
            D(MakeMCQ(0, "Giao thức nào là nền tảng cho việc truyền dữ liệu trên Internet?", 3, 1, "HTML", "TCP/IP", "JPEG", "SQL", 2), "Easy"),
            D(MakeMCQ(0, "HTTPS an toàn hơn HTTP chủ yếu vì lí do nào?", 4, 1, "HTTPS không cần trình duyệt.", "HTTPS mã hóa dữ liệu trao đổi giữa trình duyệt và máy chủ.", "HTTPS chỉ chạy trong mạng LAN.", "HTTPS làm máy tính không cần địa chỉ IP.", 2), "Medium"),
            D(MakeMCQ(0, "Tường lửa có vai trò chính nào sau đây?", 5, 1, "Biên dịch chương trình Python.", "Lọc và kiểm soát lưu lượng mạng theo quy tắc bảo mật.", "Tăng dung lượng ổ cứng.", "Chuyển đổi ảnh sang văn bản.", 2), "Medium"),
            D(MakeMCQ(0, "Mã hóa dữ liệu giúp đạt mục tiêu nào?", 6, 1, "Làm dữ liệu luôn đúng tuyệt đối.", "Làm dữ liệu dễ đọc với mọi người.", "Bảo vệ nội dung khỏi người không có khóa giải mã phù hợp.", "Xóa bỏ nhu cầu sao lưu dữ liệu.", 3), "Medium"),
            D(MakeMCQ(0, "Việc sử dụng phần mềm thương mại không có giấy phép hợp lệ thường vi phạm điều gì?", 7, 1, "Quyền tác giả/quyền sở hữu trí tuệ.", "Quy tắc đặt tên tệp.", "Chuẩn mã hóa Unicode.", "Giao thức TCP/IP.", 1), "Easy"),
            D(MakeMCQ(0, "Giấy phép phần mềm nguồn mở thường cho phép điều nào sau đây?", 8, 1, "Luôn cấm xem mã nguồn.", "Có thể sử dụng, nghiên cứu, sửa đổi và phân phối theo điều kiện giấy phép.", "Chỉ được chạy trên một máy duy nhất.", "Không cần tuân thủ bất kì điều kiện nào.", 2), "Medium")
        ]);

        await SeedExerciseAsync(db, "Python cơ bản - Luyện tập đọc hiểu chương trình", 10, null, Practice, "Medium", 1200, "Python cơ bản",
        [
            D(MakeMCQ(0, "Kết quả của lệnh `print(type(5))` là gì?", 1, 1, "<class 'str'>", "<class 'float'>", "<class 'int'>", "<class 'bool'>", 3), "Easy"),
            D(MakeMCQ(0, "Trong Python, hàm `input()` trả về dữ liệu thuộc kiểu nào?", 2, 1, "int", "float", "bool", "str", 4), "Easy"),
            D(MakeMCQ(0, "Kết quả của đoạn lệnh sau là gì?\n\nx = 7\nif x % 2 == 0:\n    print('A')\nelse:\n    print('B')", 3, 1, "A", "B", "7", "Không in gì", 2), "Easy"),
            D(MakeMCQ(0, "Kết quả của `print(len('Tin hoc'))` là bao nhiêu?", 4, 1, "6", "7", "8", "5", 2), "Easy"),
            D(MakeMCQ(0, "Đoạn chương trình sau in ra gì?\n\ns = 0\nfor i in range(1, 4):\n    s += i\nprint(s)", 5, 1, "3", "4", "6", "10", 3), "Medium"),
            D(MakeMCQ(0, "Sau đoạn lệnh `a = '12'; b = int(a) + 3`, giá trị của `b` là gì?", 6, 1, "'123'", "15", "12", "Lỗi cú pháp", 2), "Medium"),
            D(MakeMCQ(0, "Vòng lặp `while` nên được dùng khi nào là phù hợp nhất?", 7, 1, "Khi luôn biết chính xác số lần lặp trước khi chạy.", "Khi cần lặp trong khi một điều kiện còn đúng.", "Khi chỉ cần khai báo biến.", "Khi muốn chương trình dừng ngay lập tức.", 2), "Medium"),
            D(MakeMCQ(0, "Đoạn chương trình sau in ra gì?\n\nx = 3\nwhile x > 0:\n    print(x, end=' ')\n    x -= 1", 8, 1, "1 2 3", "3 2 1", "3 2 1 0", "Lặp vô hạn", 2), "Medium")
        ]);

        await SeedExerciseAsync(db, "Khối 10 - Đề chủ đề tổng hợp", 10, null, TopicExam, "Medium", 1500, "Python cơ bản",
        [
            D(MakeMCQ(0, "Thiết bị số lưu trữ, xử lí và truyền dữ liệu chủ yếu dưới dạng nào?", 1, 1, "Tín hiệu chữ viết tay", "Dãy bit", "Tệp giấy", "Âm thanh tương tự", 2), "Easy"),
            D(MakeMCQ(0, "Số thập phân 13 được biểu diễn trong hệ nhị phân là gì?", 2, 1, "1011", "1100", "1101", "1110", 3), "Medium"),
            D(MakeMCQ(0, "Phát biểu nào đúng về mạng WAN?", 3, 1, "Chỉ kết nối hai thiết bị trong cùng bàn học.", "Có phạm vi rộng, có thể liên kết các mạng ở nhiều khu vực địa lí.", "Không dùng giao thức truyền thông.", "Luôn nhanh hơn LAN trong mọi tình huống.", 2), "Medium"),
            D(MakeMCQ(0, "Trong địa chỉ web `https://example.com`, phần `https` cho biết điều gì?", 4, 1, "Tên miền của máy chủ", "Giao thức truy cập có mã hóa", "Tên tệp đang tải", "Địa chỉ MAC của máy", 2), "Easy"),
            D(MakeMCQ(0, "Kết quả của đoạn lệnh sau là gì?\n\nx = 2\nx = x * 3 + 1\nprint(x)", 5, 1, "6", "7", "8", "5", 2), "Easy"),
            D(MakeMCQ(0, "Kết quả của `print(17 // 5, 17 % 5)` là gì?", 6, 1, "3 2", "2 3", "3.4 2", "4 1", 1), "Medium"),
            D(MakeMCQ(0, "Đoạn lệnh sau in ra gì?\n\nfor i in range(2, 7, 2):\n    print(i, end=' ')", 7, 1, "2 4 6", "2 3 4 5 6", "4 6", "2 4 6 8", 1), "Medium"),
            D(MakeMCQ(0, "Hành vi nào phù hợp với đạo đức số khi dùng tài liệu trên Internet?", 8, 1, "Sao chép nguyên văn và nhận là của mình.", "Trích dẫn nguồn khi sử dụng nội dung của người khác.", "Tự ý công bố dữ liệu cá nhân của bạn học.", "Chia sẻ mật khẩu lớp học lên mạng xã hội.", 2), "Easy"),
            D(MakeMCQ(0, "Đoạn chương trình sau in ra gì?\n\nn = 5\nif n > 5:\n    print('A')\nelif n == 5:\n    print('B')\nelse:\n    print('C')", 9, 1, "A", "B", "C", "Không in gì", 2), "Medium"),
            D(MakeMCQ(0, "Một mật khẩu mạnh nên có đặc điểm nào?", 10, 1, "Ngắn, dễ đoán và dùng lại ở mọi tài khoản.", "Chỉ gồm ngày sinh.", "Đủ dài, có nhiều loại kí tự và không dùng lại tùy tiện.", "Giống tên đăng nhập để dễ nhớ.", 3), "Medium"),
            D(MakeMultipleTrue(0, "Xét các phát biểu về dữ liệu và mã hóa văn bản.", 11, 1,
                ("ASCII ban đầu chủ yếu mã hóa các kí tự tiếng Anh cơ bản.", true),
                ("Unicode hỗ trợ biểu diễn nhiều hệ chữ hơn ASCII truyền thống.", true),
                ("Một byte luôn biểu diễn được mọi kí tự Unicode.", false),
                ("Dữ liệu trong máy tính có thể được biểu diễn bằng dãy bit.", true)), "Hard"),
            D(MakeMultipleTrue(0, "Xét đoạn chương trình Python:\n\ns = 0\nfor i in range(1, 5):\n    s += i\nprint(s)", 12, 1,
                ("Vòng lặp thực hiện 4 lần.", true),
                ("Giá trị cuối cùng của `s` là 10.", true),
                ("Chương trình in ra 15.", false),
                ("Biến `i` lần lượt nhận các giá trị 1, 2, 3, 4.", true)), "Hard")
        ]);
    }

    private static async Task SeedKhoi11CsAsync(DataContext db)
    {
        await SeedExerciseAsync(db, "CS 11 - Lập trình nâng cao - Luyện tập", 11, Orientations.CS, Practice, "Medium", 1200, "Lập trình nâng cao",
        [
            D(MakeMCQ(0, "Trong Python, biểu thức nào truy cập phần tử đầu tiên của danh sách `a`?", 1, 1, "a[1]", "a[0]", "a.first()", "a[-0]", 2), "Medium"),
            D(MakeMCQ(0, "Kết quả của `print([1, 2, 3][1])` là gì?", 2, 1, "1", "2", "3", "Lỗi chỉ số", 2), "Medium"),
            D(MakeMCQ(0, "Thuật toán tìm kiếm nhị phân yêu cầu dữ liệu đầu vào thường phải có đặc điểm nào?", 3, 1, "Được sắp xếp", "Có đúng 2 phần tử", "Chỉ gồm số chẵn", "Không có phần tử trùng", 1), "Medium"),
            D(MakeMCQ(0, "Độ phức tạp thời gian của tìm kiếm tuần tự trong trường hợp xấu nhất là gì?", 4, 1, "O(1)", "O(log n)", "O(n)", "O(n^2)", 3), "Medium"),
            D(MakeMCQ(0, "Trong sắp xếp nổi bọt, thao tác cốt lõi là gì?", 5, 1, "Chia đôi mảng liên tục", "So sánh và đổi chỗ các cặp phần tử kề nhau khi sai thứ tự", "Chọn ngẫu nhiên phần tử chốt", "Tạo bảng băm", 2), "Medium"),
            D(MakeMCQ(0, "Mục đích chính của kiểm thử chương trình là gì?", 6, 1, "Chứng minh chương trình luôn đúng trong mọi trường hợp.", "Phát hiện lỗi và tăng độ tin cậy của chương trình.", "Xóa toàn bộ chú thích.", "Thay thế thuật toán bằng giao diện.", 2), "Medium"),
            D(MakeMCQ(0, "Module trong Python giúp ích chủ yếu ở điểm nào?", 7, 1, "Làm chương trình không cần biến.", "Tổ chức và tái sử dụng mã nguồn.", "Chuyển mọi lỗi thành cảnh báo.", "Tự động sắp xếp dữ liệu.", 2), "Medium"),
            D(MakeMCQ(0, "Kết quả của đoạn lệnh sau là gì?\n\ndef f(x):\n    return x * x\nprint(f(4))", 8, 1, "4", "8", "16", "Lỗi vì thiếu input()", 3), "Medium")
        ]);

        await SeedExerciseAsync(db, "CS 11 - Đề chủ đề Lập trình nâng cao", 11, Orientations.CS, TopicExam, "Hard", 1800, "Lập trình nâng cao",
        [
            D(MakeMCQ(0, "Đoạn lệnh sau in ra gì?\n\na = [3, 1, 4]\na.append(2)\nprint(len(a), a[-1])", 1, 1, "3 4", "4 2", "4 4", "2 4", 2), "Medium"),
            D(MakeMCQ(0, "Kết quả của đoạn lệnh sau là gì?\n\na = [5, 2, 7, 2]\nprint(a.count(2))", 2, 1, "1", "2", "3", "4", 2), "Medium"),
            D(MakeMCQ(0, "Với danh sách đã sắp xếp tăng dần, tìm kiếm nhị phân loại bỏ khoảng bao nhiêu phần dữ liệu sau mỗi lần so sánh?", 3, 1, "Một phần tử", "Một nửa khoảng tìm kiếm", "Toàn bộ danh sách", "Hai phần tử đầu", 2), "Medium"),
            D(MakeMCQ(0, "Độ phức tạp trung bình thường nêu của tìm kiếm nhị phân là gì?", 4, 1, "O(1)", "O(log n)", "O(n)", "O(n log n)", 2), "Medium"),
            D(MakeMCQ(0, "Đoạn lệnh sau in ra gì?\n\ndef g(a):\n    a[0] = 9\nx = [1, 2]\ng(x)\nprint(x[0])", 5, 1, "1", "2", "9", "Lỗi kiểu dữ liệu", 3), "Hard"),
            D(MakeMCQ(0, "Trong thiết kế chương trình theo mô đun, điều nào là hợp lí?", 6, 1, "Viết toàn bộ chương trình trong một hàm duy nhất.", "Tách chương trình thành các phần có nhiệm vụ rõ ràng.", "Không đặt tên hàm.", "Chỉ dùng biến toàn cục.", 2), "Medium"),
            D(MakeMCQ(0, "Kết quả của đoạn lệnh sau là gì?\n\ns = 0\nfor x in [1, 3, 5]:\n    s += x\nprint(s)", 7, 1, "8", "9", "10", "135", 2), "Medium"),
            D(MakeMCQ(0, "Trường hợp kiểm thử biên thường dùng để làm gì?", 8, 1, "Kiểm tra các giá trị ở ranh giới miền dữ liệu.", "Chỉ kiểm tra giao diện màu sắc.", "Xóa dữ liệu đầu vào.", "Tạo dữ liệu ngẫu nhiên vô hạn.", 1), "Hard"),
            D(MakeMCQ(0, "Với hai vòng lặp lồng nhau cùng chạy từ 1 đến n, số bước thường tăng theo bậc nào?", 9, 1, "O(log n)", "O(n)", "O(n^2)", "O(1)", 3), "Hard"),
            D(MakeMCQ(0, "Trong Python, câu lệnh `import math` dùng để làm gì?", 10, 1, "Tạo một danh sách mới.", "Nạp module math để dùng các hàm/hằng toán học.", "Biến mọi số thành số nguyên.", "Chạy trình duyệt web.", 2), "Medium"),
            D(MakeMCQ(0, "Kết quả của đoạn lệnh sau là gì?\n\na = [4, 1, 3]\na.sort()\nprint(a)", 11, 1, "[4, 1, 3]", "[1, 3, 4]", "[3, 1, 4]", "None", 2), "Medium"),
            D(MakeMCQ(0, "Phát biểu nào đúng về lỗi logic?", 12, 1, "Chương trình không chạy được vì sai cú pháp.", "Chương trình chạy nhưng cho kết quả không đúng mong muốn.", "Máy tính bị mất nguồn.", "Tệp mã nguồn không thể lưu.", 2), "Medium"),
            D(MakeMultipleTrue(0, "Xét thuật toán tìm kiếm nhị phân trên danh sách tăng dần.", 13, 1,
                ("Danh sách cần được sắp xếp để thuật toán hoạt động đúng.", true),
                ("Sau mỗi bước, thuật toán có thể thu hẹp khoảng tìm kiếm.", true),
                ("Thuật toán luôn kiểm tra tất cả phần tử.", false),
                ("Trường hợp xấu nhất thường có độ phức tạp O(log n).", true)), "Hard"),
            D(MakeMultipleTrue(0, "Xét đoạn mã:\n\na = [2, 4, 6]\nfor i in range(len(a)):\n    a[i] += 1\nprint(a)", 14, 1,
                ("`len(a)` bằng 3.", true),
                ("Sau vòng lặp, danh sách là [3, 5, 7].", true),
                ("Vòng lặp chạy với i nhận 1, 2, 3.", false),
                ("Chương trình không làm thay đổi danh sách a.", false)), "Hard"),
            D(MakeMultipleTrue(0, "Xét kiểm thử chương trình.", 15, 1,
                ("Cần có ca kiểm thử với dữ liệu hợp lệ.", true),
                ("Cần xem xét dữ liệu biên nếu bài toán có miền giá trị.", true),
                ("Nếu chương trình chạy một lần đúng thì chắc chắn không còn lỗi.", false),
                ("Kiểm thử giúp phát hiện lỗi nhưng không chứng minh tuyệt đối chương trình đúng.", true)), "Hard")
        ]);
    }

    private static async Task SeedKhoi11IctAsync(DataContext db)
    {
        await SeedExerciseAsync(db, "ICT 11 - Cơ sở dữ liệu - Luyện tập", 11, Orientations.ICT, Practice, "Medium", 1200, "Cơ sở dữ liệu",
        [
            D(MakeMCQ(0, "Cơ sở dữ liệu là gì?", 1, 1, "Tập hợp dữ liệu được tổ chức để lưu trữ, quản lí và khai thác.", "Một chương trình vẽ ảnh.", "Một giao thức mạng.", "Một kiểu dữ liệu trong Python.", 1), "Easy"),
            D(MakeMCQ(0, "DBMS là gì?", 2, 1, "Hệ điều hành", "Hệ quản trị cơ sở dữ liệu", "Thiết bị mạng", "Ngôn ngữ đánh dấu", 2), "Easy"),
            D(MakeMCQ(0, "Trong mô hình quan hệ, dữ liệu thường được tổ chức thành gì?", 3, 1, "Bảng", "Slide", "Khung hình", "Tệp âm thanh", 1), "Easy"),
            D(MakeMCQ(0, "Khóa chính có đặc điểm nào?", 4, 1, "Có thể trùng lặp tùy ý.", "Dùng để xác định duy nhất mỗi bản ghi.", "Chỉ dùng cho trường văn bản.", "Không liên quan đến bảng.", 2), "Medium"),
            D(MakeMCQ(0, "Khóa ngoại thường dùng để làm gì?", 5, 1, "Tạo liên kết giữa các bảng.", "Mã hóa mật khẩu.", "Định dạng chữ in đậm.", "Sắp xếp thư mục.", 1), "Medium"),
            D(MakeMCQ(0, "Câu lệnh SQL nào dùng để lấy dữ liệu từ bảng?", 6, 1, "SELECT", "DELETE", "CREATE", "INSERT", 1), "Easy"),
            D(MakeMCQ(0, "Mệnh đề `WHERE` trong SQL dùng để làm gì?", 7, 1, "Lọc các bản ghi thỏa điều kiện.", "Đổi tên bảng.", "Xóa toàn bộ CSDL.", "Tạo giao diện biểu mẫu.", 1), "Medium"),
            D(MakeMCQ(0, "`ORDER BY Diem DESC` có ý nghĩa gì?", 8, 1, "Sắp xếp tăng dần theo Diem.", "Sắp xếp giảm dần theo Diem.", "Nhóm dữ liệu theo Diem.", "Chỉ lấy các điểm bằng 0.", 2), "Medium")
        ]);

        await SeedExerciseAsync(db, "ICT 11 - Đề chủ đề Cơ sở dữ liệu", 11, Orientations.ICT, TopicExam, "Hard", 1800, "Cơ sở dữ liệu",
        [
            D(MakeMCQ(0, "Bảng HOCSINH(MaHS, HoTen, Lop). Trường nào phù hợp nhất làm khóa chính?", 1, 1, "HoTen", "Lop", "MaHS", "Không trường nào", 3), "Medium"),
            D(MakeMCQ(0, "Câu SQL nào lấy tất cả cột từ bảng HOCSINH?", 2, 1, "SELECT * FROM HOCSINH;", "GET ALL HOCSINH;", "SELECT HOCSINH WHERE *;", "FROM HOCSINH SELECT *;", 1), "Easy"),
            D(MakeMCQ(0, "Câu SQL nào lấy học sinh lớp '12A1'?", 3, 1, "SELECT * FROM HOCSINH WHERE Lop = '12A1';", "SELECT Lop = '12A1' FROM HOCSINH;", "FILTER HOCSINH BY Lop;", "SELECT * WHERE HOCSINH = '12A1';", 1), "Medium"),
            D(MakeMCQ(0, "Bảng DIEM(MaHS, Mon, Diem). Câu nào lấy điểm từ cao xuống thấp?", 4, 1, "SELECT * FROM DIEM ORDER BY Diem ASC;", "SELECT * FROM DIEM ORDER BY Diem DESC;", "SELECT * FROM DIEM GROUP BY Diem;", "SELECT * FROM DIEM WHERE Diem DESC;", 2), "Medium"),
            D(MakeMCQ(0, "Mệnh đề GROUP BY thường dùng khi nào?", 5, 1, "Khi muốn nhóm bản ghi để tính toán tổng hợp.", "Khi muốn xóa khóa chính.", "Khi muốn đổi màu bảng.", "Khi muốn tạo mật khẩu.", 1), "Medium"),
            D(MakeMCQ(0, "Câu SQL `SELECT COUNT(*) FROM HOCSINH;` trả về gì?", 6, 1, "Danh sách tên học sinh", "Số bản ghi trong bảng HOCSINH", "Tên cột đầu tiên", "Bảng mới tên COUNT", 2), "Medium"),
            D(MakeMCQ(0, "Với bảng SACH(MaSach, TenSach, NamXB), câu nào lọc sách xuất bản sau năm 2020?", 7, 1, "SELECT * FROM SACH WHERE NamXB > 2020;", "SELECT * FROM SACH ORDER BY NamXB > 2020;", "SELECT NamXB > 2020 FROM SACH;", "GROUP BY NamXB > 2020;", 1), "Medium"),
            D(MakeMCQ(0, "Trong Access hoặc MySQL, truy vấn thường dùng để làm gì?", 8, 1, "Khai thác dữ liệu theo điều kiện/yêu cầu.", "Làm tăng dung lượng ổ cứng.", "Chuyển màn hình sang chế độ tối.", "Thay thế hệ điều hành.", 1), "Medium"),
            D(MakeMCQ(0, "Ràng buộc khóa ngoại giúp hạn chế lỗi nào?", 9, 1, "Lỗi tham chiếu tới bản ghi không tồn tại.", "Lỗi gõ sai chữ hoa.", "Lỗi thiếu hình minh họa.", "Lỗi mất kết nối Wi-Fi.", 1), "Hard"),
            D(MakeMCQ(0, "Câu SQL nào lấy các lớp khác nhau trong bảng HOCSINH?", 10, 1, "SELECT DISTINCT Lop FROM HOCSINH;", "SELECT UNIQUE * HOCSINH;", "SELECT Lop GROUP DISTINCT;", "SELECT Lop FROM DISTINCT HOCSINH;", 1), "Medium"),
            D(MakeMCQ(0, "Câu SQL `SELECT AVG(Diem) FROM DIEM WHERE Mon = 'Tin học';` trả về gì?", 11, 1, "Điểm trung bình môn Tin học", "Điểm cao nhất môn Tin học", "Tất cả học sinh học Tin", "Tên môn học đầu tiên", 1), "Medium"),
            D(MakeMCQ(0, "Khi thiết kế bảng, vì sao nên chọn kiểu dữ liệu phù hợp cho từng trường?", 12, 1, "Để giảm lỗi nhập liệu và hỗ trợ xử lí chính xác.", "Để bảng không cần khóa chính.", "Để mọi trường đều thành văn bản.", "Để bỏ qua sao lưu.", 1), "Medium"),
            D(MakeMultipleTrue(0, "Xét bảng HOCSINH(MaHS, HoTen, Lop).", 13, 1,
                ("MaHS phù hợp để làm khóa chính nếu mỗi học sinh có một mã duy nhất.", true),
                ("HoTen luôn chắc chắn là khóa chính tốt nhất.", false),
                ("Có thể lọc học sinh theo lớp bằng mệnh đề WHERE.", true),
                ("Bảng quan hệ gồm các hàng và cột.", true)), "Hard"),
            D(MakeMultipleTrue(0, "Xét câu SQL: SELECT Lop, COUNT(*) FROM HOCSINH GROUP BY Lop;", 14, 1,
                ("Truy vấn nhóm học sinh theo lớp.", true),
                ("COUNT(*) đếm số bản ghi trong từng nhóm.", true),
                ("Truy vấn bắt buộc phải có WHERE mới chạy được.", false),
                ("Kết quả có thể dùng để biết sĩ số từng lớp.", true)), "Hard"),
            D(MakeMultipleTrue(0, "Xét thiết kế CSDL quan hệ.", 15, 1,
                ("Khóa ngoại có thể dùng để liên kết hai bảng.", true),
                ("Khóa chính được phép trùng ở nhiều bản ghi trong cùng bảng.", false),
                ("Một bảng có thể có nhiều trường dữ liệu.", true),
                ("Hệ quản trị CSDL hỗ trợ lưu trữ và truy vấn dữ liệu.", true)), "Hard")
        ]);
    }

    private static async Task SeedKhoi12CsAsync(DataContext db)
    {
        await SeedExerciseAsync(db, "CS 12 - Trí tuệ nhân tạo và Học máy - Luyện tập", 12, Orientations.CS, Practice, "Hard", 1200, "Trí tuệ nhân tạo và Học máy",
        [
            D(MakeMCQ(0, "Trí tuệ nhân tạo (AI) hướng tới mục tiêu nào?", 1, 1, "Tạo hệ thống có khả năng thực hiện một số nhiệm vụ cần trí tuệ con người.", "Chỉ lưu trữ văn bản.", "Thay thế hoàn toàn mọi thiết bị mạng.", "Chỉ định dạng trang web.", 1), "Medium"),
            D(MakeMCQ(0, "Machine Learning là gì?", 2, 1, "Một nhánh của AI cho phép hệ thống học từ dữ liệu để cải thiện nhiệm vụ.", "Một chuẩn cáp mạng.", "Một phần mềm nén tệp.", "Một thẻ HTML.", 1), "Medium"),
            D(MakeMCQ(0, "Bài toán dự đoán giá nhà từ diện tích và vị trí thường thuộc loại học nào?", 3, 1, "Học có giám sát", "Học không giám sát", "Mã hóa đối xứng", "Sắp xếp nổi bọt", 1), "Hard"),
            D(MakeMCQ(0, "Phân nhóm khách hàng theo hành vi mua sắm khi chưa có nhãn nhóm là ví dụ của gì?", 4, 1, "Học có giám sát", "Học không giám sát", "Kiểm thử cú pháp", "Thiết kế CSS", 2), "Hard"),
            D(MakeMCQ(0, "Dữ liệu huấn luyện trong học máy dùng để làm gì?", 5, 1, "Giúp mô hình học quy luật từ ví dụ.", "Xóa kết quả dự đoán.", "Thay thế hệ điều hành.", "Tạo địa chỉ IP.", 1), "Medium"),
            D(MakeMCQ(0, "Hiện tượng mô hình học quá sát dữ liệu huấn luyện và kém trên dữ liệu mới gọi là gì?", 6, 1, "Overfitting", "Firewall", "Unicode", "Routing", 1), "Hard"),
            D(MakeMCQ(0, "Trong ứng dụng AI, dữ liệu cá nhân cần được xử lí thế nào?", 7, 1, "Thu thập càng nhiều càng tốt mà không cần thông báo.", "Tuân thủ quyền riêng tư, mục đích sử dụng và quy định bảo vệ dữ liệu.", "Công khai toàn bộ để mô hình học nhanh hơn.", "Chỉ mã hóa khi dữ liệu là ảnh.", 2), "Medium"),
            D(MakeMCQ(0, "Ví dụ nào sau đây là ứng dụng AI phổ biến?", 8, 1, "Nhận dạng khuôn mặt trong ảnh.", "Đổi tên thư mục thủ công.", "Gõ văn bản bằng bàn phím.", "Cắm dây mạng vào máy tính.", 1), "Medium")
        ]);
    }

    private static async Task SeedKhoi12IctAsync(DataContext db)
    {
        await SeedExerciseAsync(db, "ICT 12 - Thiết kế Web - Luyện tập", 12, Orientations.ICT, Practice, "Medium", 1200, "Thiết kế Web",
        [
            D(MakeMCQ(0, "Một tệp HTML thường dùng để làm gì?", 1, 1, "Mô tả cấu trúc nội dung trang web.", "Huấn luyện mô hình AI.", "Tạo khóa chính trong CSDL.", "Định tuyến gói tin.", 1), "Easy"),
            D(MakeMCQ(0, "Thẻ nào thường chứa phần nội dung hiển thị chính của trang HTML?", 2, 1, "<head>", "<body>", "<title>", "<meta>", 2), "Easy"),
            D(MakeMCQ(0, "Thuộc tính `href` thường xuất hiện trong thẻ nào để tạo liên kết?", 3, 1, "<a>", "<p>", "<table>", "<body>", 1), "Easy"),
            D(MakeMCQ(0, "CSS dùng để làm gì?", 4, 1, "Định dạng, trình bày giao diện trang web.", "Truy vấn dữ liệu bằng SQL.", "Biên dịch Python.", "Mã hóa ổ đĩa.", 1), "Easy"),
            D(MakeMCQ(0, "Selector `.menu` trong CSS chọn phần tử nào?", 5, 1, "Phần tử có id là menu", "Phần tử có class là menu", "Tất cả thẻ menu", "Phần tử có thuộc tính href", 2), "Medium"),
            D(MakeMCQ(0, "Trong box model, `padding` là phần nào?", 6, 1, "Khoảng cách giữa nội dung và viền.", "Khoảng cách ngoài viền.", "Độ rộng của ảnh nền.", "Tên của font chữ.", 1), "Medium"),
            D(MakeMCQ(0, "Khai báo `display: flex` thường dùng để làm gì?", 7, 1, "Bố trí các phần tử con linh hoạt theo hàng/cột.", "Xóa toàn bộ CSS.", "Tạo bảng CSDL.", "Chạy vòng lặp Python.", 1), "Medium"),
            D(MakeMCQ(0, "Đoạn CSS `p { color: red; }` có tác dụng gì?", 8, 1, "Làm chữ trong mọi thẻ p có màu đỏ.", "Tạo liên kết đến red.html.", "Ẩn mọi thẻ p.", "Đổi nền trang thành đỏ.", 1), "Easy")
        ]);

        await SeedExerciseAsync(db, "Khối 12 - Đề chủ đề AI và Thiết kế Web", 12, null, TopicExam, "Hard", 1800, "Thiết kế Web",
        [
            D(MakeMCQ(0, "Thẻ `<title>` trong HTML thường nằm ở đâu?", 1, 1, "Trong phần `<head>`", "Trong phần `<body>` bắt buộc cuối trang", "Trong tệp CSS", "Trong câu lệnh SQL", 1), "Medium"),
            D(MakeMCQ(0, "Đoạn HTML `<a href='home.html'>Trang chủ</a>` tạo ra gì?", 2, 1, "Một liên kết đến home.html", "Một tiêu đề cấp 1", "Một bảng dữ liệu", "Một ảnh", 1), "Easy"),
            D(MakeMCQ(0, "Selector `#main` trong CSS chọn phần tử nào?", 3, 1, "Phần tử có class main", "Phần tử có id main", "Mọi thẻ main và p", "Phần tử có thuộc tính name='main'", 2), "Medium"),
            D(MakeMCQ(0, "Nếu CSS có `margin: 10px`, điều gì xảy ra?", 4, 1, "Tạo khoảng cách bên ngoài viền phần tử.", "Tạo khóa ngoại.", "Đổi kiểu dữ liệu sang số.", "Mã hóa văn bản.", 1), "Medium"),
            D(MakeMCQ(0, "AI nhận dạng thư rác trong email thường cần gì để học tốt?", 5, 1, "Dữ liệu email đã được gán nhãn thư rác/không rác.", "Một bảng màu CSS.", "Chỉ một địa chỉ IP.", "Một thẻ `<br>`.", 1), "Medium"),
            D(MakeMCQ(0, "Trong học máy, nhãn của dữ liệu huấn luyện là gì?", 6, 1, "Kết quả/loại đúng đi kèm ví dụ để mô hình học trong bài toán có giám sát.", "Tên tệp HTML.", "Địa chỉ MAC.", "Lệnh xóa bảng.", 1), "Hard"),
            D(MakeMCQ(0, "Vấn đề thiên lệch dữ liệu trong AI có thể gây hậu quả nào?", 7, 1, "Mô hình đưa ra kết quả thiếu công bằng hoặc kém chính xác cho một số nhóm.", "Trang web tự động có flexbox.", "SQL chạy nhanh hơn.", "Mật khẩu luôn mạnh hơn.", 1), "Hard"),
            D(MakeMCQ(0, "HTML, CSS và JavaScript thường phối hợp theo cách nào?", 8, 1, "HTML cấu trúc, CSS trình bày, JavaScript tạo tương tác.", "HTML truy vấn, CSS lưu trữ, JavaScript cấp phát IP.", "HTML mã hóa, CSS giải mã, JavaScript sao lưu.", "Cả ba đều chỉ dùng để tạo CSDL.", 1), "Medium"),
            D(MakeMCQ(0, "Thuộc tính CSS nào thường dùng để đổi màu chữ?", 9, 1, "font-size", "color", "href", "src", 2), "Easy"),
            D(MakeMCQ(0, "Thẻ nào phù hợp để chèn ảnh vào trang web?", 10, 1, "<img>", "<a>", "<style>", "<script>", 1), "Easy"),
            D(MakeMCQ(0, "Trong AI, vì sao cần bảo vệ dữ liệu cá nhân?", 11, 1, "Vì dữ liệu cá nhân có thể tiết lộ thông tin riêng tư và gây rủi ro nếu bị lạm dụng.", "Vì dữ liệu cá nhân không thể lưu trong máy tính.", "Vì mọi mô hình đều không cần dữ liệu.", "Vì HTML không hỗ trợ tiếng Việt.", 1), "Medium"),
            D(MakeMCQ(0, "Kết quả hiển thị của `<strong>Tin học</strong>` thường là gì?", 12, 1, "Chữ Tin học được nhấn mạnh/in đậm.", "Một liên kết đến Tin học.", "Một ảnh tên Tin học.", "Một bảng hai cột.", 1), "Easy"),
            D(MakeMultipleTrue(0, "Xét HTML cơ bản.", 13, 1,
                ("Thẻ `<body>` chứa phần lớn nội dung hiển thị trên trang.", true),
                ("Thẻ `<a>` có thể dùng để tạo liên kết.", true),
                ("CSS là ngôn ngữ chính để truy vấn CSDL.", false),
                ("Thuộc tính `src` thường dùng để chỉ nguồn ảnh trong thẻ `<img>`.", true)), "Hard"),
            D(MakeMultipleTrue(0, "Xét CSS box model.", 14, 1,
                ("`padding` là khoảng cách giữa nội dung và viền.", true),
                ("`margin` là khoảng cách bên ngoài viền.", true),
                ("`border` luôn bằng 0 và không thể thay đổi.", false),
                ("Kích thước hiển thị có thể bị ảnh hưởng bởi content, padding và border.", true)), "Hard"),
            D(MakeMultipleTrue(0, "Xét AI và học máy.", 15, 1,
                ("Học có giám sát sử dụng dữ liệu có nhãn.", true),
                ("Học không giám sát có thể dùng để phân cụm.", true),
                ("Mọi hệ thống AI đều chắc chắn không sai.", false),
                ("Đạo đức AI liên quan đến quyền riêng tư và tính công bằng.", true)), "Hard")
        ]);
    }

    private static async Task SeedTrialExamsAsync(DataContext db)
    {
        await SeedTrialExamAsync(db, "Đề thi thử Tin học 2025 - Đề số 1", TrialSet1());
        await SeedTrialExamAsync(db, "Đề thi thử Tin học 2025 - Đề số 2", TrialSet2());
    }

    private static List<Question> TrialSet1() =>
    [
        D(MakeMCQ(0, "Đơn vị nhỏ nhất dùng để biểu diễn dữ liệu trong máy tính là gì?", 1, 1, "Byte", "Bit", "KB", "Pixel", 2), "Easy"),
        D(MakeMCQ(0, "Số nhị phân 1110 bằng số thập phân nào?", 2, 1, "12", "13", "14", "15", 3), "Easy"),
        D(MakeMCQ(0, "Thiết bị nào thường dùng để kết nối nhiều máy trong mạng LAN có dây?", 3, 1, "Switch", "Máy chiếu", "Máy quét", "Loa", 1), "Easy"),
        D(MakeMCQ(0, "HTTPS giúp bảo vệ trao đổi dữ liệu bằng cách nào?", 4, 1, "Mã hóa kênh truyền", "Xóa địa chỉ IP", "Tắt trình duyệt", "Đổi tệp HTML thành ảnh", 1), "Medium"),
        D(MakeMCQ(0, "Phát biểu nào đúng về Unicode?", 5, 1, "Chỉ biểu diễn số nguyên.", "Hỗ trợ nhiều hệ chữ và kí tự.", "Không dùng trong văn bản.", "Luôn dùng đúng 1 bit cho mỗi kí tự.", 2), "Medium"),
        D(MakeMCQ(0, "Việc dùng phần mềm lậu có thể vi phạm điều gì?", 6, 1, "Bản quyền phần mềm", "Định dạng CSS", "Khóa ngoại", "Tìm kiếm nhị phân", 1), "Easy"),
        D(MakeMCQ(0, "Kết quả của `print(2 ** 3)` là gì?", 7, 1, "6", "8", "9", "5", 2), "Easy"),
        D(MakeMCQ(0, "Đoạn lệnh sau in ra gì?\n\nx = 4\nif x > 3:\n    print('Tin')\nelse:\n    print('Hoc')", 8, 1, "Tin", "Hoc", "4", "Không in gì", 1), "Easy"),
        D(MakeMCQ(0, "Kết quả của `print(list(range(2, 7, 2)))` là gì?", 9, 1, "[2, 4, 6]", "[2, 3, 4, 5, 6]", "[4, 6]", "[2, 4, 6, 8]", 1), "Medium"),
        D(MakeMCQ(0, "Độ phức tạp của tìm kiếm tuần tự trong trường hợp xấu nhất là gì?", 10, 1, "O(1)", "O(log n)", "O(n)", "O(n^2)", 3), "Medium"),
        D(MakeMCQ(0, "Kết quả của đoạn lệnh sau là gì?\n\na = [1, 2, 3]\nprint(a[-1])", 11, 1, "1", "2", "3", "Lỗi", 3), "Medium"),
        D(MakeMCQ(0, "Tìm kiếm nhị phân phù hợp nhất với dữ liệu nào?", 12, 1, "Danh sách đã sắp xếp", "Danh sách ảnh", "Dữ liệu âm thanh thô", "Mật khẩu chưa mã hóa", 1), "Medium"),
        D(MakeMCQ(0, "SQL dùng chủ yếu để làm việc với loại hệ thống nào?", 13, 1, "Cơ sở dữ liệu quan hệ", "Trình duyệt web", "Tệp âm thanh", "Tường lửa", 1), "Easy"),
        D(MakeMCQ(0, "Câu `SELECT * FROM HOCSINH WHERE Lop='12A1';` có tác dụng gì?", 14, 1, "Lấy học sinh lớp 12A1", "Xóa lớp 12A1", "Tạo bảng lớp 12A1", "Sắp xếp lớp 12A1", 1), "Medium"),
        D(MakeMCQ(0, "Khóa ngoại có vai trò nào?", 15, 1, "Liên kết dữ liệu giữa các bảng", "Đổi màu chữ", "Chạy vòng lặp", "Mã hóa HTML", 1), "Medium"),
        D(MakeMCQ(0, "`ORDER BY Ten ASC` nghĩa là gì?", 16, 1, "Sắp xếp Ten tăng dần", "Sắp xếp Ten giảm dần", "Nhóm theo Ten", "Xóa cột Ten", 1), "Medium"),
        D(MakeMCQ(0, "Thẻ HTML nào tạo tiêu đề lớn nhất trong các lựa chọn?", 17, 1, "<h1>", "<p>", "<a>", "<span>", 1), "Easy"),
        D(MakeMCQ(0, "CSS selector `.card` chọn phần tử nào?", 18, 1, "Có id card", "Có class card", "Có thẻ card", "Có thuộc tính card", 2), "Medium"),
        D(MakeMCQ(0, "Thuộc tính CSS nào điều khiển màu nền?", 19, 1, "background-color", "font-weight", "href", "src", 1), "Easy"),
        D(MakeMCQ(0, "Trong flexbox, `justify-content` thường điều khiển gì?", 20, 1, "Căn chỉnh phần tử theo trục chính", "Tên miền website", "Khóa chính", "Kiểu dữ liệu Python", 1), "Hard"),
        D(MakeMCQ(0, "Học có giám sát cần loại dữ liệu nào?", 21, 1, "Dữ liệu có nhãn", "Dữ liệu không thể đọc", "Chỉ dữ liệu âm thanh", "Không cần dữ liệu", 1), "Medium"),
        D(MakeMCQ(0, "Ứng dụng nào là ví dụ gần với AI?", 22, 1, "Gợi ý video theo sở thích", "Đổi tên tệp thủ công", "Cắm dây mạng", "Gõ phím", 1), "Easy"),
        D(MakeMCQ(0, "Một rủi ro đạo đức khi dùng AI là gì?", 23, 1, "Thiên lệch dữ liệu gây quyết định thiếu công bằng", "Trang web có nhiều thẻ div", "SQL có mệnh đề WHERE", "Mạng dùng giao thức TCP", 1), "Hard"),
        D(MakeMCQ(0, "Dữ liệu kiểm tra trong học máy dùng để làm gì?", 24, 1, "Đánh giá mô hình trên dữ liệu chưa dùng để huấn luyện", "Xóa mô hình", "Định dạng HTML", "Tạo khóa ngoại", 1), "Hard"),
        D(MakeMultipleTrue(0, "Cụm đúng/sai về mạng và an toàn số.", 25, 1,
            ("LAN thường có phạm vi nhỏ hơn WAN.", true),
            ("HTTPS có cơ chế bảo vệ tốt hơn HTTP thông thường.", true),
            ("Tường lửa dùng để biên dịch mã Python.", false),
            ("Mã hóa giúp bảo vệ nội dung dữ liệu khi truyền/lưu.", true)), "Hard"),
        D(MakeMultipleTrue(0, "Cụm đúng/sai về lập trình Python.", 26, 1,
            ("`range(3)` tạo các giá trị 0, 1, 2.", true),
            ("`input()` trả về chuỗi.", true),
            ("Danh sách Python không thể thay đổi phần tử.", false),
            ("`len([2, 4, 6])` bằng 3.", true)), "Hard"),
        D(MakeMultipleTrue(0, "Cụm đúng/sai về CSDL.", 27, 1,
            ("Khóa chính dùng để xác định duy nhất bản ghi.", true),
            ("SELECT là câu lệnh truy vấn dữ liệu.", true),
            ("WHERE dùng để sắp xếp kết quả.", false),
            ("Khóa ngoại có thể tạo quan hệ giữa hai bảng.", true)), "Hard"),
        D(MakeMultipleTrue(0, "Cụm đúng/sai về AI.", 28, 1,
            ("Machine Learning là một nhánh của AI.", true),
            ("Học không giám sát có thể dùng cho phân cụm.", true),
            ("Mô hình AI luôn đúng tuyệt đối.", false),
            ("Bảo vệ dữ liệu cá nhân là vấn đề quan trọng khi triển khai AI.", true)), "Hard")
    ];

    private static List<Question> TrialSet2() =>
    [
        D(MakeMCQ(0, "Trong máy tính, ảnh số cũng được lưu trữ dưới dạng nào ở mức thấp?", 1, 1, "Dãy bit", "Trang giấy", "Tín hiệu mùi", "Dòng lệnh SQL", 1), "Easy"),
        D(MakeMCQ(0, "Số nhị phân 10010 bằng số thập phân nào?", 2, 1, "16", "17", "18", "20", 3), "Medium"),
        D(MakeMCQ(0, "WAN là loại mạng có đặc điểm nào?", 3, 1, "Phạm vi rộng, kết nối qua khu vực địa lí lớn", "Chỉ gồm một bàn phím", "Không cần giao thức", "Không thể kết nối Internet", 1), "Easy"),
        D(MakeMCQ(0, "Giao thức HTTP/HTTPS thường liên quan trực tiếp tới dịch vụ nào?", 4, 1, "Truy cập web", "In văn bản", "Sạc pin", "Chụp màn hình", 1), "Easy"),
        D(MakeMCQ(0, "Mật khẩu nào có xu hướng an toàn hơn?", 5, 1, "123456", "ngaysinh", "TinHoc@2025#A", "abcdef", 3), "Easy"),
        D(MakeMCQ(0, "Việc trích dẫn nguồn khi dùng tài liệu trên mạng thể hiện điều gì?", 6, 1, "Tôn trọng bản quyền và đạo đức số", "Tăng tốc CPU", "Tạo khóa ngoại", "Mã hóa router", 1), "Easy"),
        D(MakeMCQ(0, "Kết quả của `print(15 % 4)` là gì?", 7, 1, "2", "3", "4", "15", 2), "Easy"),
        D(MakeMCQ(0, "Đoạn lệnh sau in ra gì?\n\nx = 'Tin'\ny = 'Hoc'\nprint(x + y)", 8, 1, "Tin Hoc", "TinHoc", "x+y", "Lỗi", 2), "Easy"),
        D(MakeMCQ(0, "Kết quả của đoạn lệnh sau là gì?\n\ns = 1\nfor i in range(1, 4):\n    s *= i\nprint(s)", 9, 1, "3", "4", "6", "24", 3), "Medium"),
        D(MakeMCQ(0, "Sắp xếp nổi bọt dựa trên thao tác nào?", 10, 1, "So sánh và đổi chỗ các phần tử kề nhau", "Mã hóa dữ liệu", "Tạo khóa chính", "Tách HTML khỏi CSS", 1), "Medium"),
        D(MakeMCQ(0, "Kết quả của `print(len([0, 2, 4, 6]))` là gì?", 11, 1, "3", "4", "6", "8", 2), "Easy"),
        D(MakeMCQ(0, "Độ phức tạp O(n^2) thường xuất hiện khi nào?", 12, 1, "Hai vòng lặp lồng nhau cùng phụ thuộc n", "Một phép gán duy nhất", "Tìm kiếm nhị phân", "Truy cập một phần tử theo chỉ số", 1), "Hard"),
        D(MakeMCQ(0, "Trong bảng quan hệ, mỗi hàng thường biểu diễn gì?", 13, 1, "Một bản ghi", "Một tệp CSS", "Một địa chỉ IP", "Một mô hình AI", 1), "Easy"),
        D(MakeMCQ(0, "Câu SQL nào chỉ lấy cột HoTen từ bảng HOCSINH?", 14, 1, "SELECT HoTen FROM HOCSINH;", "GET HoTen IN HOCSINH;", "FROM HoTen SELECT HOCSINH;", "SELECT HOCSINH.HoTen WHERE;", 1), "Medium"),
        D(MakeMCQ(0, "Mệnh đề `GROUP BY Lop` thường tạo kết quả theo cách nào?", 15, 1, "Nhóm bản ghi theo lớp", "Xóa các lớp", "Đổi tên lớp", "Mã hóa lớp", 1), "Medium"),
        D(MakeMCQ(0, "Ràng buộc NOT NULL có ý nghĩa gì?", 16, 1, "Trường không được để trống giá trị NULL", "Trường phải là khóa ngoại", "Trường chỉ nhận ảnh", "Trường tự sắp xếp", 1), "Medium"),
        D(MakeMCQ(0, "Trong HTML, thẻ `<ul>` thường dùng để tạo gì?", 17, 1, "Danh sách không thứ tự", "Bảng dữ liệu", "Liên kết", "Biểu mẫu nhập mật khẩu", 1), "Easy"),
        D(MakeMCQ(0, "Thuộc tính `alt` của thẻ ảnh thường dùng để làm gì?", 18, 1, "Cung cấp văn bản thay thế cho ảnh", "Tạo khóa chính", "Đặt màu nền", "Chạy Python", 1), "Medium"),
        D(MakeMCQ(0, "CSS `font-weight: bold;` có tác dụng gì?", 19, 1, "Làm chữ đậm", "Ẩn chữ", "Tạo liên kết", "Đổi ảnh", 1), "Easy"),
        D(MakeMCQ(0, "Trong flexbox, `align-items` thường căn chỉnh theo trục nào?", 20, 1, "Trục chéo", "Địa chỉ IP", "Khóa chính", "Dòng lệnh SQL", 1), "Hard"),
        D(MakeMCQ(0, "Học không giám sát thường phù hợp với nhiệm vụ nào?", 21, 1, "Phân cụm dữ liệu chưa có nhãn", "Dự đoán với nhãn đã biết duy nhất", "Định dạng CSS", "Tạo bảng HTML", 1), "Medium"),
        D(MakeMCQ(0, "Một ví dụ về dữ liệu cá nhân là gì?", 22, 1, "Số căn cước công dân", "Tên thẻ HTML", "Từ khóa SELECT", "Thuộc tính margin", 1), "Easy"),
        D(MakeMCQ(0, "Mục tiêu của tập kiểm tra trong học máy là gì?", 23, 1, "Ước lượng khả năng khái quát hóa của mô hình", "Làm dữ liệu mất nhãn", "Tạo thêm thẻ CSS", "Thay đổi khóa chính", 1), "Hard"),
        D(MakeMCQ(0, "Khi dùng AI tạo nội dung, hành động nào phù hợp hơn?", 24, 1, "Kiểm chứng thông tin và ghi nhận nguồn/công cụ khi cần", "Tin tuyệt đối mọi kết quả", "Công bố dữ liệu riêng tư", "Xóa toàn bộ dữ liệu gốc", 1), "Medium"),
        D(MakeMultipleTrue(0, "Cụm đúng/sai về mạng.", 25, 1,
            ("Internet là mạng máy tính toàn cầu.", true),
            ("TCP/IP là bộ giao thức nền tảng của Internet.", true),
            ("HTTP luôn mã hóa nội dung như HTTPS.", false),
            ("Thiết bị trong mạng cần tuân theo giao thức để trao đổi dữ liệu.", true)), "Hard"),
        D(MakeMultipleTrue(0, "Cụm đúng/sai về Python.", 26, 1,
            ("`for i in range(1,4)` cho i nhận 1, 2, 3.", true),
            ("`//` là phép chia lấy phần nguyên trong Python.", true),
            ("`elif` chỉ được dùng ngoài cấu trúc điều kiện.", false),
            ("Danh sách có thể được duyệt bằng vòng lặp for.", true)), "Hard"),
        D(MakeMultipleTrue(0, "Cụm đúng/sai về SQL.", 27, 1,
            ("ORDER BY có thể dùng để sắp xếp kết quả.", true),
            ("GROUP BY thường dùng cùng các hàm tổng hợp.", true),
            ("Khóa chính trong cùng một bảng được phép trùng tùy ý.", false),
            ("DBMS hỗ trợ tạo, cập nhật và khai thác CSDL.", true)), "Hard"),
        D(MakeMultipleTrue(0, "Cụm đúng/sai về AI và dữ liệu.", 28, 1,
            ("Dữ liệu huấn luyện ảnh hưởng đến chất lượng mô hình.", true),
            ("Thiên lệch dữ liệu có thể dẫn đến kết quả thiếu công bằng.", true),
            ("Overfitting là khi mô hình luôn tổng quát hóa tốt.", false),
            ("Quyền riêng tư cần được xem xét khi xử lí dữ liệu cá nhân.", true)), "Hard")
    ];

    private static async Task SeedTrialExamAsync(DataContext db, string title, List<Question> questions)
    {
        var topicMap = new Dictionary<int, string>();
        foreach (var order in new[] { 1, 2 })
            topicMap[order] = "Thông tin và dữ liệu";
        foreach (var order in new[] { 3, 4, 5, 6, 25 })
            topicMap[order] = "Mạng máy tính và An toàn số";
        foreach (var order in new[] { 7, 8, 9, 10, 11, 12, 26 })
            topicMap[order] = "Python cơ bản";
        foreach (var order in new[] { 13, 14, 15, 16, 27 })
            topicMap[order] = "Cơ sở dữ liệu";
        foreach (var order in new[] { 17, 18, 19, 20 })
            topicMap[order] = "Thiết kế Web";
        foreach (var order in new[] { 21, 22, 23, 24, 28 })
            topicMap[order] = "Trí tuệ nhân tạo và Học máy";

        await SeedExerciseAsync(db, title, 12, null, TrialExam, "Medium", 3000, "Trí tuệ nhân tạo và Học máy", questions, createExam: true, questionTopicMap: topicMap);
    }

    private static async Task SeedExerciseAsync(
        DataContext db,
        string title,
        int grade,
        string? orientation,
        string kind,
        string difficulty,
        int timeLimit,
        string topicName,
        List<Question> questions,
        bool createExam = false,
        Dictionary<int, string>? questionTopicMap = null)
    {
        if (await db.Exercises.AnyAsync(e => e.Title == title))
        {
            return;
        }

        var topic = await db.Topics.FirstAsync(t => t.Grade == grade && t.Name == topicName);
        var topicsByName = await db.Topics.ToDictionaryAsync(t => $"{t.Grade}|{t.Name}", t => t.Id);

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var exercise = new Exercise
            {
                Title = title,
                Grade = grade,
                Orientation = orientation,
                TopicId = topic.Id,
                Difficulty = difficulty,
                TimeLimit = timeLimit,
                Status = "Active",
                CreatedAt = SeedDate
            };

            db.Exercises.Add(exercise);
            await db.SaveChangesAsync();

            foreach (var question in questions)
            {
                question.ExerciseId = exercise.Id;
                question.TopicId = questionTopicMap is not null
                    && questionTopicMap.TryGetValue(question.OrderIndex, out var mappedTopicName)
                    && topicsByName.TryGetValue($"{MappedGrade(mappedTopicName)}|{mappedTopicName}", out var mappedTopicId)
                        ? mappedTopicId
                        : topic.Id;
                question.Type = question.QuestionType;
                question.CreatedAt = SeedDate;
                BalanceMcqAnswerLabels(question);
                foreach (var answer in question.Answers)
                {
                    answer.Question = question;
                }
            }

            await db.Questions.AddRangeAsync(questions);
            await db.SaveChangesAsync();

            if (createExam)
            {
                var exam = new Exam
                {
                    Title = title,
                    Grade = grade,
                    Duration = 50,
                    TotalQuestions = 40,
                    Status = "Active",
                    CreatedAt = SeedDate
                };

                db.Exams.Add(exam);
                await db.SaveChangesAsync();

                await db.ExamQuestions.AddRangeAsync(questions
                    .OrderBy(q => q.OrderIndex)
                    .Select((q, index) => new ExamQuestion
                    {
                        ExamId = exam.Id,
                        QuestionId = q.Id,
                        OrderIndex = index + 1
                    }));
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private static int MappedGrade(string topicName) => topicName switch
    {
        "Thông tin và dữ liệu" => 10,
        "Mạng máy tính và An toàn số" => 10,
        "Python cơ bản" => 10,
        "Cơ sở dữ liệu" => 11,
        "Thiết kế Web" => 12,
        "Trí tuệ nhân tạo và Học máy" => 12,
        _ => 12
    };

    private static void BalanceMcqAnswerLabels(Question question)
    {
        if (question.QuestionType != QuestionTypes.MultipleChoice)
        {
            return;
        }

        var answers = question.Answers.ToList();
        var correct = answers.First(a => a.IsCorrect);
        var targetIndex = (question.OrderIndex - 1) % 4;
        answers.Remove(correct);
        answers.Insert(targetIndex, correct);

        var labels = new[] { "A", "B", "C", "D" };
        for (var i = 0; i < answers.Count; i++)
        {
            answers[i].Label = labels[i];
            answers[i].SubIndex = null;
        }

        question.Answers = answers;
    }

    private static Question MakeMCQ(
        int exerciseId, string content,
        int order, decimal points,
        string optA, string optB,
        string optC, string optD,
        int correctIndex)
    {
        return new Question
        {
            ExerciseId = exerciseId,
            Content = content,
            OrderIndex = order,
            Difficulty = "Medium",
            Type = QuestionTypes.MultipleChoice,
            QuestionType = QuestionTypes.MultipleChoice,
            Answers = new List<Answer>
            {
                new() { Label = "A", Content = optA, IsCorrect = correctIndex == 1 },
                new() { Label = "B", Content = optB, IsCorrect = correctIndex == 2 },
                new() { Label = "C", Content = optC, IsCorrect = correctIndex == 3 },
                new() { Label = "D", Content = optD, IsCorrect = correctIndex == 4 }
            }
        };
    }

    private static Question MakeMultipleTrue(
        int exerciseId, string content,
        int order, decimal points,
        (string text, bool isCorrect) a,
        (string text, bool isCorrect) b,
        (string text, bool isCorrect) c,
        (string text, bool isCorrect) d)
    {
        return new Question
        {
            ExerciseId = exerciseId,
            Content = content,
            OrderIndex = order,
            Difficulty = "Hard",
            Type = QuestionTypes.MultipleTrue,
            QuestionType = QuestionTypes.MultipleTrue,
            Answers = new List<Answer>
            {
                new() { Label = "a", SubIndex = "a", Content = a.text, IsCorrect = a.isCorrect },
                new() { Label = "b", SubIndex = "b", Content = b.text, IsCorrect = b.isCorrect },
                new() { Label = "c", SubIndex = "c", Content = c.text, IsCorrect = c.isCorrect },
                new() { Label = "d", SubIndex = "d", Content = d.text, IsCorrect = d.isCorrect }
            }
        };
    }

    private static Question D(Question question, string difficulty)
    {
        question.Difficulty = difficulty;
        return question;
    }

    private static async Task SeedCodingProblemsAsync(DataContext db)
    {
        if (await db.CodingProblems.AnyAsync()) return;

        var topic10 = await db.Topics.FirstAsync(t => t.Grade == 10);
        var topic11 = await db.Topics.FirstAsync(t => t.Grade == 11);
        var topic12 = await db.Topics.FirstAsync(t => t.Grade == 12);

        var problems = new List<CodingProblem>
        {
            new() {
                Title = "Tính tổng từ 1 đến n", Grade = 10, TopicId = topic10.Id, Difficulty = "Easy",
                Description = "Viết chương trình nhập vào một số nguyên dương `n`. Tính và in ra tổng S = 1 + 2 + ... + n.",
                SampleInput = "5", SampleOutput = "15",
                TestCases = [
                    new TestCase { Input = "5", ExpectedOutput = "15", IsSample = true, OrderIndex = 1 },
                    new TestCase { Input = "10", ExpectedOutput = "55", IsSample = false, OrderIndex = 2 },
                    new TestCase { Input = "100", ExpectedOutput = "5050", IsSample = false, OrderIndex = 3 }
                ]
            },
            new() {
                Title = "Kiểm tra chẵn lẻ", Grade = 10, TopicId = topic10.Id, Difficulty = "Easy",
                Description = "Nhập một số nguyên `n`. In ra `YES` nếu `n` là số chẵn, ngược lại in `NO`.",
                SampleInput = "4", SampleOutput = "YES",
                TestCases = [
                    new TestCase { Input = "4", ExpectedOutput = "YES", IsSample = true, OrderIndex = 1 },
                    new TestCase { Input = "7", ExpectedOutput = "NO", IsSample = true, OrderIndex = 2 },
                    new TestCase { Input = "0", ExpectedOutput = "YES", IsSample = false, OrderIndex = 3 },
                    new TestCase { Input = "-5", ExpectedOutput = "NO", IsSample = false, OrderIndex = 4 }
                ]
            },
            new() {
                Title = "Tìm giá trị lớn nhất trong mảng", Grade = 11, TopicId = topic11.Id, Difficulty = "Medium",
                Description = "Dòng đầu chứa số `n` (số phần tử). Dòng sau chứa `n` số nguyên cách nhau bằng khoảng trắng. Tìm và in ra số lớn nhất.",
                SampleInput = "4\n1 5 3 2", SampleOutput = "5",
                TestCases = [
                    new TestCase { Input = "4\n1 5 3 2", ExpectedOutput = "5", IsSample = true, OrderIndex = 1 },
                    new TestCase { Input = "3\n-1 -5 -2", ExpectedOutput = "-1", IsSample = false, OrderIndex = 2 },
                    new TestCase { Input = "1\n42", ExpectedOutput = "42", IsSample = false, OrderIndex = 3 }
                ]
            },
            new() {
                Title = "Sắp xếp nổi bọt", Grade = 11, TopicId = topic11.Id, Difficulty = "Medium",
                Description = "Nhập mảng `n` phần tử. Sắp xếp mảng tăng dần và in ra, các số cách nhau bằng 1 khoảng trắng.",
                SampleInput = "5\n4 2 5 1 3", SampleOutput = "1 2 3 4 5",
                TestCases = [
                    new TestCase { Input = "5\n4 2 5 1 3", ExpectedOutput = "1 2 3 4 5", IsSample = true, OrderIndex = 1 },
                    new TestCase { Input = "3\n3 2 1", ExpectedOutput = "1 2 3", IsSample = false, OrderIndex = 2 }
                ]
            },
            new() {
                Title = "Đếm tần suất ký tự", Grade = 12, TopicId = topic12.Id, Difficulty = "Medium",
                Description = "Nhập một chuỗi ký tự (chỉ gồm chữ cái viết thường không dấu). Đếm số lượng chữ 'a' có trong chuỗi.",
                SampleInput = "banana", SampleOutput = "3",
                TestCases = [
                    new TestCase { Input = "banana", ExpectedOutput = "3", IsSample = true, OrderIndex = 1 },
                    new TestCase { Input = "hello", ExpectedOutput = "0", IsSample = false, OrderIndex = 2 },
                    new TestCase { Input = "aaaaa", ExpectedOutput = "5", IsSample = false, OrderIndex = 3 }
                ]
            }
        };

        db.CodingProblems.AddRange(problems);
        await db.SaveChangesAsync();
    }
}
