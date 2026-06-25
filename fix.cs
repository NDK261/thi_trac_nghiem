using System;
using System.IO;
class Program {
    static void Main() {
        string path = @"QLThiTracNghiem\formDangNhap.cs";
        string text = File.ReadAllText(path, System.Text.Encoding.UTF8);
        
        // 1. Password hardcoding removal for btnDangNhap
        text = text.Replace("Program.BuildConnectionString(\"sv\", \"123\")", "Program.BuildConnectionString(\"sv\", pass)");
        text = text.Replace("Program.mLogin = \"sv\";", "Program.mLogin = \"sv\";\n                string pass = txtMatKhau.Text.Trim();");
        
        // 2. Remove the empty password check inside rdoGiangVien block so we can put it outside
        string oldCheck = "                if (txtMatKhau.Text.Trim() == \"\")\r\n                {\r\n                    MessageBox.Show(\"Vui lAng nh-p m-t khcu!\", \"ThA'ng bAo\", MessageBoxButtons.OK, MessageBoxIcon.Warning);\r\n                    txtMatKhau.Focus();\r\n                    return;\r\n                }";
        text = text.Replace(oldCheck, "");
        
        // Let's use simpler replacement if that encoding fails
        int idx = text.IndexOf("if (txtMatKhau.Text.Trim() == \"\")");
        if(idx != -1) {
            int endIdx = text.IndexOf("}", idx);
            if(endIdx != -1) {
                text = text.Remove(idx, endIdx - idx + 1);
            }
        }

        string newCheck = @"            if (txtMatKhau.Text.Trim() == """")
            {
                MessageBox.Show(""Vui lòng nhập mật khẩu!"", ""Thông báo"", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhau.Focus();
                return;
            }

            // 1.";
            
        text = text.Replace("// 1.", newCheck);

        // 3. Fix rdoSinhVien_CheckedChanged
        int svIdx = text.IndexOf("private void rdoSinhVien_CheckedChanged");
        int svEndIdx = text.IndexOf("}", text.IndexOf("{", svIdx)) + 1;
        string svFunc = @"        private void rdoSinhVien_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSinhVien.Checked)
            {
                labelTenDangNhap.Text = ""Mã SV"";
                txtMatKhau.Clear();
                txtMatKhau.ReadOnly = false;
                txtMatKhau.Enabled = true;
            }
        }";
        text = text.Remove(svIdx, svEndIdx - svIdx);
        text = text.Insert(svIdx, svFunc);

        // 4. Fix rdoGiangVien_CheckedChanged
        int gvIdx = text.IndexOf("private void rdoGiangVien_CheckedChanged");
        int gvEndIdx = text.IndexOf("}", text.IndexOf("{", gvIdx)) + 1;
        string gvFunc = @"        private void rdoGiangVien_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoGiangVien.Checked)
            {
                labelTenDangNhap.Text = ""Tên Đăng Nhập"";
                txtMatKhau.Clear();
                txtMatKhau.ReadOnly = false;
                txtMatKhau.Enabled = true;
            }
        }";
        text = text.Remove(gvIdx, gvEndIdx - gvIdx);
        text = text.Insert(gvIdx, gvFunc);

        File.WriteAllText(path, text, System.Text.Encoding.UTF8);
    }
}
