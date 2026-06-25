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
    public partial class formLopSinhVien : Form
    {
        bool isAdding = false; // Cờ này cho biết nút Ghi đang thêm mới hay sửa sinh viên.
        DataTable dtSinhVienGoc; // Giữ danh sách sinh viên gốc để tìm kiếm ngay trên form.
        private readonly string maLopDuocTruyen;
        private bool embeddedMode => !this.TopLevel;

        public formLopSinhVien()
        {
            InitializeComponent();
            this.Resize += formLopSinhVien_Resize;
            WireUpFocusEvents();
        }

        // --- PUBLIC STATE FOR PARENT CONTROL ---
        public event EventHandler SubformStateChanged;
        public event EventHandler SubformFocusEntered;

        public bool IsAdding => isAdding;
        public bool IsEditing => isAdding || btnGhi.Enabled;
        public bool HasData => dgvSinhVien.Rows.Count > 0;
        public bool ViewingInactive => chkHienThiNgungHoatDong.Checked;
        public bool HasCurrentRow => dgvSinhVien.CurrentRow != null && !dgvSinhVien.CurrentRow.IsNewRow;

        private void NotifyStateChanged()
        {
            SubformStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void WireUpFocusEvents()
        {
            // Báo cho form cha biết người dùng đang bấm vào các control của Sinh Viên
            dgvSinhVien.Enter += (s, e) => SubformFocusEntered?.Invoke(this, EventArgs.Empty);
            txtMaSV.Enter += (s, e) => SubformFocusEntered?.Invoke(this, EventArgs.Empty);
            txtHo.Enter += (s, e) => SubformFocusEntered?.Invoke(this, EventArgs.Empty);
            txtTen.Enter += (s, e) => SubformFocusEntered?.Invoke(this, EventArgs.Empty);
            txtDiaChi.Enter += (s, e) => SubformFocusEntered?.Invoke(this, EventArgs.Empty);
            dtpNgaySinh.Enter += (s, e) => SubformFocusEntered?.Invoke(this, EventArgs.Empty);
            txtTimKiem.Enter += (s, e) => SubformFocusEntered?.Invoke(this, EventArgs.Empty);
        }

        public formLopSinhVien(string maLop) : this()
        {
            maLopDuocTruyen = (maLop ?? "").Trim();
        }

        private void ThietLapGioiHanNhap()
        {
            txtMaSV.MaxLength = 8;
            txtHo.MaxLength = 40;
            txtTen.MaxLength = 10;
            txtDiaChi.MaxLength = 100;
            txtMaSV.CharacterCasing = CharacterCasing.Upper;
            dtpNgaySinh.MaxDate = DateTime.Today;

            cmbTimKiem.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTimKiem.Items.Clear();
            cmbTimKiem.Items.AddRange(new string[] { "Tất cả", "Mã SV", "Họ tên", "Ngày sinh", "Địa chỉ" });
            cmbTimKiem.SelectedIndex = 0;
            
            if (embeddedMode)
            {
                label1.Visible = false;
                cmbLop.Visible = false;
                btnThoat.Visible = false;
                btnThem.Visible = false;
                btnXoa.Visible = false;
                btnSua.Visible = false;
                btnGhi.Visible = false;
                btnPhucHoi.Visible = false;
                btnKhoiPhucSinhVien.Visible = false;
            }
        }

        // Đưa form về trạng thái xem, thêm mới hoặc sửa sinh viên.
        private void SetNormalState()
        {
            isAdding = false;
            SetInputState(false);

            bool viewingInactive = chkHienThiNgungHoatDong.Checked;
            cmbLop.Enabled = true;
            dgvSinhVien.Enabled = true;
            txtTimKiem.Enabled = true;
            cmbTimKiem.Enabled = true;
            btnTimKiem.Enabled = true;
            btnLamMoiTimKiem.Enabled = true;
            chkHienThiNgungHoatDong.Enabled = true;

            btnGhi.Enabled = false;
            btnPhucHoi.Enabled = false;
            btnThem.Enabled = !viewingInactive;

            bool hasCurrentRow = dgvSinhVien.CurrentRow != null && !dgvSinhVien.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow && !viewingInactive;
            btnXoa.Enabled = hasCurrentRow && !viewingInactive;
            btnKhoiPhucSinhVien.Enabled = hasCurrentRow && viewingInactive;
            btnThoat.Enabled = !embeddedMode; // Khi nhúng thì ẩn/khóa Thoát.
            
            NotifyStateChanged();
        }

        private void SetEditingState(bool adding)
        {
            isAdding = adding;
            SetInputState(true);

            // Đang nhập thì khóa lớp và lưới.
            cmbLop.Enabled = false;
            dgvSinhVien.Enabled = false;
            txtTimKiem.Enabled = false;
            cmbTimKiem.Enabled = false;
            btnTimKiem.Enabled = false;
            btnLamMoiTimKiem.Enabled = false;
            chkHienThiNgungHoatDong.Enabled = false;

            // MASV chỉ nhập khi thêm; khi sửa dùng để tìm dòng UPDATE.
            txtMaSV.Enabled = adding;

            btnGhi.Enabled = true;
            btnPhucHoi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
            btnKhoiPhucSinhVien.Enabled = false;
            btnThoat.Enabled = true;

            NotifyStateChanged();
        }

        private void SetInputState(bool enabled)
        {
            txtMaSV.Enabled = enabled;
            txtHo.Enabled = enabled;
            txtTen.Enabled = enabled;
            txtDiaChi.Enabled = enabled;
            dtpNgaySinh.Enabled = enabled;
        }

        private void ClearInput()
        {
            txtMaSV.Clear();
            txtHo.Clear();
            txtTen.Clear();
            txtDiaChi.Clear();
            dtpNgaySinh.Value = DateTime.Today;
        }

        private void LoadCurrentRowToInput()
        {
            if (dgvSinhVien.CurrentRow == null || dgvSinhVien.CurrentRow.IsNewRow)
            {
                ClearInput();
                return;
            }

            DataGridViewRow row = dgvSinhVien.CurrentRow;
            txtMaSV.Text = row.Cells["MASV"].Value?.ToString();
            txtHo.Text = row.Cells["HO"].Value?.ToString();
            txtTen.Text = row.Cells["TEN"].Value?.ToString();
            txtDiaChi.Text = row.Cells["DIACHI"].Value?.ToString();

            // NGAYSINH có thể NULL nên kiểm tra trước khi đổi sang DateTime.
            if (row.Cells["NGAYSINH"].Value != DBNull.Value && row.Cells["NGAYSINH"].Value != null)
            {
                DateTime ngaySinh = Convert.ToDateTime(row.Cells["NGAYSINH"].Value).Date;
                dtpNgaySinh.Value = ngaySinh > DateTime.Today ? DateTime.Today : ngaySinh;
            }
            else
                dtpNgaySinh.Value = DateTime.Today;
        }

        // Mở form thì tải danh sách lớp vào ComboBox.
        private void formLopSinhVien_Resize(object sender, EventArgs e)
        {
            // Removed manual resize to let WinForms Anchor handle the input fields naturally
        }

        private void formLopSinhVien_Load(object sender, EventArgs e)
        {
            try
            {
                ThietLapGioiHanNhap();

                DataTable dtLop = DBHelper.ExecuteDataTable("SP_GET_LOP");
                cmbLop.SelectedIndexChanged -= cmbLop_SelectedIndexChanged;
                
                cmbLop.DisplayMember = "TENLOP"; // Hiển thị tên lớp.
                cmbLop.ValueMember = "MALOP";    // Truy vấn bằng mã lớp.
                cmbLop.DataSource = dtLop;

                // Vừa mở form thì chỉ xem dữ liệu.
                if (!string.IsNullOrWhiteSpace(maLopDuocTruyen))
                {
                    cmbLop.SelectedValue = maLopDuocTruyen;
                    this.Text = "Sinh viên lớp " + maLopDuocTruyen;
                }
                
                cmbLop.SelectedIndexChanged += cmbLop_SelectedIndexChanged;
                
                if (cmbLop.SelectedValue != null)
                {
                    LoadSinhVien(cmbLop.SelectedValue.ToString());
                }

                SetNormalState();
                
                if (embeddedMode)
                {
                    label1.Visible = false;
                    cmbLop.Visible = false;
                    btnThoat.Visible = false;
                    btnThem.Visible = false;
                    btnXoa.Visible = false;
                    btnSua.Visible = false;
                    btnGhi.Visible = false;
                    btnPhucHoi.Visible = false;
                    btnKhoiPhucSinhVien.Visible = false;

                    // Thay đổi Anchor để có thể tự thay đổi kích thước lưới khi nhúng
                    dgvSinhVien.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    
                    Action resizeGrid = () => 
                    {
                        if (txtMaSV != null && dgvSinhVien != null)
                        {
                            dgvSinhVien.Height = Math.Max(0, txtMaSV.Top - dgvSinhVien.Top - 10);
                        }
                    };

                    this.Resize += (s, ev) => resizeGrid();
                    resizeGrid(); // Gọi ngay lần đầu
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách Lớp: " + ex.Message);
            }
        }

        private void cmbLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Chọn lớp nào thì tải sinh viên lớp đó.
            if (cmbLop.SelectedValue != null && cmbLop.SelectedValue is string)
            {
                string maLopDuocChon = cmbLop.SelectedValue.ToString();
                LoadSinhVien(maLopDuocChon);
            }
        }
        // Tải sinh viên theo lớp đang chọn.
        private void LoadSinhVien(string maLop)
        {
            try
            {
                // Dùng SqlParameter thay vì ghép chuỗi SQL.
                string procedureName = chkHienThiNgungHoatDong.Checked
                    ? "SP_GET_SINHVIEN_NGUNG_HOATDONG_THEO_LOP"
                    : "SP_GET_SINHVIEN_THEO_LOP";

                DataTable dtSV = DBHelper.ExecuteDataTable(
                    procedureName,
                    new SqlParameter("@MALOP", maLop));

                dtSinhVienGoc = dtSV;
                dgvSinhVien.DataSource = dtSinhVienGoc;
                txtTimKiem.Clear();

                if (dgvSinhVien.Columns.Count > 0)
                {
                    dgvSinhVien.Columns["MASV"].HeaderText = "Mã SV";
                    dgvSinhVien.Columns["HO"].HeaderText = "Họ";
                    dgvSinhVien.Columns["TEN"].HeaderText = "Tên";
                    dgvSinhVien.Columns["NGAYSINH"].HeaderText = "Ngày Sinh";
                    dgvSinhVien.Columns["DIACHI"].HeaderText = "Địa Chỉ";
                    dgvSinhVien.Columns["DIACHI"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvSinhVien.Columns["NGAYSINH"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }

                dgvSinhVien.ReadOnly = true;
                dgvSinhVien.AllowUserToAddRows = false;
                dgvSinhVien.AllowUserToDeleteRows = false;
                dgvSinhVien.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvSinhVien.MultiSelect = false;
                ApplyGridFormat();

                LoadCurrentRowToInput();
                SetNormalState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách Sinh Viên: " + ex.Message);
            }
        }

        // Tìm kiếm không dấu, ví dụ "thanh" vẫn ra "THÀNH".
        private string ChuyenKhongDau(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                System.Globalization.UnicodeCategory unicodeCategory =
                    System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(System.Text.NormalizationForm.FormC)
                .Replace("đ", "d")
                .Replace("Đ", "d")
                .ToLower();
        }

        private bool IsValidCode(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.All(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'));
        }

        private bool IsValidPersonName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            foreach (char c in value)
            {
                if (char.IsLetter(c) || char.IsWhiteSpace(c))
                    continue;

                return false;
            }

            return true;
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            LocSinhVienTheoTuKhoa();
        }

        private void LocSinhVienTheoTuKhoa()
        {
            if (dtSinhVienGoc == null) return;

            string keyword = ChuyenKhongDau(txtTimKiem.Text.Trim());
            string kieuTim = cmbTimKiem.SelectedItem?.ToString() ?? "Tất cả";

            if (keyword == "")
            {
                dgvSinhVien.DataSource = dtSinhVienGoc;
                ApplyGridFormat();
                LoadCurrentRowToInput();
                SetNormalState();
                return;
            }

            DataTable dtLoc = dtSinhVienGoc.Clone();

            foreach (DataRow row in dtSinhVienGoc.Rows)
            {
                string maSV = ChuyenKhongDau(row["MASV"].ToString());
                string ho = ChuyenKhongDau(row["HO"].ToString());
                string ten = ChuyenKhongDau(row["TEN"].ToString());
                string diaChi = ChuyenKhongDau(row["DIACHI"].ToString());
                string hoTen = ChuyenKhongDau(row["HO"].ToString() + " " + row["TEN"].ToString());
                string ngaySinh = "";
                if (row["NGAYSINH"] != DBNull.Value && row["NGAYSINH"] != null)
                    ngaySinh = Convert.ToDateTime(row["NGAYSINH"]).ToString("dd/MM/yyyy");

                bool khop =
                    (kieuTim == "Tất cả" &&
                        (maSV.Contains(keyword) ||
                         ho.Contains(keyword) ||
                         ten.Contains(keyword) ||
                         hoTen.Contains(keyword) ||
                         ChuyenKhongDau(ngaySinh).Contains(keyword) ||
                         diaChi.Contains(keyword))) ||
                    (kieuTim == "Mã SV" && maSV.Contains(keyword)) ||
                    (kieuTim == "Họ tên" && hoTen.Contains(keyword)) ||
                    (kieuTim == "Ngày sinh" && ChuyenKhongDau(ngaySinh).Contains(keyword)) ||
                    (kieuTim == "Địa chỉ" && diaChi.Contains(keyword));

                if (khop)
                {
                    dtLoc.ImportRow(row);
                }
            }

            dgvSinhVien.DataSource = dtLoc;
            ApplyGridFormat();
            LoadCurrentRowToInput();
            SetNormalState();
        }

        private void ApplyGridFormat()
        {
            dgvSinhVien.ReadOnly = true;
            dgvSinhVien.AllowUserToAddRows = false;
            dgvSinhVien.AllowUserToDeleteRows = false;
            dgvSinhVien.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSinhVien.MultiSelect = false;
            dgvSinhVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSinhVien.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvSinhVien.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvSinhVien.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            dgvSinhVien.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvSinhVien.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvSinhVien.RowTemplate.Height = 40;
            dgvSinhVien.BackgroundColor = Color.White;

            DatHeader("MASV", "Mã SV");
            DatHeader("HO", "Họ");
            DatHeader("TEN", "Tên");
            DatHeader("NGAYSINH", "Ngày sinh");
            DatHeader("DIACHI", "Địa chỉ");

            if (dgvSinhVien.Columns.Contains("MALOP"))
                dgvSinhVien.Columns["MALOP"].Visible = false;

            DatTyLeCot("MASV", 90, 80);
            DatTyLeCot("HO", 180, 130);
            DatTyLeCot("TEN", 90, 80);
            DatTyLeCot("NGAYSINH", 115, 105);
            DatTyLeCot("DIACHI", 340, 220);

            CanGiuaCot("MASV");
            CanGiuaCot("TEN");
            CanGiuaCot("NGAYSINH");

            if (dgvSinhVien.Columns.Contains("NGAYSINH"))
                dgvSinhVien.Columns["NGAYSINH"].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private void DatHeader(string columnName, string header)
        {
            if (dgvSinhVien.Columns.Contains(columnName))
                dgvSinhVien.Columns[columnName].HeaderText = header;
        }

        private void DatTyLeCot(string columnName, float fillWeight, int minimumWidth)
        {
            if (!dgvSinhVien.Columns.Contains(columnName)) return;

            DataGridViewColumn column = dgvSinhVien.Columns[columnName];
            column.FillWeight = fillWeight;
            column.MinimumWidth = minimumWidth;
        }

        private void CanGiuaCot(string columnName)
        {
            if (dgvSinhVien.Columns.Contains(columnName))
                dgvSinhVien.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            LocSinhVienTheoTuKhoa();
        }

        private void btnLamMoiTimKiem_Click(object sender, EventArgs e)
        {
            txtTimKiem.Clear();
            cmbTimKiem.SelectedIndex = 0;
            LocSinhVienTheoTuKhoa();
            txtTimKiem.Focus();
        }

        private void cmbTimKiem_SelectedIndexChanged(object sender, EventArgs e)
        {
            LocSinhVienTheoTuKhoa();
        }

        private void chkHienThiNgungHoatDong_CheckedChanged(object sender, EventArgs e)
        {
            if (cmbLop.SelectedValue != null)
                LoadSinhVien(cmbLop.SelectedValue.ToString());
            NotifyStateChanged();
        }

        private void dgvSinhVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                LoadCurrentRowToInput();
                SetNormalState();
                NotifyStateChanged();
            }
        }
        
        // --- PUBLIC CRUD METHODS FOR PARENT FORM ---
        public void RequestAdd() => btnThem_Click(null, EventArgs.Empty);
        public void RequestEdit() => btnSua_Click(null, EventArgs.Empty);
        public void RequestDelete() => btnXoa_Click(null, EventArgs.Empty);
        public void RequestSave() => btnGhi_Click(null, EventArgs.Empty);
        public void RequestCancel() => btnPhucHoi_Click(null, EventArgs.Empty);
        public void RequestRestoreSV() => btnKhoiPhucSinhVien_Click(null, EventArgs.Empty);

        private void btnThem_Click(object sender, EventArgs e)
        {
            ClearInput();
            SetEditingState(true);
            txtMaSV.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaSV.Text == "") return;
            if (chkHienThiNgungHoatDong.Checked)
            {
                MessageBox.Show("Sinh viên đang ngưng hoạt động. Hãy dùng chức năng Khôi phục SV nếu muốn mở lại.", "Thông báo");
                return;
            }

            bool coDuLieuLienQuan = SinhVienCoDuLieuLienQuan(txtMaSV.Text.Trim(), out int soBangDiem, out int soBaiThiTam);
            string thongBaoXoa = coDuLieuLienQuan
                ? "Sinh viên này đã có dữ liệu liên quan:\n" +
                  $"- Số bảng điểm: {soBangDiem}\n" +
                  $"- Số bài thi tạm: {soBaiThiTam}\n\n" +
                  "Hệ thống sẽ ngưng hoạt động sinh viên thay vì xóa hẳn. Sinh viên này sẽ không đăng nhập/thi được nữa. Bạn có chắc muốn tiếp tục?"
                : "Sinh viên này chưa có dữ liệu thi liên quan.\n\n" +
                  $"Mã SV: {txtMaSV.Text.Trim()}\n" +
                  $"Họ tên: {txtHo.Text.Trim()} {txtTen.Text.Trim()}\n\n" +
                  "Hệ thống sẽ xóa hẳn sinh viên khỏi database. Bạn có chắc muốn xóa?";

            if (MessageBox.Show(thongBaoXoa, "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int result = DBHelper.ExecuteNonQueryWithReturn(
                        "SP_XOA_SINHVIEN",
                        new SqlParameter("@MASV", txtMaSV.Text.Trim()));

                    if (result == 1)
                    {
                        MessageBox.Show("Không thể xóa sinh viên này!", "Báo lỗi");
                    }
                    else if (result == 2)
                    {
                        MessageBox.Show("Sinh viên đã có dữ liệu liên quan nên hệ thống đã ngưng hoạt động sinh viên.", "Thông báo");
                        LoadSinhVien(cmbLop.SelectedValue.ToString());
                    }
                    else
                    {
                        MessageBox.Show("Xóa hẳn sinh viên thành công!", "Thông báo");
                        LoadSinhVien(cmbLop.SelectedValue.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
                }
            }
        }

        private bool SinhVienCoDuLieuLienQuan(string maSV, out int soBangDiem, out int soBaiThiTam)
        {
            soBangDiem = 0;
            soBaiThiTam = 0;

            using (SqlConnection conn = DBHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        SoBangDiem = (SELECT COUNT(*) FROM dbo.BANGDIEM WHERE RTRIM(MASV) = RTRIM(@MASV)),
                        SoBaiThiTam = (SELECT COUNT(*) FROM dbo.BAITHI_TAM WHERE RTRIM(MASV) = RTRIM(@MASV));", conn))
                {
                    cmd.Parameters.AddWithValue("@MASV", maSV);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            soBangDiem = Convert.ToInt32(reader["SoBangDiem"]);
                            soBaiThiTam = Convert.ToInt32(reader["SoBaiThiTam"]);
                        }
                    }
                }
            }

            return soBangDiem > 0 || soBaiThiTam > 0;
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (txtMaSV.Text == "")
            {
                MessageBox.Show("Vui lòng chọn Sinh viên cần sửa trên lưới!", "Thông báo");
                return;
            }
            SetEditingState(false);
            txtHo.Focus();
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            string maSV = txtMaSV.Text.Trim().ToUpper();
            string ho = txtHo.Text.Trim();
            string ten = txtTen.Text.Trim();
            string diaChi = txtDiaChi.Text.Trim();

            if (maSV == "" || ho == "" || ten == "")
            {
                MessageBox.Show("Mã Sinh Viên, Họ và Tên không được để trống!", "Báo lỗi");
                return;
            }

            // Độ dài lấy theo bảng SINHVIEN, kiểm tra trước để báo lỗi dễ hiểu.
            if (maSV.Length > 8)
            {
                MessageBox.Show("Mã sinh viên tối đa 8 ký tự!", "Báo lỗi");
                txtMaSV.Focus();
                return;
            }

            if (!IsValidCode(maSV))
            {
                MessageBox.Show("Mã sinh viên chỉ được gồm chữ cái A-Z và chữ số 0-9!", "Báo lỗi");
                txtMaSV.Focus();
                return;
            }

            if (ho.Length > 40)
            {
                MessageBox.Show("Họ sinh viên tối đa 40 ký tự!", "Báo lỗi");
                txtHo.Focus();
                return;
            }

            if (!IsValidPersonName(ho))
            {
                MessageBox.Show("Họ sinh viên chỉ được nhập chữ và khoảng trắng!", "Báo lỗi");
                txtHo.Focus();
                return;
            }

            if (ten.Length > 10)
            {
                MessageBox.Show("Tên sinh viên tối đa 10 ký tự!", "Báo lỗi");
                txtTen.Focus();
                return;
            }

            if (!IsValidPersonName(ten))
            {
                MessageBox.Show("Tên sinh viên chỉ được nhập chữ và khoảng trắng!", "Báo lỗi");
                txtTen.Focus();
                return;
            }

            if (diaChi.Length > 100)
            {
                MessageBox.Show("Địa chỉ tối đa 100 ký tự!", "Báo lỗi");
                txtDiaChi.Focus();
                return;
            }

            if (cmbLop.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn lớp cho sinh viên!", "Báo lỗi");
                return;
            }

            if (dtpNgaySinh.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Ngày sinh không được lớn hơn ngày hiện tại!", "Báo lỗi");
                dtpNgaySinh.Focus();
                return;
            }

            txtMaSV.Text = maSV;
            txtHo.Text = ho;
            txtTen.Text = ten;
            txtDiaChi.Text = diaChi;

            try
            {
                // isAdding quyết định gọi SP thêm mới hay sửa sinh viên.
                string procedureName = isAdding ? "SP_THEM_SINHVIEN" : "SP_SUA_SINHVIEN";
                int result = DBHelper.ExecuteNonQueryWithReturn(
                    procedureName,
                    new SqlParameter("@MASV", maSV),
                    new SqlParameter("@HO", ho),
                    new SqlParameter("@TEN", ten),
                    new SqlParameter("@NGAYSINH", dtpNgaySinh.Value),
                    new SqlParameter("@DIACHI", diaChi),
                    new SqlParameter("@MALOP", cmbLop.SelectedValue.ToString()));

                if (result == 1)
                {
                    MessageBox.Show("Lỗi: Mã Sinh Viên này đã tồn tại!", "Báo lỗi");
                }
                else if (result == 2)
                {
                    MessageBox.Show("Không tìm thấy sinh viên cần sửa hoặc lớp không tồn tại!", "Báo lỗi");
                }
                else if (result == 3)
                {
                    MessageBox.Show("Dữ liệu sinh viên không hợp lệ. Vui lòng kiểm tra mã SV, họ tên, ngày sinh và độ dài địa chỉ!", "Báo lỗi");
                }
                else if (result == 0)
                {
                    MessageBox.Show("Cập nhật dữ liệu thành công!", "Thông báo");
                    LoadSinhVien(cmbLop.SelectedValue.ToString()); // Lưu xong thì tải lại lưới.
                    SetNormalState(); // Quay về trạng thái xem.
                }
                else
                {
                    MessageBox.Show("Không lưu được sinh viên. Mã lỗi SP: " + result, "Báo lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            isAdding = false;
            if (dgvSinhVien.CurrentRow != null)
                LoadCurrentRowToInput();
            else
                ClearInput();

            SetNormalState();
        }

        private void btnKhoiPhucSinhVien_Click(object sender, EventArgs e)
        {
            if (!chkHienThiNgungHoatDong.Checked)
            {
                MessageBox.Show("Hãy bật 'Hiển thị SV ngưng hoạt động' trước khi khôi phục.", "Thông báo");
                return;
            }

            if (txtMaSV.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng chọn sinh viên cần khôi phục!", "Thông báo");
                return;
            }

            DialogResult dr = MessageBox.Show(
                $"Bạn có chắc muốn khôi phục sinh viên {txtMaSV.Text.Trim()}?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            try
            {
                int result = DBHelper.ExecuteNonQueryWithReturn(
                    "SP_KHOIPHUC_SINHVIEN",
                    new SqlParameter("@MASV", txtMaSV.Text.Trim()));

                if (result == 0)
                {
                    MessageBox.Show("Khôi phục sinh viên thành công!", "Thông báo");
                    LoadSinhVien(cmbLop.SelectedValue.ToString());
                }
                else
                {
                    MessageBox.Show("Không thể khôi phục sinh viên này!", "Báo lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Đang nhập dở thì hỏi lại trước khi thoát.
            if (btnGhi.Enabled)
            {
                DialogResult result = MessageBox.Show(
                    "Bạn đang thêm/sửa sinh viên nhưng chưa ghi dữ liệu. Bạn có chắc muốn thoát không?",
                    "Xác nhận thoát",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;
            }

            this.Close();
        }
    }
}
