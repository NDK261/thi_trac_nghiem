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
    public partial class formDangNhap : Form
    {
        public formDangNhap()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            string taiKhoan = txtTaiKhoan.Text.Trim();
            string matKhau = txtMatKhau.Text.Trim();

            if (taiKhoan == "")
            {
                MessageBox.Show("Nhập tên đăng nhập");
                txtTaiKhoan.Focus();
                return;
            }

            if (matKhau == "")
            {
                MessageBox.Show("Nhập mật khẩu");
                txtMatKhau.Focus();
                return;
            }

            // Tạm thời đăng nhập giả để dựng khung chương trình
            formMain f = new formMain();
            f.Show();
            this.Hide();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Application.Exit();  
}
    }
}
