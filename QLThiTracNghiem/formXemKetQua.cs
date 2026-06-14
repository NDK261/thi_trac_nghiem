using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formXemKetQua : Form
    {
        private DataTable dtKyThi;
        private readonly string maSvDuocChon;
        private readonly string maMhDuocChon;
        private readonly int? lanDuocChon;

        public formXemKetQua()
        {
            InitializeComponent();
        }

        public formXemKetQua(string masv, string mamh, int lan) : this()
        {
            maSvDuocChon = (masv ?? "").Trim();
            maMhDuocChon = (mamh ?? "").Trim();
            lanDuocChon = lan;
        }

        private void formXemKetQua_Load(object sender, EventArgs e)
        {
            CaiDatGrid();
            LoadDanhSachKyThi();

            if (!string.IsNullOrWhiteSpace(maSvDuocChon) &&
                !string.IsNullOrWhiteSpace(maMhDuocChon) &&
                lanDuocChon.HasValue &&
                dtKyThi != null &&
                dtKyThi.Rows.Count > 0)
            {
                if (ChonKyThiDuocMo())
                    btnXem_Click(this, EventArgs.Empty);
                else
                    MessageBox.Show("Không tìm thấy kỳ thi tương ứng của sinh viên này.", "Thông báo");
            }
        }

        private void CaiDatGrid()
        {
            dgvKetQua.ReadOnly = true;
            dgvKetQua.AllowUserToAddRows = false;
            dgvKetQua.AllowUserToDeleteRows = false;
            dgvKetQua.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKetQua.MultiSelect = false;
            dgvKetQua.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKetQua.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvKetQua.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKetQua.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvKetQua.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            dgvKetQua.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvKetQua.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvKetQua.RowTemplate.Height = 46;
            dgvKetQua.BackgroundColor = Color.White;
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

                    // Sinh viên tự xem thì dùng MASV đang đăng nhập; giáo viên mở từ bảng điểm thì dùng MASV được truyền vào.
                    string masv = string.IsNullOrWhiteSpace(maSvDuocChon) ? Program.mUserName : maSvDuocChon;
                    cmd.Parameters.AddWithValue("@MASV", masv);

                    dtKyThi = new DataTable();
                    da.Fill(dtKyThi);

                    if (dtKyThi.Rows.Count == 0)
                    {
                        MessageBox.Show("Sinh viên này chưa có kết quả thi nào.", "Thông báo");
                        cmbKyThi.DataSource = null;
                        return;
                    }

                    // Tạo cột phụ chỉ để ComboBox hiển thị kỳ thi dễ nhìn hơn.
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

        private bool ChonKyThiDuocMo()
        {
            if (dtKyThi == null || dtKyThi.Rows.Count == 0) return false;

            foreach (object item in cmbKyThi.Items)
            {
                DataRowView row = item as DataRowView;
                if (row == null) continue;

                string mamh = row["MAMH"].ToString().Trim();
                int lan = Convert.ToInt32(row["LAN"]);

                if (mamh == maMhDuocChon && lanDuocChon.HasValue && lan == lanDuocChon.Value)
                {
                    cmbKyThi.SelectedItem = row;
                    return true;
                }
            }

            cmbKyThi.SelectedIndex = -1;
            return false;
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

                    // Thông tin đầu form lấy từ dòng đầu vì các dòng chi tiết cùng một bài thi.
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

                    // Các cột thông tin chung đã hiển thị ở đầu form nên ẩn khỏi lưới chi tiết.
                    AnCotNeuCo("LOP");
                    AnCotNeuCo("HOTEN");
                    AnCotNeuCo("MASV");
                    AnCotNeuCo("MONTHI");
                    AnCotNeuCo("NGAYTHI");
                    AnCotNeuCo("LAN");
                    AnCotNeuCo("TRINHDO");

                    // Đặt lại tiêu đề cột cho đúng ngôn ngữ người dùng.
                    DatHeader("STT_HIENTHI", "STT");
                    DatHeader("CAUSO", "Câu số");
                    DatHeader("NOIDUNG", "Nội dung câu hỏi");
                    DatHeader("A", "A");
                    DatHeader("B", "B");
                    DatHeader("C", "C");
                    DatHeader("D", "D");
                    DatHeader("TRALOI_SV", "Trả lời của SV");
                    DatHeader("DAP_AN", "Đáp án");
                    DinhDangCotKetQua();
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

        private void DinhDangCotKetQua()
        {
            dgvKetQua.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            DatTyLeCot("STT_HIENTHI", 42, 42);
            DatTyLeCot("CAUSO", 58, 58);
            DatTyLeCot("NOIDUNG", 260, 170);
            DatTyLeCot("A", 150, 110);
            DatTyLeCot("B", 150, 110);
            DatTyLeCot("C", 150, 110);
            DatTyLeCot("D", 150, 110);
            DatTyLeCot("TRALOI_SV", 70, 70);
            DatTyLeCot("DAP_AN", 62, 62);

            CanGiuaCot("STT_HIENTHI");
            CanGiuaCot("CAUSO");
            CanGiuaCot("TRALOI_SV");
            CanGiuaCot("DAP_AN");
        }

        private void DatTyLeCot(string columnName, float fillWeight, int minimumWidth)
        {
            if (!dgvKetQua.Columns.Contains(columnName)) return;

            DataGridViewColumn column = dgvKetQua.Columns[columnName];
            column.FillWeight = fillWeight;
            column.MinimumWidth = minimumWidth;
        }

        private void CanGiuaCot(string columnName)
        {
            if (dgvKetQua.Columns.Contains(columnName))
                dgvKetQua.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
