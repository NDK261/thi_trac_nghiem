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

        private void cmbLop_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbTrinhDo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (txtSoCauThi.Text == "") return;
            isAdding = false;

            // Khóa các ô làm Khóa Chính (Lớp, Môn, Lần) không cho sửa
            cmbLop.Enabled = false; cmbMonHoc.Enabled = false; cmbLanThi.Enabled = false;

            btnGhi.Enabled = true; btnPhucHoi.Enabled = true;
            btnThem.Enabled = false; btnSua.Enabled = false; btnXoa.Enabled = false;
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
        }

        private void dgvDangKy_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDangKy.Rows[e.RowIndex];
                cmbLop.SelectedValue = row.Cells["MALOP"].Value.ToString();
                cmbMonHoc.SelectedValue = row.Cells["MAMH"].Value.ToString();
                cmbTrinhDo.Text = row.Cells["TRINHDO"].Value.ToString();
                cmbLanThi.Text = row.Cells["LAN"].Value.ToString();
                txtSoCauThi.Text = row.Cells["SOCAUTHI"].Value.ToString();
                txtThoiGian.Text = row.Cells["THOIGIAN"].Value.ToString();
                if (row.Cells["NGAYTHI"].Value != DBNull.Value)
                    dtpNgayThi.Value = Convert.ToDateTime(row.Cells["NGAYTHI"].Value);
                dgvDangKy.Columns["NGAYTHI"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            txtSoCauThi.Text = ""; txtThoiGian.Text = "";
            cmbLop.Enabled = true; cmbMonHoc.Enabled = true; cmbLanThi.Enabled = true; // Cho phép chọn mới
            txtSoCauThi.Focus();

            btnGhi.Enabled = true; btnPhucHoi.Enabled = true;
            btnThem.Enabled = false; btnSua.Enabled = false; btnXoa.Enabled = false;
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            cmbLop.Enabled = true; cmbMonHoc.Enabled = true; cmbLanThi.Enabled = true;
            btnGhi.Enabled = false; btnPhucHoi.Enabled = false;
            btnThem.Enabled = true; btnSua.Enabled = true; btnXoa.Enabled = true;
            LoadDangKy();
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (txtSoCauThi.Text == "" || txtThoiGian.Text == "")
            {
                MessageBox.Show("Vui lòng nhập Số câu thi và Thời gian!", "Báo lỗi");
                return;
            }

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
                        cmd.Parameters.AddWithValue("@SOCAUTHI", int.Parse(txtSoCauThi.Text));
                        cmd.Parameters.AddWithValue("@THOIGIAN", int.Parse(txtThoiGian.Text));

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
                            btnPhucHoi.PerformClick();
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
