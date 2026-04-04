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

        // Giả sử các thông tin này được truyền từ màn hình Chọn Môn Thi trước đó
        string maLop = "TH04";
        string maMon = "AVCB";
        string trinhDo = "A";
        int lanThi = 1;

        public formThi()
        {
            InitializeComponent();
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

        private void formThi_Load(object sender, EventArgs e)
        {
            // Hiển thị thông tin sinh viên
            lblHoTen.Text = "Họ Tên SV: " + Program.mHoTen;

            // Đổ dữ liệu Lớp
            DataTable dtLop = DBHelper.GetDataTable("EXEC SP_GET_LOP");
            cmbLop.DataSource = dtLop;
            cmbLop.DisplayMember = "TENLOP";
            cmbLop.ValueMember = "MALOP";

            // Đổ dữ liệu Môn Học
            DataTable dtMon = DBHelper.GetDataTable("EXEC SP_GET_MONHOC");
            cmbMonThi.DataSource = dtMon;
            cmbMonThi.DisplayMember = "TENMH";
            cmbMonThi.ValueMember = "MAMH";

            // Đổ dữ liệu Lần Thi
            cmbLanThi.Items.Clear();
            cmbLanThi.Items.AddRange(new string[] { "1", "2" });
            cmbLanThi.SelectedIndex = 0;

            lblNgayThi.Text = "Ngày thi: " + DateTime.Now.ToString("dd/MM/yyyy");

            // Khóa các control chưa cho phép bấm
            btnCauTruoc.Enabled = false;
            btnCauSau.Enabled = false;
            btnNopBai.Enabled = false;
            groupBox1.Enabled = false; // Khóa luôn khu vực chọn đáp án
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

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
                if (item.DapAnDaChon == item.DapAnDung)
                {
                    soCauDung++;
                }
            }

            // Tính điểm trên thang điểm 10
            double diem = Math.Round((double)soCauDung * 10 / tongSoCau, 2);

            // 3. Hiển thị kết quả cho sinh viên
            MessageBox.Show($"Kết quả bài thi của bạn:\n- Số câu đúng: {soCauDung}/{tongSoCau}\n- Điểm: {diem}", "Kết quả");

            // 4. Lưu kết quả vào cơ sở dữ liệu (Bảng BANGDIEM)
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    // Câu lệnh SQL chèn vào bảng điểm
                    string sql = "INSERT INTO BANGDIEM (MASV, MAMH, LAN, NGAYTHI, DIEM) " +
                                 "VALUES (@MASV, @MAMH, @LAN, @NGAYTHI, @DIEM)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MASV", Program.mUserName); // Mã SV đang thi
                        cmd.Parameters.AddWithValue("@MAMH", maMon);
                        cmd.Parameters.AddWithValue("@LAN", lanThi);
                        cmd.Parameters.AddWithValue("@NGAYTHI", DateTime.Now);
                        cmd.Parameters.AddWithValue("@DIEM", diem);

                        cmd.ExecuteNonQuery();
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
                // 1. Lấy thông tin Lớp, Môn, Lần thi từ ComboBox mà em vừa chọn trên giao diện
                string maMon = cmbMonThi.SelectedValue.ToString();
                string maLop = cmbLop.SelectedValue.ToString();
                int lanThi = int.Parse(cmbLanThi.Text);

                // 2. Tìm lịch thi trong Database dựa vào 3 thông tin trên
                string sqlCheck = $"SELECT SOCAUTHI, THOIGIAN, NGAYTHI, TRINHDO FROM GIAOVIEN_DANGKY WHERE MAMH = '{maMon}' AND MALOP = '{maLop}' AND LAN = {lanThi}";
                DataTable dtDangKy = DBHelper.GetDataTable(sqlCheck);

                if (dtDangKy.Rows.Count == 0)
                {
                    MessageBox.Show("Chưa có lịch thi cho lớp và môn này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 3. Ràng buộc Ngày Thi chặt chẽ
                DateTime ngayThi = Convert.ToDateTime(dtDangKy.Rows[0]["NGAYTHI"]);
                DateTime homNay = DateTime.Now.Date;

                if (homNay < ngayThi.Date)
                {
                    MessageBox.Show($"Chưa đến ngày thi! Lịch thi là ngày {ngayThi.ToString("dd/MM/yyyy")}.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (homNay > ngayThi.Date)
                {
                    MessageBox.Show($"Đã quá hạn thi! Môn này đã thi vào ngày {ngayThi.ToString("dd/MM/yyyy")}.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 4. Lấy các thông số cấu hình của đợt thi đó
                tongSoCau = Convert.ToInt32(dtDangKy.Rows[0]["SOCAUTHI"]);
                int soPhut = Convert.ToInt32(dtDangKy.Rows[0]["THOIGIAN"]);
                string trinhDo = dtDangKy.Rows[0]["TRINHDO"].ToString(); // Tự động biết trình độ A, B hay C
                thoiGianConLai = soPhut * 60;

                // 5. Bốc bộ đề ngẫu nhiên từ CSDL dựa vào Môn và Trình Độ
                string sqlLayDe = $"EXEC SP_LAY_DE_THI '{maMon}', '{trinhDo}', {tongSoCau}";
                DataTable dtDeThi = DBHelper.GetDataTable(sqlLayDe);

                if (dtDeThi.Rows.Count < tongSoCau)
                {
                    MessageBox.Show("Lỗi: Kho đề không đủ số câu hỏi yêu cầu!", "Báo lỗi");
                    return;
                }

                // 6. Đổ dữ liệu từ DataTable vào List để dễ lật qua lật lại
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
                    ch.DapAnDung = row["DAP_AN"].ToString();
                    ch.DapAnDaChon = "";
                    danhSachCauHoi.Add(ch);
                }

                // 7. Mở khóa giao diện và hiển thị câu đầu tiên
                cauHienTai = 0;
                HienThiCauHoi(cauHienTai);

                // Khóa các nút không cho chọn lại khi đang thi
                btnBatDau.Enabled = false;
                btnNopBai.Enabled = true;
                cmbMonThi.Enabled = false;
                cmbLop.Enabled = false;
                cmbLanThi.Enabled = false;

                groupBox1.Enabled = true;

                // 8. Khởi động đồng hồ
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
