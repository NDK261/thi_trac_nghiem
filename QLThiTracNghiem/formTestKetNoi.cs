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
    public partial class formTestKetNoi : Form
    {
        public formTestKetNoi()
        {
            InitializeComponent();
        }


        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            string message;
            bool ok = DBHelper.TestConnection(out message);
            MessageBox.Show(message, "Thông báo");
        }

        private void btnLoadMonHoc_Click(object sender, EventArgs e)
        {
            try
            {
                dgvData.DataSource = DBHelper.GetDataTable("SELECT * FROM MONHOC");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi");
            }
        }
    }
}
