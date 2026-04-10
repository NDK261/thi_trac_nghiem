using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formLop : Form
    {
        private bool isAdding = false;

        public formLop()
        {
            InitializeComponent();
            dgvLop.SelectionChanged += dgvLop_SelectionChanged;
        }

        private void formLop_Load(object sender, EventArgs e)
        {
            LoadData();
            SetEditingState(false);
        }

        private void SetEditingState(bool editing)
        {
            btnGhi.Enabled = editing;
            btnThem.Enabled = !editing;
            btnSua.Enabled = !editing;
            btnXoa.Enabled = !editing;
            btnTim.Enabled = !editing;

            txtMaLop.Enabled = editing && isAdding;
            textTenLop.Enabled = editing;

            dgvLop.Enabled = !editing;
            textTimKiem.Enabled = !editing;
        }

        private void ClearInput()
        {
            txtMaLop.Text = "";
            textTenLop.Text = "";
        }

        private bool ValidateInput()
        {
            txtMaLop.Text = (txtMaLop.Text ?? "").Trim().ToUpper();
            textTenLop.Text = (textTenLop.Text ?? "").Trim();

            if (txtMaLop.Text == "")
            {
                MessageBox.Show("Mã lớp không được để trống!", "Báo lỗi");
                txtMaLop.Focus();
                return false;
            }

            if (txtMaLop.Text.Length > 10)
            {
                MessageBox.Show("Mã lớp tối đa 10 ký tự!", "Báo lỗi");
                txtMaLop.Focus();
                return false;
            }

            if (textTenLop.Text == "")
            {
                MessageBox.Show("Tên lớp không được để trống!", "Báo lỗi");
                textTenLop.Focus();
                return false;
            }

            if (textTenLop.Text.Length > 100)
            {
                MessageBox.Show("Tên lớp tối đa 100 ký tự!", "Báo lỗi");
                textTenLop.Focus();
                return false;
            }

            return true;
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = DBHelper.GetDataTable("EXEC dbo.SP_GET_LOP");
                dgvLop.DataSource = dt;

                if (dgvLop.Columns.Contains("MALOP"))
                    dgvLop.Columns["MALOP"].HeaderText = "Mã Lớp";
                if (dgvLop.Columns.Contains("TENLOP"))
                    dgvLop.Columns["TENLOP"].HeaderText = "Tên Lớp";

                dgvLop.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvLop.ReadOnly = true;
                dgvLop.AllowUserToAddRows = false;
                dgvLop.AllowUserToDeleteRows = false;
                dgvLop.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvLop.MultiSelect = false;

                if (dgvLop.Rows.Count > 0)
                {
                    dgvLop.Rows[0].Selected = true;
                    LoadCurrentRowToInput();
                }
                else
                {
                    ClearInput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Báo lỗi");
            }
        }

        private void LoadCurrentRowToInput()
        {
            if (dgvLop.CurrentRow == null) return;

            DataGridViewRow row = dgvLop.CurrentRow;

            txtMaLop.Text = row.Cells["MALOP"].Value?.ToString().Trim() ?? "";
            textTenLop.Text = row.Cells["TENLOP"].Value?.ToString().Trim() ?? "";
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            ClearInput();
            SetEditingState(true);
            txtMaLop.Focus();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (txtMaLop.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng chọn lớp cần sửa!", "Thông báo");
                return;
            }

            isAdding = false;
            SetEditingState(true);
            textTenLop.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaLop.Text.Trim() == "") return;

            DialogResult dr = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa lớp này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.SP_XOA_LOP", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MALOP", txtMaLop.Text.Trim());

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = 0;
                        if (returnValue.Value != null && returnValue.Value != DBNull.Value)
                            int.TryParse(returnValue.Value.ToString(), out result);

                        if (result == 1)
                        {
                            MessageBox.Show("Không thể xóa lớp này vì đã có sinh viên!", "Báo lỗi");
                        }
                        else
                        {
                            MessageBox.Show("Xóa thành công!", "Thông báo");
                            LoadData();
                            ClearInput();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (isAdding)
                        {
                            cmd.CommandText = "dbo.SP_THEM_LOP";
                            cmd.Parameters.AddWithValue("@MALOP", txtMaLop.Text.Trim());
                            cmd.Parameters.AddWithValue("@TENLOP", textTenLop.Text.Trim());
                        }
                        else
                        {
                            cmd.CommandText = "dbo.SP_SUA_LOP";
                            cmd.Parameters.AddWithValue("@MALOP", txtMaLop.Text.Trim());
                            cmd.Parameters.AddWithValue("@TENLOP", textTenLop.Text.Trim());
                        }

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = 0;
                        if (returnValue.Value != null && returnValue.Value != DBNull.Value)
                            int.TryParse(returnValue.Value.ToString(), out result);

                        if (result == 1)
                        {
                            MessageBox.Show(isAdding
                                ? "Lỗi: Trùng mã lớp!"
                                : "Không tìm thấy lớp để sửa!", "Báo lỗi");
                            return;
                        }

                        MessageBox.Show("Cập nhật thành công!", "Thông báo");
                        LoadData();
                        SetEditingState(false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            string key = (textTimKiem.Text ?? "").Trim();

            try
            {
                DataTable dt = DBHelper.GetDataTable("EXEC dbo.SP_GET_LOP");

                if (key == "")
                {
                    dgvLop.DataSource = dt;
                    return;
                }

                var filtered = dt.AsEnumerable().Where(r =>
                    (r.Field<string>("MALOP") ?? "").IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Field<string>("TENLOP") ?? "").IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0);

                dgvLop.DataSource = filtered.Any() ? filtered.CopyToDataTable() : dt.Clone();

                if (dgvLop.Rows.Count > 0)
                {
                    dgvLop.Rows[0].Selected = true;
                    LoadCurrentRowToInput();
                }
                else
                {
                    ClearInput();
                    MessageBox.Show("Không tìm thấy lớp phù hợp!", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvLop_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLop.CurrentRow != null && !isAdding)
            {
                LoadCurrentRowToInput();
            }
        }
    }
}
