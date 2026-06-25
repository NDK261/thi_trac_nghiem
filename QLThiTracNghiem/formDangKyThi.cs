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
    public partial class formDangKyThi : Form
    {
        bool isAdding = false;
        DataTable dtDangKyGoc;
        public formDangKyThi()
        {
            InitializeComponent();
            txtSoCauThi.KeyPress += ChiChoNhapSo_KeyPress;
            txtThoiGian.KeyPress += ChiChoNhapSo_KeyPress;
        }

        private DateTime NgayThiToiThieu
        {
            get { return DateTime.Today.AddDays(1); }
        }

        private void ThietLapGioiHanNhap()
        {
            txtSoCauThi.MaxLength = 3;
            txtThoiGian.MaxLength = 2;

            cmbLop.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMonHoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTrinhDo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanThi.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTimKiem.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTimKiem.Items.Clear();
            cmbTimKiem.Items.AddRange(new string[]
            {
                "Tất cả",
                "Mã lớp",
                "Mã môn",
                "Trình độ",
                "Lần thi",
                "Ngày thi",
                "Mã giáo viên",
                "Số câu",
                "Thời gian"
            });
            cmbTimKiem.SelectedIndex = 0;
        }

        private void BatGioiHanNgayThi()
        {
            DateTime ngayToiThieu = NgayThiToiThieu;
            if (dtpNgayThi.Value.Date < ngayToiThieu)
                dtpNgayThi.Value = ngayToiThieu;

            dtpNgayThi.MinDate = ngayToiThieu;
        }

        private void TatGioiHanNgayThi()
        {
            dtpNgayThi.MinDate = DateTimePicker.MinimumDateTime;
        }

        private void ChiChoNhapSo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        // Đưa form về trạng thái xem, thêm mới hoặc sửa lịch thi.
        private void SetNormalState()
        {
            isAdding = false;
            TatGioiHanNgayThi();
            SetInputState(false, false);

            dgvDangKy.Enabled = true;
            txtTimKiem.Enabled = true;
            cmbTimKiem.Enabled = true;
            btnTimKiem.Enabled = true;
            btnLamMoiTimKiem.Enabled = true;
            btnGhi.Enabled = false;
            btnPhucHoi.Enabled = false;
            btnThem.Enabled = true;

            bool hasCurrentRow = dgvDangKy.CurrentRow != null && !dgvDangKy.CurrentRow.IsNewRow;
            btnSua.Enabled = hasCurrentRow;
            btnXoa.Enabled = hasCurrentRow;
            btnThoat.Enabled = true;
        }

        private void SetEditingState(bool adding)
        {
            isAdding = adding;
            SetInputState(true, adding);
            BatGioiHanNgayThi();

            // Đang thêm/sửa thì khóa lưới để không đổi dòng giữa chừng.
            dgvDangKy.Enabled = false;
            txtTimKiem.Enabled = false;
            cmbTimKiem.Enabled = false;
            btnTimKiem.Enabled = false;
            btnLamMoiTimKiem.Enabled = false;

            btnGhi.Enabled = true;
            btnPhucHoi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
            btnThoat.Enabled = true;
        }

        private void SetInputState(bool editing, bool adding)
        {
            // Thêm mới được chọn khóa lịch; sửa được đổi trình độ, ngày, số câu và thời gian.
            cmbLop.Enabled = editing && adding;
            cmbMonHoc.Enabled = editing && adding;
            cmbTrinhDo.Enabled = editing;
            cmbLanThi.Enabled = editing && adding;

            dtpNgayThi.Enabled = editing;
            txtSoCauThi.Enabled = editing;
            txtThoiGian.Enabled = editing;

            txtMaGV.Enabled = false;
            txtMaGV.ReadOnly = true;
        }

        private void ClearInput()
        {
            txtSoCauThi.Clear();
            txtThoiGian.Clear();
            // Mặc định ngày mai vì lịch thi không được tạo cho hôm nay.
            dtpNgayThi.Value = NgayThiToiThieu;
            txtMaGV.Text = Program.mUserName;

            if (cmbTrinhDo.Items.Count > 0) cmbTrinhDo.SelectedIndex = 0;
            if (cmbLanThi.Items.Count > 0) cmbLanThi.SelectedIndex = 0;
        }

        private void LoadCurrentRowToInput()
        {
            if (dgvDangKy.CurrentRow == null || dgvDangKy.CurrentRow.IsNewRow)
            {
                ClearInput();
                return;
            }

            DataGridViewRow row = dgvDangKy.CurrentRow;
            cmbLop.SelectedValue = row.Cells["MALOP"].Value?.ToString();
            cmbMonHoc.SelectedValue = row.Cells["MAMH"].Value?.ToString();
            cmbTrinhDo.Text = row.Cells["TRINHDO"].Value?.ToString();
            cmbLanThi.Text = row.Cells["LAN"].Value?.ToString();
            txtSoCauThi.Text = row.Cells["SOCAUTHI"].Value?.ToString();
            txtThoiGian.Text = row.Cells["THOIGIAN"].Value?.ToString();
            txtMaGV.Text = row.Cells["MAGV"].Value?.ToString();

            TatGioiHanNgayThi();

            if (row.Cells["NGAYTHI"].Value != DBNull.Value && row.Cells["NGAYTHI"].Value != null)
                dtpNgayThi.Value = Convert.ToDateTime(row.Cells["NGAYTHI"].Value);
            else
                dtpNgayThi.Value = NgayThiToiThieu;
        }

        private bool ValidateInput()
        {
            if (cmbLop.SelectedValue == null || cmbMonHoc.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ Lớp và Môn học!", "Báo lỗi");
                return false;
            }

            if (!int.TryParse(txtSoCauThi.Text.Trim(), out int soCauThi))
            {
                MessageBox.Show("Số câu thi phải là số nguyên!", "Báo lỗi");
                txtSoCauThi.Focus();
                return false;
            }

            // Số câu thi khớp CHECK constraint trong SQL.
            if (soCauThi < 10 || soCauThi > 100)
            {
                MessageBox.Show("Số câu thi phải từ 10 đến 100.", "Báo lỗi");
                txtSoCauThi.Focus();
                return false;
            }

            if (!int.TryParse(txtThoiGian.Text.Trim(), out int thoiGian))
            {
                MessageBox.Show("Thời gian thi phải là số nguyên tính bằng phút!", "Báo lỗi");
                txtThoiGian.Focus();
                return false;
            }

            // Thời gian thi phải khớp ràng buộc 5-60 phút.
            if (thoiGian < 5 || thoiGian > 60)
            {
                MessageBox.Show("Thời gian thi phải từ 5 đến 60 phút.", "Báo lỗi");
                txtThoiGian.Focus();
                return false;
            }

            // Ngày thi phải từ ngày mai trở đi.
            if (dtpNgayThi.Value.Date <= DateTime.Today)
            {
                MessageBox.Show("Ngày thi phải từ ngày mai trở đi, không được chọn hôm nay hoặc ngày đã qua!", "Báo lỗi");
                dtpNgayThi.Focus();
                return false;
            }

            return true;
        }

        private bool DangKyDaCoSinhVienThi(string maMH, string maLop, int lan)
        {
            // Có điểm hoặc bài thi tạm thì không được sửa/xóa lịch thi.
            object result = DBHelper.ExecuteScalar(
                "SP_KIEMTRA_DANGKY_DA_CO_SV_THI",
                new SqlParameter("@MAMH", maMH),
                new SqlParameter("@MALOP", maLop),
                new SqlParameter("@LAN", lan));

            return Convert.ToInt32(result) == 1;
        }

        private void cmbLop_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbTrinhDo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (txtSoCauThi.Text == "") return;

            // Kiểm tra quyền sở hữu ca thi trước khi sửa
            if (dgvDangKy.CurrentRow != null)
            {
                string maGV = dgvDangKy.CurrentRow.Cells["MAGV"].Value.ToString().Trim();
                if (Program.mGroup.Trim().ToUpper() != "PGV" && maGV != Program.mUserName.Trim())
                {
                    MessageBox.Show("Bạn không có quyền sửa lịch đăng ký thi của giảng viên khác!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                if (dgvDangKy.CurrentRow != null)
                {
                    string maMH = dgvDangKy.CurrentRow.Cells["MAMH"].Value.ToString();
                    string maLop = dgvDangKy.CurrentRow.Cells["MALOP"].Value.ToString();
                    int lan = int.Parse(dgvDangKy.CurrentRow.Cells["LAN"].Value.ToString());

                    if (DangKyDaCoSinhVienThi(maMH, maLop, lan))
                    {
                        MessageBox.Show(
                            "Không thể sửa đăng ký thi này vì đã có sinh viên thi hoặc đang có bài thi tạm.",
                            "Không cho sửa",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kiểm tra đăng ký thi: " + ex.Message, "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SetEditingState(false);
            txtSoCauThi.Focus();
        }

        private void formDangKyThi_Load(object sender, EventArgs e)
        {
            try
            {
                // MAGV lấy theo tài khoản đang đăng nhập.
                txtMaGV.Text = Program.mUserName;
                ThietLapGioiHanNhap();

                // Tải lớp cho ComboBox.
                DataTable dtLop = DBHelper.ExecuteDataTable("SP_GET_LOP");
                cmbLop.DisplayMember = "TENLOP";
                cmbLop.ValueMember = "MALOP";
                cmbLop.DataSource = dtLop;

                // Tải môn học cho ComboBox.
                DataTable dtMon = DBHelper.ExecuteDataTable("SP_GET_MONHOC");
                cmbMonHoc.DisplayMember = "TENMH";
                cmbMonHoc.ValueMember = "MAMH";
                cmbMonHoc.DataSource = dtMon;

                // Giá trị cố định theo đề: trình độ A/B/C, lần thi 1/2.
                cmbTrinhDo.Items.AddRange(new string[] { "A", "B", "C" });
                cmbTrinhDo.SelectedIndex = 0;

                cmbLanThi.Items.AddRange(new string[] { "1", "2" });
                cmbLanThi.SelectedIndex = 0;

                // Tải lịch thi hiện có lên lưới.
                LoadDangKy();
                SetNormalState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải form: " + ex.Message);
            }
        }
        private void LoadDangKy()
        {
            DataTable dt = DBHelper.ExecuteDataTable("SP_GET_DANGKYTHI");
            dtDangKyGoc = dt;
            dgvDangKy.DataSource = dtDangKyGoc;
            txtTimKiem.Clear();

            dgvDangKy.ReadOnly = true;
            dgvDangKy.AllowUserToAddRows = false;
            dgvDangKy.AllowUserToDeleteRows = false;
            dgvDangKy.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDangKy.MultiSelect = false;

            if (dgvDangKy.Columns.Contains("NGAYTHI"))
                dgvDangKy.Columns["NGAYTHI"].DefaultCellStyle.Format = "dd/MM/yyyy";

            LoadCurrentRowToInput();
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

        private string LayNgayThiDangChuoi(DataRow row)
        {
            if (row["NGAYTHI"] == DBNull.Value || row["NGAYTHI"] == null)
                return "";

            return Convert.ToDateTime(row["NGAYTHI"]).ToString("dd/MM/yyyy");
        }

        private void LocDangKyTheoTuKhoa()
        {
            if (dtDangKyGoc == null) return;

            string keyword = ChuyenKhongDau(txtTimKiem.Text.Trim());
            string kieuTim = cmbTimKiem.SelectedItem?.ToString() ?? "Tất cả";

            if (keyword == "")
            {
                dgvDangKy.DataSource = dtDangKyGoc;
                LoadCurrentRowToInput();
                SetNormalState();
                return;
            }

            DataTable dtLoc = dtDangKyGoc.Clone();

            foreach (DataRow row in dtDangKyGoc.Rows)
            {
                string maLop = ChuyenKhongDau(row["MALOP"].ToString());
                string maMH = ChuyenKhongDau(row["MAMH"].ToString());
                string trinhDo = ChuyenKhongDau(row["TRINHDO"].ToString().Trim());
                string lanThi = ChuyenKhongDau(row["LAN"].ToString().Trim());
                string ngayThi = ChuyenKhongDau(LayNgayThiDangChuoi(row));
                string maGV = ChuyenKhongDau(row["MAGV"].ToString());
                string soCau = ChuyenKhongDau(row["SOCAUTHI"].ToString().Trim());
                string thoiGian = ChuyenKhongDau(row["THOIGIAN"].ToString().Trim());

                bool khop =
                    (kieuTim == "Tất cả" &&
                        (maLop.Contains(keyword) ||
                         maMH.Contains(keyword) ||
                         trinhDo.Contains(keyword) ||
                         lanThi.Contains(keyword) ||
                         ngayThi.Contains(keyword) ||
                         maGV.Contains(keyword) ||
                         soCau.Contains(keyword) ||
                         thoiGian.Contains(keyword))) ||
                    (kieuTim == "Mã lớp" && maLop.Contains(keyword)) ||
                    (kieuTim == "Mã môn" && maMH.Contains(keyword)) ||
                    (kieuTim == "Trình độ" && trinhDo == keyword) ||
                    (kieuTim == "Lần thi" && lanThi == keyword) ||
                    (kieuTim == "Ngày thi" && ngayThi.Contains(keyword)) ||
                    (kieuTim == "Mã giáo viên" && maGV.Contains(keyword)) ||
                    (kieuTim == "Số câu" && soCau == keyword) ||
                    (kieuTim == "Thời gian" && thoiGian == keyword);

                if (khop)
                {
                    dtLoc.ImportRow(row);
                }
            }

            dgvDangKy.DataSource = dtLoc;
            LoadCurrentRowToInput();
            SetNormalState();
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            LocDangKyTheoTuKhoa();
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            LocDangKyTheoTuKhoa();
        }

        private void btnLamMoiTimKiem_Click(object sender, EventArgs e)
        {
            txtTimKiem.Clear();
            cmbTimKiem.SelectedIndex = 0;
            LocDangKyTheoTuKhoa();
            txtTimKiem.Focus();
        }

        private void cmbTimKiem_SelectedIndexChanged(object sender, EventArgs e)
        {
            LocDangKyTheoTuKhoa();
        }

        private void dgvDangKy_CellClick(object sender, DataGridViewCellEventArgs e)
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
            txtSoCauThi.Focus();
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            // Phục hồi chỉ hủy phần đang gõ dở và tải lại dữ liệu.
            LoadDangKy();
            SetNormalState();
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                string procedureName = isAdding ? "SP_THEM_DANGKYTHI" : "SP_SUA_DANGKYTHI";
                int result = DBHelper.ExecuteNonQueryWithReturn(
                    procedureName,
                    new SqlParameter("@MAGV", Program.mUserName),
                    new SqlParameter("@MAMH", cmbMonHoc.SelectedValue.ToString()),
                    new SqlParameter("@MALOP", cmbLop.SelectedValue.ToString()),
                    new SqlParameter("@TRINHDO", cmbTrinhDo.Text),
                    new SqlParameter("@NGAYTHI", dtpNgayThi.Value.Date),
                    new SqlParameter("@LAN", int.Parse(cmbLanThi.Text)),
                    new SqlParameter("@SOCAUTHI", int.Parse(txtSoCauThi.Text.Trim())),
                    new SqlParameter("@THOIGIAN", int.Parse(txtThoiGian.Text.Trim())));

                if (result == 1) MessageBox.Show("Lỗi: Lớp này đã đăng ký thi môn này ở lần thi này rồi!", "Báo lỗi");
                else if (result == 2)
                {
                    MessageBox.Show(
                        "Lỗi: Kho bộ đề chưa đủ câu hỏi theo luật 70% trình độ chính và 30% trình độ thấp hơn!",
                        "Báo lỗi");
                }
                else if (result == 3)
                {
                    MessageBox.Show("Không thể sửa đăng ký thi này vì đã có sinh viên thi hoặc đang có bài thi tạm.", "Báo lỗi");
                }
                else if (result == 4)
                {
                    MessageBox.Show("Thông tin đăng ký thi không hợp lệ. Vui lòng kiểm tra ngày thi, số câu, thời gian, lần thi và trình độ.", "Báo lỗi");
                }
                else if (result == 5)
                {
                    MessageBox.Show("Lỗi: Thứ tự ngày thi không hợp lệ (Ngày thi lần 2 phải sau hoặc cùng ngày với lần 1)!", "Báo lỗi");
                }
                else if (result == 6)
                {
                    MessageBox.Show("Lỗi: Bạn không có quyền chỉnh sửa lịch đăng ký thi của giảng viên khác!", "Báo lỗi");
                }
                else
                {
                    MessageBox.Show("Đăng ký thi thành công!", "Thông báo");
                    LoadDangKy();
                    SetNormalState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Đang nhập dở thì hỏi lại trước khi thoát.
            if (btnGhi.Enabled)
            {
                DialogResult result = MessageBox.Show(
                    "Bạn đang thêm/sửa đăng ký thi nhưng chưa ghi dữ liệu. Bạn có chắc muốn thoát không?",
                    "Xác nhận thoát",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;
            }

            this.Close();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvDangKy.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa trên lưới!", "Thông báo");
                return;
            }

            // Kiểm tra quyền sở hữu ca thi của giảng viên trước khi hỏi xác nhận xóa
            if (dgvDangKy.CurrentRow != null)
            {
                string maGV = dgvDangKy.CurrentRow.Cells["MAGV"].Value.ToString().Trim();
                if (Program.mGroup.Trim().ToUpper() != "PGV" && maGV != Program.mUserName.Trim())
                {
                    MessageBox.Show("Bạn không có quyền xóa lịch đăng ký thi của giảng viên khác!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Lấy khóa chính của lịch đang chọn để xóa đúng dòng.
            string maMH = dgvDangKy.CurrentRow.Cells["MAMH"].Value.ToString();
            string maLop = dgvDangKy.CurrentRow.Cells["MALOP"].Value.ToString();
            int lan = int.Parse(dgvDangKy.CurrentRow.Cells["LAN"].Value.ToString());
            string maMHThongBao = maMH.Trim();
            string maLopThongBao = maLop.Trim();

            bool daCoSinhVienThi;
            try
            {
                daCoSinhVienThi = DangKyDaCoSinhVienThi(maMH, maLop, lan);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kiểm tra đăng ký thi: " + ex.Message, "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (daCoSinhVienThi)
            {
                MessageBox.Show(
                    "Không thể xóa đăng ký thi này vì đã có sinh viên thi hoặc đang có bài thi tạm.",
                    "Không cho xóa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Nếu xóa Lần 1, kiểm tra xem có lịch thi Lần 2 hay không
            if (lan == 1)
            {
                try
                {
                    string sql = $"SELECT COUNT(*) FROM dbo.GIAOVIEN_DANGKY WHERE MAMH = '{maMH.Trim()}' AND MALOP = '{maLop.Trim()}' AND LAN = 2";
                    DataTable dtCount = DBHelper.GetDataTable(sql);

                    if (dtCount.Rows.Count > 0 && Convert.ToInt32(dtCount.Rows[0][0]) > 0)
                    {
                        MessageBox.Show(
                            "Không thể xóa đăng ký thi Lần 1 vì lịch đăng ký thi Lần 2 của môn học và lớp này vẫn đang tồn tại. Vui lòng xóa Lần 2 trước!",
                            "Không cho xóa",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kiểm tra lịch thi Lần 2: " + ex.Message, "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string thongBaoXoa =
                "Bạn có chắc muốn xóa đăng ký thi này?\n\n" +
                $"Môn: {maMHThongBao}\n" +
                $"Lớp: {maLopThongBao}\n" +
                $"Lần thi: {lan}";

            if (MessageBox.Show(thongBaoXoa,
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int result = DBHelper.ExecuteNonQueryWithReturn(
                        "SP_XOA_DANGKYTHI",
                        new SqlParameter("@MAMH", maMH),
                        new SqlParameter("@MALOP", maLop),
                        new SqlParameter("@LAN", lan));

                    if (result == 1)
                    {
                        MessageBox.Show("Không thể xóa đăng ký thi này vì đã có sinh viên thi hoặc đang có bài thi tạm.", "Báo lỗi");
                    }
                    else if (result == 2)
                    {
                        MessageBox.Show("Lỗi: Bạn không có quyền xóa lịch đăng ký thi của giảng viên khác!", "Báo lỗi");
                    }
                    else if (result == 3)
                    {
                        MessageBox.Show("Không thể xóa đăng ký thi Lần 1 vì lịch đăng ký thi Lần 2 của lớp và môn học này vẫn đang tồn tại. Vui lòng xóa Lần 2 trước!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Xóa đăng ký thi thành công!", "Thông báo");
                        LoadDangKy(); // Xóa xong thì tải lại lưới.
                        SetNormalState();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Báo lỗi");
                }
            }
        }
    }
}
