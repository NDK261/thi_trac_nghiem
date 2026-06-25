using System;
using System.IO;
using System.Text.RegularExpressions;
class Program {
    static void Main() {
        string path = @"QLThiTracNghiem\formDangNhap.cs";
        string text = File.ReadAllText(path, System.Text.Encoding.UTF8);
        
        // 1. Password hardcoding removal for btnDangNhap
        text = text.Replace("Program.BuildConnectionString(\"sv\", \"123\")", "Program.BuildConnectionString(\"sv\", pass)");
        text = text.Replace("Program.mLogin = \"sv\";", "Program.mLogin = \"sv\";\n                string pass = txtMatKhau.Text.Trim();");
        
        // 2. Remove the empty password check inside rdoGiangVien block so we can put it outside
        text = Regex.Replace(text, @"// GI[^\n]*\n\s*if\s*\(txtMatKhau\.Text\.Trim\(\)\s*==\s*\""\""\)\s*\{\s*MessageBox\.Show[^\}]*\}\s*", "");

        string newCheck = @"            if (txtMatKhau.Text.Trim() == """")
            {
                MessageBox.Show(""Vui lòng nhập mật khẩu!"", ""Thông báo"", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhau.Focus();
                return;
            }

            // 1.";
            
        text = text.Replace("// 1.", newCheck);

        // 3. Fix rdoSinhVien_CheckedChanged
        text = Regex.Replace(text, @"private void rdoSinhVien_CheckedChanged.*?\{.*?labelTenDangNhap\.Text =.*?txtMatKhau\.Text =.*?txtMatKhau\.ReadOnly =.*?txtMatKhau\.Enabled =.*?\}", 
@"private void rdoSinhVien_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSinhVien.Checked)
            {
                labelTenDangNhap.Text = ""Mã SV"";
                txtMatKhau.Clear();
                txtMatKhau.ReadOnly = false;
                txtMatKhau.Enabled = true;
            }
        }", RegexOptions.Singleline);

        // 4. Fix rdoGiangVien_CheckedChanged
        text = Regex.Replace(text, @"private void rdoGiangVien_CheckedChanged.*?\{.*?labelTenDangNhap\.Text =.*?txtMatKhau\.Clear.*?txtMatKhau\.ReadOnly =.*?txtMatKhau\.Enabled =.*?\}", 
@"private void rdoGiangVien_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoGiangVien.Checked)
            {
                labelTenDangNhap.Text = ""Tên Đăng Nhập"";
                txtMatKhau.Clear();
                txtMatKhau.ReadOnly = false;
                txtMatKhau.Enabled = true;
            }
        }", RegexOptions.Singleline);

        File.WriteAllText(path, text, System.Text.Encoding.UTF8);
    }
}
