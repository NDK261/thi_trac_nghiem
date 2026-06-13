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
        public formLopSinhVien()
        {
            InitializeComponent();
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
        }

        // Gom trạng thái form: xem dữ liệu, thêm sinh viên hoặc sửa sinh viên.
        private void SetNormalState()
        {
            isAdding = false;
            SetInputState(false);

            cmbLop.Enabled = true;
            dgvSinhVien.Enabled = true;
            txtTimKiem.Enabled = true;
            cmbTimKiem.Enabled = true;
            btnTimKiem.Enabled = true;
            btnLamMoiTimKiem.Enabled = true;

            btnGhi.Enabled = false;
            btnPhucHoi.Enabled = false;
            btnThem.Enabled = true;

            bool hasCurrentRow = dgvSinhVien.CurrentRow != null && !dgvSinhVien.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;
            btnThoat.Enabled = true;
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

            // MASV chỉ nhập khi thêm; khi sửa dùng để tìm dòng UPDATE.
            txtMaSV.Enabled = adding;

            btnGhi.Enabled = true;
            btnPhucHoi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
            btnThoat.Enabled = true;
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
        private void formLopSinhVien_Load(object sender, EventArgs e)
        {
            try
            {
                ThietLapGioiHanNhap();

                DataTable dtLop = DBHelper.ExecuteDataTable("SP_GET_LOP");
                cmbLop.DataSource = dtLop;
                cmbLop.DisplayMember = "TENLOP"; // Hiển thị tên lớp.
                cmbLop.ValueMember = "MALOP";    // Truy vấn bằng mã lớp.

                // Vừa mở form thì chỉ xem dữ liệu.
                SetNormalState();
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
                DataTable dtSV = DBHelper.ExecuteDataTable(
                    "SP_GET_SINHVIEN_THEO_LOP",
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
            LoadCurrentRowToInput();
            SetNormalState();
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

        private void dgvSinhVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                LoadCurrentRowToInput();
                SetNormalState();
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            ClearInput();
            SetEditingState(true);
            txtMaSV.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaSV.Text == "") return;

            string thongBaoXoa =
                "Bạn có chắc muốn xóa sinh viên này?\n\n" +
                $"Mã SV: {txtMaSV.Text.Trim()}\n" +
                $"Họ tên: {txtHo.Text.Trim()} {txtTen.Text.Trim()}";

            if (MessageBox.Show(thongBaoXoa, "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // SP trả 1 nếu sinh viên đã có điểm nên không được xóa.
                    int result = DBHelper.ExecuteNonQueryWithReturn(
                        "SP_XOA_SINHVIEN",
                        new SqlParameter("@MASV", txtMaSV.Text.Trim()));

                    if (result == 1)
                        MessageBox.Show("Không thể xóa vì sinh viên này đã có điểm thi!", "Báo lỗi");
                    else
                    {
                        MessageBox.Show("Xóa thành công!", "Thông báo");
                        LoadSinhVien(cmbLop.SelectedValue.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
                }
            }
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

            if (ho.Length > 40)
            {
                MessageBox.Show("Họ sinh viên tối đa 40 ký tự!", "Báo lỗi");
                txtHo.Focus();
                return;
            }

            if (ten.Length > 10)
            {
                MessageBox.Show("Tên sinh viên tối đa 10 ký tự!", "Báo lỗi");
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
                else
                {
                    MessageBox.Show("Cập nhật dữ liệu thành công!", "Thông báo");
                    LoadSinhVien(cmbLop.SelectedValue.ToString()); // Lưu xong thì tải lại lưới.
                    SetNormalState(); // Quay về trạng thái xem.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            // Phục hồi chỉ bỏ dữ liệu đang nhập dở và tải lại dữ liệu.
            if (cmbLop.SelectedValue != null)
                LoadSinhVien(cmbLop.SelectedValue.ToString());

            SetNormalState();
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
