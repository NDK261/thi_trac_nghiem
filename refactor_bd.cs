using System;
using System.IO;
using System.Text.RegularExpressions;

class Program {
    static void Main() {
        // 3. For formBangDiem.cs
        string pathBangDiem = @"QLThiTracNghiem\formBangDiem.cs";
        string textBD = File.ReadAllText(pathBangDiem, System.Text.Encoding.UTF8);

        string uiSetupBD = @"
        private Panel pnlKetQua;
        private formXemKetQua currentSubFormBD;
        private void SetupSubformUI() {
            this.Height = 700;
            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Horizontal;
            split.SplitterDistance = 350;
            
            Control[] controls = new Control[this.Controls.Count];
            this.Controls.CopyTo(controls, 0);
            foreach(Control c in controls) {
                split.Panel1.Controls.Add(c);
            }
            this.Controls.Add(split);
            
            pnlKetQua = new Panel();
            pnlKetQua.Dock = DockStyle.Fill;
            split.Panel2.Controls.Add(pnlKetQua);
            
            btnXemBaiThi.Visible = false;
        }

        private void LoadSubForm(string masv, string mamh, short lan) {
            if (currentSubFormBD != null) {
                currentSubFormBD.Close();
                currentSubFormBD.Dispose();
            }
            
            currentSubFormBD = new formXemKetQua(masv, mamh, lan);
            currentSubFormBD.TopLevel = false;
            currentSubFormBD.FormBorderStyle = FormBorderStyle.None;
            currentSubFormBD.Dock = DockStyle.Fill;
            pnlKetQua.Controls.Add(currentSubFormBD);
            currentSubFormBD.Show();
        }
";
        int loadBDIdx = textBD.IndexOf("private void formBangDiem_Load");
        if(loadBDIdx != -1) {
            textBD = textBD.Insert(loadBDIdx, uiSetupBD);
            int loadStart = textBD.IndexOf("{", loadBDIdx) + 1;
            textBD = textBD.Insert(loadStart, "\n            SetupSubformUI();");
        }

        // Add dgvBangDiem_SelectionChanged
        string selectionEvent = @"
        private void dgvBangDiem_SelectionChanged(object sender, EventArgs e) {
            if (dgvBangDiem.CurrentRow != null && cmbMonHoc.SelectedValue != null) {
                if (!dgvBangDiem.Columns.Contains(""MASV"")) return;
                string masv = dgvBangDiem.CurrentRow.Cells[""MASV""].Value?.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(masv) && TryGetLanThi(out int lan)) {
                    LoadSubForm(masv, cmbMonHoc.SelectedValue.ToString(), (short)lan);
                }
            }
        }";
        int formClassEnd = textBD.LastIndexOf("}");
        formClassEnd = textBD.LastIndexOf("}", formClassEnd - 1); // before last closing brace
        textBD = textBD.Insert(formClassEnd, selectionEvent + "\n");

        // also hook it up in formBangDiem constructor
        int ctorBDIdx = textBD.IndexOf("public formBangDiem()");
        int ctorBDEnd = textBD.IndexOf("}", textBD.IndexOf("{", ctorBDIdx));
        textBD = textBD.Insert(ctorBDEnd, "\n            dgvBangDiem.SelectionChanged += dgvBangDiem_SelectionChanged;\n        ");

        File.WriteAllText(pathBangDiem, textBD, System.Text.Encoding.UTF8);
    }
}
