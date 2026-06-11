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
    public partial class formBoDe : Form
    {
        bool isAdding = false;
        public formBoDe()
        {
            InitializeComponent();
        }

        // Gom trạng thái nút/ô nhập để các thao tác Thêm, Sửa, Ghi, Phục hồi không bị lệch nhau.
        private void SetNormalState()
        {
            isAdding = false;

            // Trạng thái xem: chỉ chọn môn và chọn dòng trên lưới.
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

            // Đang nhập thì khóa môn và lưới để không đổi dòng giữa chừng.
            SetInputState(true);
            cmbMonHoc.Enabled = false;
            dgvBoDe.Enabled = false;

            // CAUHOI do database tự cấp nên không cho nhập tay.
            txtCauHoi.Enabled = false;
            txtCauHoi.ReadOnly = true;

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

            // MAGV lấy theo tài khoản đăng nhập.
            txtMaGV.Enabled = false;
            txtMaGV.ReadOnly = true;

            // Mã câu hỏi cũng để database quản lý.
            txtCauHoi.ReadOnly = true;
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

            // Thêm mới thì MAGV là tài khoản đang đăng nhập.
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

        private bool ValidateInput()
        {
            // Khi sửa phải có CAUHOI để cập nhật đúng dòng.
            if (!isAdding && !int.TryParse(txtCauHoi.Text.Trim(), out int _))
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

            if (txtNoiDung.Text.Trim().Length > 200)
            {
                MessageBox.Show("Nội dung câu hỏi tối đa 200 ký tự!", "Báo lỗi");
                txtNoiDung.Focus();
                return false;
            }

            if (txtA.Text.Trim().Length > 200 ||
                txtB.Text.Trim().Length > 200 ||
                txtC.Text.Trim().Length > 200 ||
                txtD.Text.Trim().Length > 200)
            {
                MessageBox.Show("Mỗi đáp án A, B, C, D tối đa 200 ký tự!", "Báo lỗi");
                return false;
            }

            // 4 đáp án phải khác nhau; so sánh không phân biệt hoa/thường.
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
                // Đổ môn học vào ComboBox.
                DataTable dtMonHoc = DBHelper.GetDataTable("EXEC SP_GET_MONHOC");
                cmbMonHoc.DataSource = dtMonHoc;
                cmbMonHoc.DisplayMember = "TENMH";
                cmbMonHoc.ValueMember = "MAMH";

                // Giới hạn nhập theo cấu trúc bảng BODE để người dùng biết lỗi ngay trên form.
                txtNoiDung.MaxLength = 200;
                txtA.MaxLength = 200;
                txtB.MaxLength = 200;
                txtC.MaxLength = 200;
                txtD.MaxLength = 200;

                // Trình độ theo đề: A, B, C.
                cmbTrinhDo.Items.AddRange(new string[] { "A", "B", "C" });
                cmbTrinhDo.SelectedIndex = 0;

                // Đáp án đúng chỉ nhận A, B, C, D.
                cmbDapAn.Items.AddRange(new string[] { "A", "B", "C", "D" });
                cmbDapAn.SelectedIndex = 0;

                // Mã giáo viên lấy theo tài khoản đăng nhập.
                txtMaGV.Text = Program.mUserName;

                // Vừa mở form thì chỉ xem dữ liệu.
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
                DataTable dtBoDe = DBHelper.ExecuteDataTable(
                    "SP_GET_BODE_THEO_MONHOC",
                    new SqlParameter("@MAMH", maMH));
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
            txtCauHoi.Text = "Tự động";
            SetEditingState(true);
            cmbTrinhDo.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtCauHoi.Text == "") return;

            if (txtMaGV.Text.Trim() != Program.mUserName.Trim())
            {
                MessageBox.Show("Bạn không có quyền xóa câu hỏi của người khác!", "Cảnh báo");
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

                            // SP trả mã lỗi để biết câu đã thi hay không đúng giáo viên.
                            System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                            returnValue.Direction = ParameterDirection.ReturnValue;
                            cmd.Parameters.Add(returnValue);

                            cmd.ExecuteNonQuery();

                            int result = (int)returnValue.Value;
                            if (result == 1) MessageBox.Show("Không thể xóa câu hỏi đã xuất hiện trong bài thi của sinh viên!", "Báo lỗi");
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

            // Form kiểm tra trước, SP vẫn kiểm tra lại ở database.
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

                        // Thêm mới không truyền @CAUHOI; sửa thì phải truyền để SP biết dòng cần cập nhật.
                        if (!isAdding)
                            cmd.Parameters.AddWithValue("@CAUHOI", int.Parse(txtCauHoi.Text.Trim()));

                        cmd.Parameters.AddWithValue("@MAMH", cmbMonHoc.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@TRINHDO", cmbTrinhDo.Text);
                        cmd.Parameters.AddWithValue("@NOIDUNG", txtNoiDung.Text.Trim());
                        cmd.Parameters.AddWithValue("@A", txtA.Text.Trim());
                        cmd.Parameters.AddWithValue("@B", txtB.Text.Trim());
                        cmd.Parameters.AddWithValue("@C", txtC.Text.Trim());
                        cmd.Parameters.AddWithValue("@D", txtD.Text.Trim());
                        cmd.Parameters.AddWithValue("@DAP_AN", cmbDapAn.Text);
                        cmd.Parameters.AddWithValue("@MAGV", Program.mUserName); // SP dùng MAGV để kiểm tra quyền.

                        // SP kiểm tra lại quyền, câu đã thi và dữ liệu đáp án.
                        System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = (int)returnValue.Value;
                        if (result == 1) MessageBox.Show("Mã câu hỏi đã tồn tại!", "Báo lỗi");
                        else if (result == 2) MessageBox.Show("Bạn không có quyền sửa câu hỏi này!", "Báo lỗi");
                        else if (result == 3) MessageBox.Show("Không thể sửa câu hỏi đã xuất hiện trong bài thi, vì sửa sẽ làm sai lịch sử bài làm của sinh viên!", "Báo lỗi");
                        else if (result == 4) MessageBox.Show("Dữ liệu câu hỏi không hợp lệ hoặc có đáp án bị trùng!", "Báo lỗi");
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
            // Phục hồi chỉ bỏ phần đang gõ dở và tải lại dữ liệu từ database.
            if (cmbMonHoc.SelectedValue != null)
                LoadBoDe(cmbMonHoc.SelectedValue.ToString());

            SetNormalState();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Đang nhập dở thì hỏi lại trước khi thoát.
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
