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
            cmbLanThi.Items.Add("Lần 3");
            cmbLanThi.SelectedIndex = -1;
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

            try
            {
                string malop = cmbLop.SelectedValue.ToString();
                string mamh = cmbMonHoc.SelectedValue.ToString();
                int lan = cmbLanThi.SelectedIndex + 1;

                using (SqlConnection conn = DBHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("dbo.SP_GET_BANGDIEM", conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
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

                    // Cấu hình cột hiển thị
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

            // TODO: Implement printing functionality
            MessageBox.Show("Chức năng in đang được phát triển.", "Thông báo");
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
