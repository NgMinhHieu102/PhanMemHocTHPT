namespace TinHocTHPT.Data;

internal static class LearningContentCatalog
{
    public static readonly TopicSeed[] Topics =
    [
        new(10, "Python cơ bản", "Làm quen với môi trường lập trình, câu lệnh nhập xuất, biểu thức và cách kiểm tra kiểu dữ liệu trong Python."),
        new(10, "Biến và kiểu dữ liệu", "Biến, kiểu số, kiểu logic, kiểu chuỗi, ép kiểu và lỗi thường gặp khi xử lí dữ liệu đầu vào."),
        new(10, "Cấu trúc rẽ nhánh", "Biểu thức điều kiện, toán tử so sánh, toán tử logic và cách dùng if, elif, else để mô tả quyết định."),
        new(10, "Vòng lặp", "Vòng lặp for, while, range, biến đếm, biến tích lũy, break, continue và kĩ thuật dò lỗi vòng lặp."),
        new(10, "Danh sách và xử lí dãy số", "List trong Python, chỉ số, duyệt danh sách, thêm, sửa, sắp xếp và xử lí các bài toán dãy số cơ bản."),
        new(10, "Hàm và tư duy mô đun", "Thiết kế hàm, tham số, giá trị trả về, phạm vi biến và cách chia bài toán lớn thành các bước nhỏ."),
        new(10, "Xâu ký tự", "Xử lí chuỗi, chỉ số, cắt chuỗi, tìm kiếm, tách, ghép và chuẩn hóa dữ liệu văn bản."),
        new(10, "Mạng máy tính và Internet", "Khái niệm mạng máy tính, Internet, dịch vụ trực tuyến, địa chỉ mạng và nguyên tắc sử dụng tài nguyên số."),
        new(10, "Đạo đức số và bản quyền", "Quyền riêng tư, bản quyền, trích dẫn nguồn, chia sẻ thông tin có trách nhiệm và ứng xử văn minh trong môi trường số."),
        new(10, "Thiết kế đồ họa số", "Nguyên lí bố cục, màu sắc, lớp ảnh, định dạng tệp ảnh và tạo sản phẩm truyền thông số đơn giản."),

        new(11, "Hệ điều hành và phần mềm", "Vai trò của hệ điều hành, phần mềm ứng dụng, phần mềm nguồn mở, cài đặt và quản lí tài nguyên máy tính."),
        new(11, "Cơ sở dữ liệu quan hệ", "Bảng, bản ghi, trường, khóa chính, khóa ngoại, quan hệ giữa bảng và nguyên tắc giảm trùng lặp dữ liệu."),
        new(11, "SQL cơ bản", "Truy vấn dữ liệu bằng SELECT, WHERE, ORDER BY, GROUP BY và các hàm thống kê cơ bản."),
        new(11, "Kĩ thuật lập trình", "Hàm, kiểm thử, tìm kiếm, sắp xếp, đệ quy cơ bản và đánh giá độ phức tạp ở mức nhập môn."),
        new(11, "An toàn và văn hóa ứng xử trên mạng", "Nhận diện rủi ro trực tuyến, mật khẩu mạnh, xác thực nhiều lớp, lừa đảo số và chia sẻ thông tin có kiểm chứng."),
        new(11, "Chỉnh sửa ảnh và video", "Cắt ghép, lớp, bộ lọc, hiệu ứng chuyển cảnh, âm thanh và xuất bản sản phẩm đa phương tiện."),
        new(11, "Nghề quản trị cơ sở dữ liệu", "Nhiệm vụ, kĩ năng và trách nhiệm của người quản trị cơ sở dữ liệu trong tổ chức."),

        new(12, "HTML cơ bản", "Cấu trúc tài liệu HTML, thẻ ngữ nghĩa, liên kết, hình ảnh, biểu mẫu và nguyên tắc truy cập được."),
        new(12, "CSS cơ bản", "Selector, cascade, box model, màu sắc, font chữ, flexbox, grid và bố cục đáp ứng."),
        new(12, "Tạo trang web với HTML, CSS và JavaScript", "Xây dựng trang web hoàn chỉnh, tổ chức nội dung, định kiểu, tương tác cơ bản và kiểm tra trên nhiều kích thước màn hình."),
        new(12, "Bảo vệ dữ liệu và phần mềm", "Sao lưu, khôi phục, phòng chống mã độc, cập nhật phần mềm, cài đặt và gỡ bỏ phần mềm an toàn."),
        new(12, "Phân tích dữ liệu bằng bảng tính", "Làm sạch dữ liệu, hàm thống kê, biểu đồ, bảng tổng hợp và diễn giải kết quả từ dữ liệu thực tế."),
        new(12, "Học máy và khoa học dữ liệu nhập môn", "Dữ liệu, đặc trưng, nhãn, huấn luyện, kiểm thử, sai lệch dữ liệu và ứng dụng học máy ở mức phổ thông."),
        new(12, "Mô phỏng trong giải quyết vấn đề", "Xây dựng mô hình đơn giản, thay đổi tham số, quan sát kết quả và dùng mô phỏng để hỗ trợ ra quyết định."),
        new(12, "Hướng nghiệp dịch vụ và quản trị hệ thống", "Nhóm nghề dịch vụ số, quản trị hệ thống, quản trị mạng, hỗ trợ người dùng và yêu cầu đạo đức nghề nghiệp.")
    ];

