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
    public partial class formMonHoc : Form
    {
        bool isAdding = false; // Biến kiểm tra xem đang Thêm hay Sửa
        public formMonHoc()
        {
            InitializeComponent();
        }

        // Hàm dùng để gọi dữ liệu từ SQL và đổ vào Lưới
        private void LoadData()
        {
            try
            {
                // Dùng SP_GET_MONHOC đã viết dưới SQL Server
                string sql = "EXEC SP_GET_MONHOC";
                DataTable dt = DBHelper.GetDataTable(sql);

                dgvMonHoc.DataSource = dt;

                // Chỉnh lại tiêu đề cột cho đẹp
                dgvMonHoc.Columns["MAMH"].HeaderText = "Mã Môn Học";
                dgvMonHoc.Columns["TENMH"].HeaderText = "Tên Môn Học";
                dgvMonHoc.Columns["TENMH"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Cho cột tên môn dãn đầy lưới
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // Sự kiện chạy khi form vừa mở lên
        private void formMonHoc_Load_1(object sender, EventArgs e)
        {
            LoadData();
            btnGhi.Enabled = false;
        }

        private void dgvMonHoc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kiểm tra xem có bấm đúng vào dòng có dữ liệu không
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvMonHoc.Rows[e.RowIndex];
                txtMaMH.Text = row.Cells["MAMH"].Value.ToString();
                txtTenMH.Text = row.Cells["TENMH"].Value.ToString();
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            txtMaMH.Text = "";
            txtTenMH.Text = "";
            txtMaMH.Enabled = true; // Mở khóa cho nhập Mã
            txtMaMH.Focus();

            // Bật nút Ghi, tắt các nút khác
            btnGhi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaMH.Text == "") return;

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa môn học này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
                    {
                        conn.Open();
                        using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SP_XOA_MONHOC", conn))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MAMH", txtMaMH.Text.Trim());

                            System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                            returnValue.Direction = System.Data.ParameterDirection.ReturnValue;
                            cmd.Parameters.Add(returnValue);

                            cmd.ExecuteNonQuery();

                            if ((int)returnValue.Value == 1)
                                MessageBox.Show("Không thể xóa môn học này vì đã có đề thi/bảng điểm!", "Báo lỗi");
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
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (txtMaMH.Text == "")
            {
                MessageBox.Show("Vui lòng chọn môn học cần sửa từ danh sách!", "Thông báo");
                return;
            }
            isAdding = false;
            txtMaMH.Enabled = false; // Mã môn học không được sửa
            txtTenMH.Focus();

            btnGhi.Enabled = true;
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (txtMaMH.Text.Trim() == "" || txtTenMH.Text.Trim() == "")
            {
                MessageBox.Show("Mã và Tên môn học không được để trống!", "Báo lỗi");
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
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Gọi Stored Procedure tương ứng
                        if (isAdding)
                            cmd.CommandText = "SP_THEM_MONHOC";
                        else
                            cmd.CommandText = "SP_SUA_MONHOC";

                        cmd.Parameters.AddWithValue("@MAMH", txtMaMH.Text.Trim());
                        cmd.Parameters.AddWithValue("@TENMH", txtTenMH.Text.Trim());

                        // Thực thi SP và nhận mã lỗi trả về (Return Value)
                        System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                        returnValue.Direction = System.Data.ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();
                        int result = (int)returnValue.Value;

                        if (result == 1) MessageBox.Show("Lỗi: Trùng Mã Môn Học!", "Báo lỗi");
                        else if (result == 2) MessageBox.Show("Lỗi: Tên Môn Học đã tồn tại!", "Báo lỗi");
                        else
                        {
                            MessageBox.Show("Cập nhật thành công!", "Thông báo");
                            LoadData(); // Tải lại lưới

                            // Reset lại trạng thái nút
                            btnGhi.Enabled = false;
                            btnThem.Enabled = true;
                            btnSua.Enabled = true;
                            btnXoa.Enabled = true;
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
    }
}
