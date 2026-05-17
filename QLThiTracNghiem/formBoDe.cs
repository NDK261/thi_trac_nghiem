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
    public partial class formBoDe : Form
    {
        bool isAdding = false;
        public formBoDe()
        {
            InitializeComponent();
        }

        // Chuẩn chung cho form Bộ đề:
        // - Bình thường: chỉ cho chọn môn/chọn câu hỏi trên lưới, chưa cho sửa trực tiếp ô nhập.
        // - Đang Thêm/Sửa: mở các ô cần nhập, bật Ghi + Phục hồi, khóa Thêm/Sửa/Xóa để tránh bấm lẫn thao tác.
        // Làm riêng thành hàm giúp mọi nút đưa form về đúng một trạng thái, không bị mỗi nút bật/tắt một kiểu.
        private void SetNormalState()
        {
            isAdding = false;

            // Ở trạng thái xem dữ liệu, chỉ cho chọn môn và chọn dòng trên lưới.
            // Các ô nhập bị khóa để người dùng hiểu rằng muốn đổi dữ liệu thì phải bấm Sửa trước.
            SetInputState(false);
            cmbMonHoc.Enabled = true;
            dgvBoDe.Enabled = true;

            btnGhi.Enabled = false;
            btnPhucHoi.Enabled = false;
            btnThem.Enabled = true;

            bool hasCurrentRow = dgvBoDe.CurrentRow != null && !dgvBoDe.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;
            btnThoat.Enabled = true;
        }

        private void SetEditingState(bool adding)
        {
            isAdding = adding;

            // Khi đang nhập, khóa ComboBox môn học và lưới để tránh đổi dòng/đổi môn giữa chừng.
            // Nếu đổi dòng trong lúc đang nhập thì dữ liệu đang gõ dở rất dễ bị ghi nhầm hoặc mất.
            SetInputState(true);
            cmbMonHoc.Enabled = false;
            dgvBoDe.Enabled = false;

            // Câu hỏi là khóa chính nên chỉ mở khi Thêm mới.
            // Khi Sửa, mã câu hỏi phải giữ nguyên để Stored Procedure tìm đúng dòng cần cập nhật.
            txtCauHoi.Enabled = adding;

            btnGhi.Enabled = true;
            btnPhucHoi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
            btnThoat.Enabled = true;
        }

        private void SetInputState(bool enabled)
        {
            txtCauHoi.Enabled = enabled;
            cmbTrinhDo.Enabled = enabled;
            txtNoiDung.Enabled = enabled;
            txtA.Enabled = enabled;
            txtB.Enabled = enabled;
            txtC.Enabled = enabled;
            txtD.Enabled = enabled;
            cmbDapAn.Enabled = enabled;

            // Mã giảng viên luôn lấy theo tài khoản đang đăng nhập, không cho người dùng tự sửa.
            txtMaGV.Enabled = false;
            txtMaGV.ReadOnly = true;
        }

        private void ClearInput()
        {
            txtCauHoi.Clear();
            txtNoiDung.Clear();
            txtA.Clear();
            txtB.Clear();
            txtC.Clear();
            txtD.Clear();

            if (cmbTrinhDo.Items.Count > 0) cmbTrinhDo.SelectedIndex = 0;
            if (cmbDapAn.Items.Count > 0) cmbDapAn.SelectedIndex = 0;

            // Khi thêm câu hỏi mới, người tạo luôn là giáo viên đang đăng nhập.
            txtMaGV.Text = Program.mUserName;
        }

        private void LoadCurrentRowToInput()
        {
            if (dgvBoDe.CurrentRow == null || dgvBoDe.CurrentRow.IsNewRow)
            {
                ClearInput();
                return;
            }

            DataGridViewRow row = dgvBoDe.CurrentRow;
            txtCauHoi.Text = row.Cells["CAUHOI"].Value?.ToString();
            cmbTrinhDo.Text = row.Cells["TRINHDO"].Value?.ToString();
            txtNoiDung.Text = row.Cells["NOIDUNG"].Value?.ToString();
            txtA.Text = row.Cells["A"].Value?.ToString();
            txtB.Text = row.Cells["B"].Value?.ToString();
            txtC.Text = row.Cells["C"].Value?.ToString();
            txtD.Text = row.Cells["D"].Value?.ToString();
            cmbDapAn.Text = row.Cells["DAP_AN"].Value?.ToString();
            txtMaGV.Text = row.Cells["MAGV"].Value?.ToString();
        }

        private bool MonHocDaCoSinhVienThi(string maMH)
        {
            // Database hiện tại chỉ lưu điểm trong BANGDIEM, chưa có bảng lưu chi tiết
            // từng câu hỏi mà sinh viên đã gặp trong bài thi. Vì đề thi được bốc ngẫu nhiên,
            // khi môn học đã có sinh viên thi thì ta không thể biết chắc câu hỏi nào đã xuất hiện.
            // Cách an toàn là khóa xóa câu hỏi của môn đó để bảo toàn lịch sử đề thi/điểm thi.
            using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM BANGDIEM WHERE MAMH = @MAMH";

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MAMH", maMH);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private bool ValidateInput()
        {
            if (!int.TryParse(txtCauHoi.Text.Trim(), out int _))
            {
                MessageBox.Show("Mã câu hỏi phải là số nguyên!", "Báo lỗi");
                txtCauHoi.Focus();
                return false;
            }

            if (cmbMonHoc.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn môn học cho câu hỏi!", "Báo lỗi");
                cmbMonHoc.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNoiDung.Text) ||
                string.IsNullOrWhiteSpace(txtA.Text) ||
                string.IsNullOrWhiteSpace(txtB.Text) ||
                string.IsNullOrWhiteSpace(txtC.Text) ||
                string.IsNullOrWhiteSpace(txtD.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ nội dung câu hỏi và 4 đáp án A, B, C, D!", "Báo lỗi");
                return false;
            }

            // Góp ý trong ảnh có nhắc "câu trả lời khác nhau".
            // Vì vậy khi lưu câu hỏi, 4 phương án A/B/C/D không được trùng nội dung sau khi đã Trim.
            // So sánh không phân biệt hoa/thường để tránh trường hợp "True" và "true" vẫn bị xem là khác.
            string[] dapAn = new string[]
            {
                txtA.Text.Trim(),
                txtB.Text.Trim(),
                txtC.Text.Trim(),
                txtD.Text.Trim()
            };

            if (dapAn.Distinct(StringComparer.OrdinalIgnoreCase).Count() < 4)
            {
                MessageBox.Show("Bốn đáp án A, B, C, D phải khác nhau, không được nhập trùng nội dung!", "Báo lỗi");
                return false;
            }

            return true;
        }

        private void formBoDe_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Đổ dữ liệu vào ComboBox Môn Học
                DataTable dtMonHoc = DBHelper.GetDataTable("EXEC SP_GET_MONHOC");
                cmbMonHoc.DataSource = dtMonHoc;
                cmbMonHoc.DisplayMember = "TENMH";
                cmbMonHoc.ValueMember = "MAMH";

                // 2. Thêm dữ liệu cứng cho Trình Độ (A, B, C)
                cmbTrinhDo.Items.AddRange(new string[] { "A", "B", "C" });
                cmbTrinhDo.SelectedIndex = 0;

                // 3. Thêm dữ liệu cứng cho Đáp Án Đúng (A, B, C, D)
                cmbDapAn.Items.AddRange(new string[] { "A", "B", "C", "D" });
                cmbDapAn.SelectedIndex = 0;

                // 4. Tự động điền Mã Giảng Viên đang đăng nhập vào textbox
                txtMaGV.Text = Program.mUserName;

                // Form vừa mở lên chỉ nên ở trạng thái xem dữ liệu.
                // Người dùng muốn nhập mới thì bấm Thêm, muốn đổi câu hỏi cũ thì bấm Sửa.
                SetNormalState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
            }
        }
        private void cmbMonHoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMonHoc.SelectedValue != null && cmbMonHoc.SelectedValue is string)
            {
                string maMH = cmbMonHoc.SelectedValue.ToString();
                LoadBoDe(maMH);
            }
        }
        private void LoadBoDe(string maMH)
        {
            try
            {
                string sql = $"EXEC SP_GET_BODE_THEO_MONHOC @MAMH = '{maMH}'";
                DataTable dtBoDe = DBHelper.GetDataTable(sql);
                dgvBoDe.DataSource = dtBoDe;

                dgvBoDe.ReadOnly = true;
                dgvBoDe.AllowUserToAddRows = false;
                dgvBoDe.AllowUserToDeleteRows = false;
                dgvBoDe.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvBoDe.MultiSelect = false;

                LoadCurrentRowToInput();
                SetNormalState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải bộ đề: " + ex.Message);
            }
        }

        private void dgvBoDe_CellClick(object sender, DataGridViewCellEventArgs e)
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
            txtCauHoi.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtCauHoi.Text == "") return;

            if (txtMaGV.Text.Trim() != Program.mUserName.Trim())
            {
                MessageBox.Show("Bạn không có quyền xóa câu hỏi của người khác!", "Cảnh báo");
                return;
            }

            string maMH = cmbMonHoc.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(maMH))
            {
                MessageBox.Show("Không xác định được môn học của câu hỏi cần xóa!", "Báo lỗi");
                return;
            }

            if (MonHocDaCoSinhVienThi(maMH))
            {
                MessageBox.Show(
                    "Không thể xóa câu hỏi vì môn học này đã có sinh viên thi.\n" +
                    "Database hiện chưa lưu chi tiết câu hỏi từng bài, nên form phải khóa xóa để không làm sai lịch sử đề thi.",
                    "Không cho xóa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc muốn xóa câu hỏi này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
                    {
                        conn.Open();
                        using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SP_XOA_BODE", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CAUHOI", int.Parse(txtCauHoi.Text.Trim()));
                            cmd.Parameters.AddWithValue("@MAGV", Program.mUserName);

                            System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                            returnValue.Direction = ParameterDirection.ReturnValue;
                            cmd.Parameters.Add(returnValue);

                            cmd.ExecuteNonQuery();

                            int result = (int)returnValue.Value;
                            if (result == 1) MessageBox.Show("Không thể xóa câu hỏi đã được thi!", "Báo lỗi");
                            else if (result == 2) MessageBox.Show("Bạn không có quyền xóa câu hỏi này!", "Báo lỗi");
                            else
                            {
                                MessageBox.Show("Xóa thành công!", "Thông báo");
                                LoadBoDe(cmbMonHoc.SelectedValue.ToString());
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
            if (txtCauHoi.Text == "") return;

            // Kiểm tra quyền Sửa ở cấp giao diện trước
            if (txtMaGV.Text.Trim() != Program.mUserName.Trim())
            {
                MessageBox.Show("Bạn không có quyền sửa câu hỏi của người khác!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetEditingState(false);
            txtNoiDung.Focus();
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.CommandText = isAdding ? "SP_THEM_BODE" : "SP_SUA_BODE";

                        cmd.Parameters.AddWithValue("@CAUHOI", int.Parse(txtCauHoi.Text.Trim()));
                        cmd.Parameters.AddWithValue("@MAMH", cmbMonHoc.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@TRINHDO", cmbTrinhDo.Text);
                        cmd.Parameters.AddWithValue("@NOIDUNG", txtNoiDung.Text.Trim());
                        cmd.Parameters.AddWithValue("@A", txtA.Text.Trim());
                        cmd.Parameters.AddWithValue("@B", txtB.Text.Trim());
                        cmd.Parameters.AddWithValue("@C", txtC.Text.Trim());
                        cmd.Parameters.AddWithValue("@D", txtD.Text.Trim());
                        cmd.Parameters.AddWithValue("@DAP_AN", cmbDapAn.Text);
                        cmd.Parameters.AddWithValue("@MAGV", Program.mUserName); // Luôn truyền mã GV hiện tại

                        System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = (int)returnValue.Value;
                        if (result == 1) MessageBox.Show("Mã câu hỏi đã tồn tại!", "Báo lỗi");
                        else if (result == 2) MessageBox.Show("Bạn không có quyền sửa câu hỏi này!", "Báo lỗi");
                        else
                        {
                            MessageBox.Show("Lưu thành công!", "Thông báo");
                            LoadBoDe(cmbMonHoc.SelectedValue.ToString());
                            SetNormalState();
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
            // Phục hồi nghĩa là hủy phần đang gõ dở và tải lại dữ liệu thật từ database/lưới.
            // Nút này không ghi gì xuống SQL Server.
            if (cmbMonHoc.SelectedValue != null)
                LoadBoDe(cmbMonHoc.SelectedValue.ToString());

            SetNormalState();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Nếu đang Thêm/Sửa mà chưa bấm Ghi, thoát ngay sẽ làm mất nội dung đang nhập.
            // Vì vậy nút Thoát vẫn bật, nhưng hỏi xác nhận để an toàn hơn.
            if (btnGhi.Enabled)
            {
                DialogResult result = MessageBox.Show(
                    "Bạn đang thêm/sửa câu hỏi nhưng chưa ghi dữ liệu. Bạn có chắc muốn thoát không?",
                    "Xác nhận thoát",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;
            }

            this.Close();
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }
}
