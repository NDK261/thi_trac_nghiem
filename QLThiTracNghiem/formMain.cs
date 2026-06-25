using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace QLThiTracNghiem
{
    public partial class formMain : Form
    {
        private Panel dashboardPanel;
        private TableLayoutPanel dashboardLayout;
        private HeroPanel heroPanel;
        private PictureBox logoPictureBox;
        private Label lblHeroTitle;
        private Label lblHeroSubtitle;
        private Label lblHeroUser;
        private Label lblHeroRole;
        private FlowLayoutPanel infoPanel;
        private FlowLayoutPanel shortcutPanel;
        private Label lblShortcutTitle;
        private Image logoImage;
        private Image campusImage;
        private readonly List<ShortcutItem> shortcutItems = new List<ShortcutItem>();

        public formMain()
        {
            InitializeComponent();
            BuildDashboard();
        }

        private void dangXuatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất khỏi tài khoản này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                Application.Restart();
            }
        }

        private void thoatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            string nhom = (Program.mGroup ?? "").Trim().ToUpper();

            // Menu cha vẫn hiện hết
            menuHeThong.Visible = true;
            menuDanhMuc.Visible = true;
            menuNghiepVu.Visible = true;
            menuBaoCao.Visible = true;

            // Menu cha vẫn bấm mở xuống được
            menuHeThong.Enabled = true;
            menuDanhMuc.Enabled = true;
            menuNghiepVu.Enabled = true;
            menuBaoCao.Enabled = true;

            // Khóa hết menu con trước
            tạoTàiKhoảnToolStripMenuItem.Enabled = false;

            mônHọcToolStripMenuItem.Enabled = false;
            giáoViênToolStripMenuItem.Enabled = false;
            lớpToolStripMenuItem.Enabled = false;
            sinhViênToolStripMenuItem.Enabled = false;

            bộĐềToolStripMenuItem.Enabled = false;
            đăngKíThiToolStripMenuItem.Enabled = false;
            thiToolStripMenuItem.Enabled = false;

            xemKetQuaToolStripMenuItem.Enabled = false;
            bảngĐiểmToolStripMenuItem.Enabled = false;

            if (nhom == "PGV")
            {
                tạoTàiKhoảnToolStripMenuItem.Enabled = true;

                mônHọcToolStripMenuItem.Enabled = true;
                giáoViênToolStripMenuItem.Enabled = true;
                lớpToolStripMenuItem.Enabled = true;
                sinhViênToolStripMenuItem.Enabled = true;

                bộĐềToolStripMenuItem.Enabled = true;
                đăngKíThiToolStripMenuItem.Enabled = true;

                xemKetQuaToolStripMenuItem.Enabled = true;
                bảngĐiểmToolStripMenuItem.Enabled = true;
            }
            else if (nhom == "GIANGVIEN")
            {
                bộĐềToolStripMenuItem.Enabled = true;
                đăngKíThiToolStripMenuItem.Enabled = true;
                xemKetQuaToolStripMenuItem.Enabled = true;
                bảngĐiểmToolStripMenuItem.Enabled = true;
                thiToolStripMenuItem.Enabled = true;
            }
            else if (nhom == "SINHVIEN")
            {
                thiToolStripMenuItem.Enabled = true;
                xemKetQuaToolStripMenuItem.Enabled = true;
            }

            this.Text = $"HỆ THỐNG THI TRẮC NGHIỆM - Đang đăng nhập: {Program.mHoTen} ({nhom})";
            UpdateDashboard();
        }

        private void BuildDashboard()
        {
            MinimumSize = new Size(1040, 680);
            BackColor = Color.FromArgb(248, 249, 251);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            StyleMenu();
            logoImage = LoadAssetImage("ptit-logo.png");
            campusImage = LoadAssetImage("ptit-campus.png");

            dashboardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 251),
                Padding = new Padding(26, 22, 26, 22)
            };

            dashboardLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 5
            };
            dashboardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 255F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            BuildHero();
            BuildInfoPanel();
            BuildShortcutPanel();

            dashboardLayout.Controls.Add(heroPanel, 0, 0);
            dashboardLayout.Controls.Add(infoPanel, 0, 2);
            dashboardLayout.Controls.Add(lblShortcutTitle, 0, 3);
            dashboardLayout.Controls.Add(shortcutPanel, 0, 4);

            dashboardPanel.Controls.Add(dashboardLayout);
            Controls.Add(dashboardPanel);
            menuStrip1.BringToFront();
        }

        private void StyleMenu()
        {
            menuStrip1.BackColor = Color.White;
            menuStrip1.ForeColor = Color.FromArgb(55, 55, 55);
            menuStrip1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            menuStrip1.Padding = new Padding(18, 6, 0, 6);
            menuStrip1.RenderMode = ToolStripRenderMode.Professional;
            menuStrip1.Renderer = new ToolStripProfessionalRenderer(new RedMenuColorTable());

            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                item.Margin = new Padding(0, 0, 10, 0);
                item.Padding = new Padding(8, 0, 8, 0);
            }
        }

        private void BuildHero()
        {
            heroPanel = new HeroPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(150, 25, 32),
                CampusImage = campusImage
            };

            logoPictureBox = new PictureBox
            {
                Image = logoImage,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            lblHeroTitle = CreateHeroLabel("HỆ THỐNG THI TRẮC NGHIỆM", 25F, FontStyle.Bold);
            lblHeroSubtitle = CreateHeroLabel("Học viện Công nghệ Bưu chính Viễn thông", 12.5F, FontStyle.Regular);
            lblHeroUser = CreateHeroLabel("", 11F, FontStyle.Bold);
            lblHeroRole = CreateHeroLabel("", 10F, FontStyle.Regular);

            heroPanel.Controls.Add(logoPictureBox);
            heroPanel.Controls.Add(lblHeroTitle);
            heroPanel.Controls.Add(lblHeroSubtitle);
            heroPanel.Controls.Add(lblHeroUser);
            heroPanel.Controls.Add(lblHeroRole);
            heroPanel.Resize += (sender, e) => LayoutHero();
            LayoutHero();
        }

        private Label CreateHeroLabel(string text, float size, FontStyle style)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", size, style),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void LayoutHero()
        {
            if (heroPanel == null) return;

            int width = heroPanel.ClientSize.Width;
            int height = heroPanel.ClientSize.Height;

            logoPictureBox.Size = new Size(106, 106);
            logoPictureBox.Location = new Point(34, 34);

            int textX = 164;
            lblHeroTitle.Location = new Point(textX, 42);
            lblHeroTitle.Size = new Size(Math.Max(300, width - textX - 34), 48);

            lblHeroSubtitle.Location = new Point(textX + 3, 92);
            lblHeroSubtitle.Size = new Size(Math.Max(300, width - textX - 34), 34);

            lblHeroUser.Location = new Point(34, height - 76);
            lblHeroUser.Size = new Size(Math.Max(360, width - 68), 28);

            lblHeroRole.Location = new Point(34, height - 45);
            lblHeroRole.Size = new Size(Math.Max(360, width - 68), 24);
        }

        private void BuildInfoPanel()
        {
            infoPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
        }

        private void BuildShortcutPanel()
        {
            lblShortcutTitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Truy cập nhanh",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 19, 28),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            shortcutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(0, 4, 0, 0),
                Margin = new Padding(0)
            };

            shortcutItems.Add(new ShortcutItem("Tạo tài khoản", tạoTàiKhoảnToolStripMenuItem));
            shortcutItems.Add(new ShortcutItem("Môn học", mônHọcToolStripMenuItem));
            shortcutItems.Add(new ShortcutItem("Giáo viên", giáoViênToolStripMenuItem));
            shortcutItems.Add(new ShortcutItem("Lớp", lớpToolStripMenuItem));
            //shortcutItems.Add(new ShortcutItem("Sinh viên", sinhViênToolStripMenuItem));
            shortcutItems.Add(new ShortcutItem("Bộ đề", bộĐềToolStripMenuItem));
            shortcutItems.Add(new ShortcutItem("Đăng ký thi", đăngKíThiToolStripMenuItem));
            shortcutItems.Add(new ShortcutItem("Thi", thiToolStripMenuItem));
            shortcutItems.Add(new ShortcutItem("Bảng điểm", bảngĐiểmToolStripMenuItem));
            //shortcutItems.Add(new ShortcutItem("Xem kết quả", xemKetQuaToolStripMenuItem));
        }

        private void UpdateDashboard()
        {
            string nhom = (Program.mGroup ?? "").Trim().ToUpper();
            string hoTen = string.IsNullOrWhiteSpace(Program.mHoTen) ? "Người dùng" : Program.mHoTen.Trim();
            string tenDangNhap = string.IsNullOrWhiteSpace(Program.mLogin) ? Program.mUserName : Program.mLogin;

            lblHeroUser.Text = $"Xin chào, {hoTen}";
            lblHeroRole.Text = $"Nhóm quyền: {nhom}    |    Tài khoản: {tenDangNhap}    |    CSDL: {Program.serverName} / {Program.dbName}";

            infoPanel.Controls.Clear();
            infoPanel.Controls.Add(CreateInfoCard("Phiên đăng nhập", hoTen, nhom));
            infoPanel.Controls.Add(CreateInfoCard("Nhóm quyền", GetRoleName(nhom), GetRoleDescription(nhom)));
            infoPanel.Controls.Add(CreateInfoCard("Cơ sở dữ liệu", Program.dbName, Program.serverName));
            infoPanel.Controls.Add(CreateInfoCard("Trạng thái", "Sẵn sàng", "Chọn chức năng từ menu hoặc nút nhanh"));

            shortcutPanel.Controls.Clear();
            foreach (ShortcutItem item in shortcutItems)
            {
                if (item.MenuItem.Enabled)
                    shortcutPanel.Controls.Add(CreateShortcutButton(item.Text, item.MenuItem));
            }
        }

        private Panel CreateInfoCard(string title, string value, string detail)
        {
            Panel card = new Panel
            {
                Width = 238,
                Height = 92,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 14, 10)
            };

            card.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen border = new Pen(Color.FromArgb(228, 232, 238)))
                    e.Graphics.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
                using (SolidBrush strip = new SolidBrush(Color.FromArgb(184, 28, 38)))
                    e.Graphics.FillRectangle(strip, 0, 0, 5, card.Height);
            };

            Label lblTitle = new Label
            {
                Text = title.ToUpper(),
                AutoSize = false,
                Location = new Point(18, 12),
                Size = new Size(card.Width - 30, 20),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 42, 48)
            };

            Label lblValue = new Label
            {
                Text = value,
                AutoSize = false,
                Location = new Point(18, 34),
                Size = new Size(card.Width - 30, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 35, 35)
            };

            Label lblDetail = new Label
            {
                Text = detail,
                AutoSize = false,
                Location = new Point(18, 61),
                Size = new Size(card.Width - 30, 22),
                Font = new Font("Segoe UI", 8.7F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblDetail);
            return card;
        }

        private Button CreateShortcutButton(string text, ToolStripMenuItem menuItem)
        {
            Button button = new Button
            {
                Text = text,
                Width = 168,
                Height = 54,
                Margin = new Padding(0, 0, 12, 12),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(138, 22, 31),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(196, 42, 52);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 242, 243);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(245, 222, 225);
            button.Click += (sender, e) => menuItem.PerformClick();
            return button;
        }

        private Image LoadAssetImage(string fileName)
        {
            string[] candidates =
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName),
                Path.Combine(Application.StartupPath, "Assets", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Assets", fileName)
            };

            foreach (string candidate in candidates)
            {
                string fullPath = Path.GetFullPath(candidate);
                if (!File.Exists(fullPath)) continue;

                byte[] bytes = File.ReadAllBytes(fullPath);
                using (MemoryStream stream = new MemoryStream(bytes))
                using (Image loadedImage = Image.FromStream(stream))
                    return new Bitmap(loadedImage);
            }

            return null;
        }

        private string GetRoleName(string role)
        {
            if (role == "PGV") return "Phòng giáo vụ";
            if (role == "GIANGVIEN") return "Giảng viên";
            if (role == "SINHVIEN") return "Sinh viên";
            return "Chưa xác định";
        }

        private string GetRoleDescription(string role)
        {
            if (role == "PGV") return "Quản trị danh mục, tài khoản và báo cáo";
            if (role == "GIANGVIEN") return "Bộ đề, đăng ký thi, thi thử và báo cáo";
            if (role == "SINHVIEN") return "Thi và xem lại kết quả";
            return "Đăng nhập để sử dụng hệ thống";
        }

        private void mônHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formMonHoc f = new formMonHoc();
            f.ShowDialog();
        }

        private void tạoTàiKhoảnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formTaoTaiKhoan f = new formTaoTaiKhoan();
            f.ShowDialog();
        }

        private void giáoviênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formGiaoVien f = new formGiaoVien();
            f.ShowDialog();
        }

        private void lớpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formLop f = new formLop();
            f.ShowDialog();
        }

        private void xemKetQuaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formXemKetQua f = new formXemKetQua();
            f.ShowDialog();
        }

        private void sinhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formLopSinhVien f = new formLopSinhVien();
            f.ShowDialog();
        }

        private void bộĐềToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formBoDe f = new formBoDe();
            f.ShowDialog();
        }

        private void đăngKíThiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formDangKyThi f = new formDangKyThi();
            f.ShowDialog();
        }

        private void thiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formThi f = new formThi();
            f.ShowDialog();
        }

        private void bảngĐiểmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formBangDiem f = new formBangDiem();
            f.ShowDialog();
        }

        private sealed class ShortcutItem
        {
            public ShortcutItem(string text, ToolStripMenuItem menuItem)
            {
                Text = text;
                MenuItem = menuItem;
            }

            public string Text { get; }
            public ToolStripMenuItem MenuItem { get; }
        }
    }

    internal sealed class HeroPanel : Panel
    {
        public Image CampusImage { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0) return;

            if (CampusImage != null)
            {
                Rectangle dest = ClientRectangle;
                Rectangle src = GetCoverSourceRectangle(CampusImage, dest);
                e.Graphics.DrawImage(CampusImage, dest, src, GraphicsUnit.Pixel);
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(154, 24, 34)))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            using (LinearGradientBrush overlay = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(225, 124, 20, 29),
                Color.FromArgb(120, 124, 20, 29),
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(overlay, ClientRectangle);
            }

            using (SolidBrush light = new SolidBrush(Color.FromArgb(42, 255, 255, 255)))
            {
                Point[] shape =
                {
                    new Point(ClientRectangle.Width - 300, 0),
                    new Point(ClientRectangle.Width, 0),
                    new Point(ClientRectangle.Width, ClientRectangle.Height),
                    new Point(ClientRectangle.Width - 520, ClientRectangle.Height)
                };
                e.Graphics.FillPolygon(light, shape);
            }

            using (Pen border = new Pen(Color.FromArgb(220, 220, 220)))
                e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        }

        private Rectangle GetCoverSourceRectangle(Image image, Rectangle dest)
        {
            float imageRatio = (float)image.Width / image.Height;
            float destRatio = (float)Math.Max(1, dest.Width) / Math.Max(1, dest.Height);

            if (imageRatio > destRatio)
            {
                int width = (int)(image.Height * destRatio);
                int x = (image.Width - width) / 2;
                return new Rectangle(x, 0, width, image.Height);
            }

            int height = (int)(image.Width / destRatio);
            int y = (image.Height - height) / 2;
            return new Rectangle(0, y, image.Width, height);
        }
    }

    internal sealed class RedMenuColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(255, 235, 238);
        public override Color MenuItemBorder => Color.FromArgb(184, 28, 38);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(255, 235, 238);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(255, 235, 238);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(184, 28, 38);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(184, 28, 38);
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color ImageMarginGradientBegin => Color.White;
        public override Color ImageMarginGradientMiddle => Color.White;
        public override Color ImageMarginGradientEnd => Color.White;
    }
}
