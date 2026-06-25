using System;
using System.IO;

class Program {
    static void Main() {
        string pathLop = @"QLThiTracNghiem\formLop.cs";
        string textLop = File.ReadAllText(pathLop, System.Text.Encoding.UTF8);
        textLop = textLop.Replace("        private void SetupSubformUI() {\r\n            SetupSubformUI();", "        private void SetupSubformUI() {");
        textLop = textLop.Replace("        private void SetupSubformUI() {\n            SetupSubformUI();", "        private void SetupSubformUI() {");
        textLop = textLop.Replace("private void formLop_Load(object sender, EventArgs e)\r\n        {\r\n            LoadData();", "private void formLop_Load(object sender, EventArgs e)\r\n        {\r\n            SetupSubformUI();\r\n            LoadData();");
        textLop = textLop.Replace("private void formLop_Load(object sender, EventArgs e)\n        {\n            LoadData();", "private void formLop_Load(object sender, EventArgs e)\n        {\n            SetupSubformUI();\n            LoadData();");
        File.WriteAllText(pathLop, textLop, System.Text.Encoding.UTF8);

        string pathBD = @"QLThiTracNghiem\formBangDiem.cs";
        string textBD = File.ReadAllText(pathBD, System.Text.Encoding.UTF8);
        textBD = textBD.Replace("        private void SetupSubformUI() {\r\n            SetupSubformUI();", "        private void SetupSubformUI() {");
        textBD = textBD.Replace("        private void SetupSubformUI() {\n            SetupSubformUI();", "        private void SetupSubformUI() {");
        textBD = textBD.Replace("private void formBangDiem_Load(object sender, EventArgs e)\r\n        {\r\n            CaiDatGrid();", "private void formBangDiem_Load(object sender, EventArgs e)\r\n        {\r\n            SetupSubformUI();\r\n            CaiDatGrid();");
        textBD = textBD.Replace("private void formBangDiem_Load(object sender, EventArgs e)\n        {\n            CaiDatGrid();", "private void formBangDiem_Load(object sender, EventArgs e)\n        {\n            SetupSubformUI();\n            CaiDatGrid();");
        File.WriteAllText(pathBD, textBD, System.Text.Encoding.UTF8);
    }
}
