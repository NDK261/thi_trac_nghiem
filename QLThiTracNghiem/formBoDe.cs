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
        DataTable dtBoDeGoc;
        public formBoDe()
        {
            InitializeComponent();
        }

        private void ThietLapGioiHanNhap()
        {
            txtNoiDung.MaxLength = 200;
            txtA.MaxLength = 200;
            txtB.MaxLength = 200;
            txtC.MaxLength = 200;
            txtD.MaxLength = 200;

            cmbMonHoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTrinhDo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDapAn.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTimKiem.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTimKiem.Items.Clear();
            cmbTimKiem.Items.AddRange(new string[]
            {
                "Tất cả",
                "Mã câu hỏi",
                "Trình độ",
                "Nội dung",
                "Đáp án A-D",
                "Đáp án đúng",
                "Mã giáo viên"
            });
            cmbTimKiem.SelectedIndex = 0;
        }

        // Đưa các nút và ô nhập về đúng trạng thái hiện tại của form.
        private void SetNormalState()
        {
            isAdding = false;

            // Trạng thái xem: chỉ chọn môn và chọn dòng trên lưới.
            SetInputState(false);
            cmbMonHoc.Enabled = true;
            dgvBoDe.Enabled = true;
            txtTimKiem.Enabled = true;
            cmbTimKiem.Enabled = true;
            btnTimKiem.Enabled = true;
            btnLamMoiTimKiem.Enabled = true;

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
            txtTimKiem.Enabled = false;
            cmbTimKiem.Enabled = false;
            btnTimKiem.Enabled = false;
            btnLamMoiTimKiem.Enabled = false;

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
                DataTable dtMonHoc = DBHelper.ExecuteDataTable("SP_GET_MONHOC");
                cmbMonHoc.DataSource = dtMonHoc;
                cmbMonHoc.DisplayMember = "TENMH";
                cmbMonHoc.ValueMember = "MAMH";

                // Giới hạn nhập theo cấu trúc bảng BODE để người dùng biết lỗi ngay trên form.
                ThietLapGioiHanNhap();

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
                    new SqlParameter("@MAMH", maMH),
                    new SqlParameter("@MAGV", Program.mUserName ?? string.Empty),
                    new SqlParameter("@NHOM", Program.mGroup ?? string.Empty));
                dtBoDeGoc = dtBoDe;
                dgvBoDe.DataSource = dtBoDeGoc;
                ApplyGridFormat();
                txtTimKiem.Clear();

                LoadCurrentRowToInput();
                SetNormalState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải bộ đề: " + ex.Message);
            }
        }

        private string ChuyenKhongDau(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                System.Globalization.UnicodeCategory unicodeCategory =
                    System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(System.Text.NormalizationForm.FormC)
                .Replace("đ", "d")
                .Replace("Đ", "d")
                .ToLower();
        }

        private void LocBoDeTheoTuKhoa()
        {
            if (dtBoDeGoc == null) return;

            string keyword = ChuyenKhongDau(txtTimKiem.Text.Trim());
            string kieuTim = cmbTimKiem.SelectedItem?.ToString() ?? "Tất cả";
            if (keyword == "")
            {
                dgvBoDe.DataSource = dtBoDeGoc;
                ApplyGridFormat();
                LoadCurrentRowToInput();
                SetNormalState();
                return;
            }

            DataTable dtLoc = dtBoDeGoc.Clone();

            foreach (DataRow row in dtBoDeGoc.Rows)
            {
                string maCauHoi = ChuyenKhongDau(row["CAUHOI"].ToString());
                string trinhDo = ChuyenKhongDau(row["TRINHDO"].ToString().Trim());
                string noiDung = ChuyenKhongDau(row["NOIDUNG"].ToString());
                string dapAnA = ChuyenKhongDau(row["A"].ToString());
                string dapAnB = ChuyenKhongDau(row["B"].ToString());
                string dapAnC = ChuyenKhongDau(row["C"].ToString());
                string dapAnD = ChuyenKhongDau(row["D"].ToString());
                string dapAnDung = ChuyenKhongDau(row["DAP_AN"].ToString().Trim());
                string maGV = ChuyenKhongDau(row["MAGV"].ToString());

                bool khop =
                    (kieuTim == "Tất cả" &&
                        (maCauHoi.Contains(keyword) ||
                         trinhDo.Contains(keyword) ||
                         noiDung.Contains(keyword) ||
                         dapAnA.Contains(keyword) ||
                         dapAnB.Contains(keyword) ||
                         dapAnC.Contains(keyword) ||
                         dapAnD.Contains(keyword) ||
                         dapAnDung.Contains(keyword) ||
                         maGV.Contains(keyword))) ||
                    (kieuTim == "Mã câu hỏi" && maCauHoi == keyword) ||
                    (kieuTim == "Trình độ" && trinhDo == keyword) ||
                    (kieuTim == "Nội dung" && noiDung.Contains(keyword)) ||
                    (kieuTim == "Đáp án A-D" &&
                        (dapAnA.Contains(keyword) ||
                         dapAnB.Contains(keyword) ||
                         dapAnC.Contains(keyword) ||
                         dapAnD.Contains(keyword))) ||
                    (kieuTim == "Đáp án đúng" && dapAnDung == keyword) ||
                    (kieuTim == "Mã giáo viên" && maGV.Contains(keyword));

                if (khop)
                {
                    dtLoc.ImportRow(row);
                }
            }

            dgvBoDe.DataSource = dtLoc;
            ApplyGridFormat();
            LoadCurrentRowToInput();
            SetNormalState();
        }

        private void ApplyGridFormat()
        {
            dgvBoDe.ReadOnly = true;
            dgvBoDe.AllowUserToAddRows = false;
            dgvBoDe.AllowUserToDeleteRows = false;
            dgvBoDe.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBoDe.MultiSelect = false;
            dgvBoDe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBoDe.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvBoDe.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvBoDe.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            dgvBoDe.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvBoDe.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvBoDe.RowTemplate.Height = 46;
            dgvBoDe.BackgroundColor = Color.White;

            DatHeader("CAUHOI", "Mã câu");
            DatHeader("TRINHDO", "Trình độ");
            DatHeader("NOIDUNG", "Nội dung câu hỏi");
            DatHeader("DAP_AN", "Đáp án");
            DatHeader("MAGV", "Mã GV");

            if (dgvBoDe.Columns.Contains("MAMH"))
                dgvBoDe.Columns["MAMH"].Visible = false;

            DatTyLeCot("CAUHOI", 60, 58);
            DatTyLeCot("TRINHDO", 60, 58);
            DatTyLeCot("NOIDUNG", 300, 230);
            DatTyLeCot("A", 145, 110);
            DatTyLeCot("B", 145, 110);
            DatTyLeCot("C", 145, 110);
            DatTyLeCot("D", 145, 110);
            DatTyLeCot("DAP_AN", 62, 62);
            DatTyLeCot("MAGV", 82, 78);

            CanGiuaCot("CAUHOI");
            CanGiuaCot("TRINHDO");
            CanGiuaCot("DAP_AN");
            CanGiuaCot("MAGV");
        }

        private void DatHeader(string columnName, string header)
        {
            if (dgvBoDe.Columns.Contains(columnName))
                dgvBoDe.Columns[columnName].HeaderText = header;
        }

        private void DatTyLeCot(string columnName, float fillWeight, int minimumWidth)
        {
            if (!dgvBoDe.Columns.Contains(columnName)) return;

            DataGridViewColumn column = dgvBoDe.Columns[columnName];
            column.FillWeight = fillWeight;
            column.MinimumWidth = minimumWidth;
        }

        private void CanGiuaCot(string columnName)
        {
            if (dgvBoDe.Columns.Contains(columnName))
                dgvBoDe.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            LocBoDeTheoTuKhoa();
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            LocBoDeTheoTuKhoa();
        }

        private void btnLamMoiTimKiem_Click(object sender, EventArgs e)
        {
            txtTimKiem.Clear();
            cmbTimKiem.SelectedIndex = 0;
            LocBoDeTheoTuKhoa();
            txtTimKiem.Focus();
        }

        private void cmbTimKiem_SelectedIndexChanged(object sender, EventArgs e)
        {
            LocBoDeTheoTuKhoa();
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
            if (txtCauHoi.Text == "")
            {
                MessageBox.Show("Vui lòng chọn câu hỏi cần xóa trên lưới!", "Thông báo");
                return;
            }

            if (txtMaGV.Text.Trim() != Program.mUserName.Trim())
            {
                MessageBox.Show("Bạn không có quyền xóa câu hỏi của người khác!", "Cảnh báo");
                return;
            }

            string noiDungNgan = txtNoiDung.Text.Trim();
            if (noiDungNgan.Length > 90)
                noiDungNgan = noiDungNgan.Substring(0, 90) + "...";

            string thongBaoXoa =
                "Bạn có chắc muốn xóa câu hỏi này?\n\n" +
                $"Mã câu hỏi: {txtCauHoi.Text.Trim()}\n" +
                $"Nội dung: {noiDungNgan}";

            if (MessageBox.Show(thongBaoXoa, "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // SP trả mã lỗi để biết câu đã thi hay không đúng giáo viên.
                    int result = DBHelper.ExecuteNonQueryWithReturn(
                        "SP_XOA_BODE",
                        new SqlParameter("@CAUHOI", int.Parse(txtCauHoi.Text.Trim())),
                        new SqlParameter("@MAGV", Program.mUserName));

                    if (result == 1) MessageBox.Show("Không thể xóa câu hỏi đã xuất hiện trong bài thi hoặc bài thi tạm của sinh viên!", "Báo lỗi");
                    else if (result == 2) MessageBox.Show("Bạn không có quyền xóa câu hỏi này!", "Báo lỗi");
                    else
                    {
                        MessageBox.Show("Xóa thành công!", "Thông báo");
                        LoadBoDe(cmbMonHoc.SelectedValue.ToString());
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
            if (txtCauHoi.Text == "")
            {
                MessageBox.Show("Vui lòng chọn câu hỏi cần sửa trên lưới!", "Thông báo");
                return;
            }

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
                string procedureName = isAdding ? "SP_THEM_BODE" : "SP_SUA_BODE";
                List<SqlParameter> parameters = new List<SqlParameter>();

                // Thêm mới không truyền @CAUHOI; sửa thì phải truyền để SP biết dòng cần cập nhật.
                if (!isAdding)
                    parameters.Add(new SqlParameter("@CAUHOI", int.Parse(txtCauHoi.Text.Trim())));

                parameters.AddRange(new SqlParameter[]
                {
                    new SqlParameter("@MAMH", cmbMonHoc.SelectedValue.ToString()),
                    new SqlParameter("@TRINHDO", cmbTrinhDo.Text),
                    new SqlParameter("@NOIDUNG", txtNoiDung.Text.Trim()),
                    new SqlParameter("@A", txtA.Text.Trim()),
                    new SqlParameter("@B", txtB.Text.Trim()),
                    new SqlParameter("@C", txtC.Text.Trim()),
                    new SqlParameter("@D", txtD.Text.Trim()),
                    new SqlParameter("@DAP_AN", cmbDapAn.Text),
                    new SqlParameter("@MAGV", Program.mUserName) // SP dùng MAGV để kiểm tra quyền.
                });

                // SP kiểm tra lại quyền, câu đã thi và dữ liệu đáp án.
                int result = DBHelper.ExecuteNonQueryWithReturn(procedureName, parameters.ToArray());
                if (result == 1) MessageBox.Show("Mã câu hỏi đã tồn tại!", "Báo lỗi");
                else if (result == 2) MessageBox.Show("Bạn không có quyền sửa câu hỏi này!", "Báo lỗi");
                else if (result == 3) MessageBox.Show("Không thể sửa câu hỏi đã xuất hiện trong bài thi hoặc bài thi tạm, vì sửa sẽ làm sai nội dung bài làm của sinh viên!", "Báo lỗi");
                else if (result == 4) MessageBox.Show("Dữ liệu câu hỏi không hợp lệ hoặc có đáp án bị trùng!", "Báo lỗi");
                else
                {
                    MessageBox.Show("Lưu thành công!", "Thông báo");
                    LoadBoDe(cmbMonHoc.SelectedValue.ToString());
                    SetNormalState();
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
