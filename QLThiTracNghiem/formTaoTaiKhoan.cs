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
            if (cmbNhomQuyen.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn nhóm quyền cho tài khoản!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbNhomQuyen.Focus();
                return;
            }

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

                        // Hứng giá trị trả về (Return Value) từ SQL Server.
                        // Chỉ result == 0 mới là tạo thành công, các mã khác đều là lỗi.
                        System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                        returnValue.Direction = System.Data.ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = (int)returnValue.Value;

                        switch (result)
                        {
                            case 0:
                                MessageBox.Show("Tạo tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Reset lại ô nhập liệu cho lần tạo tiếp theo
                                txtTaiKhoan.Text = "";
                                txtMatKhau.Text = "";
                                break;

                            case 1:
                                MessageBox.Show("Tên đăng nhập không được để trống!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtTaiKhoan.Focus();
                                break;

                            case 2:
                                MessageBox.Show("Mật khẩu không được để trống!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtMatKhau.Focus();
                                break;

                            case 3:
                                MessageBox.Show("Mã giảng viên không được để trống!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                cmbGiaoVien.Focus();
                                break;

                            case 4:
                                MessageBox.Show("Nhóm quyền không hợp lệ. Chỉ được chọn PGV hoặc GIANGVIEN!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                cmbNhomQuyen.Focus();
                                break;

                            case 5:
                                MessageBox.Show("Mã giảng viên không tồn tại trong bảng GIAOVIEN!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                cmbGiaoVien.Focus();
                                break;

                            case 6:
                                MessageBox.Show("Tên đăng nhập (Login) này đã có người sử dụng. Vui lòng chọn tên khác!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtTaiKhoan.Focus();
                                break;

                            case 7:
                                MessageBox.Show("Giảng viên này đã được cấp tài khoản rồi! Mỗi người chỉ được 1 tài khoản.", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                cmbGiaoVien.Focus();
                                break;

                            case 8:
                                MessageBox.Show("Nhóm quyền PGV/GIANGVIEN chưa tồn tại trong database. Hãy chạy lại script phân quyền trước!", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;

                            case 99:
                                MessageBox.Show("SQL Server không tạo được Login/User. Thường là do tài khoản PGV chưa đủ quyền tạo Login hoặc gán Role.", "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;

                            default:
                                MessageBox.Show("Tạo tài khoản thất bại. Mã lỗi từ SQL Server: " + result, "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
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

