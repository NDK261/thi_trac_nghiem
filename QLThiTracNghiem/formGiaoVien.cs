using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formGiaoVien : Form
    {
        private bool isAdding = false;
        private DataTable giaoVienData;

        public formGiaoVien()
        {
            InitializeComponent();
            dgvGiaoVien.SelectionChanged += dgvGiaoVien_SelectionChanged;
            textTimKiem.TextChanged += textTimKiem_TextChanged;
        }

        private void formGiaoVien_Load(object sender, EventArgs e)
        {
            LoadData();
            SetEditingState(false);
        }

        private void SetEditingState(bool editing)
        {
            bool viewingInactive = chkHienThiNgungHoatDong.Checked;
            bool hasCurrentTeacher = dgvGiaoVien.CurrentRow != null && txtMaGV.Text.Trim() != "";

            btnGhi.Enabled = editing;
            btnThem.Enabled = !editing && !viewingInactive;
            btnSua.Enabled = !editing && !viewingInactive;
            btnXoa.Enabled = !editing && !viewingInactive;
            btnTim.Enabled = !editing;
            btnPhucHoi.Enabled = editing;
            btnKhoiPhucGiaoVien.Enabled = !editing && viewingInactive && hasCurrentTeacher;
            btnThoat.Enabled = true;

            txtMaGV.Enabled = editing && isAdding;
            textHo.Enabled = editing;
            textTen.Enabled = editing;
            textSoDT.Enabled = editing;
            txtTenMH.Enabled = editing; // txtTenMH đang dùng làm Địa chỉ

            dgvGiaoVien.Enabled = !editing;
            textTimKiem.Enabled = !editing;
            chkHienThiNgungHoatDong.Enabled = !editing;
        }

        private void ClearInput()
        {
            txtMaGV.Text = "";
            textHo.Text = "";
            textTen.Text = "";
            textSoDT.Text = "";
            txtTenMH.Text = "";
        }

        private bool IsValidTeacherCode(string value)
        {
            return value.All(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'));
        }

        private bool IsValidNameText(string value)
        {
            return value.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }

        private bool IsValidPhoneNumber(string value)
        {
            return value.Length == 10 && value[0] == '0' && value.All(char.IsDigit);
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

            if (!IsValidTeacherCode(txtMaGV.Text))
            {
                MessageBox.Show("Mã giáo viên chỉ được gồm chữ cái A-Z và chữ số 0-9!", "Báo lỗi");
                txtMaGV.Focus();
                return false;
            }

            if (textHo.Text == "")
            {
                MessageBox.Show("Họ không được để trống!", "Báo lỗi");
                textHo.Focus();
                return false;
            }

            if (textHo.Text.Length > 40)
            {
                MessageBox.Show("Họ giáo viên tối đa 40 ký tự!", "Báo lỗi");
                textHo.Focus();
                return false;
            }

            if (textTen.Text == "")
            {
                MessageBox.Show("Tên không được để trống!", "Báo lỗi");
                textTen.Focus();
                return false;
            }

            if (textTen.Text.Length > 10)
            {
                MessageBox.Show("Tên tối đa 10 ký tự!", "Báo lỗi");
                textTen.Focus();
                return false;
            }

            if (!IsValidNameText(textHo.Text))
            {
                MessageBox.Show("Họ chỉ được gồm chữ cái và khoảng trắng!", "Báo lỗi");
                textHo.Focus();
                return false;
            }

            if (!IsValidNameText(textTen.Text))
            {
                MessageBox.Show("Tên chỉ được gồm chữ cái và khoảng trắng!", "Báo lỗi");
                textTen.Focus();
                return false;
            }

            if (textSoDT.Text == "")
            {
                MessageBox.Show("Số điện thoại không được để trống!", "Báo lỗi");
                textSoDT.Focus();
                return false;
            }

            if (!IsValidPhoneNumber(textSoDT.Text))
            {
                MessageBox.Show("Số điện thoại phải bắt đầu bằng 0 và gồm đúng 10 chữ số!", "Báo lỗi");
                textSoDT.Focus();
                return false;
            }

            if (txtTenMH.Text.Length > 50)
            {
                MessageBox.Show("Địa chỉ tối đa 50 ký tự!", "Báo lỗi");
                txtTenMH.Focus();
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

            if (dgvGiaoVien.Columns.Contains("HOTEN"))
                dgvGiaoVien.Columns["HOTEN"].Visible = false;

            dgvGiaoVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvGiaoVien.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvGiaoVien.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvGiaoVien.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            dgvGiaoVien.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvGiaoVien.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvGiaoVien.RowTemplate.Height = 38;
            dgvGiaoVien.BackgroundColor = Color.White;
            dgvGiaoVien.ReadOnly = true;
            dgvGiaoVien.AllowUserToAddRows = false;
            dgvGiaoVien.AllowUserToDeleteRows = false;
            dgvGiaoVien.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGiaoVien.MultiSelect = false;

            DatTyLeCot("MAGV", 90, 80);
            DatTyLeCot("HO", 150, 120);
            DatTyLeCot("TEN", 90, 80);
            DatTyLeCot("SODTLL", 130, 110);
            DatTyLeCot("DIACHI", 260, 170);
            CanGiuaCot("MAGV");
            CanGiuaCot("TEN");
            CanGiuaCot("SODTLL");
        }

        private void DatTyLeCot(string columnName, float fillWeight, int minimumWidth)
        {
            if (!dgvGiaoVien.Columns.Contains(columnName)) return;

            DataGridViewColumn column = dgvGiaoVien.Columns[columnName];
            column.FillWeight = fillWeight;
            column.MinimumWidth = minimumWidth;
        }

        private void CanGiuaCot(string columnName)
        {
            if (dgvGiaoVien.Columns.Contains(columnName))
                dgvGiaoVien.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
                string sql = chkHienThiNgungHoatDong.Checked
                    ? "EXEC dbo.SP_GET_GIAOVIEN_NGUNG_HOATDONG"
                    : "EXEC dbo.SP_GET_GIAOVIEN";

                giaoVienData = DBHelper.GetDataTable(sql);
                ApplySearch(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Báo lỗi");
            }
        }

        private void ApplySearch(bool showNotFoundMessage)
        {
            if (giaoVienData == null)
                return;

            string key = NormalizeSearchText((textTimKiem.Text ?? "").Trim());
            DataTable dtResult;

            if (key == "")
            {
                dtResult = giaoVienData;
            }
            else
            {
                var filtered = giaoVienData.AsEnumerable()
                    .Where(r =>
                        NormalizeSearchText(r.Field<string>("MAGV")).Contains(key) ||
                        NormalizeSearchText(r.Field<string>("HO")).Contains(key) ||
                        NormalizeSearchText(r.Field<string>("TEN")).Contains(key) ||
                        NormalizeSearchText((r.Field<string>("HO") ?? "") + " " + (r.Field<string>("TEN") ?? "")).Contains(key) ||
                        NormalizeSearchText(r.Field<string>("HOTEN")).Contains(key) ||
                        NormalizeSearchText(r.Field<string>("SODTLL")).Contains(key) ||
                        NormalizeSearchText(r.Field<string>("DIACHI")).Contains(key))
                    .ToList();

                dtResult = filtered.Any() ? filtered.CopyToDataTable() : giaoVienData.Clone();
            }

            BindGrid(dtResult);

            if (dgvGiaoVien.Rows.Count > 0)
            {
                dgvGiaoVien.Rows[0].Selected = true;
                LoadCurrentRowToInput();
            }
            else
            {
                ClearInput();

                if (showNotFoundMessage)
                    MessageBox.Show("Không tìm thấy giáo viên phù hợp!", "Thông báo");
            }

            SetEditingState(false);
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

        private bool GiaoVienCoDuLieuLienQuan(string maGV, out int soCauHoi, out int soLichDangKy)
        {
            soCauHoi = 0;
            soLichDangKy = 0;

            using (SqlConnection conn = DBHelper.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        SoCauHoi = (SELECT COUNT(*) FROM dbo.BODE WHERE RTRIM(MAGV) = RTRIM(@MAGV)),
                        SoLichDangKy = (SELECT COUNT(*) FROM dbo.GIAOVIEN_DANGKY WHERE RTRIM(MAGV) = RTRIM(@MAGV));", conn))
                {
                    cmd.Parameters.AddWithValue("@MAGV", maGV);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            soCauHoi = Convert.ToInt32(reader["SoCauHoi"]);
                            soLichDangKy = Convert.ToInt32(reader["SoLichDangKy"]);
                        }
                    }
                }
            }

            return soCauHoi > 0 || soLichDangKy > 0;
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            string maGV = txtMaGV.Text.Trim();
            if (maGV == "") return;
            if (chkHienThiNgungHoatDong.Checked)
            {
                MessageBox.Show("Giáo viên đang ngưng hoạt động. Hãy dùng chức năng Khôi phục GV nếu muốn mở lại.", "Thông báo");
                return;
            }

            bool coDuLieuLienQuan = GiaoVienCoDuLieuLienQuan(maGV, out int soCauHoi, out int soLichDangKy);
            string noiDungXacNhan = coDuLieuLienQuan
                ? $"Giáo viên này đang có dữ liệu liên quan:\n- Số câu hỏi đã soạn: {soCauHoi}\n- Số lịch thi đã đăng ký: {soLichDangKy}\n\nHệ thống sẽ ngưng hoạt động giáo viên và khóa tài khoản thay vì xóa hẳn. Bạn có chắc chắn muốn tiếp tục?"
                : "Giáo viên này chưa có dữ liệu liên quan.\n\nHệ thống sẽ xóa hẳn giáo viên khỏi database. Bạn có chắc chắn muốn xóa?";

            DialogResult dr = MessageBox.Show(
                noiDungXacNhan,
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
                        cmd.Parameters.AddWithValue("@MAGV", maGV);

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = 0;
                        if (returnValue.Value != null && returnValue.Value != DBNull.Value)
                            int.TryParse(returnValue.Value.ToString(), out result);

                        if (result == 0)
                        {
                            MessageBox.Show("Xóa hẳn giáo viên thành công!", "Thông báo");
                            LoadData();
                        }
                        else if (result == 2)
                        {
                            MessageBox.Show(
                                "Giáo viên đã có dữ liệu liên quan nên hệ thống đã ngưng hoạt động và khóa tài khoản.",
                                "Thông báo");
                            LoadData();
                        }
                        else if (result == 3)
                        {
                            MessageBox.Show("Không thể xóa hoặc khóa tài khoản đang đăng nhập!", "Báo lỗi");
                        }
                        else
                        {
                            MessageBox.Show("Không thể xóa giáo viên này!", "Báo lỗi");
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
                                isAdding
                                    ? "Không thể thêm giáo viên. Mã giáo viên bị trùng hoặc dữ liệu không hợp lệ!"
                                    : "Không thể sửa giáo viên. Mã giáo viên không tồn tại hoặc dữ liệu không hợp lệ!",
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

            if (dgvGiaoVien.CurrentRow != null)
                LoadCurrentRowToInput();
            else
                ClearInput();

            SetEditingState(false);
        }

        private void btnKhoiPhucGiaoVien_Click(object sender, EventArgs e)
        {
            if (!chkHienThiNgungHoatDong.Checked)
            {
                MessageBox.Show("Hãy tích Hiển thị GV ngưng hoạt động rồi chọn giáo viên cần khôi phục.", "Thông báo");
                return;
            }

            if (txtMaGV.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng chọn giáo viên cần khôi phục!", "Thông báo");
                return;
            }

            DialogResult dr = MessageBox.Show(
                "Bạn có chắc chắn muốn khôi phục giáo viên này không?\n\nNếu giáo viên có tài khoản bị khóa, hệ thống sẽ mở khóa tài khoản.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.SP_KHOIPHUC_GIAOVIEN", conn))
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

                        if (result == 0)
                        {
                            MessageBox.Show("Khôi phục giáo viên và mở khóa tài khoản thành công!", "Thông báo");
                            LoadData();
                        }
                        else if (result == 2)
                        {
                            MessageBox.Show("Khôi phục giáo viên thành công. Giáo viên này chưa có tài khoản để mở khóa.", "Thông báo");
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Không thể khôi phục giáo viên này!", "Báo lỗi");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
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

        private void textTimKiem_TextChanged(object sender, EventArgs e)
        {
            if (!textTimKiem.Enabled)
                return;

            ApplySearch(false);
        }

        private void chkHienThiNgungHoatDong_CheckedChanged(object sender, EventArgs e)
        {
            LoadData();
            SetEditingState(false);
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
