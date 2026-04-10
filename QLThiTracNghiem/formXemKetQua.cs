using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formXemKetQua : Form
    {
        private DataTable dtKyThi;

        public formXemKetQua()
        {
            InitializeComponent();
        }

        private void formXemKetQua_Load(object sender, EventArgs e)
        {
            CaiDatGrid();
            LoadDanhSachKyThi();
        }

        private void CaiDatGrid()
        {
            dgvKetQua.ReadOnly = true;
            dgvKetQua.AllowUserToAddRows = false;
            dgvKetQua.AllowUserToDeleteRows = false;
            dgvKetQua.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKetQua.MultiSelect = false;
            dgvKetQua.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadDanhSachKyThi()
        {
            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("dbo.SP_DS_KETQUA_SINHVIEN", conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Giả sử Program.mUserName đang lưu MASV sau khi sinh viên đăng nhập
                    cmd.Parameters.AddWithValue("@MASV", Program.mUserName);

                    dtKyThi = new DataTable();
                    da.Fill(dtKyThi);

                    if (dtKyThi.Rows.Count == 0)
                    {
                        MessageBox.Show("Sinh viên này chưa có kết quả thi nào.", "Thông báo");
                        cmbKyThi.DataSource = null;
                        return;
                    }

                    // Tạo cột hiển thị đẹp hơn cho ComboBox
                    if (!dtKyThi.Columns.Contains("HIENTHI"))
                        dtKyThi.Columns.Add("HIENTHI", typeof(string));

                    foreach (DataRow row in dtKyThi.Rows)
                    {
                        string tenmh = row["TENMH"].ToString().Trim();
                        string lan = row["LAN"].ToString().Trim();
                        string ngay = row["NGAYTHI"] == DBNull.Value
                            ? ""
                            : Convert.ToDateTime(row["NGAYTHI"]).ToString("dd/MM/yyyy");

                        row["HIENTHI"] = $"{tenmh} - Lần {lan} - {ngay}";
                    }

                    cmbKyThi.DataSource = dtKyThi;
                    cmbKyThi.DisplayMember = "HIENTHI";
                    cmbKyThi.ValueMember = "MAMH";
                    cmbKyThi.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách kỳ thi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnXem_Click(object sender, EventArgs e)
        {
            if (cmbKyThi.SelectedIndex < 0 || dtKyThi == null || dtKyThi.Rows.Count == 0)
            {
                MessageBox.Show("Không có kỳ thi để xem.", "Thông báo");
                return;
            }

            DataRowView drv = cmbKyThi.SelectedItem as DataRowView;
            if (drv == null) return;

            string masv = drv["MASV"].ToString().Trim();
            string mamh = drv["MAMH"].ToString().Trim();
            int lan = Convert.ToInt32(drv["LAN"]);

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("dbo.SP_XEM_KETQUA", conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MASV", masv);
                    cmd.Parameters.AddWithValue("@MAMH", mamh);
                    cmd.Parameters.AddWithValue("@LAN", lan);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Không có dữ liệu chi tiết bài thi.", "Thông báo");
                        dgvKetQua.DataSource = null;
                        XoaThongTinHeader();
                        return;
                    }

                    // Đổ phần thông tin đầu form
                    lblMaSVValue.Text = dt.Rows[0]["MASV"].ToString().Trim();
                    lblHoTenValue.Text = dt.Rows[0]["HOTEN"].ToString().Trim();
                    lblLopValue.Text = dt.Rows[0]["LOP"].ToString().Trim();
                    lblMonThiValue.Text = dt.Rows[0]["MONTHI"].ToString().Trim();
                    lblNgayThiValue.Text = dt.Rows[0]["NGAYTHI"] == DBNull.Value
                        ? "---"
                        : Convert.ToDateTime(dt.Rows[0]["NGAYTHI"]).ToString("dd/MM/yyyy");
                    lblLanThiValue.Text = dt.Rows[0]["LAN"].ToString().Trim();
                    lblTrinhDoValue.Text = dt.Rows[0]["TRINHDO"] == DBNull.Value
                        ? "---"
                        : dt.Rows[0]["TRINHDO"].ToString().Trim();

                    dgvKetQua.DataSource = dt;

                    // Ẩn các cột header lặp lại
                    AnCotNeuCo("LOP");
                    AnCotNeuCo("HOTEN");
                    AnCotNeuCo("MASV");
                    AnCotNeuCo("MONTHI");
                    AnCotNeuCo("NGAYTHI");
                    AnCotNeuCo("LAN");
                    AnCotNeuCo("TRINHDO");

                    // Đặt tên cột
                    DatHeader("STT_HIENTHI", "STT");
                    DatHeader("CAUSO", "Câu số");
                    DatHeader("NOIDUNG", "Nội dung câu hỏi");
                    DatHeader("A", "A");
                    DatHeader("B", "B");
                    DatHeader("C", "C");
                    DatHeader("D", "D");
                    DatHeader("TRALOI_SV", "Trả lời của SV");
                    DatHeader("DAP_AN", "Đáp án");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xem kết quả: " + ex.Message, "Báo lỗi");
            }
        }

        private void DatHeader(string columnName, string header)
        {
            if (dgvKetQua.Columns.Contains(columnName))
                dgvKetQua.Columns[columnName].HeaderText = header;
        }

        private void AnCotNeuCo(string columnName)
        {
            if (dgvKetQua.Columns.Contains(columnName))
                dgvKetQua.Columns[columnName].Visible = false;
        }

        private void XoaThongTinHeader()
        {
            lblMaSVValue.Text = "---";
            lblHoTenValue.Text = "---";
            lblLopValue.Text = "---";
            lblMonThiValue.Text = "---";
            lblNgayThiValue.Text = "---";
            lblLanThiValue.Text = "---";
            lblTrinhDoValue.Text = "---";
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}