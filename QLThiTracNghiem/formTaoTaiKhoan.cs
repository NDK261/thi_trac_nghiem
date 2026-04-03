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
    public partial class formTaoTaiKhoan : Form
    {
        public formTaoTaiKhoan()
        {
            InitializeComponent();
        }

        private void formTaoTaiKhoan_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Đổ dữ liệu vào ComboBox Giáo viên
                System.Data.DataTable dtGV = DBHelper.GetDataTable("EXEC SP_GET_GIAOVIEN");
                cmbGiaoVien.DataSource = dtGV;
                cmbGiaoVien.DisplayMember = "HOTEN"; // Chữ hiện lên màn hình
                cmbGiaoVien.ValueMember = "MAGV";    // Cái mã ngầm ẩn bên dưới để xài

                // 2. Add cứng 2 Nhóm quyền vào ComboBox Nhóm Quyền
                cmbNhomQuyen.Items.Add("PGV");
                cmbNhomQuyen.Items.Add("GIANGVIEN");
                cmbNhomQuyen.SelectedIndex = 1; // Mặc định chọn GIANGVIEN
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách: " + ex.Message);
            }
        }
        // 3. Code sự kiện khi chọn 1 Tên Giáo viên thì Mã GV tự động nhảy vào txtMaGV
        private void cmbGiaoVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGiaoVien.SelectedValue != null && cmbGiaoVien.SelectedValue is string)
            {
                txtMaGV.Text = cmbGiaoVien.SelectedValue.ToString();
            }
        }

        private void btnTaoTaiKhoan_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra xem người dùng có bỏ trống ô nào không
            if (txtTaiKhoan.Text.Trim() == "" || txtMatKhau.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên Tài Khoản và Mật Mã!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTaiKhoan.Focus();
                return;
            }

            if (txtMaGV.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng chọn Giảng viên cần cấp tài khoản!", "Báo lỗi");
                return;
            }

            // Lấy các giá trị từ Form
            string loginName = txtTaiKhoan.Text.Trim();
            string pass = txtMatKhau.Text.Trim();
            string userName = txtMaGV.Text.Trim(); // Mã GV
            string role = cmbNhomQuyen.SelectedItem.ToString(); // Nhóm quyền (PGV hoặc GIANGVIEN)

            // 2. Gọi Stored Procedure thực thi tạo tài khoản
            try
            {
                using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
                {
                    conn.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SP_TAOLOGIN", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Truyền tham số
                        cmd.Parameters.AddWithValue("@LGNAME", loginName);
                        cmd.Parameters.AddWithValue("@PASS", pass);
                        cmd.Parameters.AddWithValue("@USERNAME", userName);
                        cmd.Parameters.AddWithValue("@ROLE", role);

                        // Hứng giá trị trả về (Return Value) từ SQL Server
                        // SP_TAOLOGIN trả về: 1 (Trùng Login), 2 (Trùng User/Đã có TK), 0 (Thành công)
                        System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                        returnValue.Direction = System.Data.ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = (int)returnValue.Value;

                        if (result == 1)
                        {
                            MessageBox.Show("Tên đăng nhập (Login) này đã có người sử dụng. Vui lòng chọn tên khác!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtTaiKhoan.Focus();
                        }
                        else if (result == 2)
                        {
                            MessageBox.Show("Giảng viên này đã được cấp tài khoản rồi! Mỗi người chỉ được 1 tài khoản.", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else // result == 0
                        {
                            MessageBox.Show("Tạo tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Reset lại ô nhập liệu cho lần tạo tiếp theo
                            txtTaiKhoan.Text = "";
                            txtMatKhau.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi trong quá trình tạo tài khoản:\n" + ex.Message, "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

