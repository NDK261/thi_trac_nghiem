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
        public formDangKyThi()
        {
            InitializeComponent();
        }

        // Trạng thái chung của form Đăng ký thi:
        // - Bình thường: xem danh sách, chọn dòng trên lưới, chưa cho sửa trực tiếp.
        // - Đang Thêm: mở các thông tin tạo lịch thi mới.
        // - Đang Sửa: chỉ mở Ngày thi, Số câu thi, Thời gian.
        //   Lớp + Môn + Lần thi là phần định danh lịch thi nên mình không cho sửa trực tiếp.
        //   Trình độ cũng khóa lại để tránh đổi kiểu đề sau khi đã lập lịch.
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

            // Đang thêm/sửa thì khóa lưới, tránh chọn nhầm dòng khác khi còn dữ liệu đang gõ.
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
            // Thêm mới thì được chọn đủ lớp, môn, trình độ, lần thi.
            // Khi sửa chỉ cho đổi thông tin cấu hình như ngày, số câu, thời gian.
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
            // Lịch thi phải tạo trước ít nhất một ngày, nên khi thêm mình mồi sẵn ngày mai.
            dtpNgayThi.Value = DateTime.Today.AddDays(1);
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

            // Thời gian thi hợp lệ là từ 5 đến 60 phút, trùng với ràng buộc ở database.
            if (thoiGian < 5 || thoiGian > 60)
            {
                MessageBox.Show("Thời gian thi phải từ 5 đến 60 phút theo yêu cầu đề tài!", "Báo lỗi");
                txtThoiGian.Focus();
                return false;
            }

            // Ngày thi phải từ ngày mai trở đi. Hôm nay và ngày quá khứ đều không hợp lệ.
            // Cách này giúp lịch thi được chuẩn bị trước, không phải vừa tạo lịch là thi ngay.
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
            // Nếu BANGDIEM đã có điểm của sinh viên thuộc lớp/môn/lần này
            // thì xem như lịch đã được sử dụng, không cho sửa/xóa nữa.
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
                // Mã giáo viên lấy theo tài khoản đang đăng nhập.
                txtMaGV.Text = Program.mUserName;

                // Load danh sách lớp cho ComboBox.
                DataTable dtLop = DBHelper.GetDataTable("EXEC SP_GET_LOP");
                cmbLop.DataSource = dtLop;
                cmbLop.DisplayMember = "TENLOP";
                cmbLop.ValueMember = "MALOP";

                // Load danh sách môn học cho ComboBox.
                DataTable dtMon = DBHelper.GetDataTable("EXEC SP_GET_MONHOC");
                cmbMonHoc.DataSource = dtMon;
                cmbMonHoc.DisplayMember = "TENMH";
                cmbMonHoc.ValueMember = "MAMH";

                // Các giá trị cấu hình cố định theo đề: trình độ A/B/C, lần thi 1/2.
                cmbTrinhDo.Items.AddRange(new string[] { "A", "B", "C" });
                cmbTrinhDo.SelectedIndex = 0;

                cmbLanThi.Items.AddRange(new string[] { "1", "2" });
                cmbLanThi.SelectedIndex = 0;

                // Tải danh sách lịch thi hiện có lên lưới.
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
            // Phục hồi chỉ hủy phần đang gõ dở và tải lại dữ liệu từ database.
            // Không gọi SP thêm/sửa/xóa nên không làm đổi dữ liệu.
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
                        else if (result == 2)
                        {
                            MessageBox.Show(
                                "Lỗi: Kho bộ đề chưa đủ câu hỏi theo luật 70% trình độ chính và 30% trình độ thấp hơn!",
                                "Báo lỗi");
                        }
                        else if (result == 3)
                        {
                            MessageBox.Show("Không thể sửa đăng ký thi này vì đã có sinh viên thi và có điểm.", "Báo lỗi");
                        }
                        else if (result == 4)
                        {
                            MessageBox.Show("Ngày thi, số câu, thời gian, lần thi hoặc trình độ không hợp lệ!", "Báo lỗi");
                        }
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
            // Nếu đang thêm/sửa mà chưa Ghi thì hỏi lại trước khi thoát.
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

            // Lấy khóa của dòng đang chọn để xóa đúng lịch thi.
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

                            SqlParameter returnValue = new SqlParameter();
                            returnValue.Direction = ParameterDirection.ReturnValue;
                            cmd.Parameters.Add(returnValue);

                            cmd.ExecuteNonQuery();
                            int result = (int)returnValue.Value;

                            if (result == 1)
                            {
                                MessageBox.Show("Không thể xóa đăng ký thi này vì đã có sinh viên của lớp thi và có điểm.", "Báo lỗi");
                            }
                            else
                            {
                                MessageBox.Show("Xóa đăng ký thi thành công!", "Thông báo");
                                LoadDangKy(); // Xóa xong thì tải lại lưới.
                                SetNormalState();
                            }
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
