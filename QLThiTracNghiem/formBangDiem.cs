using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formBangDiem : Form
    {
        public formBangDiem()
        {
            InitializeComponent();
        }

        private void formBangDiem_Load(object sender, EventArgs e)
        {
            CaiDatGrid();
            LoadDanhSachLop();
            LoadDanhSachMonHoc();
            LoadDanhSachLanThi();
        }

        private void CaiDatGrid()
        {
            dgvBangDiem.ReadOnly = true;
            dgvBangDiem.AllowUserToAddRows = false;
            dgvBangDiem.AllowUserToDeleteRows = false;
            dgvBangDiem.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBangDiem.MultiSelect = false;
            dgvBangDiem.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadDanhSachLop()
        {
            try
            {
                // Lấy danh sách lớp từ SP để chọn điều kiện in/xem bảng điểm.
                using (SqlConnection conn = DBHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("dbo.SP_GET_LOP", conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbLop.DataSource = dt;
                    cmbLop.DisplayMember = "TENLOP";
                    cmbLop.ValueMember = "MALOP";
                    cmbLop.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp: " + ex.Message, "Báo lỗi");
            }
        }

        private void LoadDanhSachMonHoc()
        {
            try
            {
                // Lấy danh sách môn học từ SP, tránh viết lại SELECT ở nhiều form.
                using (SqlConnection conn = DBHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("dbo.SP_GET_MONHOC", conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbMonHoc.DataSource = dt;
                    cmbMonHoc.DisplayMember = "TENMH";
                    cmbMonHoc.ValueMember = "MAMH";
                    cmbMonHoc.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách môn học: " + ex.Message, "Báo lỗi");
            }
        }

        private void LoadDanhSachLanThi()
        {
            cmbLanThi.Items.Clear();
            cmbLanThi.Items.Add("Lần 1");
            cmbLanThi.Items.Add("Lần 2");
            cmbLanThi.SelectedIndex = -1;
        }

        private bool TryGetLanThi(out int lan)
        {
            lan = 0;

            if (cmbLanThi.SelectedIndex == -1)
            {
                return false;
            }

            switch (cmbLanThi.SelectedIndex)
            {
                case 0:
                    lan = 1;
                    return true;
                case 1:
                    lan = 2;
                    return true;
                default:
                    return false;
            }
        }

        private void btnXem_Click(object sender, EventArgs e)
        {
            if (cmbLop.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn lớp!", "Thông báo");
                return;
            }

            if (cmbMonHoc.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn môn học!", "Thông báo");
                return;
            }

            if (cmbLanThi.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn lần thi!", "Thông báo");
                return;
            }

            if (!TryGetLanThi(out int lan))
            {
                MessageBox.Show("Lần thi chỉ được chọn lần 1 hoặc lần 2!", "Thông báo");
                return;
            }

            try
            {
                string malop = cmbLop.SelectedValue.ToString();
                string mamh = cmbMonHoc.SelectedValue.ToString();

                using (SqlConnection conn = DBHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("dbo.SP_GET_BANGDIEM", conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Ba tham số này đúng với khóa tra cứu bảng điểm: lớp, môn và lần thi.
                    cmd.Parameters.AddWithValue("@MALOP", malop);
                    cmd.Parameters.AddWithValue("@MAMH", mamh);
                    cmd.Parameters.AddWithValue("@LAN", lan);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Không có dữ liệu bảng điểm cho lớp và môn học này!", "Thông báo");
                        dgvBangDiem.DataSource = null;
                        return;
                    }

                    // Đổi tên cột cho dễ đọc, dữ liệu vẫn lấy từ SP_GET_BANGDIEM.
                    dgvBangDiem.DataSource = dt;

                    if (dgvBangDiem.Columns.Contains("STT"))
                        dgvBangDiem.Columns["STT"].HeaderText = "STT";

                    if (dgvBangDiem.Columns.Contains("MASV"))
                        dgvBangDiem.Columns["MASV"].HeaderText = "Mã SV";

                    if (dgvBangDiem.Columns.Contains("HOTEN"))
                        dgvBangDiem.Columns["HOTEN"].HeaderText = "Họ Tên";

                    if (dgvBangDiem.Columns.Contains("DIEM"))
                        dgvBangDiem.Columns["DIEM"].HeaderText = "Điểm";

                    if (dgvBangDiem.Columns.Contains("DIEMCHU"))
                        dgvBangDiem.Columns["DIEMCHU"].HeaderText = "Điểm Chữ";

                    lblThongTin.Text = $"Lớp: {cmbLop.Text} | Môn: {cmbMonHoc.Text} | Lần: {cmbLanThi.Text} | Số SV: {dgvBangDiem.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xem bảng điểm: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnIn_Click(object sender, EventArgs e)
        {
            if (dgvBangDiem.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để in!", "Thông báo");
                return;
            }

            // Phần in báo cáo chưa làm, hiện chỉ nhắc người dùng thay vì để nút im lặng.
            MessageBox.Show("Chức năng in đang được phát triển.", "Thông báo");
        }

        private void btnXemBaiThi_Click(object sender, EventArgs e)
        {
            if (dgvBangDiem.CurrentRow == null || dgvBangDiem.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Vui lòng chọn sinh viên cần xem bài thi.", "Thông báo");
                return;
            }

            if (cmbMonHoc.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn môn học.", "Thông báo");
                return;
            }

            if (!TryGetLanThi(out int lan))
            {
                MessageBox.Show("Vui lòng chọn lần thi.", "Thông báo");
                return;
            }

            if (!dgvBangDiem.Columns.Contains("MASV"))
            {
                MessageBox.Show("Bảng điểm chưa có cột mã sinh viên.", "Báo lỗi");
                return;
            }

            string masv = dgvBangDiem.CurrentRow.Cells["MASV"].Value?.ToString().Trim();
            if (string.IsNullOrWhiteSpace(masv))
            {
                MessageBox.Show("Không đọc được mã sinh viên từ dòng đang chọn.", "Báo lỗi");
                return;
            }

            if (dgvBangDiem.Columns.Contains("DIEM"))
            {
                object diem = dgvBangDiem.CurrentRow.Cells["DIEM"].Value;
                if (diem == null || diem == DBNull.Value || string.IsNullOrWhiteSpace(diem.ToString()))
                {
                    MessageBox.Show("Sinh viên này chưa có điểm/bài thi để xem chi tiết.", "Thông báo");
                    return;
                }
            }

            string mamh = cmbMonHoc.SelectedValue.ToString();
            // Mở lại form xem kết quả với đúng sinh viên, môn và lần thi đang chọn.
            using (formXemKetQua f = new formXemKetQua(masv, mamh, lan))
            {
                f.ShowDialog();
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
