using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formGiaoVien : Form
    {
        private bool isAdding = false;

        public formGiaoVien()
        {
            InitializeComponent();
            dgvGiaoVien.SelectionChanged += dgvGiaoVien_SelectionChanged;
        }

        private void formGiaoVien_Load(object sender, EventArgs e)
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
            btnThoat.Enabled = true;

            txtMaGV.Enabled = editing && isAdding;
            textHo.Enabled = editing;
            textTen.Enabled = editing;
            textSoDT.Enabled = editing;
            txtTenMH.Enabled = editing; // txtTenMH đang dùng làm Địa chỉ

            dgvGiaoVien.Enabled = !editing;
            textTimKiem.Enabled = !editing;
        }

        private void ClearInput()
        {
            txtMaGV.Text = "";
            textHo.Text = "";
            textTen.Text = "";
            textSoDT.Text = "";
            txtTenMH.Text = "";
        }

        private bool ValidateInput()
        {
            txtMaGV.Text = (txtMaGV.Text ?? "").Trim().ToUpper();
            textHo.Text = (textHo.Text ?? "").Trim();
            textTen.Text = (textTen.Text ?? "").Trim();
            textSoDT.Text = (textSoDT.Text ?? "").Trim();
            txtTenMH.Text = (txtTenMH.Text ?? "").Trim();

            if (txtMaGV.Text == "")
            {
                MessageBox.Show("Mã giáo viên không được để trống!", "Báo lỗi");
                txtMaGV.Focus();
                return false;
            }

            if (txtMaGV.Text.Length > 8)
            {
                MessageBox.Show("Mã giáo viên tối đa 8 ký tự!", "Báo lỗi");
                txtMaGV.Focus();
                return false;
            }

            if (textHo.Text == "")
            {
                MessageBox.Show("Họ không được để trống!", "Báo lỗi");
                textHo.Focus();
                return false;
            }

            if (textTen.Text == "")
            {
                MessageBox.Show("Tên không được để trống!", "Báo lỗi");
                textTen.Focus();
                return false;
            }

            if (textSoDT.Text.Length > 15)
            {
                MessageBox.Show("Số điện thoại tối đa 15 ký tự!", "Báo lỗi");
                textSoDT.Focus();
                return false;
            }

            string allowed = "+0123456789 ";
            if (textSoDT.Text != "" && textSoDT.Text.Any(c => !allowed.Contains(c)))
            {
                MessageBox.Show("Số điện thoại không hợp lệ!", "Báo lỗi");
                textSoDT.Focus();
                return false;
            }

            return true;
        }

        private void ApplyGridFormat()
        {
            if (dgvGiaoVien.Columns.Contains("MAGV"))
                dgvGiaoVien.Columns["MAGV"].HeaderText = "Mã Giáo Viên";

            if (dgvGiaoVien.Columns.Contains("HO"))
                dgvGiaoVien.Columns["HO"].HeaderText = "Họ";

            if (dgvGiaoVien.Columns.Contains("TEN"))
                dgvGiaoVien.Columns["TEN"].HeaderText = "Tên";

            if (dgvGiaoVien.Columns.Contains("SODTLL"))
                dgvGiaoVien.Columns["SODTLL"].HeaderText = "Số Điện Thoại";

            if (dgvGiaoVien.Columns.Contains("DIACHI"))
                dgvGiaoVien.Columns["DIACHI"].HeaderText = "Địa Chỉ";

            dgvGiaoVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvGiaoVien.ReadOnly = true;
            dgvGiaoVien.AllowUserToAddRows = false;
            dgvGiaoVien.AllowUserToDeleteRows = false;
            dgvGiaoVien.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGiaoVien.MultiSelect = false;
        }

        private void BindGrid(DataTable dt)
        {
            dgvGiaoVien.DataSource = null;
            dgvGiaoVien.Columns.Clear();
            dgvGiaoVien.AutoGenerateColumns = true;
            dgvGiaoVien.DataSource = dt;
            ApplyGridFormat();
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = DBHelper.GetDataTable("EXEC dbo.SP_GET_GIAOVIEN");
                BindGrid(dt);

                if (dgvGiaoVien.Rows.Count > 0)
                {
                    dgvGiaoVien.Rows[0].Selected = true;
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
            try
            {
                if (dgvGiaoVien.CurrentRow == null)
                {
                    ClearInput();
                    return;
                }

                DataGridViewRow row = dgvGiaoVien.CurrentRow;

                txtMaGV.Text = dgvGiaoVien.Columns.Contains("MAGV")
                    ? row.Cells["MAGV"].Value?.ToString().Trim() ?? ""
                    : "";

                textHo.Text = dgvGiaoVien.Columns.Contains("HO")
                    ? row.Cells["HO"].Value?.ToString().Trim() ?? ""
                    : "";

                textTen.Text = dgvGiaoVien.Columns.Contains("TEN")
                    ? row.Cells["TEN"].Value?.ToString().Trim() ?? ""
                    : "";

                textSoDT.Text = dgvGiaoVien.Columns.Contains("SODTLL")
                    ? row.Cells["SODTLL"].Value?.ToString().Trim() ?? ""
                    : "";

                txtTenMH.Text = dgvGiaoVien.Columns.Contains("DIACHI")
                    ? row.Cells["DIACHI"].Value?.ToString().Trim() ?? ""
                    : "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dữ liệu dòng: " + ex.Message, "Báo lỗi");
                ClearInput();
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            ClearInput();
            SetEditingState(true);
            txtMaGV.Focus();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (txtMaGV.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng chọn giáo viên cần sửa!", "Thông báo");
                return;
            }

            isAdding = false;
            SetEditingState(true);
            textHo.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaGV.Text.Trim() == "") return;

            DialogResult dr = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa giáo viên này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.SP_XOA_GIAOVIEN", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MAGV", txtMaGV.Text.Trim());

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = 0;
                        if (returnValue.Value != null && returnValue.Value != DBNull.Value)
                            int.TryParse(returnValue.Value.ToString(), out result);

                        if (result == 1)
                        {
                            MessageBox.Show("Không thể xóa giáo viên này vì đã có dữ liệu liên quan!", "Báo lỗi");
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
                        cmd.CommandText = isAdding ? "dbo.SP_THEM_GIAOVIEN" : "dbo.SP_SUA_GIAOVIEN";

                        cmd.Parameters.AddWithValue("@MAGV", txtMaGV.Text.Trim());
                        cmd.Parameters.AddWithValue("@HO", textHo.Text.Trim());
                        cmd.Parameters.AddWithValue("@TEN", textTen.Text.Trim());
                        cmd.Parameters.AddWithValue("@SODTLL", textSoDT.Text.Trim());
                        cmd.Parameters.AddWithValue("@DIACHI", txtTenMH.Text.Trim());

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = 0;
                        if (returnValue.Value != null && returnValue.Value != DBNull.Value)
                            int.TryParse(returnValue.Value.ToString(), out result);

                        if (result == 1)
                        {
                            MessageBox.Show(
                                isAdding ? "Lỗi: Trùng mã giáo viên!" : "Không tìm thấy giáo viên để sửa!",
                                "Báo lỗi");
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
                DataTable dt = DBHelper.GetDataTable("EXEC dbo.SP_GET_GIAOVIEN");

                if (key == "")
                {
                    BindGrid(dt);
                    if (dgvGiaoVien.Rows.Count > 0)
                    {
                        dgvGiaoVien.Rows[0].Selected = true;
                        LoadCurrentRowToInput();
                    }
                    return;
                }

                var filtered = dt.AsEnumerable().Where(r =>
                    (r.Field<string>("MAGV") ?? "").IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Field<string>("HO") ?? "").IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Field<string>("TEN") ?? "").IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    ((r.Field<string>("HO") ?? "") + " " + (r.Field<string>("TEN") ?? ""))
                        .IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Field<string>("SODTLL") ?? "").IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Field<string>("DIACHI") ?? "").IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0
                );

                DataTable dtResult = filtered.Any() ? filtered.CopyToDataTable() : dt.Clone();
                BindGrid(dtResult);

                if (dgvGiaoVien.Rows.Count > 0)
                {
                    dgvGiaoVien.Rows[0].Selected = true;
                    LoadCurrentRowToInput();
                }
                else
                {
                    ClearInput();
                    MessageBox.Show("Không tìm thấy giáo viên phù hợp!", "Thông báo");
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

        private void dgvGiaoVien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                LoadCurrentRowToInput();
        }

        private void dgvGiaoVien_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvGiaoVien.Enabled && dgvGiaoVien.CurrentRow != null)
                LoadCurrentRowToInput();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void textTen_TextChanged(object sender, EventArgs e)
        {
        }
    }
}