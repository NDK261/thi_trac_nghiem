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
        bool isAdding = false; // Cờ kiểm tra đang Thêm hay Sửa
        public formLopSinhVien()
        {
            InitializeComponent();
        }

        // Chuẩn trạng thái cho form Lớp - Sinh viên:
        // - Bình thường: chọn lớp, chọn sinh viên trên lưới, các ô nhập bị khóa.
        // - Đang Thêm: mở ô nhập, cho nhập Mã SV mới, bật Ghi + Phục hồi.
        // - Đang Sửa: mở ô nhập nhưng khóa Mã SV vì MASV là khóa chính, không nên sửa trực tiếp.
        private void SetNormalState()
        {
            isAdding = false;
            SetInputState(false);

            cmbLop.Enabled = true;
            dgvSinhVien.Enabled = true;

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

            // Khóa lớp và lưới trong lúc đang nhập để tránh đang gõ sinh viên lớp A
            // nhưng vô tình đổi sang lớp B hoặc chọn dòng khác.
            cmbLop.Enabled = false;
            dgvSinhVien.Enabled = false;

            // Mã sinh viên chỉ được nhập khi thêm mới. Khi sửa, MASV dùng để tìm đúng sinh viên cần UPDATE.
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

            // NGAYSINH có thể bị NULL trong database, nên phải kiểm tra trước khi Convert.
            if (row.Cells["NGAYSINH"].Value != DBNull.Value && row.Cells["NGAYSINH"].Value != null)
                dtpNgaySinh.Value = Convert.ToDateTime(row.Cells["NGAYSINH"].Value);
            else
                dtpNgaySinh.Value = DateTime.Now;
        }

        // 1. Khi Form vừa mở lên -> Tải danh sách Lớp vào ComboBox
        private void formLopSinhVien_Load(object sender, EventArgs e)
        {
            try
            {
                DataTable dtLop = DBHelper.GetDataTable("EXEC SP_GET_LOP");
                cmbLop.DataSource = dtLop;
                cmbLop.DisplayMember = "TENLOP"; // Chữ hiện lên cho người dùng xem
                cmbLop.ValueMember = "MALOP";    // Mã ẩn bên dưới để hệ thống dùng

                // Vừa load form thì đưa về trạng thái xem dữ liệu.
                // Muốn thay đổi dữ liệu thì phải bấm Thêm hoặc Sửa.
                SetNormalState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách Lớp: " + ex.Message);
            }
        }

        private void cmbLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Kiểm tra xem ComboBox đã có giá trị ValueMember chưa
            if (cmbLop.SelectedValue != null && cmbLop.SelectedValue is string)
            {
                string maLopDuocChon = cmbLop.SelectedValue.ToString();
                LoadSinhVien(maLopDuocChon);
            }
        }
        // Hàm hỗ trợ tải sinh viên
        private void LoadSinhVien(string maLop)
        {
            try
            {
                // Gọi SP truyền kèm tham số Mã Lớp
                string sql = $"EXEC SP_GET_SINHVIEN_THEO_LOP @MALOP = '{maLop}'";
                DataTable dtSV = DBHelper.GetDataTable(sql);
                dgvSinhVien.DataSource = dtSV;

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
            if (txtMaSV.Text.Trim() == "" || txtTen.Text.Trim() == "")
            {
                MessageBox.Show("Mã Sinh Viên và Tên không được để trống!", "Báo lỗi");
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

                        cmd.Parameters.AddWithValue("@MASV", txtMaSV.Text.Trim());
                        cmd.Parameters.AddWithValue("@HO", txtHo.Text.Trim());
                        cmd.Parameters.AddWithValue("@TEN", txtTen.Text.Trim());
                        cmd.Parameters.AddWithValue("@NGAYSINH", dtpNgaySinh.Value);
                        cmd.Parameters.AddWithValue("@DIACHI", txtDiaChi.Text.Trim());
                        cmd.Parameters.AddWithValue("@MALOP", cmbLop.SelectedValue.ToString());

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        if ((int)returnValue.Value == 1)
                        {
                            MessageBox.Show("Lỗi: Mã Sinh Viên này đã tồn tại!", "Báo lỗi");
                        }
                        else
                        {
                            MessageBox.Show("Cập nhật dữ liệu thành công!", "Thông báo");
                            LoadSinhVien(cmbLop.SelectedValue.ToString()); // Tải lại lưới
                            SetNormalState(); // Lưu xong thì quay về trạng thái xem dữ liệu
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
            // Phục hồi nghĩa là hủy phần đang nhập/sửa dở và nạp lại dữ liệu gốc từ database.
            // Vì không gọi stored procedure thêm/sửa/xóa nên thao tác này không làm thay đổi dữ liệu.
            LoadSinhVien(cmbLop.SelectedValue.ToString());
            SetNormalState();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Nếu đang thêm/sửa sinh viên mà chưa Ghi, hỏi lại trước khi đóng form.
            // Đây là cách giữ nút Thoát luôn dùng được nhưng vẫn tránh mất dữ liệu nhập dở.
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
