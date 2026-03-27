using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formMain : Form
    {
        public formMain()
        {
            InitializeComponent();
        }

        private void dangXuatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formDangNhap f = new formDangNhap();
            f.Show();
            this.Close();
        }

        private void thoatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            // 1. Phân quyền Menu dựa vào nhóm quyền (mGroup)
            if (Program.mGroup == "SINHVIEN")
            {
                // Sinh viên
                menuDanhMuc.Visible = false;
                menuHeThong.Visible = true;
                menuNghiepVu.Visible = true; // Nơi chứa nút Bắt đầu thi
                menuBaoCao.Visible = true;   // Nơi xem lại bài thi
            }
            else if (Program.mGroup == "GIANGVIEN")
            {
                // Giảng viên được thao tác nhiều hơn
                menuDanhMuc.Visible = true;  // Nơi chứa nút Cập nhật Bộ đề
                menuHeThong.Visible = true;
                menuNghiepVu.Visible = true; // Thi thử, Đăng ký thi
                menuBaoCao.Visible = true;   // In bảng điểm
            }
            else if (Program.mGroup == "PGV")
            {
                // PGV có toàn quyền
                menuDanhMuc.Visible = true;
                menuHeThong.Visible = true;
                menuNghiepVu.Visible = true;
                menuBaoCao.Visible = true;
            }

            // 2. Hiển thị thông tin người đăng nhập lên tiêu đề của Form
            this.Text = $"HỆ THỐNG THI TRẮC NGHIỆM - Đang đăng nhập: {Program.mHoTen} ({Program.mGroup})";
        }

        private void mônHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formMonHoc f = new formMonHoc();
            f.ShowDialog(); // Mở form Môn học lên
        }
    }
}
