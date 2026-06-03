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

        // Trạng thái chung của form Lớp - Sinh viên:
        // - Bình thường: chọn lớp, chọn sinh viên trên lưới, các ô nhập bị khóa.
        // - Đang Thêm: mở ô nhập, cho nhập Mã SV mới, bật Ghi + Phục hồi.
        // - Đang Sửa: mở ô nhập nhưng khóa Mã SV vì MASV là khóa chính.
        private void SetNormalState()
        {
            isAdding = false;
            SetInputState(false);

            cmbLop.Enabled = true;
            dgvSinhVien.Enabled = true;
            txtTimKiem.Enabled = true;

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

            // Đang thêm/sửa thì khóa lớp và lưới để tránh đổi dòng giữa chừng.
            cmbLop.Enabled = false;
            dgvSinhVien.Enabled = false;
            txtTimKiem.Enabled = false;

            // Mã sinh viên chỉ nhập khi thêm. Khi sửa, MASV dùng để xác định dòng cần UPDATE.
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
            dtpNgaySinh.Value = DateTime.Now;
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
                dtpNgaySinh.Value = Convert.ToDateTime(row.Cells["NGAYSINH"].Value);
            else
                dtpNgaySinh.Value = DateTime.Now;
        }

        // Khi form mở lên thì tải danh sách lớp vào ComboBox.
        private void formLopSinhVien_Load(object sender, EventArgs e)
        {
            try
            {
                DataTable dtLop = DBHelper.GetDataTable("EXEC SP_GET_LOP");
                cmbLop.DataSource = dtLop;
                cmbLop.DisplayMember = "TENLOP"; // Người dùng nhìn thấy tên lớp.
                cmbLop.ValueMember = "MALOP";    // Code dùng mã lớp để truy vấn sinh viên.

                // Vừa mở form chỉ ở trạng thái xem. Muốn đổi dữ liệu thì bấm Thêm hoặc Sửa.
                SetNormalState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách Lớp: " + ex.Message);
            }
        }

        private void cmbLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Khi ComboBox đã có mã lớp thì tải sinh viên của lớp đó.
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
                // Gọi SP bằng SqlParameter để tránh ghép chuỗi SQL trực tiếp.
                // MALOP là NCHAR nên dùng parameter cũng đỡ lỗi khoảng trắng đệm hơn.
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

        // Chuyển tiếng Việt có dấu thành không dấu để tìm kiếm dễ hơn.
        // Ví dụ gõ "thanh" vẫn tìm được "THÀNH".
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
            if (dtSinhVienGoc == null) return;

            string keyword = ChuyenKhongDau(txtTimKiem.Text.Trim());

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

                if (maSV.Contains(keyword) ||
                    ho.Contains(keyword) ||
                    ten.Contains(keyword) ||
                    hoTen.Contains(keyword) ||
                    diaChi.Contains(keyword))
                {
                    dtLoc.ImportRow(row);
                }
            }

            dgvSinhVien.DataSource = dtLoc;
            LoadCurrentRowToInput();
            SetNormalState();
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

            if (MessageBox.Show($"Bạn có chắc muốn xóa sinh viên {txtHo.Text} {txtTen.Text}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = DBHelper.GetConnection())
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SP_XOA_SINHVIEN", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MASV", txtMaSV.Text.Trim());

                            SqlParameter returnValue = new SqlParameter();
                            returnValue.Direction = ParameterDirection.ReturnValue;
                            cmd.Parameters.Add(returnValue);

                            cmd.ExecuteNonQuery();

                            if ((int)returnValue.Value == 1)
                                MessageBox.Show("Không thể xóa vì sinh viên này đã có điểm thi!", "Báo lỗi");
                            else
                            {
                                MessageBox.Show("Xóa thành công!", "Thông báo");
                                LoadSinhVien(cmbLop.SelectedValue.ToString());
                            }
                        }
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
            string maSV = txtMaSV.Text.Trim();
            string ho = txtHo.Text.Trim();
            string ten = txtTen.Text.Trim();
            string diaChi = txtDiaChi.Text.Trim();

            if (maSV == "" || ho == "" || ten == "")
            {
                MessageBox.Show("Mã Sinh Viên, Họ và Tên không được để trống!", "Báo lỗi");
                return;
            }

            // Các độ dài này lấy theo bảng SINHVIEN.
            // Kiểm tra ở form trước để báo lỗi rõ hơn trước khi gửi xuống SQL Server.
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

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (isAdding) cmd.CommandText = "SP_THEM_SINHVIEN";
                        else cmd.CommandText = "SP_SUA_SINHVIEN";

                        cmd.Parameters.AddWithValue("@MASV", maSV);
                        cmd.Parameters.AddWithValue("@HO", ho);
                        cmd.Parameters.AddWithValue("@TEN", ten);
                        cmd.Parameters.AddWithValue("@NGAYSINH", dtpNgaySinh.Value);
                        cmd.Parameters.AddWithValue("@DIACHI", diaChi);
                        cmd.Parameters.AddWithValue("@MALOP", cmbLop.SelectedValue.ToString());

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        if ((int)returnValue.Value == 1)
                        {
                            MessageBox.Show("Lỗi: Mã Sinh Viên này đã tồn tại!", "Báo lỗi");
                        }
                        else if ((int)returnValue.Value == 2)
                        {
                            MessageBox.Show("Không tìm thấy sinh viên cần sửa hoặc lớp không tồn tại!", "Báo lỗi");
                        }
                        else
                        {
                            MessageBox.Show("Cập nhật dữ liệu thành công!", "Thông báo");
                            LoadSinhVien(cmbLop.SelectedValue.ToString()); // Lưu xong thì tải lại lưới.
                            SetNormalState(); // Quay về trạng thái xem dữ liệu.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            // Phục hồi chỉ bỏ dữ liệu đang nhập dở và tải lại dữ liệu từ database.
            // Không gọi SP thêm/sửa/xóa nên dữ liệu thật không bị thay đổi.
            LoadSinhVien(cmbLop.SelectedValue.ToString());
            SetNormalState();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Nếu đang thêm/sửa mà chưa Ghi thì hỏi lại trước khi thoát.
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
