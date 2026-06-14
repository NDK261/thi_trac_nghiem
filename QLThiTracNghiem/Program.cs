using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        // Cấu hình SQL Server đang dùng trong SSMS.
        public static string serverName = @".\SQLEXPRESS";
        public static string dbName = "THITRACNGHIEM";
        public static string adminLogin = "sa";
        public static string adminPassword = "123";

        public static string BuildConnectionString(string userId, string password)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = dbName,
                UserID = userId,
                Password = password,
                Encrypt = true,
                TrustServerCertificate = true,
                ConnectTimeout = 15
            };

            return builder.ConnectionString;
        }

        public static string GetDefaultConnectionString()
        {
            return BuildConnectionString(adminLogin, adminPassword);
        }

        public static string GetActiveConnectionString()
        {
            return string.IsNullOrWhiteSpace(connStr) ? GetDefaultConnectionString() : connStr;
        }

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