    public static readonly ExerciseSeed[] Exercises =
    [
        new(
            "Python cơ bản - Nhập xuất và biểu thức",
            10,
            "Python cơ bản",
            "Easy",
            900,
            [
                Multiple("Tệp chương trình Python thường có phần mở rộng nào?", "A",
                    A("A", ".py"), A("B", ".docx"), A("C", ".xlsx"), A("D", ".png")),
                Multiple("Trong Python, hàm input() luôn trả về dữ liệu thuộc kiểu nào nếu chưa ép kiểu?", "C",
                    A("A", "int"), A("B", "float"), A("C", "str"), A("D", "bool")),
                Code("Kết quả của biểu thức int(\"12\") + 3 là gì?", "B",
                    A("A", "123"), A("B", "15"), A("C", "12 + 3"), A("D", "Lỗi vì không cộng được chuỗi với số")),
                Code("Kết quả của biểu thức 7 // 2 trong Python là gì?", "A",
                    A("A", "3"), A("B", "3.5"), A("C", "1"), A("D", "4")),
                Code("Biểu thức not (3 > 5) có giá trị nào?", "D",
                    A("A", "3"), A("B", "5"), A("C", "False"), A("D", "True"))
            ]),

        new(
            "Cấu trúc rẽ nhánh - Điều kiện và biểu thức logic",
            10,
            "Cấu trúc rẽ nhánh",
            "Medium",
            1000,
            [
                Multiple("Trong chuỗi if, elif, else, khi một điều kiện đúng thì Python sẽ xử lí như thế nào?", "B",
                    A("A", "Luôn kiểm tra tiếp tất cả điều kiện còn lại"), A("B", "Thực hiện nhánh đúng đầu tiên rồi bỏ qua các nhánh sau"), A("C", "Chỉ thực hiện nhánh else"), A("D", "Báo lỗi nếu có nhiều hơn một điều kiện")),
                Multiple("Toán tử and cho kết quả True khi nào?", "C",
                    A("A", "Ít nhất một biểu thức thành phần đúng"), A("B", "Cả hai biểu thức thành phần sai"), A("C", "Tất cả biểu thức thành phần đều đúng"), A("D", "Biểu thức bên trái đúng, bên phải sai")),
                Code("Điều kiện nào kiểm tra số n chia hết cho cả 2 và 3?", "A",
                    A("A", "n % 2 == 0 and n % 3 == 0"), A("B", "n % 2 == 0 or n % 3 == 0"), A("C", "n / 2 == 0 and n / 3 == 0"), A("D", "n // 2 == 0 and n // 3 == 0")),
                Multiple("Trong Python, phạm vi các câu lệnh thuộc cùng một khối if được xác định chủ yếu bằng yếu tố nào?", "D",
                    A("A", "Dấu ngoặc nhọn"), A("B", "Dấu chấm phẩy"), A("C", "Từ khóa end"), A("D", "Mức thụt lề")),
                Code("Cho x = 8. Đoạn lệnh if x > 10: print(\"A\") elif x >= 8: print(\"B\") else: print(\"C\") in ra gì?", "B",
                    A("A", "A"), A("B", "B"), A("C", "C"), A("D", "Không in gì"))
            ]),

        new(
            "Vòng lặp - Duyệt và xử lí dãy số",
            10,
            "Vòng lặp",
            "Medium",
            1200,
            [
                Code("range(2, 8, 2) tạo ra dãy giá trị nào khi duyệt bằng for?", "A",
                    A("A", "2, 4, 6"), A("B", "2, 4, 6, 8"), A("C", "0, 2, 4, 6"), A("D", "2, 3, 4, 5, 6, 7")),
                Multiple("Với vòng lặp while, điều kiện được kiểm tra vào thời điểm nào?", "B",
                    A("A", "Chỉ sau khi thân vòng lặp chạy xong"), A("B", "Trước mỗi lần thực hiện thân vòng lặp"), A("C", "Chỉ một lần ở cuối chương trình"), A("D", "Không cần điều kiện")),
                Multiple("Câu lệnh break có tác dụng gì trong vòng lặp?", "C",
                    A("A", "Bỏ qua lần lặp hiện tại và chuyển sang lần tiếp theo"), A("B", "Khởi động lại vòng lặp"), A("C", "Thoát ngay khỏi vòng lặp gần nhất"), A("D", "Tạm dừng chương trình vĩnh viễn")),
                Multiple("Câu lệnh continue thường được dùng để làm gì?", "A",
                    A("A", "Bỏ qua phần còn lại của lần lặp hiện tại"), A("B", "Kết thúc toàn bộ chương trình"), A("C", "Tạo biến mới"), A("D", "Sắp xếp danh sách")),
                Code("Kết quả của đoạn lệnh s = 0; for i in range(2, 7, 2): s += i; print(s) là gì?", "D",
                    A("A", "6"), A("B", "8"), A("C", "10"), A("D", "12"))
            ]),

        new(
            "Danh sách, xâu ký tự và hàm",
            10,
            "Danh sách và xử lí dãy số",
            "Hard",
            1500,
            [
                Multiple("Trong Python, phần tử đầu tiên của danh sách a được truy cập bằng biểu thức nào?", "A",
                    A("A", "a[0]"), A("B", "a[1]"), A("C", "a[first]"), A("D", "a[-0]")),
                Code("len(\"Tin hoc\") trả về giá trị nào?", "C",
                    A("A", "6"), A("B", "8"), A("C", "7"), A("D", "5")),
                Multiple("Trong hàm Python, câu lệnh nào trả kết quả về nơi gọi hàm?", "D",
                    A("A", "print"), A("B", "input"), A("C", "break"), A("D", "return")),
                Code("Cho a = [3, 1, 2]; a.sort(); print(a[0]). Kết quả là gì?", "B",
                    A("A", "3"), A("B", "1"), A("C", "2"), A("D", "[1, 2, 3]")),
                Code("Cho s = \"PYTHON\". Biểu thức s[1:4] cho kết quả nào?", "C",
                    A("A", "PYT"), A("B", "YTHO"), A("C", "YTH"), A("D", "THO"))
            ]),

        new(
            "Cơ sở dữ liệu quan hệ - Bảng, khóa và liên kết",
            11,
            "Cơ sở dữ liệu quan hệ",
            "Easy",
            1100,
            [
                Multiple("Trong bảng dữ liệu quan hệ, một hàng thường biểu diễn điều gì?", "B",
                    A("A", "Một kiểu dữ liệu"), A("B", "Một bản ghi của đối tượng"), A("C", "Một phần mềm"), A("D", "Một câu lệnh SQL")),
                Multiple("Khóa chính của một bảng cần thỏa mãn yêu cầu nào?", "A",
                    A("A", "Duy nhất và không để trống"), A("B", "Có thể trùng nhau"), A("C", "Luôn là kiểu văn bản"), A("D", "Chỉ dùng để trang trí bảng")),
                Multiple("Khóa ngoại thường dùng để làm gì?", "D",
                    A("A", "Mã hóa toàn bộ cơ sở dữ liệu"), A("B", "Xóa bảng nhanh hơn"), A("C", "Thay thế tất cả khóa chính"), A("D", "Liên kết bản ghi ở bảng này với bản ghi ở bảng khác")),
                Multiple("Thiết kế cơ sở dữ liệu tốt giúp giảm vấn đề nào sau đây?", "C",
                    A("A", "Dung lượng màn hình"), A("B", "Tốc độ gõ phím"), A("C", "Trùng lặp và mâu thuẫn dữ liệu"), A("D", "Số lượng người dùng Internet")),
                Multiple("Quan hệ nhiều - nhiều giữa Học sinh và Câu lạc bộ thường được biểu diễn bằng cách nào?", "B",
                    A("A", "Chỉ dùng một bảng Học sinh"), A("B", "Thêm bảng trung gian chứa khóa của hai bảng"), A("C", "Ghi tất cả câu lạc bộ vào một ô"), A("D", "Bỏ khóa chính của hai bảng"))
            ]),

        new(
            "SQL cơ bản - Truy vấn dữ liệu",
            11,
            "SQL cơ bản",
            "Medium",
            1300,
            [
                Multiple("Cấu trúc cơ bản để lấy dữ liệu từ một bảng là gì?", "A",
                    A("A", "SELECT ... FROM ..."), A("B", "GET ... TABLE ..."), A("C", "OPEN ... READ ..."), A("D", "PRINT ... WHERE ...")),
                Multiple("Mệnh đề WHERE trong SQL dùng để làm gì?", "C",
                    A("A", "Đổi tên bảng"), A("B", "Sắp xếp kết quả"), A("C", "Lọc các bản ghi thỏa điều kiện"), A("D", "Tạo khóa chính")),
                Multiple("ORDER BY Diem DESC có ý nghĩa gì?", "B",
                    A("A", "Sắp xếp điểm tăng dần"), A("B", "Sắp xếp điểm giảm dần"), A("C", "Chỉ lấy điểm khác rỗng"), A("D", "Đếm số điểm")),
                Multiple("Hàm COUNT(*) thường dùng để làm gì?", "D",
                    A("A", "Tính trung bình"), A("B", "Tìm giá trị lớn nhất"), A("C", "Ghép chuỗi"), A("D", "Đếm số bản ghi")),
                Multiple("Mệnh đề GROUP BY thường đi kèm với nhóm hàm nào?", "A",
                    A("A", "Hàm tổng hợp như COUNT, SUM, AVG"), A("B", "Hàm nhập dữ liệu từ bàn phím"), A("C", "Hàm vẽ ảnh"), A("D", "Hàm đổi màu chữ"))
            ]),

        new(
            "Kĩ thuật lập trình - Hàm, kiểm thử và độ phức tạp",
            11,
            "Kĩ thuật lập trình",
            "Hard",
            1500,
            [
                Multiple("Lợi ích chính của việc tách chương trình thành các hàm là gì?", "A",
                    A("A", "Giảm lặp mã và dễ kiểm thử từng phần"), A("B", "Làm chương trình luôn chạy nhanh hơn mọi trường hợp"), A("C", "Không cần đặt tên biến"), A("D", "Bỏ qua bước thiết kế thuật toán")),
                Multiple("Khi kiểm thử hàm tính điểm trung bình, trường hợp nào là dữ liệu biên nên kiểm tra?", "D",
                    A("A", "Tên học sinh dài"), A("B", "Màu giao diện"), A("C", "Thứ tự nhập tên lớp"), A("D", "Điểm bằng 0 hoặc bằng 10")),
                Multiple("Tìm kiếm nhị phân chỉ áp dụng trực tiếp hiệu quả khi dữ liệu có tính chất nào?", "B",
                    A("A", "Dữ liệu được mã hóa"), A("B", "Dữ liệu đã được sắp xếp"), A("C", "Dữ liệu chỉ gồm chữ cái"), A("D", "Dữ liệu có đúng 10 phần tử")),
                Multiple("Thuật toán duyệt tuần tự một danh sách n phần tử thường có độ phức tạp thời gian nào?", "C",
                    A("A", "O(1)"), A("B", "O(log n)"), A("C", "O(n)"), A("D", "O(n²)")),
                Multiple("Trong đệ quy, điều kiện dừng có vai trò gì?", "A",
                    A("A", "Ngăn lời gọi lặp vô hạn và xác định bài toán cơ sở"), A("B", "Tăng số lần gọi hàm"), A("C", "Xóa toàn bộ dữ liệu"), A("D", "Thay thế mọi vòng lặp"))
            ]),

        new(
            "An toàn và văn hóa ứng xử trên mạng",
            11,
            "An toàn và văn hóa ứng xử trên mạng",
            "Medium",
            1000,
            [
                Multiple("Phishing là hình thức tấn công phổ biến nào?", "B",
                    A("A", "Tăng tốc độ mạng"), A("B", "Giả mạo để lừa người dùng cung cấp thông tin"), A("C", "Nén dữ liệu hợp pháp"), A("D", "Cập nhật hệ điều hành")),
                Multiple("Mật khẩu mạnh nên có đặc điểm nào?", "A",
                    A("A", "Dài, khó đoán, không dùng lại ở nhiều dịch vụ"), A("B", "Trùng với ngày sinh để dễ nhớ"), A("C", "Chỉ gồm một chữ cái"), A("D", "Chia sẻ cho bạn cùng lớp để dự phòng")),
                Multiple("Khi chia sẻ thông tin học tập lấy từ Internet, việc làm nào đúng?", "D",
                    A("A", "Xóa tên tác giả để bài ngắn hơn"), A("B", "Đăng lại nguyên văn mà không kiểm tra"), A("C", "Chỉ chọn nguồn có tiêu đề gây sốc"), A("D", "Kiểm chứng nội dung và ghi nguồn phù hợp")),
                Multiple("Xác thực hai yếu tố giúp tăng an toàn tài khoản vì sao?", "C",
                    A("A", "Làm mật khẩu ngắn hơn"), A("B", "Cho phép bỏ qua đăng nhập"), A("C", "Yêu cầu thêm một bằng chứng ngoài mật khẩu"), A("D", "Tự động công khai dữ liệu")),
                Multiple("Khi dùng Wi-Fi công cộng, lựa chọn nào an toàn hơn?", "A",
                    A("A", "Tránh đăng nhập dịch vụ nhạy cảm nếu kết nối không bảo mật"), A("B", "Tắt mọi cập nhật bảo mật"), A("C", "Gửi mật khẩu qua tin nhắn nhóm"), A("D", "Bỏ qua cảnh báo của trình duyệt"))
            ]),

        new(
            "Tạo trang web - HTML ngữ nghĩa",
            12,
            "HTML cơ bản",
            "Easy",
            1000,
            [
                Multiple("Thẻ HTML nào thường dùng để đánh dấu phần đầu trang hoặc đầu một khu vực nội dung?", "A",
                    A("A", "header"), A("B", "random"), A("C", "paint"), A("D", "data-only")),
                Multiple("Thuộc tính alt của thẻ img có ý nghĩa gì?", "C",
                    A("A", "Đổi màu ảnh"), A("B", "Tạo đường viền ảnh"), A("C", "Cung cấp văn bản thay thế cho ảnh"), A("D", "Tự động nén ảnh")),
                Multiple("Thuộc tính href thường xuất hiện trong thẻ nào để tạo liên kết?", "B",
                    A("A", "p"), A("B", "a"), A("C", "table"), A("D", "body")),
                Multiple("Để nhập địa chỉ email trong biểu mẫu, input type nào phù hợp nhất?", "D",
                    A("A", "type=\"number\""), A("B", "type=\"color\""), A("C", "type=\"range\""), A("D", "type=\"email\"")),
                Multiple("Cách dùng tiêu đề nào hợp lí hơn cho một trang web?", "A",
                    A("A", "Dùng h1 cho tiêu đề chính, h2/h3 cho các mục con"), A("B", "Dùng nhiều h1 liên tiếp chỉ để làm chữ to"), A("C", "Không dùng thẻ tiêu đề"), A("D", "Dùng h6 cho mọi tiêu đề chính"))
            ]),

        new(
            "CSS và bố cục đáp ứng",
            12,
            "CSS cơ bản",
            "Medium",
            1200,
            [
                Multiple("Flexbox phù hợp nhất để xử lí loại bố cục nào?", "B",
                    A("A", "Mô hình cơ sở dữ liệu"), A("B", "Bố cục một chiều theo hàng hoặc cột"), A("C", "Huấn luyện mô hình học máy"), A("D", "Truy vấn SQL")),
                Multiple("Media query trong CSS thường dùng để làm gì?", "A",
                    A("A", "Điều chỉnh giao diện theo kích thước màn hình hoặc điều kiện thiết bị"), A("B", "Tạo bảng dữ liệu"), A("C", "Lưu mật khẩu"), A("D", "Biên dịch Python")),
                Multiple("Với box-sizing: border-box, width của phần tử bao gồm những phần nào?", "D",
                    A("A", "Chỉ nội dung"), A("B", "Chỉ margin"), A("C", "Nội dung và margin"), A("D", "Nội dung, padding và border")),
                Multiple("Đơn vị rem trong CSS phụ thuộc chủ yếu vào yếu tố nào?", "C",
                    A("A", "Chiều rộng ảnh gần nhất"), A("B", "Số dòng trong bảng"), A("C", "Kích thước font của phần tử gốc"), A("D", "Tốc độ mạng")),
                Multiple("Selector nào thường có độ ưu tiên cao hơn?", "B",
                    A("A", "p"), A("B", ".note.warning"), A("C", "*"), A("D", "body p khi chỉ xét một class đơn"))
            ]),

        new(
            "Bảo vệ dữ liệu và phân tích bảng tính",
            12,
            "Bảo vệ dữ liệu và phần mềm",
            "Medium",
            1300,
            [
                Multiple("Quy tắc sao lưu 3-2-1 thường được hiểu đúng là gì?", "A",
                    A("A", "3 bản dữ liệu, trên 2 loại phương tiện, có 1 bản ở vị trí khác"), A("B", "3 mật khẩu, 2 tài khoản, 1 trình duyệt"), A("C", "3 lần đăng nhập, 2 email, 1 máy in"), A("D", "3 tệp giống nhau trong cùng một thư mục")),
                Multiple("Phát biểu nào đúng về phần mềm diệt mã độc?", "C",
                    A("A", "Cài xong là an toàn tuyệt đối mãi mãi"), A("B", "Không cần cập nhật cơ sở dữ liệu nhận diện"), A("C", "Cần cập nhật và kết hợp với thói quen sử dụng an toàn"), A("D", "Có thể thay thế hoàn toàn việc sao lưu")),
                Multiple("Trong bảng tính, hàm nào thường dùng để tính trung bình cộng?", "B",
                    A("A", "COUNT"), A("B", "AVERAGE"), A("C", "MAX"), A("D", "TEXT")),
                Multiple("Công thức COUNTIF(A1:A10, \">=8\") dùng để làm gì?", "D",
                    A("A", "Tính tổng các ô từ A1 đến A10"), A("B", "Tìm giá trị nhỏ nhất"), A("C", "Xóa các ô nhỏ hơn 8"), A("D", "Đếm số ô trong A1:A10 có giá trị từ 8 trở lên")),
                Multiple("Loại biểu đồ nào phù hợp để thể hiện xu hướng điểm trung bình theo thời gian?", "A",
                    A("A", "Biểu đồ đường"), A("B", "Biểu đồ tròn"), A("C", "Sơ đồ cây thư mục"), A("D", "Bảng khóa chính"))
            ]),

        new(
            "Học máy, dữ liệu và mô phỏng nhập môn",
            12,
            "Học máy và khoa học dữ liệu nhập môn",
            "Hard",
            1500,
            [
                Multiple("Học có giám sát thường cần loại dữ liệu nào?", "B",
                    A("A", "Dữ liệu không có bất kì mô tả nào"), A("B", "Dữ liệu có nhãn hoặc kết quả đúng để học"), A("C", "Chỉ dữ liệu âm thanh"), A("D", "Chỉ dữ liệu đã bị xóa")),
                Multiple("Trong khoa học dữ liệu, đặc trưng (feature) là gì?", "A",
                    A("A", "Thuộc tính có thể đo hoặc trích xuất từ đối tượng dữ liệu"), A("B", "Mật khẩu của người dùng"), A("C", "Tên phần mềm bảng tính"), A("D", "Lỗi chính tả trong dữ liệu")),
                Multiple("Vì sao thường tách dữ liệu thành tập huấn luyện và tập kiểm thử?", "D",
                    A("A", "Để xóa bớt dữ liệu"), A("B", "Để làm mô hình luôn đúng tuyệt đối"), A("C", "Để tránh phải đánh giá mô hình"), A("D", "Để ước lượng khả năng khái quát trên dữ liệu chưa dùng để huấn luyện")),
                Multiple("Mô phỏng trong giải quyết vấn đề có mục đích chính nào?", "C",
                    A("A", "Thay thế mọi dữ liệu thực tế"), A("B", "Tạo mật khẩu mới"), A("C", "Dùng mô hình đơn giản để thử nghiệm và quan sát hành vi của hệ thống"), A("D", "Bắt buộc phải dùng robot")),
                Multiple("Nếu dữ liệu huấn luyện bị thiên lệch, mô hình học máy có thể gặp vấn đề gì?", "A",
                    A("A", "Kết quả dự đoán cũng có thể thiên lệch"), A("B", "Mô hình tự sửa mọi lỗi đạo đức"), A("C", "Tốc độ mạng tăng lên"), A("D", "Không cần kiểm thử nữa"))
            ])
    ];

