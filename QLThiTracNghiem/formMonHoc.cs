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
        bool isAdding = false; // Biến kiểm tra xem đang Thêm hay Sửa
        DataTable dtMonHocGoc; // Thêm biến này để giữ dữ liệu gốc
        public formMonHoc()
        {
            InitializeComponent();
        }

        // Hàm dùng để gọi dữ liệu từ SQL và đổ vào Lưới
        private void LoadData()
        {
            try
            {
                // Dùng SP_GET_MONHOC đã viết dưới SQL Server
                string sql = "EXEC SP_GET_MONHOC";
                DataTable dt = DBHelper.GetDataTable(sql);

                // dtMonHocGoc là bảng dữ liệu gốc dùng cho ô tìm kiếm live.
                // Mỗi lần thêm/sửa/xóa xong ta gọi LoadData(), nên gán lại biến này để tìm kiếm không bị dùng dữ liệu cũ.
                dtMonHocGoc = dt;
                dgvMonHoc.DataSource = dtMonHocGoc;

                // Chỉnh lại tiêu đề cột cho đẹp
                dgvMonHoc.Columns["MAMH"].HeaderText = "Mã Môn Học";
                dgvMonHoc.Columns["TENMH"].HeaderText = "Tên Môn Học";
                dgvMonHoc.Columns["TENMH"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Cho cột tên môn dãn đầy lưới

                // Chỉ cho xem/chọn dữ liệu trên lưới; việc thêm/sửa/xóa phải đi qua các nút riêng.
                dgvMonHoc.ReadOnly = true;
                dgvMonHoc.AllowUserToAddRows = false;
                dgvMonHoc.AllowUserToDeleteRows = false;
                dgvMonHoc.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvMonHoc.MultiSelect = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // Sự kiện chạy khi form vừa mở lên
        private void formMonHoc_Load_1(object sender, EventArgs e)
        {
            LoadData();

            // 1. Khóa các ô nhập liệu
            txtMaMH.Enabled = false;
            txtTenMH.Enabled = false;
            dgvMonHoc.Enabled = true;
            txtTimKiem.Enabled = true;

            // 2. Tắt nút Ghi, Phục hồi. Bật nút Thêm, Sửa, Xóa
            btnGhi.Enabled = false;
            btnPhucHoi.Enabled = false;
            btnThem.Enabled = true;
            bool hasCurrentRow = dgvMonHoc.CurrentRow != null && !dgvMonHoc.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;
        }

        private void dgvMonHoc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kiểm tra xem có bấm đúng vào dòng có dữ liệu không
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvMonHoc.Rows[e.RowIndex];
                txtMaMH.Text = row.Cells["MAMH"].Value.ToString();
                txtTenMH.Text = row.Cells["TENMH"].Value.ToString();
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true; // Bật cờ trạng thái đang Thêm

            // 1. MỞ KHÓA CHO PHÉP NHẬP LIỆU
            txtMaMH.Enabled = true;
            txtTenMH.Enabled = true;

            // Xóa trắng chữ cũ và đưa con trỏ chuột nhấp nháy vào ô Mã
            txtMaMH.Clear();
            txtTenMH.Clear();
            txtMaMH.Focus();

            // 2. ĐẢO TRẠNG THÁI NÚT BẤM (Bật Ghi/Phục hồi, Tắt Thêm/Sửa/Xóa)
            btnGhi.Enabled = true;
            btnPhucHoi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;

            // Khóa lưới và ô tìm kiếm trong lúc đang thêm để tránh chọn dòng khác làm mất dữ liệu đang gõ.
            dgvMonHoc.Enabled = false;
            txtTimKiem.Enabled = false;
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaMH.Text == "") return;

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa môn học này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
                    {
                        conn.Open();
                        using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SP_XOA_MONHOC", conn))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MAMH", txtMaMH.Text.Trim());

                            System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                            returnValue.Direction = System.Data.ParameterDirection.ReturnValue;
                            cmd.Parameters.Add(returnValue);

                            cmd.ExecuteNonQuery();

                            int result = (int)returnValue.Value;

                            if (result == 1)
                                MessageBox.Show("Không thể xóa môn học này vì đã có câu hỏi trong Bộ đề!", "Báo lỗi");
                            else if (result == 2)
                                MessageBox.Show("Không thể xóa môn học này vì đã có lịch Đăng ký thi!", "Báo lỗi");
                            else
                            {
                                MessageBox.Show("Xóa thành công!", "Thông báo");
                                LoadData();
                                txtTimKiem.Clear();
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
            if (txtMaMH.Text == "")
            {
                MessageBox.Show("Vui lòng chọn môn học cần sửa từ danh sách!", "Thông báo");
                return;
            }
            isAdding = false; // Tắt cờ Thêm -> Tức là đang Sửa

            // 1. CHỈ MỞ KHÓA Ô TÊN MÔN HỌC
            txtMaMH.Enabled = false; // Cấm sửa mã
            txtTenMH.Enabled = true; // Cho phép sửa tên
            txtTenMH.Focus();

            // 2. ĐẢO TRẠNG THÁI NÚT BẤM
            btnGhi.Enabled = true;
            btnPhucHoi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;

            // Khi sửa, không cho đổi dòng trên lưới hoặc lọc tìm kiếm cho tới khi Ghi/Phục hồi.
            dgvMonHoc.Enabled = false;
            txtTimKiem.Enabled = false;
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (txtMaMH.Text.Trim() == "" || txtTenMH.Text.Trim() == "")
            {
                MessageBox.Show("Mã và Tên môn học không được để trống!", "Báo lỗi");
                return;
            }

            try
            {
                using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Gọi Stored Procedure tương ứng
                        if (isAdding)
                            cmd.CommandText = "SP_THEM_MONHOC";
                        else
                            cmd.CommandText = "SP_SUA_MONHOC";

                        cmd.Parameters.AddWithValue("@MAMH", txtMaMH.Text.Trim());
                        cmd.Parameters.AddWithValue("@TENMH", txtTenMH.Text.Trim());

                        // Thực thi SP và nhận mã lỗi trả về (Return Value)
                        System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                        returnValue.Direction = System.Data.ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();
                        int result = (int)returnValue.Value;

                        if (result == 1) MessageBox.Show("Lỗi: Trùng Mã Môn Học!", "Báo lỗi");
                        else if (result == 2) MessageBox.Show("Lỗi: Tên Môn Học đã tồn tại!", "Báo lỗi");
                        else
                        {
                            MessageBox.Show("Cập nhật thành công!", "Thông báo");
                            LoadData(); // Tải lại lưới
                            txtTimKiem.Clear();

                            // 1. Khóa các ô nhập liệu lại
                            txtMaMH.Enabled = false;
                            txtTenMH.Enabled = false;
                            dgvMonHoc.Enabled = true;
                            txtTimKiem.Enabled = true;

                            // 2. TẮT nút Ghi và Phục hồi (Lưu xong rồi thì không còn gì để Hủy/Phục hồi nữa)
                            btnGhi.Enabled = false;
                            btnPhucHoi.Enabled = false;

                            // 3. Mở lại các nút thao tác cơ bản để làm tiếp
                            btnThem.Enabled = true;
                            btnSua.Enabled = true;
                            btnXoa.Enabled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Nếu nút Ghi đang bật nghĩa là người dùng đang Thêm/Sửa nhưng chưa lưu.
            // Hỏi lại để tránh đóng form làm mất dữ liệu đang nhập dở.
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

        // Hàm chuyển Tiếng Việt có dấu thành không dấu
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
            // Xử lý đặc biệt cho chữ Đ/đ và chuyển về in thường
            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace("đ", "d").Replace("Đ", "d").ToLower();
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            // Nếu chưa có dữ liệu gốc thì thoát luôn
            if (dtMonHocGoc == null) return;

            // Chuyển từ khóa người dùng gõ thành không dấu
            string keyword = ChuyenKhongDau(txtTimKiem.Text.Trim());

            // Tạo một bảng ảo (trống) có cùng cấu trúc cột để chứa kết quả lọc
            DataTable dtLoc = dtMonHocGoc.Clone();

            // Duyệt qua từng dòng trong bảng gốc
            foreach (DataRow row in dtMonHocGoc.Rows)
            {
                // Đem dữ liệu trong bảng đi "lột dấu"
                string maMH = ChuyenKhongDau(row["MAMH"].ToString());
                string tenMH = ChuyenKhongDau(row["TENMH"].ToString());

                // Nếu mã hoặc tên KHÔNG DẤU có chứa TỪ KHÓA KHÔNG DẤU thì bốc dòng đó bỏ vào bảng kết quả
                if (maMH.Contains(keyword) || tenMH.Contains(keyword))
                {
                    dtLoc.ImportRow(row);
                }
            }

            // Gắn bảng kết quả đã lọc lên lại Lưới
            dgvMonHoc.DataSource = dtLoc;
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            // 1. Reset lại cờ trạng thái (Nếu em có dùng biến isAdding để phân biệt Thêm/Sửa)
            isAdding = false;

            // 2. Tải lại dữ liệu của dòng đang được chọn trên lưới lên TextBox
            // (Thao tác này giúp xóa đi những chữ em vừa gõ dở, trả lại nguyên trạng)
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

            // 3. Khóa ô Mã Môn Học lại (Mã thì không nên cho sửa tự do)
            txtMaMH.Enabled = false;
            txtTenMH.Enabled = false; // Khóa luôn ô Tên, khi nào bấm Thêm/Sửa mới mở ra
            dgvMonHoc.Enabled = true;
            txtTimKiem.Enabled = true;

            // 4. Trả các nút bấm về trạng thái Bình thường
            btnThem.Enabled = true;
            bool hasCurrentRow = dgvMonHoc.CurrentRow != null && !dgvMonHoc.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;

            btnGhi.Enabled = false;       // Tắt nút Ghi vì đã phục hồi, không có gì để lưu
            btnPhucHoi.Enabled = false;   // Tắt chính nó đi
        }
    }
}
