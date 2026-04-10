namespace QLThiTracNghiem
{
    partial class formGiaoVien
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.btnThoat = new System.Windows.Forms.Button();
            this.btnGhi = new System.Windows.Forms.Button();
            this.btnSua = new System.Windows.Forms.Button();
            this.btnXoa = new System.Windows.Forms.Button();
            this.btnThem = new System.Windows.Forms.Button();
            this.txtTenMH = new System.Windows.Forms.TextBox();
            this.txtMaGV = new System.Windows.Forms.TextBox();
            this.dgvGiaoVien = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.textHo = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textSoDT = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textTen = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textTimKiem = new System.Windows.Forms.TextBox();
            this.btnTim = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGiaoVien)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(131, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 16);
            this.label1.TabIndex = 19;
            this.label1.Text = "Mã Giáo Viên";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // btnThoat
            // 
            this.btnThoat.Location = new System.Drawing.Point(578, 412);
            this.btnThoat.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnThoat.Name = "btnThoat";
            this.btnThoat.Size = new System.Drawing.Size(93, 46);
            this.btnThoat.TabIndex = 18;
            this.btnThoat.Text = "Thoát";
            this.btnThoat.UseVisualStyleBackColor = true;
            this.btnThoat.Click += new System.EventHandler(this.btnThoat_Click);
            // 
            // btnGhi
            // 
            this.btnGhi.Location = new System.Drawing.Point(460, 412);
            this.btnGhi.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGhi.Name = "btnGhi";
            this.btnGhi.Size = new System.Drawing.Size(99, 46);
            this.btnGhi.TabIndex = 17;
            this.btnGhi.Text = "Lưu/Ghi";
            this.btnGhi.UseVisualStyleBackColor = true;
            this.btnGhi.Click += new System.EventHandler(this.btnGhi_Click);
            // 
            // btnSua
            // 
            this.btnSua.Location = new System.Drawing.Point(344, 412);
            this.btnSua.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSua.Name = "btnSua";
            this.btnSua.Size = new System.Drawing.Size(93, 46);
            this.btnSua.TabIndex = 16;
            this.btnSua.Text = "Sửa";
            this.btnSua.UseVisualStyleBackColor = true;
            this.btnSua.Click += new System.EventHandler(this.btnSua_Click);
            // 
            // btnXoa
            // 
            this.btnXoa.Location = new System.Drawing.Point(250, 412);
            this.btnXoa.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(93, 46);
            this.btnXoa.TabIndex = 15;
            this.btnXoa.Text = "Xóa";
            this.btnXoa.UseVisualStyleBackColor = true;
            this.btnXoa.Click += new System.EventHandler(this.btnXoa_Click);
            // 
            // btnThem
            // 
            this.btnThem.Location = new System.Drawing.Point(158, 412);
            this.btnThem.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(93, 46);
            this.btnThem.TabIndex = 14;
            this.btnThem.Text = "Thêm";
            this.btnThem.UseVisualStyleBackColor = true;
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            // 
            // txtTenMH
            // 
            this.txtTenMH.Location = new System.Drawing.Point(507, 92);
            this.txtTenMH.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtTenMH.Name = "txtTenMH";
            this.txtTenMH.Size = new System.Drawing.Size(89, 22);
            this.txtTenMH.TabIndex = 13;
            // 
            // txtMaGV
            // 
            this.txtMaGV.Location = new System.Drawing.Point(224, 50);
            this.txtMaGV.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtMaGV.Name = "txtMaGV";
            this.txtMaGV.Size = new System.Drawing.Size(89, 22);
            this.txtMaGV.TabIndex = 12;
            // 
            // dgvGiaoVien
            // 
            this.dgvGiaoVien.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvGiaoVien.Location = new System.Drawing.Point(113, 209);
            this.dgvGiaoVien.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvGiaoVien.Name = "dgvGiaoVien";
            this.dgvGiaoVien.RowHeadersWidth = 62;
            this.dgvGiaoVien.RowTemplate.Height = 28;
            this.dgvGiaoVien.Size = new System.Drawing.Size(635, 157);
            this.dgvGiaoVien.TabIndex = 11;
            this.dgvGiaoVien.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvGiaoVien_CellContentClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(131, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 16);
            this.label3.TabIndex = 22;
            this.label3.Text = "Họ";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // textHo
            // 
            this.textHo.Location = new System.Drawing.Point(224, 89);
            this.textHo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textHo.Name = "textHo";
            this.textHo.Size = new System.Drawing.Size(89, 22);
            this.textHo.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(438, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 16);
            this.label4.TabIndex = 26;
            this.label4.Text = "Số ĐT";
            // 
            // textSoDT
            // 
            this.textSoDT.Location = new System.Drawing.Point(507, 53);
            this.textSoDT.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textSoDT.Name = "textSoDT";
            this.textSoDT.Size = new System.Drawing.Size(89, 22);
            this.textSoDT.TabIndex = 25;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(131, 132);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 16);
            this.label5.TabIndex = 24;
            this.label5.Text = "Tên";
            // 
            // textTen
            // 
            this.textTen.Location = new System.Drawing.Point(224, 129);
            this.textTen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textTen.Name = "textTen";
            this.textTen.Size = new System.Drawing.Size(89, 22);
            this.textTen.TabIndex = 23;
            this.textTen.TextChanged += new System.EventHandler(this.textTen_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(439, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 16);
            this.label2.TabIndex = 27;
            this.label2.Text = "Địa Chỉ";
            // 
            // textTimKiem
            // 
            this.textTimKiem.Location = new System.Drawing.Point(158, 172);
            this.textTimKiem.Name = "textTimKiem";
            this.textTimKiem.Size = new System.Drawing.Size(166, 22);
            this.textTimKiem.TabIndex = 28;
            // 
            // btnTim
            // 
            this.btnTim.Location = new System.Drawing.Point(344, 172);
            this.btnTim.Name = "btnTim";
            this.btnTim.Size = new System.Drawing.Size(75, 23);
            this.btnTim.TabIndex = 29;
            this.btnTim.Text = "Tìm Kiếm";
            this.btnTim.UseVisualStyleBackColor = true;
            this.btnTim.Click += new System.EventHandler(this.btnTim_Click);
            // 
            // formGiaoVien
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 495);
            this.Controls.Add(this.btnTim);
            this.Controls.Add(this.textTimKiem);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textSoDT);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textTen);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textHo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnThoat);
            this.Controls.Add(this.btnGhi);
            this.Controls.Add(this.btnSua);
            this.Controls.Add(this.btnXoa);
            this.Controls.Add(this.btnThem);
            this.Controls.Add(this.txtTenMH);
            this.Controls.Add(this.txtMaGV);
            this.Controls.Add(this.dgvGiaoVien);
            this.Name = "formGiaoVien";
            this.Text = "formGiaoVien";
            this.Load += new System.EventHandler(this.formGiaoVien_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvGiaoVien)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnThoat;
        private System.Windows.Forms.Button btnGhi;
        private System.Windows.Forms.Button btnSua;
        private System.Windows.Forms.Button btnXoa;
        private System.Windows.Forms.Button btnThem;
        private System.Windows.Forms.TextBox txtTenMH;
        private System.Windows.Forms.TextBox txtMaGV;
        private System.Windows.Forms.DataGridView dgvGiaoVien;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textHo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textSoDT;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textTen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textTimKiem;
        private System.Windows.Forms.Button btnTim;
    }
}