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
    public partial class formMain : Form
    {
        public formMain()
        {
            InitializeComponent();
        }

        private void dangXuatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formDangNhap f = new formDangNhap();
            f.Show();
            this.Close();
        }

        private void thoatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
