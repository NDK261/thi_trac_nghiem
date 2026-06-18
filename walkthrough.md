# Báo Cáo Tổng Kết: Chức Năng In Báo Cáo (RDLC)

Dưới đây là tổng hợp toàn bộ các công việc tôi đã thực hiện để hoàn thiện chức năng **In Báo Cáo** cho đồ án, đảm bảo tiêu chí "Chuẩn bài đồ án trường đại học", dễ cài đặt và giao diện chuyên nghiệp.

## 1. Quyết định công nghệ
- **Lựa chọn:** Thay vì dùng DevExpress nặng nề và phức tạp về bản quyền, tôi đã chọn **Microsoft ReportViewer (RDLC)**. 
- **Ưu điểm mang lại:** Nhẹ, tích hợp sẵn vào file `.exe` khi biên dịch, chuẩn form mẫu đại học, không đòi hỏi cài đặt thêm tool bên ngoài.

## 2. Thiết kế Cơ sở hạ tầng (Infrastructure)
- **Tích hợp NuGet:** Đã cài đặt thư viện `Microsoft.ReportingServices.ReportViewerControl.Winforms` (v150) và `Microsoft.SqlServer.Types`.
- **Tạo `ReportDataSet.xsd`:** Xây dựng tập dữ liệu (DataSet) trung gian kết nối trực tiếp với 2 Stored Procedures `SP_GET_BANGDIEM` và `SP_XEM_KETQUA`.
- **Tạo `formReportViewer`:** Xây dựng một Form dùng chung (Generic Form) có khả năng nhận linh hoạt bất kỳ Data Source nào và load file `.rdlc` tương ứng. Tối ưu code tái sử dụng.

## 3. Thiết kế hai mẫu Báo cáo (RDLC)
Tôi đã viết code XML để căn chỉnh giao diện báo cáo chuyên nghiệp nhất:

### 📄 Bảng Điểm Môn Học (`rptBangDiem.rdlc`)
- **Tiêu đề:** "BẢNG ĐIỂM MÔN HỌC" - Font Times New Roman, 18pt, in đậm, màu Xanh Đen.
- **Cấu trúc:** Gồm các cột STT, Mã Sinh Viên, Họ Tên, Điểm, Điểm Chữ.
- **Hiệu ứng:** 
  - Kẻ khung (Borders) toàn bộ bảng.
  - Header đổ nền màu xanh thanh lịch, chữ trắng.
  - Căn lề chuẩn mực (Họ tên canh trái, Điểm canh giữa).

### 📄 Kết Quả Thi Trắc Nghiệm Chi Tiết (`rptXemKetQua.rdlc`)
- **Tiêu đề:** "KẾT QUẢ THI TRẮC NGHIỆM CHI TIẾT" - Màu Đỏ sẫm, 16pt.
- **Thông tin sinh viên:** Nhúng trực tiếp các Parameters (Lớp, Họ tên, Môn, Ngày thi, Lần thi) hiển thị ngay dưới tiêu đề dạng in nghiêng trang trọng.
- **Cấu trúc:** Gồm các cột STT, Câu, Nội Dung, Đáp Án, Trả Lời.
- **Tính năng đặc biệt (Conditional Formatting):** 
  - Tự động so sánh Đáp án và Câu trả lời của sinh viên.
  - Nếu đúng: Câu trả lời in **Màu Xanh Lá**.
  - Nếu sai: Câu trả lời in **Màu Đỏ**.

## 4. Tích hợp vào các Form Nghiệp Vụ
- **Tại `formBangDiem`:** Bắt sự kiện nút "In Báo Cáo", gọi `SP_GET_BANGDIEM`, truyền tham số Mã Lớp, Mã Môn Học, Lần Thi và gọi `formReportViewer` hiển thị.
- **Tại `formXemKetQua`:** Bắt sự kiện nút "In Báo Cáo", truyền các thông tin sinh viên bằng mảng tham số (`ReportParameter`) và nạp dữ liệu chi tiết bài thi để render báo cáo.

## 5. Xử lý Lỗi (Troubleshooting)
- **Sửa lỗi Encoding:** Khắc phục triệt để lỗi "Invalid character in the given encoding" (gây sập ReportViewer do tiếng Việt có dấu sinh tự động) bằng cách viết lại file `.rdlc` theo chuẩn `UTF-8` gốc.
- **Xử lý cảnh báo PInvokeStackImbalance:** Giải quyết cảnh báo giả (false-positive) từ công cụ gỡ lỗi MDA của Visual Studio khi Render Font mà không làm ảnh hưởng đến mã nguồn chạy thực tế.
- **Đồng bộ nhánh `main`:** Hợp nhất thành công toàn bộ code báo cáo chung với bản cập nhật chức năng thi thử của Đạt và chức năng phân quyền của Khánh.

> [!TIP]
> Giao diện đã được thiết kế bằng font `Times New Roman` để khi in ra giấy A4 hoặc xuất file PDF nộp đồ án cho giảng viên sẽ đạt được sự chuyên nghiệp, chuẩn mực và đúng form văn bản nhà nước.
