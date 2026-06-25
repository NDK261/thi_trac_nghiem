using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formLop : Form
    {
        private bool isAdding = false;
        private DataTable lopData;

        public formLop()
        {
            InitializeComponent();
            dgvLop.SelectionChanged += dgvLop_SelectionChanged;
            textTimKiem.TextChanged += textTimKiem_TextChanged;
        }

        
        private Panel pnlSinhVien;
        private formLopSinhVien currentSubForm;
        private void SetupSubformUI() {
            this.Height = 700;
            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Horizontal;
            split.SplitterDistance = 350;
            
            Control[] controls = new Control[this.Controls.Count];
            this.Controls.CopyTo(controls, 0);
            foreach(Control c in controls) {
                split.Panel1.Controls.Add(c);
            }
            this.Controls.Add(split);
            
            pnlSinhVien = new Panel();
            pnlSinhVien.Dock = DockStyle.Fill;
            split.Panel2.Controls.Add(pnlSinhVien);
            
            btnDSSinhVien.Visible = false;
        }

        private void LoadSubForm(string maLop) {
            if (currentSubForm != null) {
                currentSubForm.Close();
                currentSubForm.Dispose();
            }
            if (string.IsNullOrEmpty(maLop)) return;
            
            currentSubForm = new formLopSinhVien(maLop);
            currentSubForm.TopLevel = false;
            currentSubForm.FormBorderStyle = FormBorderStyle.None;
            currentSubForm.Dock = DockStyle.Fill;
            pnlSinhVien.Controls.Add(currentSubForm);
            currentSubForm.Show();
        }
private void formLop_Load(object sender, EventArgs e)
        {
            SetupSubformUI();
            LoadData();
            SetEditingState(false);
        }

        private void SetEditingState(bool editing)
        {
            bool hasCurrentClass = dgvLop.CurrentRow != null && txtMaLop.Text.Trim() != "";

            btnGhi.Enabled = editing;
            btnThem.Enabled = !editing;
            btnSua.Enabled = !editing;
            btnXoa.Enabled = !editing;
            btnTim.Enabled = !editing;
            btnPhucHoi.Enabled = editing;
            btnDSSinhVien.Enabled = !editing && hasCurrentClass;
            btnThoat.Enabled = true;

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

        private bool IsValidClassCode(string value)
        {
            return value.All(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'));
        }

        private string NormalizeSearchText(string value)
        {
            if (value == null) return "";

            value = value.Replace('Đ', 'D').Replace('đ', 'd');
            string normalized = value.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
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

            if (txtMaLop.Text.Length > 15)
            {
                MessageBox.Show("Mã lớp tối đa 15 ký tự!", "Báo lỗi");
                txtMaLop.Focus();
                return false;
            }

            if (!IsValidClassCode(txtMaLop.Text))
            {
                MessageBox.Show("Mã lớp chỉ được gồm chữ cái A-Z và chữ số 0-9!", "Báo lỗi");
                txtMaLop.Focus();
                return false;
            }

            if (textTenLop.Text == "")
            {
                MessageBox.Show("Tên lớp không được để trống!", "Báo lỗi");
                textTenLop.Focus();
                return false;
            }

            if (textTenLop.Text.Length > 40)
            {
                MessageBox.Show("Tên lớp tối đa 40 ký tự!", "Báo lỗi");
                textTenLop.Focus();
                return false;
            }

            return true;
        }

        private void ApplyGridFormat()
        {
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
        }

        private void BindGrid(DataTable dt)
        {
            dgvLop.DataSource = null;
            dgvLop.Columns.Clear();
            dgvLop.AutoGenerateColumns = true;
            dgvLop.DataSource = dt;
            ApplyGridFormat();
        }

        private void LoadData()
        {
            try
            {
                lopData = DBHelper.GetDataTable("EXEC dbo.SP_GET_LOP");
                ApplySearch(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Báo lỗi");
            }
        }

        private void ApplySearch(bool showNotFoundMessage)
        {
            if (lopData == null)
                return;

            string key = NormalizeSearchText((textTimKiem.Text ?? "").Trim());
            DataTable dtResult;

            if (key == "")
            {
                dtResult = lopData;
            }
            else
            {
                var filtered = lopData.AsEnumerable().Where(r =>
                    NormalizeSearchText(r.Field<string>("MALOP")).Contains(key) ||
                    NormalizeSearchText(r.Field<string>("TENLOP")).Contains(key));

                dtResult = filtered.Any() ? filtered.CopyToDataTable() : lopData.Clone();
            }

            BindGrid(dtResult);

            if (dgvLop.Rows.Count > 0)
            {
                dgvLop.Rows[0].Selected = true;
                LoadCurrentRowToInput();
            }
            else
            {
                ClearInput();

                if (showNotFoundMessage)
                    MessageBox.Show("Không tìm thấy lớp phù hợp!", "Thông báo");
            }

            SetEditingState(false);
        }

        private void LoadCurrentRowToInput()
        {
            if (dgvLop.CurrentRow == null)
            {
                ClearInput();
                return;
            }

            DataGridViewRow row = dgvLop.CurrentRow;

            txtMaLop.Text = dgvLop.Columns.Contains("MALOP")
                ? row.Cells["MALOP"].Value?.ToString().Trim() ?? ""
                : "";

            textTenLop.Text = dgvLop.Columns.Contains("TENLOP")
                ? row.Cells["TENLOP"].Value?.ToString().Trim() ?? ""
                : "";
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
                            MessageBox.Show("Không thể xóa lớp này vì đã có sinh viên hoặc đăng ký thi liên quan!", "Báo lỗi");
                        }
                        else
                        {
                            MessageBox.Show("Xóa thành công!", "Thông báo");
                            LoadData();
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
                        cmd.CommandText = isAdding ? "dbo.SP_THEM_LOP" : "dbo.SP_SUA_LOP";
                        cmd.Parameters.AddWithValue("@MALOP", txtMaLop.Text.Trim());
                        cmd.Parameters.AddWithValue("@TENLOP", textTenLop.Text.Trim());

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
                                isAdding
                                    ? "Không thể thêm lớp. Mã lớp hoặc tên lớp bị trùng, hoặc dữ liệu không hợp lệ!"
                                    : "Không thể sửa lớp. Lớp không tồn tại, tên lớp bị trùng, hoặc dữ liệu không hợp lệ!",
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
            ApplySearch(true);
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (btnGhi.Enabled)
            {
                DialogResult dr = MessageBox.Show(
                    "Bạn đang nhập dữ liệu chưa ghi. Bạn có chắc chắn muốn thoát không?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr != DialogResult.Yes) return;
            }

            this.Close();
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            isAdding = false;

            if (dgvLop.CurrentRow != null)
                LoadCurrentRowToInput();
            else
                ClearInput();

            SetEditingState(false);
        }

        private void dgvLop_SelectionChanged(object sender, EventArgs e)
        {

            if (!isAdding && dgvLop.CurrentRow != null) { LoadSubForm(dgvLop.CurrentRow.Cells["MALOP"].Value.ToString()); }
            if (dgvLop.Enabled && dgvLop.CurrentRow != null)
            {
                LoadCurrentRowToInput();
                SetEditingState(false);
            }
        }

        private void textTimKiem_TextChanged(object sender, EventArgs e)
        {
            if (!textTimKiem.Enabled)
                return;

            ApplySearch(false);
        }

        private void btnDSSinhVien_Click(object sender, EventArgs e)
        {
            string maLop = txtMaLop.Text.Trim();

            if (maLop == "")
            {
                MessageBox.Show("Vui lòng chọn lớp cần xem danh sách sinh viên!", "Thông báo");
                return;
            }

            formLopSinhVien f = new formLopSinhVien(maLop);
            f.ShowDialog();
        }
    }
}
