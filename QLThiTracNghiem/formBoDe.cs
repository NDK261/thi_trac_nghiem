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
    public partial class formBoDe : Form
    {
        bool isAdding = false;
        public formBoDe()
        {
            InitializeComponent();
        }

        private void formBoDe_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Đổ dữ liệu vào ComboBox Môn Học
                DataTable dtMonHoc = DBHelper.GetDataTable("EXEC SP_GET_MONHOC");
                cmbMonHoc.DataSource = dtMonHoc;
                cmbMonHoc.DisplayMember = "TENMH";
                cmbMonHoc.ValueMember = "MAMH";

                // 2. Thêm dữ liệu cứng cho Trình Độ (A, B, C)
                cmbTrinhDo.Items.AddRange(new string[] { "A", "B", "C" });
                cmbTrinhDo.SelectedIndex = 0;

                // 3. Thêm dữ liệu cứng cho Đáp Án Đúng (A, B, C, D)
                cmbDapAn.Items.AddRange(new string[] { "A", "B", "C", "D" });
                cmbDapAn.SelectedIndex = 0;

                // 4. Tự động điền Mã Giảng Viên đang đăng nhập vào textbox
                txtMaGV.Text = Program.mUserName;
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
                string sql = $"EXEC SP_GET_BODE_THEO_MONHOC @MAMH = '{maMH}'";
                DataTable dtBoDe = DBHelper.GetDataTable(sql);
                dgvBoDe.DataSource = dtBoDe;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải bộ đề: " + ex.Message);
            }
        }

        private void dgvBoDe_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvBoDe.Rows[e.RowIndex];
                txtCauHoi.Text = row.Cells["CAUHOI"].Value.ToString();
                cmbTrinhDo.Text = row.Cells["TRINHDO"].Value.ToString();
                txtNoiDung.Text = row.Cells["NOIDUNG"].Value.ToString();
                txtA.Text = row.Cells["A"].Value.ToString();
                txtB.Text = row.Cells["B"].Value.ToString();
                txtC.Text = row.Cells["C"].Value.ToString();
                txtD.Text = row.Cells["D"].Value.ToString();
                cmbDapAn.Text = row.Cells["DAP_AN"].Value.ToString();
                txtMaGV.Text = row.Cells["MAGV"].Value.ToString();
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            txtCauHoi.Enabled = true;
            txtCauHoi.Text = ""; txtNoiDung.Text = "";
            txtA.Text = ""; txtB.Text = ""; txtC.Text = ""; txtD.Text = "";

            // Ép Mã GV về người đang đăng nhập hiện tại
            txtMaGV.Text = Program.mUserName;

            txtCauHoi.Focus();

            btnGhi.Enabled = true; btnPhucHoi.Enabled = true;
            btnThem.Enabled = false; btnSua.Enabled = false; btnXoa.Enabled = false;
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtCauHoi.Text == "") return;

            if (txtMaGV.Text.Trim() != Program.mUserName.Trim())
            {
                MessageBox.Show("Bạn không có quyền xóa câu hỏi của người khác!", "Cảnh báo");
                return;
            }

            if (MessageBox.Show("Bạn có chắc muốn xóa câu hỏi này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (System.Data.SqlClient.SqlConnection conn = DBHelper.GetConnection())
                    {
                        conn.Open();
                        using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SP_XOA_BODE", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CAUHOI", int.Parse(txtCauHoi.Text.Trim()));
                            cmd.Parameters.AddWithValue("@MAGV", Program.mUserName);

                            System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                            returnValue.Direction = ParameterDirection.ReturnValue;
                            cmd.Parameters.Add(returnValue);

                            cmd.ExecuteNonQuery();

                            int result = (int)returnValue.Value;
                            if (result == 1) MessageBox.Show("Không thể xóa câu hỏi đã được thi!", "Báo lỗi");
                            else if (result == 2) MessageBox.Show("Bạn không có quyền xóa câu hỏi này!", "Báo lỗi");
                            else
                            {
                                MessageBox.Show("Xóa thành công!", "Thông báo");
                                LoadBoDe(cmbMonHoc.SelectedValue.ToString());
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
            if (txtCauHoi.Text == "") return;

            // Kiểm tra quyền Sửa ở cấp giao diện trước
            if (txtMaGV.Text.Trim() != Program.mUserName.Trim())
            {
                MessageBox.Show("Bạn không có quyền sửa câu hỏi của người khác!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isAdding = false;
            txtCauHoi.Enabled = false; // Không cho sửa Mã câu hỏi
            txtNoiDung.Focus();

            btnGhi.Enabled = true; btnPhucHoi.Enabled = true;
            btnThem.Enabled = false; btnSua.Enabled = false; btnXoa.Enabled = false;
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (txtCauHoi.Text == "" || txtNoiDung.Text == "" || txtA.Text == "" || txtB.Text == "" || txtC.Text == "" || txtD.Text == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ nội dung câu hỏi và các đáp án!", "Báo lỗi");
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

                        cmd.CommandText = isAdding ? "SP_THEM_BODE" : "SP_SUA_BODE";

                        cmd.Parameters.AddWithValue("@CAUHOI", int.Parse(txtCauHoi.Text.Trim()));
                        cmd.Parameters.AddWithValue("@MAMH", cmbMonHoc.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@TRINHDO", cmbTrinhDo.Text);
                        cmd.Parameters.AddWithValue("@NOIDUNG", txtNoiDung.Text.Trim());
                        cmd.Parameters.AddWithValue("@A", txtA.Text.Trim());
                        cmd.Parameters.AddWithValue("@B", txtB.Text.Trim());
                        cmd.Parameters.AddWithValue("@C", txtC.Text.Trim());
                        cmd.Parameters.AddWithValue("@D", txtD.Text.Trim());
                        cmd.Parameters.AddWithValue("@DAP_AN", cmbDapAn.Text);
                        cmd.Parameters.AddWithValue("@MAGV", Program.mUserName); // Luôn truyền mã GV hiện tại

                        System.Data.SqlClient.SqlParameter returnValue = new System.Data.SqlClient.SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = (int)returnValue.Value;
                        if (result == 1) MessageBox.Show("Mã câu hỏi đã tồn tại!", "Báo lỗi");
                        else if (result == 2) MessageBox.Show("Bạn không có quyền sửa câu hỏi này!", "Báo lỗi");
                        else
                        {
                            MessageBox.Show("Lưu thành công!", "Thông báo");
                            LoadBoDe(cmbMonHoc.SelectedValue.ToString());
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

        private void btnPhucHoi_Click(object sender, EventArgs e)
        {
            txtCauHoi.Enabled = false;
            btnGhi.Enabled = false; btnPhucHoi.Enabled = false;
            btnThem.Enabled = true; btnSua.Enabled = true; btnXoa.Enabled = true;

            if (cmbMonHoc.SelectedValue != null)
                LoadBoDe(cmbMonHoc.SelectedValue.ToString());
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }
}
