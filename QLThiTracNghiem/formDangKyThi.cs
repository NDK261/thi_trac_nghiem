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
    public partial class formDangKyThi : Form
    {
        bool isAdding = false;
        public formDangKyThi()
        {
            InitializeComponent();
        }

        // Chuẩn trạng thái cho form Đăng ký thi:
        // - Bình thường: xem danh sách, chọn dòng trên lưới, chưa cho sửa trực tiếp.
        // - Đang Thêm: mở các thông tin tạo lịch thi mới.
        // - Đang Sửa: chỉ mở Ngày thi, Số câu thi, Thời gian.
        //   Lớp + Môn + Lần thi là khóa chính theo đề; Trình độ cũng khóa lại vì SP_SUA_DANGKYTHI hiện dùng nó để tìm đúng dòng.
        private void SetNormalState()
        {
            isAdding = false;
            SetInputState(false, false);

            dgvDangKy.Enabled = true;
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

            // Khi đang thêm/sửa, khóa lưới để người dùng không đổi dòng làm mất dữ liệu đang nhập.
            dgvDangKy.Enabled = false;

            btnGhi.Enabled = true;
            btnPhucHoi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
            btnThoat.Enabled = true;
        }

        private void SetInputState(bool editing, bool adding)
        {
            // Thêm mới thì được chọn lớp, môn, trình độ, lần thi.
            // Sửa thì khóa các thông tin định danh lịch thi để tránh sửa nhầm khóa.
            cmbLop.Enabled = editing && adding;
            cmbMonHoc.Enabled = editing && adding;
            cmbTrinhDo.Enabled = editing && adding;
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
            dtpNgayThi.Value = DateTime.Now;
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

            if (row.Cells["NGAYTHI"].Value != DBNull.Value && row.Cells["NGAYTHI"].Value != null)
                dtpNgayThi.Value = Convert.ToDateTime(row.Cells["NGAYTHI"].Value);
            else
                dtpNgayThi.Value = DateTime.Now;
        }

        private bool ValidateInput()
        {
            if (!int.TryParse(txtSoCauThi.Text.Trim(), out int soCauThi))
            {
                MessageBox.Show("Số câu thi phải là số nguyên!", "Báo lỗi");
                txtSoCauThi.Focus();
                return false;
            }

            if (soCauThi < 10 || soCauThi > 100)
            {
                MessageBox.Show("Số câu thi phải từ 10 đến 100 theo yêu cầu đề tài!", "Báo lỗi");
                txtSoCauThi.Focus();
                return false;
            }

            if (!int.TryParse(txtThoiGian.Text.Trim(), out int thoiGian))
            {
                MessageBox.Show("Thời gian thi phải là số nguyên tính bằng phút!", "Báo lỗi");
                txtThoiGian.Focus();
                return false;
            }

            // File Word ghi thời gian từ 5 đến 60 phút, nhưng script database THITRACNGHIEM.sql
            // đang có CHECK constraint CK_THOIGIAN là 15 đến 60. Ở đây ưu tiên ràng buộc SQL Server
            // để người dùng không nhập 5-14 rồi bị lỗi khi Ghi xuống database.
            if (thoiGian < 15 || thoiGian > 60)
            {
                MessageBox.Show("Thời gian thi phải từ 15 đến 60 phút theo ràng buộc database!", "Báo lỗi");
                txtThoiGian.Focus();
                return false;
            }

            // Không cho tạo/sửa lịch thi về ngày đã qua.
            // Sinh viên chỉ được thi theo lịch trong bảng GIAOVIEN_DANGKY, nên ngày thi bị lùi về quá khứ
            // sẽ làm lịch thi khó kiểm soát và dễ phát sinh trường hợp "đăng ký hôm qua nhưng hôm nay vẫn thi".
            if (dtpNgayThi.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Ngày thi không được nhỏ hơn ngày hiện tại!", "Báo lỗi");
                dtpNgayThi.Focus();
                return false;
            }

            return true;
        }

        private bool DangKyDaCoSinhVienThi(string maMH, string maLop, int lan)
        {
            // Một đăng ký thi được xem là đã phát sinh bài thi nếu trong BANGDIEM đã có điểm
            // của sinh viên thuộc đúng lớp + môn + lần thi đó. Khi đã có điểm thì không được xóa/sửa
            // lịch thi nữa, vì xóa lịch sẽ làm mất căn cứ giải thích điểm thi của sinh viên.
            using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT COUNT(*)
                    FROM BANGDIEM BD
                    INNER JOIN SINHVIEN SV ON BD.MASV = SV.MASV
                    WHERE BD.MAMH = @MAMH
                      AND SV.MALOP = @MALOP
                      AND BD.LAN = @LAN";

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MAMH", maMH);
                    cmd.Parameters.AddWithValue("@MALOP", maLop);
                    cmd.Parameters.AddWithValue("@LAN", lan);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
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

            if (dgvDangKy.CurrentRow != null)
            {
                string maMH = dgvDangKy.CurrentRow.Cells["MAMH"].Value.ToString();
                string maLop = dgvDangKy.CurrentRow.Cells["MALOP"].Value.ToString();
                int lan = int.Parse(dgvDangKy.CurrentRow.Cells["LAN"].Value.ToString());

                if (DangKyDaCoSinhVienThi(maMH, maLop, lan))
                {
                    MessageBox.Show(
                        "Không thể sửa đăng ký thi này vì đã có sinh viên của lớp thi và có điểm.",
                        "Không cho sửa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            SetEditingState(false);
            txtSoCauThi.Focus();
        }

        private void formDangKyThi_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Gán Mã Giảng Viên tự động
                txtMaGV.Text = Program.mUserName;

                // 2. Load ComboBox Lớp
                DataTable dtLop = DBHelper.GetDataTable("EXEC SP_GET_LOP");
                cmbLop.DataSource = dtLop;
                cmbLop.DisplayMember = "TENLOP";
                cmbLop.ValueMember = "MALOP";

                // 3. Load ComboBox Môn Học
                DataTable dtMon = DBHelper.GetDataTable("EXEC SP_GET_MONHOC");
                cmbMonHoc.DataSource = dtMon;
                cmbMonHoc.DisplayMember = "TENMH";
                cmbMonHoc.ValueMember = "MAMH";

                // 4. Mồi dữ liệu cứng cho các ComboBox cấu hình
                cmbTrinhDo.Items.AddRange(new string[] { "A", "B", "C" });
                cmbTrinhDo.SelectedIndex = 0;

                cmbLanThi.Items.AddRange(new string[] { "1", "2" });
                cmbLanThi.SelectedIndex = 0;

                // 5. Tải dữ liệu đăng ký lên Lưới
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
            DataTable dt = DBHelper.GetDataTable("EXEC SP_GET_DANGKYTHI");
            dgvDangKy.DataSource = dt;

            dgvDangKy.ReadOnly = true;
            dgvDangKy.AllowUserToAddRows = false;
            dgvDangKy.AllowUserToDeleteRows = false;
            dgvDangKy.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDangKy.MultiSelect = false;

            if (dgvDangKy.Columns.Contains("NGAYTHI"))
                dgvDangKy.Columns["NGAYTHI"].DefaultCellStyle.Format = "dd/MM/yyyy";

            LoadCurrentRowToInput();
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
            // Phục hồi chỉ hủy dữ liệu đang gõ dở và tải lại danh sách đăng ký.
            // Không gọi INSERT/UPDATE/DELETE nên không ảnh hưởng database.
            LoadDangKy();
            SetNormalState();
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
                        cmd.CommandText = isAdding ? "SP_THEM_DANGKYTHI" : "SP_SUA_DANGKYTHI";

                        cmd.Parameters.AddWithValue("@MAGV", Program.mUserName);
                        cmd.Parameters.AddWithValue("@MAMH", cmbMonHoc.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@MALOP", cmbLop.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@TRINHDO", cmbTrinhDo.Text);
                        cmd.Parameters.AddWithValue("@NGAYTHI", dtpNgayThi.Value);
                        cmd.Parameters.AddWithValue("@LAN", int.Parse(cmbLanThi.Text));
                        cmd.Parameters.AddWithValue("@SOCAUTHI", int.Parse(txtSoCauThi.Text.Trim()));
                        cmd.Parameters.AddWithValue("@THOIGIAN", int.Parse(txtThoiGian.Text.Trim()));

                        System.Data.SqlClient.SqlParameter retVal = new System.Data.SqlClient.SqlParameter();
                        retVal.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(retVal);

                        cmd.ExecuteNonQuery();
                        int result = (int)retVal.Value;

                        if (result == 1) MessageBox.Show("Lỗi: Lớp này đã đăng ký thi môn này ở lần thi này rồi!", "Báo lỗi");
                        else if (result == 2) MessageBox.Show("Lỗi: Kho bộ đề KHÔNG ĐỦ câu hỏi cho trình độ này!", "Báo lỗi");
                        else
                        {
                            MessageBox.Show("Đăng ký thi thành công!", "Thông báo");
                            LoadDangKy();
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

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Khi đang thêm/sửa lịch thi, nút Ghi sẽ bật.
            // Nếu thoát lúc này thì dữ liệu đang gõ dở chưa được lưu xuống SQL Server.
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

            // Lấy thông tin từ dòng đang chọn để xác định đúng đợt thi cần xóa
            string maMH = dgvDangKy.CurrentRow.Cells["MAMH"].Value.ToString();
            string maLop = dgvDangKy.CurrentRow.Cells["MALOP"].Value.ToString();
            int lan = int.Parse(dgvDangKy.CurrentRow.Cells["LAN"].Value.ToString());

            if (DangKyDaCoSinhVienThi(maMH, maLop, lan))
            {
                MessageBox.Show(
                    "Không thể xóa đăng ký thi này vì đã có sinh viên của lớp thi và có điểm.",
                    "Không cho xóa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc muốn xóa đợt đăng ký thi môn {maMH} của lớp {maLop} lần {lan}?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
                    {
                        conn.Open();
                        using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SP_XOA_DANGKYTHI", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MAMH", maMH);
                            cmd.Parameters.AddWithValue("@MALOP", maLop);
                            cmd.Parameters.AddWithValue("@LAN", lan);

                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Xóa đăng ký thi thành công!", "Thông báo");
                            LoadDangKy(); // Tải lại lưới
                            SetNormalState();
                        }
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
