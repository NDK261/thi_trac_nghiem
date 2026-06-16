using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formMonHoc : Form
    {
        bool isAdding = false; // Dùng cờ này để biết nút Ghi đang xử lý Thêm hay Sửa.
        DataTable dtMonHocGoc; // Giữ lại danh sách môn ban đầu để lọc tìm kiếm trên form.
        public formMonHoc()
        {
            InitializeComponent();
        }

        private void ThietLapGioiHanNhap()
        {
            txtMaMH.MaxLength = 5;
            txtTenMH.MaxLength = 40;
            txtMaMH.CharacterCasing = CharacterCasing.Upper;

            cmbTimKiem.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTimKiem.Items.Clear();
            cmbTimKiem.Items.AddRange(new string[] { "Tất cả", "Mã môn", "Tên môn" });
            cmbTimKiem.SelectedIndex = 0;
        }

        private void LoadCurrentRowToInput()
        {
            if (dgvMonHoc.CurrentRow == null || dgvMonHoc.CurrentRow.IsNewRow)
            {
                txtMaMH.Clear();
                txtTenMH.Clear();
                return;
            }

            DataGridViewRow row = dgvMonHoc.CurrentRow;
            txtMaMH.Text = row.Cells["MAMH"].Value?.ToString();
            txtTenMH.Text = row.Cells["TENMH"].Value?.ToString();
        }

        private void CapNhatNutTheoDongHienTai()
        {
            bool hasCurrentRow = dgvMonHoc.CurrentRow != null && !dgvMonHoc.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;
        }

        // Tải danh sách môn học từ SP rồi đưa lên lưới.
        private void LoadData()
        {
            try
            {
                // SP_GET_MONHOC là nguồn dữ liệu chính của form môn học.
                DataTable dt = DBHelper.ExecuteDataTable("SP_GET_MONHOC");

                // Cập nhật bản gốc để ô tìm kiếm không lọc trên dữ liệu cũ.
                dtMonHocGoc = dt;
                dgvMonHoc.DataSource = dtMonHocGoc;

                // Đặt lại tên cột cho dễ nhìn trên form.
                dgvMonHoc.Columns["MAMH"].HeaderText = "Mã Môn Học";
                dgvMonHoc.Columns["TENMH"].HeaderText = "Tên Môn Học";
                dgvMonHoc.Columns["TENMH"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Tên môn thường dài nên cho rộng hơn.

                // Không sửa trực tiếp trên lưới, mọi thay đổi đi qua nút.
                dgvMonHoc.ReadOnly = true;
                dgvMonHoc.AllowUserToAddRows = false;
                dgvMonHoc.AllowUserToDeleteRows = false;
                dgvMonHoc.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvMonHoc.MultiSelect = false;
                LoadCurrentRowToInput();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // Mở form thì tải dữ liệu và đưa nút về trạng thái xem.
        private void formMonHoc_Load_1(object sender, EventArgs e)
        {
            ThietLapGioiHanNhap();
            LoadData();

            // Ban đầu chỉ xem dữ liệu.
            txtMaMH.Enabled = false;
            txtTenMH.Enabled = false;
            dgvMonHoc.Enabled = true;
            txtTimKiem.Enabled = true;
            cmbTimKiem.Enabled = true;
            btnTimKiem.Enabled = true;
            btnLamMoiTimKiem.Enabled = true;

            // Ghi/Phục hồi chỉ bật khi thêm hoặc sửa.
            btnGhi.Enabled = false;
            btnPhucHoi.Enabled = false;
            btnThem.Enabled = true;
            bool hasCurrentRow = dgvMonHoc.CurrentRow != null && !dgvMonHoc.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;
        }

        private void dgvMonHoc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Chỉ xử lý dòng dữ liệu thật.
            if (e.RowIndex >= 0)
            {
                LoadCurrentRowToInput();
                CapNhatNutTheoDongHienTai();
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true; // Từ đây nút Ghi sẽ gọi SP_THEM_MONHOC.

            // Thêm mới thì cho nhập cả mã môn và tên môn.
            txtMaMH.Enabled = true;
            txtTenMH.Enabled = true;

            // Xóa ô nhập để tránh nhầm với dòng đang chọn.
            txtMaMH.Clear();
            txtTenMH.Clear();
            txtMaMH.Focus();

            // Đang thêm thì chỉ cho Ghi hoặc Phục hồi.
            btnGhi.Enabled = true;
            btnPhucHoi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;

            // Khóa lưới/tìm kiếm để không đổi dòng khi đang gõ.
            dgvMonHoc.Enabled = false;
            txtTimKiem.Enabled = false;
            cmbTimKiem.Enabled = false;
            btnTimKiem.Enabled = false;
            btnLamMoiTimKiem.Enabled = false;
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaMH.Text == "")
            {
                MessageBox.Show("Vui lòng chọn môn học cần xóa từ danh sách!", "Thông báo");
                return;
            }

            string thongBaoXoa =
                "Bạn có chắc muốn xóa môn học này?\n\n" +
                $"Mã môn: {txtMaMH.Text.Trim()}\n" +
                $"Tên môn: {txtTenMH.Text.Trim()}";

            if (MessageBox.Show(thongBaoXoa, "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int result = DBHelper.ExecuteNonQueryWithReturn(
                        "SP_XOA_MONHOC",
                        new System.Data.SqlClient.SqlParameter("@MAMH", txtMaMH.Text.Trim()));

                    if (result == 1)
                        MessageBox.Show("Không thể xóa môn học này vì đã có câu hỏi trong Bộ đề!", "Báo lỗi");
                    else if (result == 2)
                        MessageBox.Show("Không thể xóa môn học này vì đã có lịch Đăng ký thi!", "Báo lỗi");
                    else if (result == 3)
                        MessageBox.Show("Không thể xóa môn học này vì đã có điểm thi của sinh viên!", "Báo lỗi");
                    else
                    {
                        MessageBox.Show("Xóa thành công!", "Thông báo");
                        LoadData();
                        txtTimKiem.Clear();
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
            if (txtMaMH.Text == "")
            {
                MessageBox.Show("Vui lòng chọn môn học cần sửa từ danh sách!", "Thông báo");
                return;
            }
            isAdding = false; // Từ đây nút Ghi sẽ gọi SP_SUA_MONHOC.

            // Khi sửa chỉ cho đổi tên môn. Mã môn là khóa chính nên không cho sửa tự do.
            txtMaMH.Enabled = false;
            txtTenMH.Enabled = true;
            txtTenMH.Focus();

            // Khóa nút khác tới khi Ghi hoặc Phục hồi.
            btnGhi.Enabled = true;
            btnPhucHoi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;

            // Đang sửa thì không đổi dòng hoặc tìm kiếm.
            dgvMonHoc.Enabled = false;
            txtTimKiem.Enabled = false;
            cmbTimKiem.Enabled = false;
            btnTimKiem.Enabled = false;
            btnLamMoiTimKiem.Enabled = false;
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            string maMH = txtMaMH.Text.Trim().ToUpper();
            string tenMH = txtTenMH.Text.Trim();

            if (maMH == "" || tenMH == "")
            {
                MessageBox.Show("Mã và Tên môn học không được để trống!", "Báo lỗi");
                return;
            }

            // Độ dài lấy theo bảng MONHOC; kiểm tra trên form để báo lỗi dễ hiểu hơn.
            if (maMH.Length > 5)
            {
                MessageBox.Show("Mã môn học tối đa 5 ký tự!", "Báo lỗi");
                txtMaMH.Focus();
                return;
            }

            if (!IsValidCode(maMH))
            {
                MessageBox.Show("Mã môn học chỉ được gồm chữ cái A-Z và chữ số 0-9!", "Báo lỗi");
                txtMaMH.Focus();
                return;
            }

            if (tenMH.Length > 40)
            {
                MessageBox.Show("Tên môn học tối đa 40 ký tự!", "Báo lỗi");
                txtTenMH.Focus();
                return;
            }

            txtMaMH.Text = maMH;
            txtTenMH.Text = tenMH;

            try
            {
                // isAdding quyết định gọi SP thêm hay sửa.
                string procedureName = isAdding ? "SP_THEM_MONHOC" : "SP_SUA_MONHOC";
                int result = DBHelper.ExecuteNonQueryWithReturn(
                    procedureName,
                    new System.Data.SqlClient.SqlParameter("@MAMH", maMH),
                    new System.Data.SqlClient.SqlParameter("@TENMH", tenMH));

                if (result == 1) MessageBox.Show("Lỗi: Trùng Mã Môn Học!", "Báo lỗi");
                else if (result == 2) MessageBox.Show("Lỗi: Tên Môn Học đã tồn tại!", "Báo lỗi");
                else if (result == 3) MessageBox.Show("Dữ liệu môn học không hợp lệ hoặc vượt quá độ dài cho phép!", "Báo lỗi");
                else if (result == 4) MessageBox.Show("Không tìm thấy môn học cần sửa/xóa!", "Báo lỗi");
                else
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo");
                    LoadData(); // Lưu xong thì tải lại lưới để thấy dữ liệu mới nhất.
                    txtTimKiem.Clear();

                    // Quay về trạng thái xem dữ liệu.
                    txtMaMH.Enabled = false;
                    txtTenMH.Enabled = false;
                    dgvMonHoc.Enabled = true;
                    txtTimKiem.Enabled = true;
                    cmbTimKiem.Enabled = true;
                    btnTimKiem.Enabled = true;
                    btnLamMoiTimKiem.Enabled = true;

                    // Lưu xong thì tắt Ghi/Phục hồi.
                    btnGhi.Enabled = false;
                    btnPhucHoi.Enabled = false;

                    // Mở lại các nút thao tác chính.
                    btnThem.Enabled = true;
                    CapNhatNutTheoDongHienTai();
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
                    "Bạn đang thêm/sửa môn học nhưng chưa ghi dữ liệu. Bạn có chắc muốn thoát không?",
                    "Xác nhận thoát",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;
            }

            this.Close();
        }

        private bool IsValidCode(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.All(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'));
        }

        // Chuyển tiếng Việt có dấu thành không dấu để ô tìm kiếm dễ dùng hơn.
        public static string ChuyenKhongDau(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            foreach (char c in normalizedString)
            {
                System.Globalization.UnicodeCategory unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            // Riêng chữ Đ/đ không tự mất dấu bằng Normalize nên xử lý thêm ở cuối.
            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace("đ", "d").Replace("Đ", "d").ToLower();
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            LocMonHocTheoTuKhoa();
        }

        private void LocMonHocTheoTuKhoa()
        {
            // Nếu chưa tải dữ liệu thì không có gì để lọc.
            if (dtMonHocGoc == null) return;

            // Đưa từ khóa và dữ liệu về không dấu để so sánh.
            string keyword = ChuyenKhongDau(txtTimKiem.Text.Trim());
            string kieuTim = cmbTimKiem.SelectedItem?.ToString() ?? "Tất cả";

            if (keyword == "")
            {
                dgvMonHoc.DataSource = dtMonHocGoc;
                LoadCurrentRowToInput();
                CapNhatNutTheoDongHienTai();
                return;
            }

            // Bảng tạm chứa các dòng tìm được.
            DataTable dtLoc = dtMonHocGoc.Clone();

            foreach (DataRow row in dtMonHocGoc.Rows)
            {
                string maMH = ChuyenKhongDau(row["MAMH"].ToString());
                string tenMH = ChuyenKhongDau(row["TENMH"].ToString());

                // Nếu mã hoặc tên môn có chứa từ khóa thì đưa dòng đó vào kết quả lọc.
                bool khop =
                    (kieuTim == "Tất cả" && (maMH.Contains(keyword) || tenMH.Contains(keyword))) ||
                    (kieuTim == "Mã môn" && maMH.Contains(keyword)) ||
                    (kieuTim == "Tên môn" && tenMH.Contains(keyword));

                if (khop)
                {
                    dtLoc.ImportRow(row);
                }
            }

            // Đưa kết quả lọc lên lưới.
            dgvMonHoc.DataSource = dtLoc;
            LoadCurrentRowToInput();
            CapNhatNutTheoDongHienTai();
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            LocMonHocTheoTuKhoa();
        }

        private void btnLamMoiTimKiem_Click(object sender, EventArgs e)
        {
            txtTimKiem.Clear();
            cmbTimKiem.SelectedIndex = 0;
            LocMonHocTheoTuKhoa();
            txtTimKiem.Focus();
        }

        private void cmbTimKiem_SelectedIndexChanged(object sender, EventArgs e)
        {
            LocMonHocTheoTuKhoa();
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            // Phục hồi là hủy phần đang gõ dở.
            isAdding = false;

            // Nạp lại dòng đang chọn lên ô nhập.
            if (dgvMonHoc.CurrentRow != null)
            {
                txtMaMH.Text = dgvMonHoc.CurrentRow.Cells["MAMH"].Value.ToString();
                txtTenMH.Text = dgvMonHoc.CurrentRow.Cells["TENMH"].Value.ToString();
            }
            else
            {
                txtMaMH.Clear();
                txtTenMH.Clear();
            }

            // Khóa lại ô nhập vì đã quay về trạng thái xem.
            txtMaMH.Enabled = false;
            txtTenMH.Enabled = false;
            dgvMonHoc.Enabled = true;
            txtTimKiem.Enabled = true;
            cmbTimKiem.Enabled = true;
            btnTimKiem.Enabled = true;
            btnLamMoiTimKiem.Enabled = true;

            // Trả các nút về trạng thái bình thường.
            btnThem.Enabled = true;
            bool hasCurrentRow = dgvMonHoc.CurrentRow != null && !dgvMonHoc.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;

            btnGhi.Enabled = false;       // Không còn nội dung nháp để lưu.
            btnPhucHoi.Enabled = false;   // Đã phục hồi xong nên tắt nút này.
        }
    }
}
