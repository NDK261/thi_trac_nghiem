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
            if (string.IsNullOrWhiteSpace(Program.connStr))
            {
                Program.connStr = Program.GetDefaultConnectionString();
            }

            Text = $"Test kết nối - {Program.serverName} / {Program.dbName}";
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
                dgvData.DataSource = DBHelper.GetDataTable("SELECT MAMH, TENMH FROM dbo.MONHOC ORDER BY MAMH");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi");
            }
        }
    }
}
