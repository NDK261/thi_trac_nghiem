using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    internal static class Program
    {
        // --- CÁC BIẾN TOÀN CỤC ---
        public static string connStr = ""; // Chuỗi kết nối động theo user đăng nhập
        public static string mGroup = "";  // Nhóm quyền: PGV, GIANGVIEN, SINHVIEN
        public static string mHoTen = "";  // Họ tên người đăng nhập
        public static string mUserName = ""; // Mã GV hoặc Mã SV
        public static string mLogin = "";  // Tên đăng nhập

        // Lấy chuỗi Server từ DBHelper (cần chỉnh lại tên Server của máy em nếu cần)
        public static string serverName = @"localhost\SQLEXPRESS";
        public static string dbName = "THITRACNGHIEM";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Khởi chạy Form Đăng Nhập đầu tiên
            Application.Run(new formDangNhap());
        }
    }
}
