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
            this.Load += formDangNhap_Load;
            this.Shown += formDangNhap_Shown;
        }

        private void formDangNhap_Shown(object sender, EventArgs e)
        {
            txtTenDangNhap.Focus();
        }

        private void formDangNhap_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AcceptButton = btnDangNhap;
            this.BackColor = Color.White;

            Panel pnlCenter = new Panel();
            pnlCenter.Size = new Size(500, 350);
            pnlCenter.Location = new Point((this.ClientSize.Width - pnlCenter.Width) / 2, (this.ClientSize.Height - pnlCenter.Height) / 2);
            pnlCenter.Anchor = AnchorStyles.None;
            pnlCenter.BorderStyle = BorderStyle.FixedSingle;
            pnlCenter.BackColor = Color.WhiteSmoke;

            Label lblTitle = new Label();
            lblTitle.Text = "ĐĂNG NHẬP HỆ THỐNG";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.DarkBlue;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(120, 20);
            pnlCenter.Controls.Add(lblTitle);

            Font f = new Font("Segoe UI", 11F);

            pnlCenter.Controls.Add(labelTenDangNhap);
            pnlCenter.Controls.Add(txtTenDangNhap);
            pnlCenter.Controls.Add(labelMatKhau);
            pnlCenter.Controls.Add(txtMatKhau);
            pnlCenter.Controls.Add(rdoGiangVien);
            pnlCenter.Controls.Add(rdoSinhVien);
            pnlCenter.Controls.Add(btnDangNhap);
            pnlCenter.Controls.Add(btnThoat);

            this.Controls.Add(pnlCenter);

            int startY = 80;
            labelTenDangNhap.Text = "Tên Đăng Nhập";
            labelTenDangNhap.Font = f;
            labelTenDangNhap.Location = new Point(40, startY);
            labelTenDangNhap.AutoSize = true;

            txtTenDangNhap.Font = f;
            txtTenDangNhap.Location = new Point(180, startY - 3);
            txtTenDangNhap.Size = new Size(220, 30);

            labelMatKhau.Text = "Mật Khẩu";
            labelMatKhau.Font = f;
            labelMatKhau.Location = new Point(40, startY + 50);
            labelMatKhau.AutoSize = true;

            txtMatKhau.Font = f;
            txtMatKhau.Location = new Point(180, startY + 47);
            txtMatKhau.Size = new Size(220, 30);
            txtMatKhau.UseSystemPasswordChar = true;

            CheckBox chkShowPass = new CheckBox();
            chkShowPass.Appearance = Appearance.Button;
            chkShowPass.FlatStyle = FlatStyle.Flat;
            chkShowPass.Text = "👁"; 
            chkShowPass.Font = new Font("Segoe UI", 10F);
            chkShowPass.Size = new Size(35, 30);
            chkShowPass.Location = new Point(410, startY + 46);
            chkShowPass.TextAlign = ContentAlignment.MiddleCenter;
            chkShowPass.Cursor = Cursors.Hand;
            chkShowPass.FlatAppearance.BorderSize = 0;
            chkShowPass.CheckedChanged += (s, ev) => { txtMatKhau.UseSystemPasswordChar = !chkShowPass.Checked; };
            pnlCenter.Controls.Add(chkShowPass);

            rdoGiangVien.Font = f;
            rdoGiangVien.Location = new Point(100, startY + 110);
            rdoGiangVien.AutoSize = true;

            rdoSinhVien.Font = f;
            rdoSinhVien.Location = new Point(280, startY + 110);
            rdoSinhVien.AutoSize = true;

            btnDangNhap.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnDangNhap.BackColor = Color.SteelBlue;
            btnDangNhap.ForeColor = Color.White;
            btnDangNhap.FlatStyle = FlatStyle.Flat;
            btnDangNhap.Location = new Point(100, startY + 170);
            btnDangNhap.Size = new Size(130, 45);
            btnDangNhap.Cursor = Cursors.Hand;

            btnThoat.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnThoat.BackColor = Color.IndianRed;
            btnThoat.ForeColor = Color.White;
            btnThoat.FlatStyle = FlatStyle.Flat;
            btnThoat.Location = new Point(260, startY + 170);
            btnThoat.Size = new Size(130, 45);
            btnThoat.Cursor = Cursors.Hand;

            txtTenDangNhap.TabIndex = 0;
            txtMatKhau.TabIndex = 1;
            chkShowPass.TabIndex = 2;
            rdoGiangVien.TabIndex = 3;
            rdoSinhVien.TabIndex = 4;
            btnDangNhap.TabIndex = 5;
            btnThoat.TabIndex = 6;

            txtTenDangNhap.KeyDown += (s, ev) => {
                if (ev.KeyCode == Keys.Enter || ev.KeyCode == Keys.Down) {
                    ev.Handled = true;
                    ev.SuppressKeyPress = true;
                    txtMatKhau.Focus();
                }
            };

            txtMatKhau.KeyDown += (s, ev) => {
                if (ev.KeyCode == Keys.Enter) {
                    ev.Handled = true;
                    ev.SuppressKeyPress = true;
                    btnDangNhap.PerformClick();
                }
                else if (ev.KeyCode == Keys.Up) {
                    ev.Handled = true;
                    ev.SuppressKeyPress = true;
                    txtTenDangNhap.Focus();
                }
            };
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
                // GIẢNG VIÊN: Kiểm tra mật khẩu không rỗng
                if (txtMatKhau.Text.Trim() == "")
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMatKhau.Focus();
                    return;
                }

                // LẤY ĐÚNG TÊN TÀI KHOẢN TỪ Ô TEXTBOX
                Program.mLogin = txtTenDangNhap.Text.Trim();
                string pass = txtMatKhau.Text.Trim();
                Program.connStr = Program.BuildConnectionString(Program.mLogin, pass);
            }
            else // Sinh viên
            {
                Program.mLogin = "sv";
                Program.connStr = Program.BuildConnectionString("sv", "123");
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
                labelTenDangNhap.Text = "      Mã SV"; // Đổi label
                txtMatKhau.Text = "123"; // Hiển thị mật khẩu cố định
                txtMatKhau.ReadOnly = true; // Không cho phép chỉnh sửa, nhưng vẫn hiển thị ***
                txtMatKhau.Enabled = true; // Để hiển thị được
            }
        }

        private void rdoGiangVien_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoGiangVien.Checked) // Khi tích chọn Giảng viên
            {
                labelTenDangNhap.Text = "Tên Đăng Nhập"; // Đổi lại label
                txtMatKhau.Clear(); // Xóa trắng mật khẩu
                txtMatKhau.ReadOnly = false; // Cho phép chỉnh sửa
                txtMatKhau.Enabled = true; // Mở lại ô nhập mật khẩu
            }
        }
    }
}
