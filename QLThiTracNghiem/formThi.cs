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
    public partial class formThi : Form
    {
        // Lưu tạm một câu hỏi trong lúc sinh viên làm bài.
        public class CauHoiThi
        {
            public int MaCauHoi { get; set; }
            public string NoiDung { get; set; }
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
            public string D { get; set; }
            public string DapAnDung { get; set; }
            public string DapAnDaChon { get; set; } // Đáp án sinh viên đã chọn trên form.
        }

        // Các biến dùng lại khi bắt đầu thi, chuyển câu và nộp bài.
        List<CauHoiThi> danhSachCauHoi = new List<CauHoiThi>();
        int cauHienTai = 0; // Vị trí câu đang hiển thị trong danhSachCauHoi.
        int tongSoCau = 0;
        int thoiGianConLai = 0; // Lưu theo giây để timer trừ từng giây cho dễ.

        string maLop;
        string maMon;
        string trinhDo;
        int lanThi;
        DateTime ngayThiDangThi; // Giữ đúng ngày thi sinh viên đã chọn khi bắt đầu.
        string tenMonHoc = "";
        Label lblThongTinLichThi;
        Label lblTrangThaiTraLoi;
        FlowLayoutPanel pnlDanhSachCauHoi;
        List<Button> nutCauHoi = new List<Button>();
        RadioButton rdoChuaChon;
        bool dangHienThiCauHoi = false;
        bool dangNopBai = false;
        int demGiayLuuTam = 0;
        bool dangLamBaiHienThi = false;
        bool laThiThuGiaoVien = false;
        private ComboBox cmbLop;
        private DataGridView dgvLichThi;
        private Label lblDanhSachLichThi;

        public formThi()
        {
            InitializeComponent();
            cmbMonThi.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanThi.DropDownStyle = ComboBoxStyle.DropDownList;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1180, 740);
            this.Text = "Thi tr\u1eafc nghi\u1ec7m";

            // Thêm label nhỏ để hiện trình độ, số câu và thời gian.
            lblThongTinLichThi = new Label();
            lblThongTinLichThi.AutoSize = false;
            lblThongTinLichThi.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblThongTinLichThi.Location = new Point(350, 165);
            lblThongTinLichThi.Size = new Size(860, 56);
            lblThongTinLichThi.Text = "";
            this.Controls.Add(lblThongTinLichThi);

            lblTrangThaiTraLoi = new Label();
            lblTrangThaiTraLoi.AutoSize = false;
            lblTrangThaiTraLoi.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            lblTrangThaiTraLoi.Location = new Point(350, 55);
            lblTrangThaiTraLoi.Size = new Size(220, 28);
            lblTrangThaiTraLoi.Text = "Đã trả lời: 0/0";
            this.Controls.Add(lblTrangThaiTraLoi);

            pnlDanhSachCauHoi = new FlowLayoutPanel();
            pnlDanhSachCauHoi.AutoScroll = true;
            pnlDanhSachCauHoi.BackColor = Color.White;
            pnlDanhSachCauHoi.FlowDirection = FlowDirection.LeftToRight;
            pnlDanhSachCauHoi.WrapContents = true;
            pnlDanhSachCauHoi.Padding = new Padding(6);
            pnlDanhSachCauHoi.Location = new Point(350, 83);
            pnlDanhSachCauHoi.Size = new Size(720, 72);
            pnlDanhSachCauHoi.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(pnlDanhSachCauHoi);

            // Đổi ngày/môn/lần thì cập nhật lại thông tin ca thi.
            cmbMonThi.SelectedIndexChanged += cmbMonThi_SelectedIndexChanged;
            cmbLanThi.SelectedIndexChanged += cmbLanThi_SelectedIndexChanged;
            dtpNgayThi.ValueChanged += dtpNgayThi_ValueChanged;

            rdoA.CheckedChanged += DapAn_CheckedChanged;
            rdoB.CheckedChanged += DapAn_CheckedChanged;
            rdoC.CheckedChanged += DapAn_CheckedChanged;
            rdoD.CheckedChanged += DapAn_CheckedChanged;
            this.FormClosing += formThi_FormClosing;
            this.Resize += formThi_Resize;

            rdoChuaChon = new RadioButton();
            rdoChuaChon.Visible = false;
            rdoChuaChon.TabStop = false;
            groupBox1.Controls.Add(rdoChuaChon);

            CaiThienGiaoDienFormThi();
            DatTrangThaiHienThiBaiThi(false);
            SapXepBoCucFormThi();
        }

        private void CaiThienGiaoDienFormThi()
        {
            this.BackColor = Color.FromArgb(245, 247, 250);

            Font fontChu = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            Font fontChuDam = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            Font fontCauHoi = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

            lblHoTen.Font = fontChuDam;
            lblTenLop.Font = fontChu;
            lblMonThi.Font = fontChu;
            lblLanThi.Font = fontChu;
            lblNgayThi.Font = fontChu;
            lblMaLop.Visible = false;

            label1.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblThoiGian.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            lblTrangThaiTraLoi.Font = fontChuDam;
            lblThongTinLichThi.Font = fontChu;

            lblNoiDungCauHoi.Font = fontCauHoi;
            lblNoiDungCauHoi.BackColor = Color.White;
            lblNoiDungCauHoi.BorderStyle = BorderStyle.FixedSingle;
            lblNoiDungCauHoi.TextAlign = ContentAlignment.MiddleLeft;

            groupBox1.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            groupBox1.BackColor = Color.White;
            rdoA.Font = fontCauHoi;
            rdoB.Font = fontCauHoi;
            rdoC.Font = fontCauHoi;
            rdoD.Font = fontCauHoi;
            rdoA.BackColor = Color.White;
            rdoB.BackColor = Color.White;
            rdoC.BackColor = Color.White;
            rdoD.BackColor = Color.White;

            pnlDanhSachCauHoi.BackColor = Color.White;

            CaiThienNutLenh(btnBatDau, Color.FromArgb(0, 120, 215), Color.White);
            CaiThienNutLenh(btnNopBai, Color.FromArgb(198, 55, 55), Color.White);
            CaiThienNutLenh(btnThoat, Color.White, Color.FromArgb(40, 40, 40));
            CaiThienNutLenh(btnCauTruoc, Color.White, Color.FromArgb(40, 40, 40));
            CaiThienNutLenh(btnCauSau, Color.White, Color.FromArgb(40, 40, 40));
        }

        private void CaiThienNutLenh(Button button, Color mauNen, Color mauChu)
        {
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button.BackColor = mauNen;
            button.ForeColor = mauChu;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(205, 213, 224);
            button.FlatAppearance.BorderSize = mauNen == Color.White ? 1 : 0;
        }

        private void DatTrangThaiHienThiBaiThi(bool dangLamBai)
        {
            dangLamBaiHienThi = dangLamBai;

            label1.Visible = dangLamBai;
            lblThoiGian.Visible = dangLamBai;
            lblTrangThaiTraLoi.Visible = dangLamBai;
            pnlDanhSachCauHoi.Visible = dangLamBai;
            lblNoiDungCauHoi.Visible = dangLamBai;
            groupBox1.Visible = dangLamBai;
            btnCauTruoc.Visible = dangLamBai;
            btnCauSau.Visible = dangLamBai;
            btnNopBai.Visible = dangLamBai;
            btnBatDau.Visible = !dangLamBai;
            btnThoat.Visible = !dangLamBai;
            this.ControlBox = !dangLamBai;

            if (lblDanhSachLichThi != null) lblDanhSachLichThi.Visible = !dangLamBai && !LaGiaoVien();
            if (dgvLichThi != null) dgvLichThi.Visible = !dangLamBai && !LaGiaoVien();

            SapXepBoCucFormThi();
        }

        private void SapXepBoCucFormThi()
        {
            int leNgoai = 32;
            int doRongCotTrai = 300;
            int doRongNoiDung = Math.Min(980, Math.Max(780, this.ClientSize.Width - 420));
            int doRongToanKhoi = doRongCotTrai + 40 + doRongNoiDung;
            int xCotTrai = Math.Max(leNgoai, (this.ClientSize.Width - doRongToanKhoi) / 2);
            int xNoiDung = xCotTrai + doRongCotTrai + 40;

            if (dangLamBaiHienThi)
            {
                // Tính chiều cao động của panel chọn nhanh câu hỏi theo số lượng câu (5 cột)
                int soCauHoi = danhSachCauHoi != null ? danhSachCauHoi.Count : 0;
                int soDong = soCauHoi > 0 ? (int)Math.Ceiling((double)soCauHoi / 5) : 1;
                int chieuCaoPnl = soDong * 40 + 20; // 40px mỗi dòng (32px + 8px margin), cộng thêm 20px padding/margin
                int maxChieuCaoPnl = Math.Max(150, this.ClientSize.Height - 300);
                chieuCaoPnl = Math.Min(chieuCaoPnl, maxChieuCaoPnl);

                int chieuCaoCotTrai = 215 + chieuCaoPnl;
                int chieuCaoCotPhai = 487; // 130 (câu hỏi) + 20 (khoảng cách) + 270 (groupBox) + 25 (khoảng cách) + 42 (nút)
                int chieuCaoKhung = Math.Max(chieuCaoCotTrai, chieuCaoCotPhai);

                // Căn giữa toàn bộ giao diện theo chiều dọc của Form
                int yDau = Math.Max(30, (this.ClientSize.Height - chieuCaoKhung) / 2);

                // --- Giao diện làm bài thi (Cấu trúc Cột bên - Sidebar Layout) ---
                
                // Cột bên trái (X = xCotTrai)
                lblHoTen.Location = new Point(xCotTrai, yDau);
                lblHoTen.MaximumSize = new Size(doRongCotTrai, 0);
                lblTenLop.Location = new Point(xCotTrai, yDau + 32);
                lblTenLop.MaximumSize = new Size(doRongCotTrai, 0);

                lblThongTinLichThi.Location = new Point(xCotTrai, yDau + 70);
                lblThongTinLichThi.Size = new Size(doRongCotTrai, 60);
                lblThongTinLichThi.TextAlign = ContentAlignment.TopLeft;

                lblTrangThaiTraLoi.Location = new Point(xCotTrai, yDau + 140);
                lblTrangThaiTraLoi.Size = new Size(doRongCotTrai, 25);

                label1.Location = new Point(xCotTrai, yDau + 175);
                lblThoiGian.Location = new Point(xCotTrai + 130, yDau + 168);

                pnlDanhSachCauHoi.Location = new Point(xCotTrai, yDau + 215);
                pnlDanhSachCauHoi.Size = new Size(doRongCotTrai, chieuCaoPnl);

                // Khung bên phải (X = xNoiDung)
                lblNoiDungCauHoi.Location = new Point(xNoiDung, yDau);
                lblNoiDungCauHoi.Size = new Size(doRongNoiDung, 130);

                groupBox1.Location = new Point(xNoiDung, yDau + 150);
                groupBox1.Size = new Size(doRongNoiDung, 270);

                int doRongLuaChon = doRongNoiDung - 78;
                rdoA.Location = new Point(38, 45);
                rdoB.Location = new Point(38, 95);
                rdoC.Location = new Point(38, 145);
                rdoD.Location = new Point(38, 195);
                rdoA.Size = new Size(doRongLuaChon, 38);
                rdoB.Size = new Size(doRongLuaChon, 38);
                rdoC.Size = new Size(doRongLuaChon, 38);
                rdoD.Size = new Size(doRongLuaChon, 38);

                int yNutLamBai = groupBox1.Bottom + 25;
                btnCauTruoc.Location = new Point(xNoiDung, yNutLamBai);
                btnCauTruoc.Size = new Size(120, 42);
                btnCauSau.Location = new Point(xNoiDung + 140, yNutLamBai);
                btnCauSau.Size = new Size(112, 42);
                btnNopBai.Location = new Point(xNoiDung + doRongNoiDung - 128, yNutLamBai);
                btnNopBai.Size = new Size(112, 42);

                btnThoat.Location = new Point(xNoiDung + doRongNoiDung - 128, yNutLamBai);
                btnThoat.Size = new Size(112, 42);
            }
            else
            {
                int yDau = Math.Max(30, Math.Min(90, (this.ClientSize.Height - 680) / 3 + 30));
                // --- Giao diện chọn ca thi (Dashboard / Selection Screen) ---
                if (LaGiaoVien())
                {
                    // Layout của Giáo viên (Thi thử)
                    lblHoTen.Location = new Point(xCotTrai, yDau);
                    lblHoTen.MaximumSize = new Size(doRongCotTrai, 0);
                    lblTenLop.Location = new Point(xCotTrai, yDau + 42);
                    lblTenLop.MaximumSize = new Size(doRongCotTrai, 0);

                    int xNhan = xCotTrai;
                    int xNhap = xCotTrai + 95;
                    int yNhap = yDau + 100;
                    lblMonThi.Location = new Point(xNhan, yNhap);
                    cmbMonThi.Location = new Point(xNhap, yNhap - 2);
                    cmbMonThi.Size = new Size(205, 28);

                    lblLanThi.Location = new Point(xNhan, yNhap + 48);
                    cmbLanThi.Location = new Point(xNhap, yNhap + 46);
                    cmbLanThi.Size = new Size(150, 28);

                    lblNgayThi.Location = new Point(xNhan, yNhap + 96);
                    dtpNgayThi.Location = new Point(xNhap, yNhap + 94);
                    dtpNgayThi.Size = new Size(205, 28);

                    btnBatDau.Location = new Point(xNhap, yNhap + 150);
                    btnBatDau.Size = new Size(128, 42);

                    btnThoat.Location = new Point(xNhap + 145, yNhap + 150);
                    btnThoat.Size = new Size(112, 42);

                    lblThongTinLichThi.Location = new Point(xNoiDung, yDau + 104);
                    lblThongTinLichThi.Size = new Size(doRongNoiDung, 86);
                    lblThongTinLichThi.TextAlign = ContentAlignment.TopLeft;
                }
                else
                {
                    // Layout của Sinh viên (Danh sách DataGridView)
                    if (dgvLichThi != null && lblDanhSachLichThi != null)
                    {
                        lblHoTen.Location = new Point(xCotTrai, yDau);
                        lblHoTen.MaximumSize = new Size(380, 0);
                        lblTenLop.Location = new Point(xCotTrai + 400, yDau);
                        lblTenLop.MaximumSize = new Size(380, 0);

                        lblDanhSachLichThi.Location = new Point(xCotTrai, yDau + 55);

                        dgvLichThi.Location = new Point(xCotTrai, yDau + 90);
                        dgvLichThi.Size = new Size(doRongToanKhoi - 40, 320);

                        int yNut = dgvLichThi.Bottom + 20;
                        int chieuRongBang = dgvLichThi.Width;
                        int centerX = xCotTrai + (chieuRongBang - 264) / 2;

                        btnBatDau.Location = new Point(centerX, yNut);
                        btnBatDau.Size = new Size(128, 42);

                        btnThoat.Location = new Point(centerX + 152, yNut);
                        btnThoat.Size = new Size(112, 42);

                        lblThongTinLichThi.Location = new Point(xCotTrai, yNut + 60);
                        lblThongTinLichThi.Size = new Size(chieuRongBang, 30);
                        lblThongTinLichThi.TextAlign = ContentAlignment.MiddleCenter;
                    }
                }
            }
        }

        private void formThi_Resize(object sender, EventArgs e)
        {
            SapXepBoCucFormThi();
        }

        private DateTime NgayThiToiThieu
        {
            get { return DateTime.Today.AddDays(1); }
        }

        private void ApDungGioiHanNgayThi()
        {
            DateTime ngayToiThieu = NgayThiToiThieu;
            dtpNgayThi.MinDate = DateTimePicker.MinimumDateTime;
            if (dtpNgayThi.Value.Date < ngayToiThieu)
                dtpNgayThi.Value = ngayToiThieu;

            dtpNgayThi.MinDate = ngayToiThieu;
        }

        private void HienThiNgayDangThi(DateTime ngayThi)
        {
            dtpNgayThi.MinDate = DateTimePicker.MinimumDateTime;
            dtpNgayThi.Value = ngayThi.Date;
        }

        private void btnCauTruoc_Click(object sender, EventArgs e)
        {
            LuuDapAn(); // Trước khi chuyển câu thì giữ lại đáp án đang chọn.
            LuuDapAnTam(cauHienTai);
            LuuTrangThaiBaiThiTam(false);
            if (cauHienTai > 0)
            {
                cauHienTai--;
                HienThiCauHoi(cauHienTai);
            }
        }

        private bool DaThi(string maMonCanKiemTra, int lanCanKiemTra)
        {
            // Gọi SP kiểm tra BANGDIEM để biết sinh viên đã thi môn/lần này chưa.
            object result = DBHelper.ExecuteScalar(
                "SP_KIEMTRA_SINHVIEN_DA_THI",
                new SqlParameter("@MASV", Program.mUserName),
                new SqlParameter("@MAMH", maMonCanKiemTra),
                new SqlParameter("@LAN", lanCanKiemTra));

            return Convert.ToInt32(result) == 1;
        }

        private DataTable LayThongTinDangKy(string maMonCanThi, int lanCanThi, DateTime ngayThi)
        {
            // Lấy cấu hình ca thi từ SP để C# không viết truy vấn SQL trực tiếp.
            return DBHelper.ExecuteDataTable(
                "SP_GET_THONGTIN_DANGKY_THI",
                new SqlParameter("@MAMH", maMonCanThi),
                new SqlParameter("@MALOP", maLop),
                new SqlParameter("@LAN", lanCanThi),
                new SqlParameter("@NGAYTHI", ngayThi.Date));
        }

        private DataTable LayDeThi(string maMonCanThi, string trinhDoCanThi, int soCauThi)
        {
            // SP_LAY_DE_THI bốc câu ngẫu nhiên theo luật 70/30.
            return DBHelper.ExecuteDataTable(
                "SP_LAY_DE_THI",
                new SqlParameter("@MAMH", maMonCanThi),
                new SqlParameter("@TRINHDO", trinhDoCanThi),
                new SqlParameter("@SOCAU", soCauThi));
        }

        private SqlParameter TaoThamSoDapAnTam(string tenThamSo, string dapAn)
        {
            SqlParameter parameter = new SqlParameter(tenThamSo, SqlDbType.NChar, 1);
            parameter.Value = string.IsNullOrWhiteSpace(dapAn)
                ? (object)DBNull.Value
                : dapAn.Trim().ToUpper();
            return parameter;
        }

        private void TaoNutCauHoi()
        {
            pnlDanhSachCauHoi.Controls.Clear();
            nutCauHoi.Clear();

            for (int i = 0; i < danhSachCauHoi.Count; i++)
            {
                Button btn = new Button();
                btn.Width = 42;
                btn.Height = 32;
                btn.Margin = new Padding(4);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.FromArgb(190, 200, 212);
                btn.FlatAppearance.BorderSize = 1;
                btn.Text = (i + 1).ToString();
                btn.Tag = i;
                btn.Click += btnSoCauHoi_Click;
                pnlDanhSachCauHoi.Controls.Add(btn);
                nutCauHoi.Add(btn);
            }

            CapNhatTrangThaiCauHoi();
        }

        private void CapNhatTrangThaiCauHoi()
        {
            int soCauDaTraLoi = danhSachCauHoi.Count(cau => !string.IsNullOrWhiteSpace(cau.DapAnDaChon));
            lblTrangThaiTraLoi.Text = $"Đã trả lời: {soCauDaTraLoi}/{danhSachCauHoi.Count}";

            for (int i = 0; i < nutCauHoi.Count; i++)
            {
                Button btn = nutCauHoi[i];
                bool daTraLoi = !string.IsNullOrWhiteSpace(danhSachCauHoi[i].DapAnDaChon);

                if (i == cauHienTai)
                {
                    btn.BackColor = Color.FromArgb(255, 236, 153);
                    btn.Font = new Font(btn.Font, FontStyle.Bold);
                }
                else
                {
                    btn.BackColor = daTraLoi ? Color.FromArgb(198, 239, 206) : Color.White;
                    btn.Font = new Font(btn.Font, FontStyle.Regular);
                }

                btn.Text = daTraLoi ? "✓" + (i + 1) : (i + 1).ToString();
            }
        }

        private void btnSoCauHoi_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index)
            {
                LuuDapAn();
                LuuDapAnTam(cauHienTai);
                LuuTrangThaiBaiThiTam(false);
                cauHienTai = index;
                HienThiCauHoi(cauHienTai);
            }
        }

        private void DapAn_CheckedChanged(object sender, EventArgs e)
        {
            if (dangHienThiCauHoi || danhSachCauHoi.Count == 0) return;

            RadioButton radio = sender as RadioButton;
            if (radio == null || !radio.Checked) return;

            if (radio == rdoA) danhSachCauHoi[cauHienTai].DapAnDaChon = "A";
            else if (radio == rdoB) danhSachCauHoi[cauHienTai].DapAnDaChon = "B";
            else if (radio == rdoC) danhSachCauHoi[cauHienTai].DapAnDaChon = "C";
            else if (radio == rdoD) danhSachCauHoi[cauHienTai].DapAnDaChon = "D";

            CapNhatTrangThaiCauHoi();
            LuuDapAnTam(cauHienTai);
        }

        private void BatDauLamBai()
        {
            DatTrangThaiHienThiBaiThi(true);
            TaoNutCauHoi();
            HienThiCauHoi(cauHienTai);

            lblThongTinLichThi.Text = $"Môn thi: {tenMonHoc}\nTrình độ: {trinhDo}  |  Lần thi: {lanThi}\nSố câu: {tongSoCau}  |  Thời gian: {thoiGianConLai / 60} phút";

            btnBatDau.Enabled = false;
            btnNopBai.Enabled = true;
            cmbMonThi.Enabled = false;
            cmbLanThi.Enabled = false;
            dtpNgayThi.Enabled = false;
            if (dgvLichThi != null) dgvLichThi.Enabled = false;
            groupBox1.Enabled = true;

            demGiayLuuTam = 0;
            lblThoiGian.ForeColor = SystemColors.ControlText;
            timer1.Start();
            SapXepBoCucFormThi();
        }

        private void TaoBaiThiTam()
        {
            if (laThiThuGiaoVien) return;
            using (SqlConnection conn = DBHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        int result = DBHelper.ExecuteNonQueryWithReturn(
                            conn,
                            tran,
                            "SP_TAO_BAITHI_TAM",
                            new SqlParameter("@MASV", Program.mUserName),
                            new SqlParameter("@MAMH", maMon),
                            new SqlParameter("@LAN", lanThi),
                            new SqlParameter("@NGAYTHI", ngayThiDangThi),
                            new SqlParameter("@MALOP", maLop),
                            new SqlParameter("@TRINHDO", trinhDo),
                            new SqlParameter("@SOCAUTHI", tongSoCau),
                            new SqlParameter("@THOIGIAN", thoiGianConLai / 60),
                            new SqlParameter("@THOIGIANCONLAI", thoiGianConLai),
                            new SqlParameter("@CAUHIENTAI", cauHienTai));

                        if (result != 0)
                            throw new Exception("Không tạo được bài thi tạm. Mã lỗi: " + result);

                        for (int i = 0; i < danhSachCauHoi.Count; i++)
                        {
                            CauHoiThi cauHoi = danhSachCauHoi[i];
                            int resultChiTiet = DBHelper.ExecuteNonQueryWithReturn(
                                conn,
                                tran,
                                "SP_LUU_CHI_TIET_BAITHI_TAM",
                                new SqlParameter("@MASV", Program.mUserName),
                                new SqlParameter("@MAMH", maMon),
                                new SqlParameter("@LAN", lanThi),
                                new SqlParameter("@STT", i + 1),
                                new SqlParameter("@CAUHOI", cauHoi.MaCauHoi),
                                TaoThamSoDapAnTam("@DAP_AN_SV", cauHoi.DapAnDaChon));

                            if (resultChiTiet != 0)
                                throw new Exception("Không lưu được câu tạm số " + (i + 1) + ". Mã lỗi: " + resultChiTiet);
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        private void LuuTrangThaiBaiThiTam(bool hienThongBaoLoi)
        {
            if (laThiThuGiaoVien) return;
            
            if (danhSachCauHoi.Count == 0 || string.IsNullOrWhiteSpace(maMon)) return;

            try
            {
                DBHelper.ExecuteNonQueryWithReturn(
                    "SP_CAPNHAT_BAITHI_TAM",
                    new SqlParameter("@MASV", Program.mUserName),
                    new SqlParameter("@MAMH", maMon),
                    new SqlParameter("@LAN", lanThi),
                    new SqlParameter("@THOIGIANCONLAI", thoiGianConLai),
                    new SqlParameter("@CAUHIENTAI", cauHienTai));
            }
            catch (Exception ex)
            {
                if (hienThongBaoLoi)
                    MessageBox.Show("Không lưu được trạng thái bài thi tạm: " + ex.Message, "Cảnh báo");
            }
        }

        private void LuuDapAnTam(int index)
        {
            if (laThiThuGiaoVien) return;
            
            if (index < 0 || index >= danhSachCauHoi.Count || string.IsNullOrWhiteSpace(maMon)) return;

            try
            {
                CauHoiThi cauHoi = danhSachCauHoi[index];
                DBHelper.ExecuteNonQueryWithReturn(
                    "SP_LUU_CHI_TIET_BAITHI_TAM",
                    new SqlParameter("@MASV", Program.mUserName),
                    new SqlParameter("@MAMH", maMon),
                    new SqlParameter("@LAN", lanThi),
                    new SqlParameter("@STT", index + 1),
                    new SqlParameter("@CAUHOI", cauHoi.MaCauHoi),
                    TaoThamSoDapAnTam("@DAP_AN_SV", cauHoi.DapAnDaChon));

                LuuTrangThaiBaiThiTam(false);
            }
            catch
            {
                // Nếu đang mất kết nối SQL tạm thời thì vẫn giữ đáp án trong RAM; lần lưu kế tiếp sẽ thử lại.
            }
        }

        private void XoaBaiThiTam()
        {
            if (laThiThuGiaoVien) return;
            
            if (string.IsNullOrWhiteSpace(maMon)) return;

            DBHelper.ExecuteNonQueryWithReturn(
                "SP_XOA_BAITHI_TAM",
                new SqlParameter("@MASV", Program.mUserName),
                new SqlParameter("@MAMH", maMon),
                new SqlParameter("@LAN", lanThi));
        }

        private bool KiemTraVaKhoiPhucBaiThiTam()
        {
            DataTable dtTam = DBHelper.ExecuteDataTable(
                "SP_GET_BAITHI_TAM_DANG_CO",
                new SqlParameter("@MASV", Program.mUserName));

            if (dtTam.Rows.Count == 0) return false;

            DataRow row = dtTam.Rows[0];
            string tenMonTam = row["TENMH"].ToString();
            int lanTam = Convert.ToInt32(row["LAN"]);
            int thoiGianTam = Convert.ToInt32(row["THOIGIANCONLAI"]);

            DialogResult result = MessageBox.Show(
                $"Bạn có bài thi tạm môn {tenMonTam} lần {lanTam}, còn khoảng {thoiGianTam / 60:00}:{thoiGianTam % 60:00}. Bạn có muốn thi tiếp không?",
                "Khôi phục bài thi tạm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                DBHelper.ExecuteNonQueryWithReturn(
                    "SP_XOA_BAITHI_TAM",
                    new SqlParameter("@MASV", Program.mUserName),
                    new SqlParameter("@MAMH", row["MAMH"].ToString()),
                    new SqlParameter("@LAN", lanTam));
                return false;
            }

            return KhoiPhucBaiThiTam(row);
        }

        private bool KhoiPhucBaiThiTam(DataRow row)
        {
            maMon = row["MAMH"].ToString().Trim();
            tenMonHoc = row["TENMH"].ToString().Trim();
            lanThi = Convert.ToInt32(row["LAN"]);
            ngayThiDangThi = Convert.ToDateTime(row["NGAYTHI"]).Date;
            trinhDo = row["TRINHDO"].ToString().Trim();
            tongSoCau = Convert.ToInt32(row["SOCAUTHI"]);
            thoiGianConLai = Convert.ToInt32(row["THOIGIANCONLAI"]);
            cauHienTai = Convert.ToInt32(row["CAUHIENTAI"]);

            DataTable dtChiTiet = DBHelper.ExecuteDataTable(
                "SP_GET_CHI_TIET_BAITHI_TAM",
                new SqlParameter("@MASV", Program.mUserName),
                new SqlParameter("@MAMH", maMon),
                new SqlParameter("@LAN", lanThi));

            if (dtChiTiet.Rows.Count == 0)
            {
                MessageBox.Show("Bài thi tạm không có chi tiết câu hỏi, hệ thống sẽ xóa bài tạm này.", "Báo lỗi");
                XoaBaiThiTam();
                return false;
            }

            danhSachCauHoi.Clear();
            foreach (DataRow chiTiet in dtChiTiet.Rows)
            {
                CauHoiThi ch = new CauHoiThi();
                ch.MaCauHoi = Convert.ToInt32(chiTiet["CAUHOI"]);
                ch.NoiDung = chiTiet["NOIDUNG"].ToString();
                ch.A = chiTiet["A"].ToString();
                ch.B = chiTiet["B"].ToString();
                ch.C = chiTiet["C"].ToString();
                ch.D = chiTiet["D"].ToString();
                ch.DapAnDung = chiTiet["DAP_AN"].ToString().Trim().ToUpper();
                ch.DapAnDaChon = chiTiet["DAP_AN_SV"] == DBNull.Value ? "" : chiTiet["DAP_AN_SV"].ToString().Trim().ToUpper();
                danhSachCauHoi.Add(ch);
            }

            tongSoCau = danhSachCauHoi.Count;
            if (cauHienTai < 0 || cauHienTai >= tongSoCau) cauHienTai = 0;

            lblThongTinLichThi.Text = $"Đang thi tiếp | Trình độ: {trinhDo} | Số câu: {tongSoCau} | Còn lại: {thoiGianConLai / 60:00}:{thoiGianConLai % 60:00}";
            lblThoiGian.Text = string.Format("{0:00}:{1:00}", thoiGianConLai / 60, thoiGianConLai % 60);
            lblNgayThi.Text = "Ngày thi:";
            HienThiNgayDangThi(ngayThiDangThi);
            BatDauLamBai();
            return true;
        }

        private void KhoiTaoGiaoDienSinhVien()
        {
            lblMonThi.Visible = false;
            cmbMonThi.Visible = false;
            lblLanThi.Visible = false;
            cmbLanThi.Visible = false;
            lblNgayThi.Visible = false;
            dtpNgayThi.Visible = false;

            if (lblDanhSachLichThi == null)
            {
                lblDanhSachLichThi = new Label();
                lblDanhSachLichThi.Text = "DANH SÁCH CÁC CA THI HỢP LỆ:";
                lblDanhSachLichThi.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                lblDanhSachLichThi.ForeColor = Color.FromArgb(44, 62, 80);
                lblDanhSachLichThi.AutoSize = true;
                this.Controls.Add(lblDanhSachLichThi);
            }
            lblDanhSachLichThi.Visible = true;
            lblDanhSachLichThi.BringToFront();

            if (dgvLichThi == null)
            {
                dgvLichThi = new DataGridView();
                dgvLichThi.AllowUserToAddRows = false;
                dgvLichThi.AllowUserToDeleteRows = false;
                dgvLichThi.ReadOnly = true;
                dgvLichThi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvLichThi.MultiSelect = false;
                dgvLichThi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvLichThi.BackgroundColor = Color.White;
                dgvLichThi.RowHeadersVisible = false;
                dgvLichThi.BorderStyle = BorderStyle.FixedSingle;
                dgvLichThi.GridColor = Color.FromArgb(224, 224, 224);

                dgvLichThi.EnableHeadersVisualStyles = false;
                dgvLichThi.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
                dgvLichThi.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvLichThi.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                dgvLichThi.ColumnHeadersHeight = 32;
                dgvLichThi.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
                dgvLichThi.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 253);

                dgvLichThi.SelectionChanged += dgvLichThi_SelectionChanged;
                dgvLichThi.CellDoubleClick += dgvLichThi_CellDoubleClick;

                this.Controls.Add(dgvLichThi);
            }
            dgvLichThi.Visible = true;
            dgvLichThi.Enabled = true;
            dgvLichThi.BringToFront();

            SapXepBoCucFormThi();
        }

        private void LoadLichThiSinhVien()
        {
            try
            {
                DataTable dtLich = DBHelper.ExecuteDataTable(
                    "SP_GET_LICH_THI_HOP_LE_CUA_SINHVIEN",
                    new SqlParameter("@MASV", Program.mUserName),
                    new SqlParameter("@MALOP", maLop));

                dgvLichThi.DataSource = dtLich;

                if (dgvLichThi.Columns.Contains("MAMH")) dgvLichThi.Columns["MAMH"].Visible = false;
                if (dgvLichThi.Columns.Contains("TENMH")) dgvLichThi.Columns["TENMH"].HeaderText = "Môn Học";
                if (dgvLichThi.Columns.Contains("LAN")) dgvLichThi.Columns["LAN"].HeaderText = "Lần Thi";
                if (dgvLichThi.Columns.Contains("TRINHDO")) dgvLichThi.Columns["TRINHDO"].HeaderText = "Trình Độ";
                if (dgvLichThi.Columns.Contains("SOCAUTHI")) dgvLichThi.Columns["SOCAUTHI"].HeaderText = "Số Câu";
                if (dgvLichThi.Columns.Contains("THOIGIAN")) dgvLichThi.Columns["THOIGIAN"].HeaderText = "Thời Gian (Phút)";
                
                if (dgvLichThi.Columns.Contains("NGAYTHI"))
                {
                    dgvLichThi.Columns["NGAYTHI"].HeaderText = "Ngày Thi";
                    dgvLichThi.Columns["NGAYTHI"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }

                if (dtLich.Rows.Count > 0)
                {
                    dgvLichThi.ClearSelection();
                    dgvLichThi.Rows[0].Selected = true;
                    CapNhatCaThiDangChon();
                    btnBatDau.Enabled = true;
                }
                else
                {
                    btnBatDau.Enabled = false;
                    lblThongTinLichThi.Text = "Lớp của bạn hiện không có ca thi nào hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch thi: " + ex.Message, "Lỗi");
            }
        }

        private void CapNhatCaThiDangChon()
        {
            if (dgvLichThi.SelectedRows.Count == 0) return;

            DataGridViewRow selectedRow = dgvLichThi.SelectedRows[0];
            DataRowView rowView = selectedRow.DataBoundItem as DataRowView;
            if (rowView == null) return;
            DataRow row = rowView.Row;

            maMon = row["MAMH"].ToString().Trim();
            tenMonHoc = row["TENMH"].ToString().Trim();
            lanThi = Convert.ToInt32(row["LAN"]);
            ngayThiDangThi = Convert.ToDateTime(row["NGAYTHI"]).Date;
            trinhDo = row["TRINHDO"].ToString().Trim();
            tongSoCau = Convert.ToInt32(row["SOCAUTHI"]);
            thoiGianConLai = Convert.ToInt32(row["THOIGIAN"]) * 60;

            lblThongTinLichThi.Text = $"Trình độ: {trinhDo} | Số câu: {tongSoCau} | Thời gian: {row["THOIGIAN"]} phút";
            btnBatDau.Enabled = true;
        }

        private void dgvLichThi_SelectionChanged(object sender, EventArgs e)
        {
            CapNhatCaThiDangChon();
        }

        private void dgvLichThi_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnBatDau.PerformClick();
        }

        private void LoadMonThiTheoNgayDaChon()
        {
            DateTime ngayDangChon = dtpNgayThi.Value.Date;

            // SP lọc môn thi trong ngày và bỏ qua môn/lần sinh viên đã có điểm.
            DataTable dtMon = DBHelper.ExecuteDataTable(
                "SP_GET_MON_THI_CHUA_THI_THEO_NGAY",
                new SqlParameter("@MASV", Program.mUserName),
                new SqlParameter("@MALOP", maLop),
                new SqlParameter("@NGAYTHI", ngayDangChon));

            cmbMonThi.DataSource = dtMon;
            cmbMonThi.DisplayMember = "TENMH";
            cmbMonThi.ValueMember = "MAMH";

            bool coMonThi = dtMon.Rows.Count > 0;
            cmbMonThi.Enabled = coMonThi;
            cmbLanThi.Enabled = coMonThi;
            btnBatDau.Enabled = coMonThi;

            if (!coMonThi)
            {
                cmbLanThi.Items.Clear();
                btnBatDau.Enabled = false;
                lblThongTinLichThi.Text = "Ngày đã chọn không có ca thi nào chưa thi.";
                return;
            }

            LoadLanThiChuaThi();
        }

        private void LoadLanThiChuaThi()
        {
            if (cmbMonThi.SelectedValue == null || !(cmbMonThi.SelectedValue is string)) return;

            string monDangChon = cmbMonThi.SelectedValue.ToString();
            DateTime ngayDangChon = dtpNgayThi.Value.Date;

            // Lấy các lần thi chưa có điểm của môn đang chọn.
            DataTable dtLan = DBHelper.ExecuteDataTable(
                "SP_GET_LAN_THI_CHUA_THI",
                new SqlParameter("@MASV", Program.mUserName),
                new SqlParameter("@MALOP", maLop),
                new SqlParameter("@MAMH", monDangChon),
                new SqlParameter("@NGAYTHI", ngayDangChon));

            cmbLanThi.Items.Clear();

            // Chỉ đưa các lần thi chưa có điểm vào ComboBox.
            foreach (DataRow row in dtLan.Rows)
            {
                cmbLanThi.Items.Add(row["LAN"].ToString());
            }

            if (cmbLanThi.Items.Count > 0)
            {
                cmbLanThi.SelectedIndex = 0;
                lblThongTinLichThi.Text = $"Trình độ: {dtLan.Rows[0]["TRINHDO"]} | Số câu: {dtLan.Rows[0]["SOCAUTHI"]} | Thời gian: {dtLan.Rows[0]["THOIGIAN"]} phút";
                btnBatDau.Enabled = true;
            }
            else
            {
                btnBatDau.Enabled = false;
                lblThongTinLichThi.Text = "Môn này không còn lần thi nào chưa thi vào ngày đã chọn.";
            }
        }

        private void CapNhatThongTinLichThi()
        {
            if (cmbMonThi.SelectedValue == null || string.IsNullOrWhiteSpace(cmbLanThi.Text)) return;

            if (!int.TryParse(cmbLanThi.Text, out int lanDangChon)) return;

            DataTable dtDangKy = LayThongTinDangKy(cmbMonThi.SelectedValue.ToString(), lanDangChon, dtpNgayThi.Value.Date);
            if (dtDangKy.Rows.Count == 0)
            {
                btnBatDau.Enabled = false;
                lblThongTinLichThi.Text = "";
                return;
            }

            lblThongTinLichThi.Text = $"Trình độ: {dtDangKy.Rows[0]["TRINHDO"]} | Số câu: {dtDangKy.Rows[0]["SOCAUTHI"]} | Thời gian: {dtDangKy.Rows[0]["THOIGIAN"]} phút";
            btnBatDau.Enabled = true;
        }

        private bool LaGiaoVien()
        {
            return string.Equals((Program.mGroup ?? "").Trim(), "GIANGVIEN", StringComparison.OrdinalIgnoreCase);
        }

        

        

        private void BatDauThiThuGiaoVien()
        {
            if (cmbMonThi.SelectedValue == null || string.IsNullOrWhiteSpace(cmbLanThi.Text))
            {
                MessageBox.Show("Vui lòng chọn môn học và trình độ để thi thử.", "Thông báo");
                return;
            }

            maMon = cmbMonThi.SelectedValue.ToString().Trim();
            trinhDo = cmbLanThi.Text.Trim().ToUpper();

            if (trinhDo != "A" && trinhDo != "B" && trinhDo != "C")
            {
                MessageBox.Show("Trình độ thi thử chỉ được chọn A, B hoặc C.", "Báo lỗi");
                return;
            }

            tongSoCau = 10;
            thoiGianConLai = 15 * 60;
            lblThoiGian.ForeColor = SystemColors.ControlText;
            lblThoiGian.Text = "15:00";

            DataTable dtDeThi = LayDeThi(maMon, trinhDo, tongSoCau);

            if (dtDeThi.Rows.Count < tongSoCau)
            {
                MessageBox.Show("Kho đề không đủ 10 câu hỏi theo trình độ đã chọn để thi thử.", "Báo lỗi");
                return;
            }

            danhSachCauHoi.Clear();
            foreach (DataRow row in dtDeThi.Rows)
            {
                CauHoiThi ch = new CauHoiThi();
                ch.MaCauHoi = Convert.ToInt32(row["CAUHOI"]);
                ch.NoiDung = row["NOIDUNG"].ToString();
                ch.A = row["A"].ToString();
                ch.B = row["B"].ToString();
                ch.C = row["C"].ToString();
                ch.D = row["D"].ToString();
                ch.DapAnDung = row["DAP_AN"].ToString().Trim().ToUpper();
                ch.DapAnDaChon = "";
                danhSachCauHoi.Add(ch);
            }

            cauHienTai = 0;
            BatDauLamBai();
        }

        private void formThi_Load(object sender, EventArgs e)
        {
            lblHoTen.Text = (LaGiaoVien() ? "Giáo viên: " : "Họ Tên SV: ") + Program.mHoTen;

            btnCauTruoc.Enabled = false;
            btnCauSau.Enabled = false;
            btnNopBai.Enabled = false;
            groupBox1.Enabled = false;
            DatTrangThaiHienThiBaiThi(false);

            if (LaGiaoVien())
            {
                laThiThuGiaoVien = true;
                
                cmbLop = new ComboBox();
                cmbLop.Location = new System.Drawing.Point(lblTenLop.Location.X + 60, lblTenLop.Location.Y - 2);
                cmbLop.Size = new System.Drawing.Size(250, 25);
                cmbLop.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbLop.SelectedIndexChanged += cmbLop_SelectedIndexChanged;
                this.Controls.Add(cmbLop);
                cmbLop.BringToFront();

                lblTenLop.Text = "Chọn Lớp:";
                lblMaLop.Visible = false;

                DataTable dtLop = DBHelper.GetDataTable("SELECT MALOP, TENLOP FROM LOP");
                cmbLop.DataSource = dtLop;
                cmbLop.DisplayMember = "TENLOP";
                cmbLop.ValueMember = "MALOP";

                ApDungGioiHanNgayThi();
                dtpNgayThi.Enabled = true;
                lblNgayThi.Text = "Ngày thi:";
            }
            else
            {
                DataTable dtSV = DBHelper.ExecuteDataTable(
                    "SP_GET_LOP_CUA_SINHVIEN",
                    new SqlParameter("@MASV", Program.mUserName));

                if (dtSV.Rows.Count > 0)
                {
                    maLop = dtSV.Rows[0]["MALOP"].ToString().Trim();
                    lblMaLop.Text = maLop;
                    lblTenLop.Text = "Tên Lớp: " + dtSV.Rows[0]["TENLOP"].ToString();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin lớp của sinh viên đang đăng nhập!", "Báo lỗi");
                    btnBatDau.Enabled = false;
                    return;
                }

                KhoiTaoGiaoDienSinhVien();

                if (KiemTraVaKhoiPhucBaiThiTam())
                {
                    return;
                }

                LoadLichThiSinhVien();
            }
        }

        private void cmbLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLop.SelectedValue != null)
            {
                maLop = cmbLop.SelectedValue.ToString().Trim();
                LoadMonThiTheoNgayDaChon();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void cmbMonThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            

            if (!string.IsNullOrWhiteSpace(maLop))
                LoadLanThiChuaThi();
        }

        private void cmbLanThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            

            CapNhatThongTinLichThi();
        }

        private void dtpNgayThi_ValueChanged(object sender, EventArgs e)
        {
            

            if (!string.IsNullOrWhiteSpace(maLop) && LaGiaoVien())
                LoadMonThiTheoNgayDaChon();
        }

        private void rdoC_CheckedChanged(object sender, EventArgs e)
        {

        }

        private List<int> LayDanhSachCauChuaTraLoi()
        {
            List<int> cauChuaTraLoi = new List<int>();

            for (int i = 0; i < danhSachCauHoi.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(danhSachCauHoi[i].DapAnDaChon))
                {
                    cauChuaTraLoi.Add(i + 1);
                }
            }

            return cauChuaTraLoi;
        }

        private string TaoChuoiCauChuaTraLoi(List<int> cauChuaTraLoi)
        {
            const int soCauHienThiToiDa = 15;
            string danhSachRutGon = string.Join(", ", cauChuaTraLoi.Take(soCauHienThiToiDa));

            if (cauChuaTraLoi.Count > soCauHienThiToiDa)
            {
                danhSachRutGon += ", ...";
            }

            return danhSachRutGon;
        }

        private bool XacNhanTruocKhiNopBai()
        {
            if (thoiGianConLai <= 0)
            {
                return true;
            }

            List<int> cauChuaTraLoi = LayDanhSachCauChuaTraLoi();

            if (cauChuaTraLoi.Count > 0)
            {
                string danhSachCau = TaoChuoiCauChuaTraLoi(cauChuaTraLoi);
                DialogResult xacNhanNopBai = MessageBox.Show(
                    $"Bạn còn {cauChuaTraLoi.Count}/{tongSoCau} câu chưa trả lời.\n\nCâu chưa trả lời: {danhSachCau}\n\nBạn có muốn nộp bài luôn không?",
                    "Còn câu chưa trả lời",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (xacNhanNopBai != DialogResult.Yes)
                {
                    cauHienTai = cauChuaTraLoi[0] - 1;
                    HienThiCauHoi(cauHienTai);
                    groupBox1.Focus();
                    return false;
                }

                return true;
            }

            DialogResult xacNhanNopDu = MessageBox.Show(
                $"Bạn đã trả lời đủ {tongSoCau}/{tongSoCau} câu.\n\nBạn có chắc chắn muốn nộp bài không?\nSau khi nộp, bạn sẽ không thể sửa đáp án.",
                "Xác nhận nộp bài",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            return xacNhanNopDu == DialogResult.Yes;
        }

        private void btnNopBai_Click(object sender, EventArgs e)
        {
            // Dừng timer trước khi chấm và lưu bài.
            timer1.Stop();
            LuuDapAn(); // Câu đang đứng cũng phải được lưu đáp án trước khi chấm.
            LuuDapAnTam(cauHienTai);
            LuuTrangThaiBaiThiTam(false);

            if (!XacNhanTruocKhiNopBai())
            {
                timer1.Start();
                return;
            }

            dangNopBai = true;
            btnNopBai.Enabled = false;

            // Chấm điểm bằng cách so sánh đáp án đã chọn với đáp án đúng.
            int soCauDung = 0;
            foreach (var item in danhSachCauHoi)
            {
                string dapAnSinhVien = item.DapAnDaChon?.Trim();
                string dapAnDung = item.DapAnDung?.Trim();

                // DAP_AN trong SQL có thể có khoảng trắng đệm, nên Trim trước khi so sánh.
                if (!string.IsNullOrWhiteSpace(dapAnSinhVien) &&
                    string.Equals(dapAnSinhVien, dapAnDung, StringComparison.OrdinalIgnoreCase))
                {
                    soCauDung++;
                }
            }

            // Quy điểm về thang 10.
            double diem = Math.Round((double)soCauDung * 10 / tongSoCau, 2);

            if (laThiThuGiaoVien)
            {
                MessageBox.Show(
                    $"Kết quả thi thử:\n- Số câu đúng: {soCauDung}/{tongSoCau}\n- Điểm tạm tính: {diem}\n\nKết quả thi thử không được ghi vào bảng điểm.",
                    "Kết quả thi thử");

                this.Close();
                return;
            }

            // Báo điểm ngay sau khi nộp.
            MessageBox.Show($"Kết quả bài thi của bạn:\n- Số câu đúng: {soCauDung}/{tongSoCau}\n- Điểm: {diem}", "Kết quả");

            // BANGDIEM lưu điểm tổng, CT_BAITHI lưu từng câu đã thi.
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();

                    // Điểm tổng và chi tiết phải cùng lưu; lỗi thì rollback.
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            int result = DBHelper.ExecuteNonQueryWithReturn(
                                conn,
                                tran,
                                "SP_GHI_DIEM_THI",
                                new SqlParameter("@MASV", Program.mUserName), // Mã sinh viên đang thi.
                                new SqlParameter("@MAMH", maMon),
                                new SqlParameter("@LAN", lanThi),
                                new SqlParameter("@NGAYTHI", ngayThiDangThi), // Lưu đúng ngày sinh viên đã chọn.
                                new SqlParameter("@DIEM", diem));

                            // SP trả 1 nếu bài đã có điểm, tránh ghi đè điểm cũ.
                            if (result == 1)
                            {
                                tran.Rollback();
                                dangNopBai = false;
                                btnNopBai.Enabled = true;
                                MessageBox.Show("Bài thi này đã có điểm trước đó, hệ thống không ghi đè điểm cũ.", "Không lưu lại");
                                if (LaGiaoVien()) LoadMonThiTheoNgayDaChon();
                                else LoadLichThiSinhVien();
                                return;
                            }

                            // Mỗi câu làm bài được lưu thành một dòng trong CT_BAITHI.
                            for (int i = 0; i < danhSachCauHoi.Count; i++)
                            {
                                CauHoiThi cauHoi = danhSachCauHoi[i];

                                // Không chọn đáp án thì lưu NULL để biết câu đó bị bỏ trống.
                                SqlParameter dapAnSV = new SqlParameter("@DAP_AN_SV", SqlDbType.NChar, 1);
                                dapAnSV.Value = string.IsNullOrWhiteSpace(cauHoi.DapAnDaChon)
                                    ? (object)DBNull.Value
                                    : cauHoi.DapAnDaChon.Trim().ToUpper();

                                int resultChiTiet = DBHelper.ExecuteNonQueryWithReturn(
                                    conn,
                                    tran,
                                    "SP_GHI_CHI_TIET_BAI_THI",
                                    new SqlParameter("@MASV", Program.mUserName),
                                    new SqlParameter("@MAMH", maMon),
                                    new SqlParameter("@LAN", lanThi),
                                    new SqlParameter("@STT", i + 1),
                                    new SqlParameter("@CAUHOI", cauHoi.MaCauHoi),
                                    dapAnSV);

                                if (resultChiTiet != 0)
                                {
                                    throw new Exception("Không lưu được chi tiết câu hỏi số " + (i + 1) + ".");
                                }
                            }

                            // Lưu đủ điểm và chi tiết bài thi thì mới xác nhận transaction.
                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }
                XoaBaiThiTam();
                MessageBox.Show("Đã lưu kết quả thi thành công!", "Thông báo");

                // Lưu xong thì đóng form.
                this.Close();
            }
            catch (Exception ex)
            {
                dangNopBai = false;
                btnNopBai.Enabled = true;
                if (thoiGianConLai > 0)
                    timer1.Start();
                MessageBox.Show("Lỗi lưu điểm: " + ex.Message);
            }
        }

        private void lblCauHoiSo_Click(object sender, EventArgs e)
        {

        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                if (laThiThuGiaoVien)
                {
                    DialogResult dr = MessageBox.Show(
                        "Bạn đang trong thời gian thi thử. Nếu thoát bây giờ, hệ thống sẽ nộp bài và tính điểm tạm. Kết quả này không ghi vào bảng điểm. Bạn có chắc chắn muốn thoát?",
                        "Cảnh báo",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (dr == DialogResult.Yes)
                    {
                        btnNopBai.PerformClick();
                    }

                    return;
                }

                MessageBox.Show(
                    "Bạn đang trong thời gian làm bài.\n\nKhông thể thoát khi chưa nộp bài. Vui lòng nộp bài trước khi rời khỏi màn hình thi.",
                    "Không thể thoát",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            this.Close();
        }

        private void formThi_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dangLamBaiHienThi && !dangNopBai)
            {
                e.Cancel = true;
                MessageBox.Show(
                    "Bạn đang trong thời gian làm bài.\n\nKhông thể đóng form khi chưa nộp bài. Vui lòng nộp bài trước khi rời khỏi màn hình thi.",
                    "Không thể đóng form",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void btnBatDau_Click(object sender, EventArgs e)
        {
            try
            {
                if (LaGiaoVien())
                {
                    if (cmbMonThi.SelectedValue == null || string.IsNullOrWhiteSpace(cmbLanThi.Text))
                    {
                        MessageBox.Show("Ngày đang chọn không có ca thi nào chưa thi.", "Thông báo");
                        return;
                    }

                    // Lưu lại môn/lần/ngày đang thi.
                    maMon = cmbMonThi.SelectedValue.ToString().Trim();
                    tenMonHoc = cmbMonThi.Text.Trim();
                    lanThi = int.Parse(cmbLanThi.Text);
                    DateTime ngayThi = dtpNgayThi.Value.Date;

                    // Ghi điểm theo ngày đã chọn, không theo ngày hệ thống.
                    ngayThiDangThi = ngayThi;

                    if (!laThiThuGiaoVien && DaThi(maMon, lanThi))
                    {
                        MessageBox.Show("Bạn đã thi môn này ở lần thi đang chọn rồi, không được thi lại!", "Không cho thi lại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        LoadMonThiTheoNgayDaChon();
                        return;
                    }

                    // Lần 2 chỉ được thi sau khi đã có điểm lần 1.
                    if (!laThiThuGiaoVien && lanThi == 2 && !DaThi(maMon, 1))
                    {
                        MessageBox.Show("Bạn phải thi lần 1 trước rồi mới được thi lần 2 của môn này!", "Sai thứ tự lần thi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Tìm đúng lịch thi theo môn, lớp, lần và ngày.
                    DataTable dtDangKy = LayThongTinDangKy(maMon, lanThi, ngayThi);

                    if (dtDangKy.Rows.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy lịch thi nào khớp với Môn học, Lần thi và Ngày thi mà bạn vừa chọn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Lấy số câu, thời gian và trình độ của ca thi.
                    tongSoCau = Convert.ToInt32(dtDangKy.Rows[0]["SOCAUTHI"]);
                    int soPhut = Convert.ToInt32(dtDangKy.Rows[0]["THOIGIAN"]);
                    trinhDo = dtDangKy.Rows[0]["TRINHDO"].ToString().Trim();
                    thoiGianConLai = soPhut * 60;
                }
                else
                {
                    if (dgvLichThi.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("Vui lòng chọn ca thi hợp lệ để bắt đầu làm bài.", "Thông báo");
                        return;
                    }

                    if (DaThi(maMon, lanThi))
                    {
                        MessageBox.Show("Bạn đã thi môn này ở lần thi đang chọn rồi, không được thi lại!", "Không cho thi lại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        LoadLichThiSinhVien();
                        return;
                    }
                }

                // Bốc đề ngẫu nhiên theo lịch thi.
                DataTable dtDeThi = LayDeThi(maMon, trinhDo, tongSoCau);

                if (dtDeThi.Rows.Count < tongSoCau)
                {
                    MessageBox.Show("Lỗi: Kho đề không đủ số câu hỏi yêu cầu!", "Báo lỗi");
                    return;
                }

                // Đưa DataTable về List để chuyển câu và lưu đáp án tạm.
                danhSachCauHoi.Clear();
                foreach (DataRow row in dtDeThi.Rows)
                {
                    CauHoiThi ch = new CauHoiThi();
                    ch.MaCauHoi = Convert.ToInt32(row["CAUHOI"]);
                    ch.NoiDung = row["NOIDUNG"].ToString();
                    ch.A = row["A"].ToString();
                    ch.B = row["B"].ToString();
                    ch.C = row["C"].ToString();
                    ch.D = row["D"].ToString();
                    ch.DapAnDung = row["DAP_AN"].ToString().Trim().ToUpper();
                    ch.DapAnDaChon = "";
                    danhSachCauHoi.Add(ch);
                }

                // Mở phần làm bài và hiện câu đầu tiên.
                cauHienTai = 0;
                TaoBaiThiTam();
                BatDauLamBai();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy đề thi: " + ex.Message, "Báo lỗi");
            }
        }
        // Hiển thị câu hỏi theo vị trí đang chọn trong danhSachCauHoi.
        private void HienThiCauHoi(int index)
        {
            if (index < 0 || index >= danhSachCauHoi.Count) return;

            CauHoiThi ch = danhSachCauHoi[index];

            // Hiện nội dung kèm số thứ tự.
            lblNoiDungCauHoi.Text = $"Câu {index + 1}/{tongSoCau}: {ch.NoiDung}";

            // Đưa 4 phương án lên radio button.
            rdoA.Text = ch.A;
            rdoB.Text = ch.B;
            rdoC.Text = ch.C;
            rdoD.Text = ch.D;

            dangHienThiCauHoi = true;

            // Câu chưa trả lời sẽ tick radio ẩn để WinForms không tự chọn phương án đầu.
            rdoA.Checked = false;
            rdoB.Checked = false;
            rdoC.Checked = false;
            rdoD.Checked = false;
            rdoChuaChon.Checked = true;

            // Quay lại câu cũ thì tick lại đáp án đã chọn.
            if (ch.DapAnDaChon == "A") rdoA.Checked = true;
            else if (ch.DapAnDaChon == "B") rdoB.Checked = true;
            else if (ch.DapAnDaChon == "C") rdoC.Checked = true;
            else if (ch.DapAnDaChon == "D") rdoD.Checked = true;

            dangHienThiCauHoi = false;

            // Câu đầu/cuối thì khóa nút Trước/Sau tương ứng.
            btnCauTruoc.Enabled = (index > 0);
            btnCauSau.Enabled = (index < danhSachCauHoi.Count - 1);
            CapNhatTrangThaiCauHoi();
        }

        // Lưu đáp án đang tick vào object câu hỏi hiện tại.
        private void LuuDapAn()
        {
            if (danhSachCauHoi.Count == 0) return;

            // Chỉ đồng bộ lại khi câu đã có đáp án do người dùng chọn.
            // Nếu câu đang bỏ trống, không đọc radio để tránh lưu nhầm phương án đầu.
            if (string.IsNullOrWhiteSpace(danhSachCauHoi[cauHienTai].DapAnDaChon))
                return;

            if (rdoA.Checked) danhSachCauHoi[cauHienTai].DapAnDaChon = "A";
            else if (rdoB.Checked) danhSachCauHoi[cauHienTai].DapAnDaChon = "B";
            else if (rdoC.Checked) danhSachCauHoi[cauHienTai].DapAnDaChon = "C";
            else if (rdoD.Checked) danhSachCauHoi[cauHienTai].DapAnDaChon = "D";
        }

        private void btnCauSau_Click(object sender, EventArgs e)
        {
            LuuDapAn(); // Trước khi chuyển câu thì giữ lại đáp án đang chọn.
            LuuDapAnTam(cauHienTai);
            LuuTrangThaiBaiThiTam(false);
            if (cauHienTai < danhSachCauHoi.Count - 1)
            {
                cauHienTai++;
                HienThiCauHoi(cauHienTai);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (thoiGianConLai > 0)
            {
                thoiGianConLai--; // Mỗi Tick của timer trừ 1 giây.

                // Đổi tổng số giây còn lại sang phút:giây để hiển thị.
                int phut = thoiGianConLai / 60;
                int giay = thoiGianConLai % 60;

                // Hiển thị dạng 00:00, ví dụ 14:59.
                lblThoiGian.Text = string.Format("{0:00}:{1:00}", phut, giay);

                // Còn ít thời gian thì đổi màu đỏ để sinh viên dễ chú ý.
                if (thoiGianConLai <= 30)
                {
                    lblThoiGian.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    lblThoiGian.ForeColor = SystemColors.ControlText;
                }

                demGiayLuuTam++;
                if (demGiayLuuTam >= 5)
                {
                    demGiayLuuTam = 0;
                    LuuTrangThaiBaiThiTam(false);
                }
            }
            else
            {
                // Hết giờ thì tự động nộp bài.
                timer1.Stop();
                MessageBox.Show("Đã hết thời gian làm bài!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnNopBai.PerformClick(); // Dùng lại đúng xử lý của nút Nộp bài.
            }
        }

        private void lblMonThi_Click(object sender, EventArgs e)
        {

        }

        private void lblNoiDungCauHoi_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
