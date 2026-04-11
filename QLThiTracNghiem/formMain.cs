using System;
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
            DialogResult dr = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất khỏi tài khoản này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                Application.Restart();
            }
        }

        private void thoatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            string nhom = (Program.mGroup ?? "").Trim().ToUpper();

            // Menu cha vẫn hiện hết
            menuHeThong.Visible = true;
            menuDanhMuc.Visible = true;
            menuNghiepVu.Visible = true;
            menuBaoCao.Visible = true;

            // Menu cha vẫn bấm mở xuống được
            menuHeThong.Enabled = true;
            menuDanhMuc.Enabled = true;
            menuNghiepVu.Enabled = true;
            menuBaoCao.Enabled = true;

            // Khóa hết menu con trước
            tạoTàiKhoảnToolStripMenuItem.Enabled = false;

            mônHọcToolStripMenuItem.Enabled = false;
            giáoViênToolStripMenuItem.Enabled = false;
            lớpToolStripMenuItem.Enabled = false;
            sinhViênToolStripMenuItem.Enabled = false;

            bộĐềToolStripMenuItem.Enabled = false;
            đăngKíThiToolStripMenuItem.Enabled = false;
            thiToolStripMenuItem.Enabled = false;

            xemKetQuaToolStripMenuItem.Enabled = false;
            bảngĐiểmToolStripMenuItem.Enabled = false;

            if (nhom == "PGV")
            {
                tạoTàiKhoảnToolStripMenuItem.Enabled = true;

                mônHọcToolStripMenuItem.Enabled = true;
                giáoViênToolStripMenuItem.Enabled = true;
                lớpToolStripMenuItem.Enabled = true;
                sinhViênToolStripMenuItem.Enabled = true;

                bộĐềToolStripMenuItem.Enabled = true;
                đăngKíThiToolStripMenuItem.Enabled = true;

                xemKetQuaToolStripMenuItem.Enabled = true;
                bảngĐiểmToolStripMenuItem.Enabled = true;
            }
            else if (nhom == "GIANGVIEN")
            {
                bộĐềToolStripMenuItem.Enabled = true;
                đăngKíThiToolStripMenuItem.Enabled = true;
                xemKetQuaToolStripMenuItem.Enabled = true;
                bảngĐiểmToolStripMenuItem.Enabled = true;
            }
            else if (nhom == "SINHVIEN")
            {
                thiToolStripMenuItem.Enabled = true;
                xemKetQuaToolStripMenuItem.Enabled = true;
            }

            this.Text = $"HỆ THỐNG THI TRẮC NGHIỆM - Đang đăng nhập: {Program.mHoTen} ({nhom})";
        }

        private void mônHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formMonHoc f = new formMonHoc();
            f.ShowDialog();
        }

        private void tạoTàiKhoảnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formTaoTaiKhoan f = new formTaoTaiKhoan();
            f.ShowDialog();
        }

        private void giáoviênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formGiaoVien f = new formGiaoVien();
            f.ShowDialog();
        }

        private void lớpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formLop f = new formLop();
            f.ShowDialog();
        }

        private void xemKetQuaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formXemKetQua f = new formXemKetQua();
            f.ShowDialog();
        }

        private void sinhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formLopSinhVien f = new formLopSinhVien();
            f.ShowDialog();
        }

        private void bộĐềToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formBoDe f = new formBoDe();
            f.ShowDialog();
        }

        private void đăngKíThiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formDangKyThi f = new formDangKyThi();
            f.ShowDialog();
        }

        private void thiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formThi f = new formThi();
            f.ShowDialog();
        }

        private void bảngĐiểmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formBangDiem f = new formBangDiem();
            f.ShowDialog();
        }
    }
}