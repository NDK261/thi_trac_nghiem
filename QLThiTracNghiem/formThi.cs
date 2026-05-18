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
        // Cấu trúc để lưu 1 câu hỏi
        public class CauHoiThi
        {
            public int MaCauHoi { get; set; }
            public string NoiDung { get; set; }
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
            public string D { get; set; }
            public string DapAnDung { get; set; }
            public string DapAnDaChon { get; set; } // Lưu lại đáp án SV chọn
        }

        // Các biến toàn cục
        List<CauHoiThi> danhSachCauHoi = new List<CauHoiThi>();
        int cauHienTai = 0; // Đang ở câu số mấy
        int tongSoCau = 0;
        int thoiGianConLai = 0; // Tính bằng giây

        string maLop;
        string maMon;
        string trinhDo;
        int lanThi;
        DateTime ngayThiDangThi; // Ngày thi mà sinh viên đã chọn trước khi bấm Bắt đầu.
        Label lblThongTinLichThi;

        public formThi()
        {
            InitializeComponent();

            // Giao diện cũ chỉ có ComboBox môn/lần/ngày, chưa có chỗ hiện trình độ của ca thi.
            // Tạo thêm một Label nhỏ bằng code để sinh viên thấy rõ ca thi đang chọn thuộc trình độ nào.
            lblThongTinLichThi = new Label();
            lblThongTinLichThi.AutoSize = true;
            lblThongTinLichThi.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblThongTinLichThi.Location = new Point(330, 201);
            lblThongTinLichThi.Text = "";
            this.Controls.Add(lblThongTinLichThi);

            // Các sự kiện này giúp danh sách lần thi và thông tin trình độ tự cập nhật
            // khi sinh viên đổi môn hoặc đổi ngày thi.
            cmbMonThi.SelectedIndexChanged += cmbMonThi_SelectedIndexChanged;
            cmbLanThi.SelectedIndexChanged += cmbLanThi_SelectedIndexChanged;
            dtpNgayThi.ValueChanged += dtpNgayThi_ValueChanged;
        }

        private void btnCauTruoc_Click(object sender, EventArgs e)
        {
            LuuDapAn(); // Lưu lại đáp án của câu hiện tại trước khi chuyển đi
            if (cauHienTai > 0)
            {
                cauHienTai--;
                HienThiCauHoi(cauHienTai);
            }
        }

        private DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            // Dùng SqlCommand có parameter thay vì ghép chuỗi SQL trực tiếp.
            // Cách này an toàn hơn và tránh lỗi khi mã môn/mã lớp có khoảng trắng do kiểu NCHAR trong SQL Server.
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
            // Bộ đề được lấy qua Stored Procedure SP_LAY_DE_THI trong SQL Server.
            // SP này đang xử lý bốc ngẫu nhiên theo luật 70% trình độ chính + 30% trình độ thấp hơn.
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

            // Theo file Word đề tài, sinh viên được chọn môn học, ngày thi và lần thi.
            // Vì vậy danh sách môn không khóa cứng theo ngày hệ thống, mà lọc theo ngày sinh viên đang chọn.
            // Nếu ngày đó có lịch thi chưa làm thì sinh viên được bấm Bắt đầu thi.
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
            // Hiển thị thông tin sinh viên
            lblHoTen.Text = "Họ Tên SV: " + Program.mHoTen;

            // Form vừa mở lên chỉ cho chọn ca thi và bấm Bắt đầu.
            // Các nút chuyển câu/nộp bài và khu vực đáp án chỉ mở sau khi lấy đề thành công.
            btnCauTruoc.Enabled = false;
            btnCauSau.Enabled = false;
            btnNopBai.Enabled = false;
            groupBox1.Enabled = false;

            // Tự động truy vấn Mã Lớp và Tên Lớp dựa vào Mã Sinh Viên đang đăng nhập
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

            // File Word yêu cầu sinh viên được chọn ngày thi.
            // DateTimePicker ở đây dùng để tra lịch theo ngày đã chọn.
            // Nếu ngày đang chọn có lịch thi hợp lệ thì sinh viên được bấm Bắt đầu.
            dtpNgayThi.Value = DateTime.Today;
            dtpNgayThi.Enabled = true;
            lblNgayThi.Text = "Ngày thi:";

            LoadMonThiTheoNgayDaChon();

            // Khóa các control chưa cho phép bấm
            btnCauTruoc.Enabled = false;
            btnCauSau.Enabled = false;
            btnNopBai.Enabled = false;
            groupBox1.Enabled = false; // Khóa luôn khu vực chọn đáp án
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
            // 1. Dừng đồng hồ lại
            timer1.Stop();
            LuuDapAn(); // Lưu đáp án của câu cuối cùng đang làm dở

            // 2. Thuật toán chấm điểm
            int soCauDung = 0;
            foreach (var item in danhSachCauHoi)
            {
                string dapAnSinhVien = item.DapAnDaChon?.Trim();
                string dapAnDung = item.DapAnDung?.Trim();

                // DAP_AN trong SQL là kiểu CHAR/NCHAR nên có thể có khoảng trắng dư ở cuối.
                // Trim + so sánh không phân biệt hoa/thường giúp tránh lỗi chấm sai vì dữ liệu bị padding.
                if (!string.IsNullOrWhiteSpace(dapAnSinhVien) &&
                    string.Equals(dapAnSinhVien, dapAnDung, StringComparison.OrdinalIgnoreCase))
                {
                    soCauDung++;
                }
            }

            // Tính điểm trên thang điểm 10
            double diem = Math.Round((double)soCauDung * 10 / tongSoCau, 2);

            // 3. Hiển thị kết quả cho sinh viên
            MessageBox.Show($"Kết quả bài thi của bạn:\n- Số câu đúng: {soCauDung}/{tongSoCau}\n- Điểm: {diem}", "Kết quả");

            // 4. Lưu kết quả vào cơ sở dữ liệu bằng Stored Procedure của SQL Server.
            // Ngoài điểm tổng trong BANGDIEM, ta lưu thêm từng câu vào CT_BAITHI.
            // Bảng chi tiết này giúp biết chính xác câu hỏi nào đã được dùng,
            // từ đó database có thể chặn xóa câu hỏi đã xuất hiện trong bài thi.
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();

                    // Dùng transaction để điểm tổng và chi tiết bài thi đi cùng nhau:
                    // nếu lưu chi tiết bị lỗi thì rollback luôn điểm tổng, tránh dữ liệu nửa vời.
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand("SP_GHI_DIEM_THI", conn, tran))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@MASV", Program.mUserName); // Mã SV đang thi
                                cmd.Parameters.AddWithValue("@MAMH", maMon);
                                cmd.Parameters.AddWithValue("@LAN", lanThi);
                                // Lưu đúng ngày thi mà sinh viên đã chọn trên form,
                                // vì đề tài cho chọn ngày thi chứ không bắt buộc lấy ngày hệ thống.
                                cmd.Parameters.AddWithValue("@NGAYTHI", ngayThiDangThi);
                                cmd.Parameters.AddWithValue("@DIEM", diem);

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

                // 5. Sau khi thi xong, khóa giao diện hoặc đóng form
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
            // Nếu đang thi (đồng hồ đang chạy) mà bấm thoát thì cảnh báo
            if (timer1.Enabled)
            {
                DialogResult dr = MessageBox.Show("Bạn đang trong thời gian làm bài! Nếu thoát bây giờ, hệ thống sẽ tự động nộp bài và tính điểm. Bạn có chắc chắn muốn thoát?", "Cảnh báo nghiêm trọng", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dr == DialogResult.Yes)
                {
                    btnNopBai.PerformClick(); // Tự động gọi nút Nộp Bài để lưu điểm
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

                // 1. Lấy thông số của ca thi đang chọn.
                // Các biến này là biến toàn cục để lát nữa Nộp bài dùng lại đúng môn/lần thi vừa bắt đầu.
                maMon = cmbMonThi.SelectedValue.ToString().Trim();
                lanThi = int.Parse(cmbLanThi.Text);
                DateTime ngayThi = dtpNgayThi.Value.Date;

                // Sinh viên được chọn ngày thi theo file Word đề tài.
                // Nếu ngày đó có đăng ký thi hợp lệ thì cho bắt đầu thi luôn theo lịch đã chọn.
                ngayThiDangThi = ngayThi;

                if (DaThi(maMon, lanThi))
                {
                    MessageBox.Show("Bạn đã thi môn này ở lần thi đang chọn rồi, không được thi lại!", "Không cho thi lại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadMonThiTheoNgayDaChon();
                    return;
                }

                // Lần 2 chỉ hợp lệ khi sinh viên đã có điểm lần 1 của cùng môn.
                // Điều này xử lý góp ý "thi lần 2 sau lần 1".
                if (lanThi == 2 && !DaThi(maMon, 1))
                {
                    MessageBox.Show("Bạn phải thi lần 1 trước rồi mới được thi lần 2 của môn này!", "Sai thứ tự lần thi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Tìm lịch thi: Khớp Môn, Lớp, Lần VÀ đúng cái NGÀY THI
                DataTable dtDangKy = LayThongTinDangKy(maMon, lanThi, ngayThi);

                if (dtDangKy.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy lịch thi nào khớp với Môn học, Lần thi và Ngày thi mà bạn vừa chọn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 3. Đã qua được bước trên nghĩa là đúng lịch thi 100%, cứ thế Lấy các thông số cấu hình của đợt thi đó
                tongSoCau = Convert.ToInt32(dtDangKy.Rows[0]["SOCAUTHI"]);
                int soPhut = Convert.ToInt32(dtDangKy.Rows[0]["THOIGIAN"]);
                trinhDo = dtDangKy.Rows[0]["TRINHDO"].ToString().Trim();
                thoiGianConLai = soPhut * 60;

                // 4. Bốc bộ đề ngẫu nhiên từ CSDL dựa vào Môn và Trình Độ
                DataTable dtDeThi = LayDeThi(maMon, trinhDo, tongSoCau);

                if (dtDeThi.Rows.Count < tongSoCau)
                {
                    MessageBox.Show("Lỗi: Kho đề không đủ số câu hỏi yêu cầu!", "Báo lỗi");
                    return;
                }

                // 5. Đổ dữ liệu từ DataTable vào List để dễ lật qua lật lại
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

                // 6. Mở khóa giao diện và hiển thị câu đầu tiên
                cauHienTai = 0;
                HienThiCauHoi(cauHienTai);

                // Khóa các nút không cho chọn lại khi đang thi
                btnBatDau.Enabled = false;
                btnNopBai.Enabled = true;
                cmbMonThi.Enabled = false;
                cmbLanThi.Enabled = false;
                dtpNgayThi.Enabled = false;

                groupBox1.Enabled = true;

                // 7. Khởi động đồng hồ
                timer1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy đề thi: " + ex.Message, "Báo lỗi");
            }
        }
        // Hàm hiển thị câu hỏi lên giao diện
        private void HienThiCauHoi(int index)
        {
            if (index < 0 || index >= danhSachCauHoi.Count) return;

            CauHoiThi ch = danhSachCauHoi[index];

            // Cập nhật nhãn câu hỏi
            lblNoiDungCauHoi.Text = $"Câu {index + 1}/{tongSoCau}: {ch.NoiDung}";

            // Cập nhật chữ cho 4 đáp án
            rdoA.Text = ch.A;
            rdoB.Text = ch.B;
            rdoC.Text = ch.C;
            rdoD.Text = ch.D;

            // Xóa các lựa chọn cũ đi để làm mới
            rdoA.Checked = false;
            rdoB.Checked = false;
            rdoC.Checked = false;
            rdoD.Checked = false;

            // Phục hồi lại đáp án sinh viên đã chọn (nếu có) khi họ bấm nút "Câu Trước" quay lại
            if (ch.DapAnDaChon == "A") rdoA.Checked = true;
            else if (ch.DapAnDaChon == "B") rdoB.Checked = true;
            else if (ch.DapAnDaChon == "C") rdoC.Checked = true;
            else if (ch.DapAnDaChon == "D") rdoD.Checked = true;

            // Bật/tắt nút Câu Trước, Câu Sau
            btnCauTruoc.Enabled = (index > 0); // Đang ở câu đầu thì tắt nút Trước
            btnCauSau.Enabled = (index < danhSachCauHoi.Count - 1); // Đang ở câu cuối thì tắt nút Sau
        }

        // Hàm ngầm để ghi nhớ đáp án sinh viên vừa chọn vào bộ nhớ
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
            LuuDapAn(); // Lưu lại đáp án của câu hiện tại
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
                thoiGianConLai--; // Trừ đi 1 giây

                // Tính phút và giây để hiển thị lên nhãn (lblThoiGian hoặc ô em đặt tên đồng hồ)
                int phut = thoiGianConLai / 60;
                int giay = thoiGianConLai % 60;

                // Hiển thị định dạng 00:00 (Ví dụ: 14:59)
                lblThoiGian.Text = string.Format("{0:00}:{1:00}", phut, giay);

                // Cảnh báo khi còn dưới 30 giây (đổi màu đỏ cho kịch tính)
                if (thoiGianConLai <= 30)
                {
                    lblThoiGian.ForeColor = System.Drawing.Color.Red;
                }
            }
            else
            {
                // Hết giờ! Tự động nộp bài
                timer1.Stop();
                MessageBox.Show("Đã hết thời gian làm bài!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnNopBai.PerformClick(); // Tự gọi nút Nộp bài
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
