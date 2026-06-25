using System;
using System.IO;
using System.Text.RegularExpressions;

class Program {
    static void Main() {
        string pathLop = @"QLThiTracNghiem\formLop.cs";
        string textLop = File.ReadAllText(pathLop, System.Text.Encoding.UTF8);

        // 1. In formLop.cs, add UI setup method
        string uiSetupCode = @"
        private Panel pnlSinhVien;
        private formLopSinhVien currentSubForm;
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
            
            pnlSinhVien = new Panel();
            pnlSinhVien.Dock = DockStyle.Fill;
            split.Panel2.Controls.Add(pnlSinhVien);
            
            btnDSSinhVien.Visible = false;
        }

        private void LoadSubForm(string maLop) {
            if (currentSubForm != null) {
                currentSubForm.Close();
                currentSubForm.Dispose();
            }
            if (string.IsNullOrEmpty(maLop)) return;
            
            currentSubForm = new formLopSinhVien(maLop);
            currentSubForm.TopLevel = false;
            currentSubForm.FormBorderStyle = FormBorderStyle.None;
            currentSubForm.Dock = DockStyle.Fill;
            pnlSinhVien.Controls.Add(currentSubForm);
            currentSubForm.Show();
        }
";
        // Insert after constructor
        int loadIdx = textLop.IndexOf("private void formLop_Load");
        if(loadIdx != -1) {
            textLop = textLop.Insert(loadIdx, uiSetupCode);
            // also call SetupSubformUI in formLop_Load
            int loadStart = textLop.IndexOf("{", loadIdx) + 1;
            textLop = textLop.Insert(loadStart, "\n            SetupSubformUI();");
        }

        // 2. Load subform when selection changes
        string dgvSelectionChanged = "private void dgvLop_SelectionChanged";
        int selIdx = textLop.IndexOf(dgvSelectionChanged);
        if(selIdx != -1) {
            int blockStart = textLop.IndexOf("{", selIdx) + 1;
            int nextLine = textLop.IndexOf("\n", blockStart) + 1;
            textLop = textLop.Insert(nextLine, "\n            if (!isAdding && dgvLop.CurrentRow != null) { LoadSubForm(dgvLop.CurrentRow.Cells[\"MALOP\"].Value.ToString()); }\n");
        }

        File.WriteAllText(pathLop, textLop, System.Text.Encoding.UTF8);

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
            
            btnXemChiTiet.Visible = false;
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
            if (dgvBangDiem.CurrentRow != null) {
                string masv = dgvBangDiem.CurrentRow.Cells[""MASV""].Value.ToString();
                if(cmbMonHoc.SelectedValue != null && txtLanThi.Text != """") {
                    short lan;
                    if(short.TryParse(txtLanThi.Text, out lan)) {
                        LoadSubForm(masv, cmbMonHoc.SelectedValue.ToString(), lan);
                    }
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
