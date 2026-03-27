using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formDangNhap : Form
    {
        public formDangNhap()
        {
            InitializeComponent();
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            if (txtTenDangNhap.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập tài khoản!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenDangNhap.Focus();
                return;
            }

            // 1. Dựng chuỗi kết nối
            if (rdoGiangVien.Checked)
            {
                // LẤY ĐÚNG TÊN TÀI KHOẢN TỪ Ô TEXTBOX
                Program.mLogin = txtTenDangNhap.Text.Trim();
                string pass = txtMatKhau.Text.Trim();
                Program.connStr = $"Server={Program.serverName};Database={Program.dbName};User Id={Program.mLogin};Password={pass};";
            }
            else // Sinh viên
            {
                Program.mLogin = "sv";
                Program.connStr = $"Server={Program.serverName};Database={Program.dbName};User Id=sv;Password=123;";
            }

            // 2. Kết nối CSDL và gọi SP
            try
            {
                using (SqlConnection conn = new SqlConnection(Program.connStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        if (rdoGiangVien.Checked)
                        {
                            cmd.CommandText = "SP_DANGNHAP";
                            // Truyền tham số GV01 xuống SQL Server
                            cmd.Parameters.AddWithValue("@TENLOGIN", txtTenDangNhap.Text.Trim());
                        }
                        else
                        {
                            cmd.CommandText = "SP_DANGNHAP_SINHVIEN";
                            // Truyền tham số Mã SV xuống SQL Server
                            cmd.Parameters.AddWithValue("@MASV", txtTenDangNhap.Text.Trim());
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Program.mUserName = reader["USERNAME"].ToString();
                                Program.mHoTen = reader["HOTEN"].ToString();
                                Program.mGroup = reader["TENNHOM"].ToString();

                                MessageBox.Show($"Đăng nhập thành công!\nXin chào {Program.mGroup}: {Program.mHoTen}", "Thông báo");

                                formMain fMain = new formMain();
                                this.Hide();
                                fMain.ShowDialog();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Tài khoản không tồn tại trong CSDL!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối hoặc sai mật khẩu!\nChi tiết: " + ex.Message, "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void rdoSinhVien_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSinhVien.Checked) // Khi tích chọn Sinh Viên
            {
                txtMatKhau.Enabled = false; // Khóa ô nhập mật khẩu
            }
        }

        private void rdoGiangVien_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoGiangVien.Checked) // Khi tích chọn Giảng viên
            {
                txtMatKhau.Enabled = true; // Mở lại ô nhập mật khẩu
            }
        }
    }
}
