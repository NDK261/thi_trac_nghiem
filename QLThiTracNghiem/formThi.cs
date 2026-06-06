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
        Label lblThongTinLichThi;

        public formThi()
        {
            InitializeComponent();

            // Thêm label nhỏ để hiện trình độ, số câu và thời gian.
            lblThongTinLichThi = new Label();
            lblThongTinLichThi.AutoSize = true;
            lblThongTinLichThi.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblThongTinLichThi.Location = new Point(330, 201);
            lblThongTinLichThi.Text = "";
            this.Controls.Add(lblThongTinLichThi);

            // Đổi ngày/môn/lần thì cập nhật lại thông tin ca thi.
            cmbMonThi.SelectedIndexChanged += cmbMonThi_SelectedIndexChanged;
            cmbLanThi.SelectedIndexChanged += cmbLanThi_SelectedIndexChanged;
            dtpNgayThi.ValueChanged += dtpNgayThi_ValueChanged;
        }

        private void btnCauTruoc_Click(object sender, EventArgs e)
        {
            LuuDapAn(); // Trước khi chuyển câu thì giữ lại đáp án đang chọn.
            if (cauHienTai > 0)
            {
                cauHienTai--;
                HienThiCauHoi(cauHienTai);
            }
        }

        private DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            // Dùng parameter để tránh lỗi dấu nháy và xử lý mã NCHAR ổn hơn.
            using (SqlConnection conn = DBHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private int ExecuteScalarInt(string sql, params SqlParameter[] parameters)
        {
            // Dùng cho các câu SELECT COUNT(*) cần lấy về một con số.
            using (SqlConnection conn = DBHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private bool DaThi(string maMonCanKiemTra, int lanCanKiemTra)
        {
            // Có dòng trong BANGDIEM nghĩa là sinh viên đã thi môn/lần đó.
            string sql = @"
                SELECT COUNT(*)
                FROM BANGDIEM
                WHERE MASV = @MASV
                  AND MAMH = @MAMH
                  AND LAN = @LAN";

            return ExecuteScalarInt(sql,
                new SqlParameter("@MASV", Program.mUserName),
                new SqlParameter("@MAMH", maMonCanKiemTra),
                new SqlParameter("@LAN", lanCanKiemTra)) > 0;
        }

        private DataTable LayThongTinDangKy(string maMonCanThi, int lanCanThi, DateTime ngayThi)
        {
            // Lấy cấu hình ca thi đúng môn, lớp, lần và ngày sinh viên chọn.
            string sql = @"
                SELECT SOCAUTHI, THOIGIAN, TRINHDO, NGAYTHI
                FROM GIAOVIEN_DANGKY
                WHERE MAMH = @MAMH
                  AND MALOP = @MALOP
                  AND LAN = @LAN
                  AND CAST(NGAYTHI AS DATE) = @NGAYTHI";

            return ExecuteQuery(sql,
                new SqlParameter("@MAMH", maMonCanThi),
                new SqlParameter("@MALOP", maLop),
                new SqlParameter("@LAN", lanCanThi),
                new SqlParameter("@NGAYTHI", ngayThi.Date));
        }

        private DataTable LayDeThi(string maMonCanThi, string trinhDoCanThi, int soCauThi)
        {
            // SP_LAY_DE_THI bốc câu ngẫu nhiên theo luật 70/30.
            using (SqlConnection conn = DBHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SP_LAY_DE_THI", conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MAMH", maMonCanThi);
                cmd.Parameters.AddWithValue("@TRINHDO", trinhDoCanThi);
                cmd.Parameters.AddWithValue("@SOCAU", soCauThi);

                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private void LoadMonThiTheoNgayDaChon()
        {
            DateTime ngayDangChon = dtpNgayThi.Value.Date;

            // Môn thi lọc theo ngày sinh viên đang chọn.
            // LEFT JOIN BANGDIEM để loại các môn/lần sinh viên đã thi rồi.
            string sql = @"
                SELECT DISTINCT MH.MAMH, MH.TENMH
                FROM GIAOVIEN_DANGKY DK
                INNER JOIN MONHOC MH ON DK.MAMH = MH.MAMH
                LEFT JOIN BANGDIEM BD
                    ON BD.MASV = @MASV
                   AND BD.MAMH = DK.MAMH
                   AND BD.LAN = DK.LAN
                WHERE DK.MALOP = @MALOP
                  AND CAST(DK.NGAYTHI AS DATE) = @NGAYTHI
                  AND BD.MASV IS NULL
                ORDER BY MH.TENMH";

            DataTable dtMon = ExecuteQuery(sql,
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
            string sql = @"
                SELECT DK.LAN, DK.NGAYTHI, DK.TRINHDO, DK.SOCAUTHI, DK.THOIGIAN
                FROM GIAOVIEN_DANGKY DK
                LEFT JOIN BANGDIEM BD
                    ON BD.MASV = @MASV
                   AND BD.MAMH = DK.MAMH
                   AND BD.LAN = DK.LAN
                WHERE DK.MALOP = @MALOP
                  AND DK.MAMH = @MAMH
                  AND CAST(DK.NGAYTHI AS DATE) = @NGAYTHI
                  AND BD.MASV IS NULL
                ORDER BY DK.LAN";

            DataTable dtLan = ExecuteQuery(sql,
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

        private void formThi_Load(object sender, EventArgs e)
        {
            // Hiển thị sinh viên đang đăng nhập.
            lblHoTen.Text = "Họ Tên SV: " + Program.mHoTen;

            // Mới mở form chỉ cho chọn ca thi.
            btnCauTruoc.Enabled = false;
            btnCauSau.Enabled = false;
            btnNopBai.Enabled = false;
            groupBox1.Enabled = false;

            // Lấy lớp của sinh viên để lọc đúng lịch thi.
            string sqlLop = @"
                SELECT SV.MALOP, L.TENLOP
                FROM SINHVIEN SV
                INNER JOIN LOP L ON SV.MALOP = L.MALOP
                WHERE SV.MASV = @MASV";
            DataTable dtSV = ExecuteQuery(sqlLop, new SqlParameter("@MASV", Program.mUserName));

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

            // Sinh viên chọn ngày thi bằng DateTimePicker.
            dtpNgayThi.Value = DateTime.Today;
            dtpNgayThi.Enabled = true;
            lblNgayThi.Text = "Ngày thi:";

            LoadMonThiTheoNgayDaChon();

            // Chưa có đề thì chưa mở phần làm bài.
            btnCauTruoc.Enabled = false;
            btnCauSau.Enabled = false;
            btnNopBai.Enabled = false;
            groupBox1.Enabled = false;
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
            if (!string.IsNullOrWhiteSpace(maLop))
                LoadMonThiTheoNgayDaChon();
        }

        private void rdoC_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnNopBai_Click(object sender, EventArgs e)
        {
            // Dừng timer trước khi chấm và lưu bài.
            timer1.Stop();
            LuuDapAn(); // Câu đang đứng cũng phải được lưu đáp án trước khi chấm.

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
                            using (SqlCommand cmd = new SqlCommand("SP_GHI_DIEM_THI", conn, tran))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@MASV", Program.mUserName); // Mã sinh viên đang thi.
                                cmd.Parameters.AddWithValue("@MAMH", maMon);
                                cmd.Parameters.AddWithValue("@LAN", lanThi);
                                // Lưu đúng ngày sinh viên đã chọn.
                                cmd.Parameters.AddWithValue("@NGAYTHI", ngayThiDangThi);
                                cmd.Parameters.AddWithValue("@DIEM", diem);

                                // SP trả 1 nếu bài đã có điểm, tránh ghi đè điểm cũ.
                                SqlParameter retVal = new SqlParameter();
                                retVal.Direction = ParameterDirection.ReturnValue;
                                cmd.Parameters.Add(retVal);

                                cmd.ExecuteNonQuery();

                                int result = Convert.ToInt32(retVal.Value);
                                if (result == 1)
                                {
                                    tran.Rollback();
                                    MessageBox.Show("Bài thi này đã có điểm trước đó, hệ thống không ghi đè điểm cũ.", "Không lưu lại");
                                    LoadMonThiTheoNgayDaChon();
                                    return;
                                }
                            }

                            // Mỗi câu làm bài được lưu thành một dòng trong CT_BAITHI.
                            for (int i = 0; i < danhSachCauHoi.Count; i++)
                            {
                                CauHoiThi cauHoi = danhSachCauHoi[i];

                                using (SqlCommand cmdChiTiet = new SqlCommand("SP_GHI_CHI_TIET_BAI_THI", conn, tran))
                                {
                                    cmdChiTiet.CommandType = CommandType.StoredProcedure;
                                    cmdChiTiet.Parameters.AddWithValue("@MASV", Program.mUserName);
                                    cmdChiTiet.Parameters.AddWithValue("@MAMH", maMon);
                                    cmdChiTiet.Parameters.AddWithValue("@LAN", lanThi);
                                    cmdChiTiet.Parameters.AddWithValue("@STT", i + 1);
                                    cmdChiTiet.Parameters.AddWithValue("@CAUHOI", cauHoi.MaCauHoi);

                                    // Không chọn đáp án thì lưu NULL để biết câu đó bị bỏ trống.
                                    SqlParameter dapAnSV = new SqlParameter("@DAP_AN_SV", SqlDbType.NChar, 1);
                                    dapAnSV.Value = string.IsNullOrWhiteSpace(cauHoi.DapAnDaChon)
                                        ? (object)DBNull.Value
                                        : cauHoi.DapAnDaChon.Trim().ToUpper();
                                    cmdChiTiet.Parameters.Add(dapAnSV);

                                    SqlParameter retValChiTiet = new SqlParameter();
                                    retValChiTiet.Direction = ParameterDirection.ReturnValue;
                                    cmdChiTiet.Parameters.Add(retValChiTiet);

                                    cmdChiTiet.ExecuteNonQuery();

                                    int resultChiTiet = Convert.ToInt32(retValChiTiet.Value);
                                    if (resultChiTiet != 0)
                                    {
                                        throw new Exception("Không lưu được chi tiết câu hỏi số " + (i + 1) + ".");
                                    }
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
                MessageBox.Show("Đã lưu kết quả thi thành công!", "Thông báo");

                // Lưu xong thì đóng form.
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu điểm: " + ex.Message);
            }
        }

        private void lblCauHoiSo_Click(object sender, EventArgs e)
        {

        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Đang làm bài mà thoát thì tự nộp bài.
            if (timer1.Enabled)
            {
                DialogResult dr = MessageBox.Show("Bạn đang trong thời gian làm bài! Nếu thoát bây giờ, hệ thống sẽ tự động nộp bài và tính điểm. Bạn có chắc chắn muốn thoát?", "Cảnh báo nghiêm trọng", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dr == DialogResult.Yes)
                {
                    btnNopBai.PerformClick(); // Gọi lại đúng luồng nộp bài để chấm và lưu điểm.
                }
            }
            else
            {
                this.Close();
            }
        }

        private void btnBatDau_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbMonThi.SelectedValue == null || string.IsNullOrWhiteSpace(cmbLanThi.Text))
                {
                    MessageBox.Show("Ngày đang chọn không có ca thi nào chưa thi.", "Thông báo");
                    return;
                }

                // Lưu lại môn/lần/ngày đang thi.
                maMon = cmbMonThi.SelectedValue.ToString().Trim();
                lanThi = int.Parse(cmbLanThi.Text);
                DateTime ngayThi = dtpNgayThi.Value.Date;

                // Ghi điểm theo ngày đã chọn, không theo ngày hệ thống.
                ngayThiDangThi = ngayThi;

                if (DaThi(maMon, lanThi))
                {
                    MessageBox.Show("Bạn đã thi môn này ở lần thi đang chọn rồi, không được thi lại!", "Không cho thi lại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadMonThiTheoNgayDaChon();
                    return;
                }

                // Lần 2 chỉ được thi sau khi đã có điểm lần 1.
                if (lanThi == 2 && !DaThi(maMon, 1))
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
                HienThiCauHoi(cauHienTai);

                // Đã bắt đầu thì không cho đổi ngày/môn/lần.
                btnBatDau.Enabled = false;
                btnNopBai.Enabled = true;
                cmbMonThi.Enabled = false;
                cmbLanThi.Enabled = false;
                dtpNgayThi.Enabled = false;

                groupBox1.Enabled = true;

                // Bắt đầu đếm giờ.
                timer1.Start();
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

            // Xóa lựa chọn cũ trước khi tick lại đáp án đã chọn.
            rdoA.Checked = false;
            rdoB.Checked = false;
            rdoC.Checked = false;
            rdoD.Checked = false;

            // Quay lại câu cũ thì tick lại đáp án đã chọn.
            if (ch.DapAnDaChon == "A") rdoA.Checked = true;
            else if (ch.DapAnDaChon == "B") rdoB.Checked = true;
            else if (ch.DapAnDaChon == "C") rdoC.Checked = true;
            else if (ch.DapAnDaChon == "D") rdoD.Checked = true;

            // Câu đầu/cuối thì khóa nút Trước/Sau tương ứng.
            btnCauTruoc.Enabled = (index > 0);
            btnCauSau.Enabled = (index < danhSachCauHoi.Count - 1);
        }

        // Lưu đáp án đang tick vào object câu hỏi hiện tại.
        private void LuuDapAn()
        {
            if (danhSachCauHoi.Count == 0) return;

            if (rdoA.Checked) danhSachCauHoi[cauHienTai].DapAnDaChon = "A";
            else if (rdoB.Checked) danhSachCauHoi[cauHienTai].DapAnDaChon = "B";
            else if (rdoC.Checked) danhSachCauHoi[cauHienTai].DapAnDaChon = "C";
            else if (rdoD.Checked) danhSachCauHoi[cauHienTai].DapAnDaChon = "D";
        }

        private void btnCauSau_Click(object sender, EventArgs e)
        {
            LuuDapAn(); // Trước khi chuyển câu thì giữ lại đáp án đang chọn.
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
