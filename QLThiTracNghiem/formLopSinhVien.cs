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
    public partial class formLopSinhVien : Form
    {
        bool isAdding = false; // Cờ kiểm tra đang Thêm hay Sửa
        public formLopSinhVien()
        {
            InitializeComponent();
        }
        // 1. Khi Form vừa mở lên -> Tải danh sách Lớp vào ComboBox
        private void formLopSinhVien_Load(object sender, EventArgs e)
        {
            try
            {
                DataTable dtLop = DBHelper.GetDataTable("EXEC SP_GET_LOP");
                cmbLop.DataSource = dtLop;
                cmbLop.DisplayMember = "TENLOP"; // Chữ hiện lên cho người dùng xem
                cmbLop.ValueMember = "MALOP";    // Mã ẩn bên dưới để hệ thống dùng

                // Vừa load form thì khóa nút Ghi lại
                btnGhi.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách Lớp: " + ex.Message);
            }
        }

        private void cmbLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Kiểm tra xem ComboBox đã có giá trị ValueMember chưa
            if (cmbLop.SelectedValue != null && cmbLop.SelectedValue is string)
            {
                string maLopDuocChon = cmbLop.SelectedValue.ToString();
                LoadSinhVien(maLopDuocChon);
            }
        }
        // Hàm hỗ trợ tải sinh viên
        private void LoadSinhVien(string maLop)
        {
            try
            {
                // Gọi SP truyền kèm tham số Mã Lớp
                string sql = $"EXEC SP_GET_SINHVIEN_THEO_LOP @MALOP = '{maLop}'";
                DataTable dtSV = DBHelper.GetDataTable(sql);
                dgvSinhVien.DataSource = dtSV;

                if (dgvSinhVien.Columns.Count > 0)
                {
                    dgvSinhVien.Columns["MASV"].HeaderText = "Mã SV";
                    dgvSinhVien.Columns["HO"].HeaderText = "Họ";
                    dgvSinhVien.Columns["TEN"].HeaderText = "Tên";
                    dgvSinhVien.Columns["NGAYSINH"].HeaderText = "Ngày Sinh";
                    dgvSinhVien.Columns["DIACHI"].HeaderText = "Địa Chỉ";
                    dgvSinhVien.Columns["DIACHI"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvSinhVien.Columns["NGAYSINH"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách Sinh Viên: " + ex.Message);
            }
        }

        private void dgvSinhVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSinhVien.Rows[e.RowIndex];
                txtMaSV.Text = row.Cells["MASV"].Value.ToString();
                txtHo.Text = row.Cells["HO"].Value.ToString();
                txtTen.Text = row.Cells["TEN"].Value.ToString();
                txtDiaChi.Text = row.Cells["DIACHI"].Value.ToString();

                // Xử lý Ngày Sinh (vì có thể có sinh viên chưa nhập ngày sinh trong SQL)
                if (row.Cells["NGAYSINH"].Value != DBNull.Value && row.Cells["NGAYSINH"].Value != null)
                {
                    dtpNgaySinh.Value = Convert.ToDateTime(row.Cells["NGAYSINH"].Value);
                }
                else
                {
                    dtpNgaySinh.Value = DateTime.Now; // Nếu rỗng thì gán ngày hiện tại
                }
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            txtMaSV.Enabled = true;
            txtMaSV.Text = ""; txtHo.Text = ""; txtTen.Text = ""; txtDiaChi.Text = "";
            dtpNgaySinh.Value = DateTime.Now;
            txtMaSV.Focus();

            // Bật tắt các nút
            btnGhi.Enabled = true; btnPhucHoi.Enabled = true;
            btnThem.Enabled = false; btnSua.Enabled = false; btnXoa.Enabled = false;
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaSV.Text == "") return;

            if (MessageBox.Show($"Bạn có chắc muốn xóa sinh viên {txtHo.Text} {txtTen.Text}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = DBHelper.GetConnection())
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SP_XOA_SINHVIEN", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MASV", txtMaSV.Text.Trim());

                            SqlParameter returnValue = new SqlParameter();
                            returnValue.Direction = ParameterDirection.ReturnValue;
                            cmd.Parameters.Add(returnValue);

                            cmd.ExecuteNonQuery();

                            if ((int)returnValue.Value == 1)
                                MessageBox.Show("Không thể xóa vì sinh viên này đã có điểm thi!", "Báo lỗi");
                            else
                            {
                                MessageBox.Show("Xóa thành công!", "Thông báo");
                                LoadSinhVien(cmbLop.SelectedValue.ToString());
                            }
                        }
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
            if (txtMaSV.Text == "")
            {
                MessageBox.Show("Vui lòng chọn Sinh viên cần sửa trên lưới!", "Thông báo");
                return;
            }
            isAdding = false;
            txtMaSV.Enabled = false; // Khóa Mã SV không cho sửa
            txtHo.Focus();

            btnGhi.Enabled = true; btnPhucHoi.Enabled = true;
            btnThem.Enabled = false; btnSua.Enabled = false; btnXoa.Enabled = false;
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (txtMaSV.Text.Trim() == "" || txtTen.Text.Trim() == "")
            {
                MessageBox.Show("Mã Sinh Viên và Tên không được để trống!", "Báo lỗi");
                return;
            }

            try
            {
                using (SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (isAdding) cmd.CommandText = "SP_THEM_SINHVIEN";
                        else cmd.CommandText = "SP_SUA_SINHVIEN";

                        cmd.Parameters.AddWithValue("@MASV", txtMaSV.Text.Trim());
                        cmd.Parameters.AddWithValue("@HO", txtHo.Text.Trim());
                        cmd.Parameters.AddWithValue("@TEN", txtTen.Text.Trim());
                        cmd.Parameters.AddWithValue("@NGAYSINH", dtpNgaySinh.Value);
                        cmd.Parameters.AddWithValue("@DIACHI", txtDiaChi.Text.Trim());
                        cmd.Parameters.AddWithValue("@MALOP", cmbLop.SelectedValue.ToString());

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        if ((int)returnValue.Value == 1)
                        {
                            MessageBox.Show("Lỗi: Mã Sinh Viên này đã tồn tại!", "Báo lỗi");
                        }
                        else
                        {
                            MessageBox.Show("Cập nhật dữ liệu thành công!", "Thông báo");
                            LoadSinhVien(cmbLop.SelectedValue.ToString()); // Tải lại lưới
                            btnPhucHoi.PerformClick(); // Tự động gọi nút Phục hồi để reset trạng thái nút
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            txtMaSV.Enabled = false;

            // Tắt Ghi, bật lại Thêm/Xóa/Sửa
            btnGhi.Enabled = false; btnPhucHoi.Enabled = false;
            btnThem.Enabled = true; btnSua.Enabled = true; btnXoa.Enabled = true;

            // Tải lại lưới để xóa các chữ đang gõ dở
            LoadSinhVien(cmbLop.SelectedValue.ToString());
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
