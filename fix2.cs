using System;
using System.IO;
class Program {
    static void Main() {
        string path = @"QLThiTracNghiem\formDangNhap.cs";
        string text = File.ReadAllText(path, System.Text.Encoding.UTF8);
        text = text.Replace("        }\r\n        }", "        }");
        text = text.Replace("        }\n        }", "        }");
        File.WriteAllText(path, text, System.Text.Encoding.UTF8);
    }
}
