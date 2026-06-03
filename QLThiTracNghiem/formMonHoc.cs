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
        bool isAdding = false; // Mình dùng cờ này để biết nút Ghi đang xử lý Thêm hay Sửa.
        DataTable dtMonHocGoc; // Giữ lại danh sách môn ban đầu để lọc tìm kiếm trên form.
        public formMonHoc()
        {
            InitializeComponent();
        }

        // Tải danh sách môn học từ SQL rồi đưa lên DataGridView.
        private void LoadData()
        {
            try
            {
                // Phần lấy dữ liệu môn học được gom trong stored procedure SP_GET_MONHOC.
                string sql = "EXEC SP_GET_MONHOC";
                DataTable dt = DBHelper.GetDataTable(sql);

                // dtMonHocGoc luôn được cập nhật lại sau khi thêm/sửa/xóa,
                // nếu không thì ô tìm kiếm có thể lọc trên dữ liệu cũ.
                dtMonHocGoc = dt;
                dgvMonHoc.DataSource = dtMonHocGoc;

                // Đặt lại tên cột cho người dùng nhìn dễ hiểu hơn tên cột trong database.
                dgvMonHoc.Columns["MAMH"].HeaderText = "Mã Môn Học";
                dgvMonHoc.Columns["TENMH"].HeaderText = "Tên Môn Học";
                dgvMonHoc.Columns["TENMH"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Cột tên môn thường dài nên cho chiếm phần còn lại của lưới.

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

        // Khi form mở lên thì load dữ liệu và đưa các nút về trạng thái xem.
        private void formMonHoc_Load_1(object sender, EventArgs e)
        {
            LoadData();

            // Ban đầu chỉ cho xem dữ liệu, chưa cho gõ trực tiếp.
            txtMaMH.Enabled = false;
            txtTenMH.Enabled = false;
            dgvMonHoc.Enabled = true;
            txtTimKiem.Enabled = true;

            // Ghi/Phục hồi chỉ bật khi đang thêm hoặc sửa.
            btnGhi.Enabled = false;
            btnPhucHoi.Enabled = false;
            btnThem.Enabled = true;
            bool hasCurrentRow = dgvMonHoc.CurrentRow != null && !dgvMonHoc.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;
        }

        private void dgvMonHoc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Chỉ nhận dòng thật trong lưới, không xử lý header hoặc dòng rỗng.
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvMonHoc.Rows[e.RowIndex];
                txtMaMH.Text = row.Cells["MAMH"].Value.ToString();
                txtTenMH.Text = row.Cells["TENMH"].Value.ToString();
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true; // Từ đây nút Ghi sẽ gọi SP_THEM_MONHOC.

            // Thêm mới thì cho nhập cả mã môn và tên môn.
            txtMaMH.Enabled = true;
            txtTenMH.Enabled = true;

            // Xóa dữ liệu cũ trên ô nhập để tránh người dùng tưởng đang sửa dòng đang chọn.
            txtMaMH.Clear();
            txtTenMH.Clear();
            txtMaMH.Focus();

            // Trong lúc thêm thì chỉ cho Ghi hoặc Phục hồi, không cho bấm lẫn thao tác khác.
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
                            else if (result == 3)
                                MessageBox.Show("Không thể xóa môn học này vì đã có điểm thi của sinh viên!", "Báo lỗi");
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
            isAdding = false; // Từ đây nút Ghi sẽ gọi SP_SUA_MONHOC.

            // Khi sửa chỉ cho đổi tên môn. Mã môn là khóa chính nên không cho sửa tự do.
            txtMaMH.Enabled = false;
            txtTenMH.Enabled = true;
            txtTenMH.Focus();

            // Khóa các nút thao tác khác cho tới khi người dùng Ghi hoặc Phục hồi.
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
            string maMH = txtMaMH.Text.Trim().ToUpper();
            string tenMH = txtTenMH.Text.Trim();

            if (maMH == "" || tenMH == "")
            {
                MessageBox.Show("Mã và Tên môn học không được để trống!", "Báo lỗi");
                return;
            }

            // Độ dài này lấy theo bảng MONHOC: MAMH tối đa 5 ký tự, TENMH tối đa 40 ký tự.
            // Kiểm tra trước trên form để lỗi dễ hiểu hơn so với lỗi SQL Server trả về.
            if (maMH.Length > 5)
            {
                MessageBox.Show("Mã môn học tối đa 5 ký tự!", "Báo lỗi");
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
                using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Tùy trạng thái đang thêm hay sửa mà gọi stored procedure tương ứng.
                        if (isAdding)
                            cmd.CommandText = "SP_THEM_MONHOC";
                        else
                            cmd.CommandText = "SP_SUA_MONHOC";

                        cmd.Parameters.AddWithValue("@MAMH", maMH);
                        cmd.Parameters.AddWithValue("@TENMH", tenMH);

                        // Stored procedure trả RETURN code để C# biết lỗi cụ thể là gì.
                        System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                        returnValue.Direction = System.Data.ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();
                        int result = (int)returnValue.Value;

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

                            // Lưu xong rồi thì không còn dữ liệu nháp để Ghi hoặc Phục hồi.
                            btnGhi.Enabled = false;
                            btnPhucHoi.Enabled = false;

                            // Mở lại các nút thao tác chính.
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
            // Nếu chưa load dữ liệu thì không có gì để lọc.
            if (dtMonHocGoc == null) return;

            // Từ khóa và dữ liệu đều đưa về không dấu để so sánh cho dễ.
            string keyword = ChuyenKhongDau(txtTimKiem.Text.Trim());

            // Tạo bảng tạm cùng cấu trúc với bảng gốc để chứa các dòng tìm được.
            DataTable dtLoc = dtMonHocGoc.Clone();

            foreach (DataRow row in dtMonHocGoc.Rows)
            {
                string maMH = ChuyenKhongDau(row["MAMH"].ToString());
                string tenMH = ChuyenKhongDau(row["TENMH"].ToString());

                // Nếu mã hoặc tên môn có chứa từ khóa thì đưa dòng đó vào kết quả lọc.
                if (maMH.Contains(keyword) || tenMH.Contains(keyword))
                {
                    dtLoc.ImportRow(row);
                }
            }

            // Đưa kết quả lọc lên lưới.
            dgvMonHoc.DataSource = dtLoc;
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            // Phục hồi là hủy phần đang gõ dở, nên đưa cờ trạng thái về bình thường.
            isAdding = false;

            // Nạp lại dòng đang chọn lên ô nhập để trả về dữ liệu trước khi sửa.
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

            // Trả các nút về trạng thái thao tác bình thường.
            btnThem.Enabled = true;
            bool hasCurrentRow = dgvMonHoc.CurrentRow != null && !dgvMonHoc.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;

            btnGhi.Enabled = false;       // Không còn nội dung nháp để lưu.
            btnPhucHoi.Enabled = false;   // Đã phục hồi xong nên tắt nút này.
        }
    }
}