    public static readonly ExamSeed[] Exams =
    [
        new("Đề luyện giữa kỳ I - Tin học 10", 10, 35, 10, ["Python cơ bản - Nhập xuất và biểu thức", "Cấu trúc rẽ nhánh - Điều kiện và biểu thức logic"]),
        new("Đề tổng hợp lập trình cơ bản - Tin học 10", 10, 45, 20, ["Python cơ bản - Nhập xuất và biểu thức", "Cấu trúc rẽ nhánh - Điều kiện và biểu thức logic", "Vòng lặp - Duyệt và xử lí dãy số", "Danh sách, xâu ký tự và hàm"]),
        new("Đề luyện CSDL và SQL - Tin học 11", 11, 35, 10, ["Cơ sở dữ liệu quan hệ - Bảng, khóa và liên kết", "SQL cơ bản - Truy vấn dữ liệu"]),
        new("Đề tổng hợp Tin học 11", 11, 45, 20, ["Cơ sở dữ liệu quan hệ - Bảng, khóa và liên kết", "SQL cơ bản - Truy vấn dữ liệu", "Kĩ thuật lập trình - Hàm, kiểm thử và độ phức tạp", "An toàn và văn hóa ứng xử trên mạng"]),
        new("Đề luyện Web và dữ liệu - Tin học 12", 12, 40, 15, ["Tạo trang web - HTML ngữ nghĩa", "CSS và bố cục đáp ứng", "Bảo vệ dữ liệu và phân tích bảng tính"]),
        new("Đề tổng hợp Tin học 12", 12, 45, 20, ["Tạo trang web - HTML ngữ nghĩa", "CSS và bố cục đáp ứng", "Bảo vệ dữ liệu và phân tích bảng tính", "Học máy, dữ liệu và mô phỏng nhập môn"])
    ];

    private static QuestionSeed Multiple(string content, string correctLabel, params AnswerSeed[] answers) =>
        new("MultipleChoice", content, "Medium", correctLabel, answers);

    private static QuestionSeed Code(string content, string correctLabel, params AnswerSeed[] answers) =>
        new("Code", content, "Medium", correctLabel, answers);

    private static AnswerSeed A(string label, string content) => new(label, content);
}

internal sealed record TopicSeed(int Grade, string Name, string Description);

internal sealed record ExerciseSeed(
    string Title,
    int Grade,
    string TopicName,
    string Difficulty,
    int TimeLimit,
    QuestionSeed[] Questions);

internal sealed record QuestionSeed(
    string Type,
    string Content,
    string Difficulty,
    string CorrectLabel,
    AnswerSeed[] Answers);

internal sealed record AnswerSeed(string Label, string Content);

internal sealed record ExamSeed(
    string Title,
    int Grade,
    int Duration,
    int QuestionCount,
    string[] ExerciseTitles);
