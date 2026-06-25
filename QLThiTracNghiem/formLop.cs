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
    public partial class formLop : Form
    {
        private bool isAdding = false;
        private DataTable lopData;

        // ===== Subform: nhúng formLopSinhVien vào nửa dưới =====
        private SplitContainer splitMain;
        private Panel pnlSinhVien;
        private formLopSinhVien currentSubForm;
        private string lastLoadedMaLop = "";

        private enum FocusContext { LOP, SINHVIEN }
        private FocusContext currentContext = FocusContext.LOP;
        private bool isUpdatingToolbar = false;

        public formLop()
        {
            InitializeComponent();
            dgvLop.SelectionChanged += dgvLop_SelectionChanged;
            textTimKiem.TextChanged += textTimKiem_TextChanged;
            
            WireUpFocusEvents();
        }

        private void WireUpFocusEvents()
        {
            dgvLop.Enter += (s, e) => SwitchContext(FocusContext.LOP);
            txtMaLop.Enter += (s, e) => SwitchContext(FocusContext.LOP);
            textTenLop.Enter += (s, e) => SwitchContext(FocusContext.LOP);
            textTimKiem.Enter += (s, e) => SwitchContext(FocusContext.LOP);
        }

        private void SwitchContext(FocusContext newContext)
        {
            if (currentContext == newContext) return;
            
            // Only allow switching if not currently editing something
            if (isAdding || (currentSubForm != null && currentSubForm.IsEditing)) return;
            
            currentContext = newContext;
            UpdateToolbarUI();
        }

        // ============================================================
        //  SUBFORM SETUP
        // ============================================================

        /// <summary>
        /// Tạo SplitContainer chia đôi form: nửa trên = quản lý lớp,
        /// nửa dưới = danh sách sinh viên (nhúng formLopSinhVien).
        /// </summary>
        private void SetupSubformUI()
        {
            // Chỉ chạy một lần.
            if (splitMain != null) return;

            // PHẢI resize form TRƯỚC khi tạo SplitContainer,
            // và gọi PerformLayout() để kích thước thực sự có hiệu lực.
            this.ClientSize = new Size(
                Math.Max(this.ClientSize.Width, 1100),
                Math.Max(this.ClientSize.Height, 850));
            this.MinimumSize = new Size(900, 500);
            this.PerformLayout();

            // Tạm gỡ anchor của dgvLop để tránh xung đột khi di chuyển.
            dgvLop.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // Tính SplitterDistance an toàn dựa trên chiều cao thực.
            int formH = this.ClientSize.Height;
            int splitterW = 6;
            int p1Min = 220;
            int p2Min = 180;
            // Cho Panel1 một độ cao cố định vừa đủ (ví dụ 300), còn lại nhường hết cho Sinh Viên
            int safeDistance = Math.Max(p1Min, Math.Min(300, formH - p2Min - splitterW));

            // Tạo SplitContainer — đặt MinSize = 0 trước, rồi set sau.
            splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                FixedPanel = FixedPanel.Panel1,
                SplitterWidth = 6,
                Panel1MinSize = 0,
                Panel2MinSize = 0,
                BackColor = SystemColors.ControlLight // Màu viền ngăn cách
            };

            splitMain.Panel1.BackColor = Color.White;
            splitMain.Panel2.BackColor = Color.White;

            // 1. Sao chép danh sách control cũ (trước khi thêm splitMain)
            Control[] controls = new Control[this.Controls.Count];
            this.Controls.CopyTo(controls, 0);

            // 2. Thêm SplitContainer vào Form trước để có kích thước thật
            this.Controls.Add(splitMain);

            // 3. Giờ mới đưa control cũ vào Panel1 để tránh lỗi lệch Anchor
            foreach (Control c in controls)
            {
                splitMain.Panel1.Controls.Add(c);
            }

            // Giờ mới set min size và splitter distance (form đã đủ lớn).
            splitMain.Panel1MinSize = p1Min;
            splitMain.Panel2MinSize = p2Min;
            splitMain.SplitterDistance = safeDistance;

            // Đặt lại anchor cho dgvLop để nó giãn theo Panel1.
            dgvLop.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                          | AnchorStyles.Left | AnchorStyles.Right;

            // Resize lần đầu cho dgvLop vừa khít Panel1.
            splitMain.Panel1.Resize += SplitPanel1_Resize;
            AdjustLopGrid();

            // Ẩn nút "DS Sinh Viên" vì giờ đã có subform bên dưới.
            btnDSSinhVien.Visible = false;

            // Tạo Panel chứa subform ở nửa dưới.
            pnlSinhVien = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            splitMain.Panel2.Controls.Add(pnlSinhVien);

            // Thêm nhãn gợi ý khi chưa chọn lớp nào.
            ShowSinhVienPlaceholder();
        }

        private void SplitPanel1_Resize(object sender, EventArgs e)
        {
            AdjustLopGrid();
        }

        /// <summary>
        /// Điều chỉnh dgvLop lấp đầy khoảng trống trong Panel1.
        /// </summary>
        private void AdjustLopGrid()
        {
            if (splitMain == null) return;

            int w = splitMain.Panel1.ClientSize.Width;
            int h = splitMain.Panel1.ClientSize.Height;
            int margin = 12;

            // Chỉnh lại kích thước lưới.
            dgvLop.Width = Math.Max(400, w - dgvLop.Left - margin);
            dgvLop.Height = Math.Max(80, h - dgvLop.Top - margin);
        }

        /// <summary>
        /// Hiển thị placeholder "Chọn lớp để xem sinh viên" khi chưa có subform.
        /// </summary>
        private void ShowSinhVienPlaceholder()
        {
            if (pnlSinhVien == null) return;
            pnlSinhVien.Controls.Clear();

            Label lbl = new Label
            {
                Dock = DockStyle.Fill,
                Text = "⬆  Chọn một lớp ở danh sách phía trên để xem danh sách sinh viên.",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(120, 120, 120),
                BackColor = Color.White,
                Font = new Font(this.Font.FontFamily, 10f, FontStyle.Italic)
            };
            pnlSinhVien.Controls.Add(lbl);
        }

        /// <summary>
        /// Nhúng formLopSinhVien vào Panel nửa dưới.
        /// </summary>
        private void LoadSubForm(string maLop)
        {
            if (pnlSinhVien == null) return;

            maLop = (maLop ?? "").Trim();

            // Không load lại nếu đang hiện cùng lớp.
            if (maLop == lastLoadedMaLop && currentSubForm != null)
                return;

            // Đóng subform cũ.
            if (currentSubForm != null)
            {
                currentSubForm.Close();
                currentSubForm.Dispose();
                currentSubForm = null;
            }
            pnlSinhVien.Controls.Clear();

            if (string.IsNullOrWhiteSpace(maLop))
            {
                lastLoadedMaLop = "";
                ShowSinhVienPlaceholder();
                return;
            }

            // Tạo và nhúng subform mới.
            currentSubForm = new formLopSinhVien(maLop);
            currentSubForm.TopLevel = false;
            currentSubForm.FormBorderStyle = FormBorderStyle.None;
            currentSubForm.MinimumSize = Size.Empty;
            currentSubForm.Dock = DockStyle.Fill;
            
            // Lắng nghe các event từ subform
            currentSubForm.SubformStateChanged += (s, e) => UpdateToolbarUI();
            currentSubForm.SubformFocusEntered += (s, e) => SwitchContext(FocusContext.SINHVIEN);

            pnlSinhVien.Controls.Add(currentSubForm);
            currentSubForm.Show();

            lastLoadedMaLop = maLop;
        }

        // ============================================================
        //  FORM LOAD & STATE
        // ============================================================

        private void formLop_Load(object sender, EventArgs e)
        {
            SetupSubformUI();
            LoadData();
            SetEditingState(false);
        }

        private void SetEditingState(bool editing)
        {
            txtMaLop.Enabled = editing && isAdding && currentContext == FocusContext.LOP;
            textTenLop.Enabled = editing && currentContext == FocusContext.LOP;

            dgvLop.Enabled = !editing;
            textTimKiem.Enabled = !editing;

            // Khi đang thêm/sửa lớp, tạm ẩn subform để tránh thao tác nhầm.
            if (pnlSinhVien != null)
                pnlSinhVien.Enabled = !(editing && currentContext == FocusContext.LOP);
                
            UpdateToolbarUI();
        }

        private void UpdateToolbarUI()
        {
            if (isUpdatingToolbar) return;
            isUpdatingToolbar = true;
            
            bool isLopContext = (currentContext == FocusContext.LOP);
            
            // Highlight panel border/background to indicate focus
            dgvLop.BackgroundColor = isLopContext ? Color.White : SystemColors.Control;
            if (currentSubForm != null && currentSubForm.Controls.ContainsKey("dgvSinhVien"))
            {
                var dgvSV = currentSubForm.Controls["dgvSinhVien"] as DataGridView;
                if (dgvSV != null)
                    dgvSV.BackgroundColor = !isLopContext ? Color.White : SystemColors.Control;
            }

            // Update texts
            btnThem.Text = isLopContext ? "Thêm Lớp" : "Thêm SV";
            btnXoa.Text = isLopContext ? "Xóa Lớp" : "Xóa SV";
            btnSua.Text = isLopContext ? "Sửa Lớp" : "Sửa SV";

            if (isLopContext)
            {
                bool hasCurrentClass = dgvLop.CurrentRow != null && txtMaLop.Text.Trim() != "";
                bool editing = isAdding || btnGhi.Enabled; // Simplified check for Lop editing state
                
                // If it's a forced override from somewhere, we check txtMaLop.Enabled etc.
                // Wait, we rely on the global 'isAdding' and btnGhi.Enabled
                
                btnGhi.Enabled = txtMaLop.Enabled || textTenLop.Enabled; // editing class
                btnPhucHoi.Enabled = btnGhi.Enabled;
                
                btnThem.Enabled = !btnGhi.Enabled;
                btnSua.Enabled = !btnGhi.Enabled && hasCurrentClass;
                btnXoa.Enabled = !btnGhi.Enabled && hasCurrentClass;
                
                btnKhoiPhuc.Enabled = false;
                btnKhoiPhuc.Visible = true;
                btnKhoiPhuc.Text = "Khôi phục SV";
            }
            else
            {
                if (currentSubForm != null)
                {
                    bool svEditing = currentSubForm.IsEditing;
                    
                    btnGhi.Enabled = svEditing;
                    btnPhucHoi.Enabled = svEditing;
                    
                    btnThem.Enabled = !svEditing && (dgvLop.CurrentRow != null);
                    btnSua.Enabled = !svEditing && currentSubForm.HasCurrentRow && !currentSubForm.ViewingInactive;
                    btnXoa.Enabled = !svEditing && currentSubForm.HasCurrentRow && !currentSubForm.ViewingInactive;
                    
                    btnKhoiPhuc.Visible = true;
                    btnKhoiPhuc.Enabled = !svEditing && currentSubForm.HasCurrentRow && currentSubForm.ViewingInactive;
                    btnKhoiPhuc.Text = "Khôi phục SV";
                }
            }

            isUpdatingToolbar = false;
        }

        // ============================================================
        //  CRUD LOGIC (giữ nguyên 100%)
        // ============================================================

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
            if (currentContext == FocusContext.LOP) ThemLop();
            else currentSubForm?.RequestAdd();
        }

        private void ThemLop()
        {
            isAdding = true;
            ClearInput();
            SetEditingState(true);
            txtMaLop.Focus();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (currentContext == FocusContext.LOP) SuaLop();
            else currentSubForm?.RequestEdit();
        }

        private void SuaLop()
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
            if (currentContext == FocusContext.LOP) XoaLop();
            else currentSubForm?.RequestDelete();
        }

        private void XoaLop()
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
            if (currentContext == FocusContext.LOP) GhiLop();
            else currentSubForm?.RequestSave();
        }

        private void GhiLop()
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
            if (currentContext == FocusContext.LOP) PhucHoiLop();
            else currentSubForm?.RequestCancel();
        }

        private void btnKhoiPhuc_Click(object sender, EventArgs e)
        {
            if (currentContext == FocusContext.SINHVIEN) currentSubForm?.RequestRestoreSV();
        }

        private void PhucHoiLop()
        {
            isAdding = false;

            if (dgvLop.CurrentRow != null)
                LoadCurrentRowToInput();
            else
                ClearInput();

            SetEditingState(false);
        }

        // ============================================================
        //  EVENTS
        // ============================================================

        private void dgvLop_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLop.Enabled && dgvLop.CurrentRow != null)
            {
                LoadCurrentRowToInput();
                SetEditingState(false);
            }

            // Load subform sinh viên theo lớp đang chọn.
            if (!isAdding &&
                dgvLop.CurrentRow != null &&
                dgvLop.Columns.Contains("MALOP"))
            {
                string maLop = dgvLop.CurrentRow.Cells["MALOP"].Value?.ToString().Trim();
                LoadSubForm(maLop);
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
