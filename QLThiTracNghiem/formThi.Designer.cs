namespace QLThiTracNghiem
{
    partial class formThi
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
            this.components = new System.ComponentModel.Container();
            this.lblLop = new System.Windows.Forms.Label();
            this.lblHoTen = new System.Windows.Forms.Label();
            this.lblMonThi = new System.Windows.Forms.Label();
            this.lblNgayThi = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.rdoA = new System.Windows.Forms.RadioButton();
            this.rdoB = new System.Windows.Forms.RadioButton();
            this.rdoC = new System.Windows.Forms.RadioButton();
            this.rdoD = new System.Windows.Forms.RadioButton();
            this.btnCauTruoc = new System.Windows.Forms.Button();
            this.btnCauSau = new System.Windows.Forms.Button();
            this.btnNopBai = new System.Windows.Forms.Button();
            this.lblThoiGian = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblNoiDungCauHoi = new System.Windows.Forms.Label();
            this.btnBatDau = new System.Windows.Forms.Button();
            this.btnThoat = new System.Windows.Forms.Button();
            this.lblLanThi = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbLop = new System.Windows.Forms.ComboBox();
            this.cmbMonThi = new System.Windows.Forms.ComboBox();
            this.cmbLanThi = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblLop
            // 
            this.lblLop.AutoSize = true;
            this.lblLop.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLop.Location = new System.Drawing.Point(45, 53);
            this.lblLop.Name = "lblLop";
            this.lblLop.Size = new System.Drawing.Size(56, 25);
            this.lblLop.TabIndex = 1;
            this.lblLop.Text = "Lớp :";
            // 
            // lblHoTen
            // 
            this.lblHoTen.AutoSize = true;
            this.lblHoTen.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHoTen.Location = new System.Drawing.Point(12, 9);
            this.lblHoTen.Name = "lblHoTen";
            this.lblHoTen.Size = new System.Drawing.Size(91, 25);
            this.lblHoTen.TabIndex = 2;
            this.lblHoTen.Text = "Tên SV :";
            // 
            // lblMonThi
            // 
            this.lblMonThi.AutoSize = true;
            this.lblMonThi.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMonThi.Location = new System.Drawing.Point(19, 98);
            this.lblMonThi.Name = "lblMonThi";
            this.lblMonThi.Size = new System.Drawing.Size(87, 25);
            this.lblMonThi.TabIndex = 3;
            this.lblMonThi.Text = "Môn thi :";
            this.lblMonThi.Click += new System.EventHandler(this.lblMonThi_Click);
            // 
            // lblNgayThi
            // 
            this.lblNgayThi.AutoSize = true;
            this.lblNgayThi.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNgayThi.Location = new System.Drawing.Point(12, 194);
            this.lblNgayThi.Name = "lblNgayThi";
            this.lblNgayThi.Size = new System.Drawing.Size(94, 25);
            this.lblNgayThi.TabIndex = 4;
            this.lblNgayThi.Text = "Ngày thi :";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // rdoA
            // 
            this.rdoA.AutoSize = true;
            this.rdoA.Location = new System.Drawing.Point(57, 53);
            this.rdoA.Name = "rdoA";
            this.rdoA.Size = new System.Drawing.Size(55, 37);
            this.rdoA.TabIndex = 7;
            this.rdoA.TabStop = true;
            this.rdoA.Text = "A";
            this.rdoA.UseVisualStyleBackColor = true;
            // 
            // rdoB
            // 
            this.rdoB.AutoSize = true;
            this.rdoB.Location = new System.Drawing.Point(57, 114);
            this.rdoB.Name = "rdoB";
            this.rdoB.Size = new System.Drawing.Size(55, 37);
            this.rdoB.TabIndex = 8;
            this.rdoB.TabStop = true;
            this.rdoB.Text = "B";
            this.rdoB.UseVisualStyleBackColor = true;
            // 
            // rdoC
            // 
            this.rdoC.AutoSize = true;
            this.rdoC.Location = new System.Drawing.Point(57, 181);
            this.rdoC.Name = "rdoC";
            this.rdoC.Size = new System.Drawing.Size(57, 37);
            this.rdoC.TabIndex = 9;
            this.rdoC.TabStop = true;
            this.rdoC.Text = "C";
            this.rdoC.UseVisualStyleBackColor = true;
            this.rdoC.CheckedChanged += new System.EventHandler(this.rdoC_CheckedChanged);
            // 
            // rdoD
            // 
            this.rdoD.AutoSize = true;
            this.rdoD.Location = new System.Drawing.Point(57, 248);
            this.rdoD.Name = "rdoD";
            this.rdoD.Size = new System.Drawing.Size(57, 37);
            this.rdoD.TabIndex = 10;
            this.rdoD.TabStop = true;
            this.rdoD.Text = "D";
            this.rdoD.UseVisualStyleBackColor = true;
            // 
            // btnCauTruoc
            // 
            this.btnCauTruoc.Enabled = false;
            this.btnCauTruoc.Location = new System.Drawing.Point(472, 702);
            this.btnCauTruoc.Name = "btnCauTruoc";
            this.btnCauTruoc.Size = new System.Drawing.Size(117, 47);
            this.btnCauTruoc.TabIndex = 11;
            this.btnCauTruoc.Text = "Câu Trước";
            this.btnCauTruoc.UseVisualStyleBackColor = true;
            this.btnCauTruoc.Click += new System.EventHandler(this.btnCauTruoc_Click);
            // 
            // btnCauSau
            // 
            this.btnCauSau.Enabled = false;
            this.btnCauSau.Location = new System.Drawing.Point(681, 702);
            this.btnCauSau.Name = "btnCauSau";
            this.btnCauSau.Size = new System.Drawing.Size(104, 47);
            this.btnCauSau.TabIndex = 12;
            this.btnCauSau.Text = "Câu Sau";
            this.btnCauSau.UseVisualStyleBackColor = true;
            this.btnCauSau.Click += new System.EventHandler(this.btnCauSau_Click);
            // 
            // btnNopBai
            // 
            this.btnNopBai.Enabled = false;
            this.btnNopBai.Location = new System.Drawing.Point(1033, 702);
            this.btnNopBai.Name = "btnNopBai";
            this.btnNopBai.Size = new System.Drawing.Size(97, 47);
            this.btnNopBai.TabIndex = 13;
            this.btnNopBai.Text = "Nộp Bài";
            this.btnNopBai.UseVisualStyleBackColor = true;
            this.btnNopBai.Click += new System.EventHandler(this.btnNopBai_Click);
            // 
            // lblThoiGian
            // 
            this.lblThoiGian.AutoSize = true;
            this.lblThoiGian.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThoiGian.Location = new System.Drawing.Point(1500, 43);
            this.lblThoiGian.Name = "lblThoiGian";
            this.lblThoiGian.Size = new System.Drawing.Size(86, 32);
            this.lblThoiGian.TabIndex = 14;
            this.lblThoiGian.Text = "45:00";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoA);
            this.groupBox1.Controls.Add(this.rdoB);
            this.groupBox1.Controls.Add(this.rdoC);
            this.groupBox1.Controls.Add(this.rdoD);
            this.groupBox1.Font = new System.Drawing.Font("Arial Narrow", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(149, 364);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1446, 306);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Câu trả lời";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // lblNoiDungCauHoi
            // 
            this.lblNoiDungCauHoi.Font = new System.Drawing.Font("Arial Narrow", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoiDungCauHoi.Location = new System.Drawing.Point(149, 250);
            this.lblNoiDungCauHoi.Name = "lblNoiDungCauHoi";
            this.lblNoiDungCauHoi.Size = new System.Drawing.Size(1446, 111);
            this.lblNoiDungCauHoi.TabIndex = 18;
            this.lblNoiDungCauHoi.Click += new System.EventHandler(this.lblNoiDungCauHoi_Click);
            // 
            // btnBatDau
            // 
            this.btnBatDau.Location = new System.Drawing.Point(206, 702);
            this.btnBatDau.Name = "btnBatDau";
            this.btnBatDau.Size = new System.Drawing.Size(117, 47);
            this.btnBatDau.TabIndex = 19;
            this.btnBatDau.Text = "Bắt Đầu Thi";
            this.btnBatDau.UseVisualStyleBackColor = true;
            this.btnBatDau.Click += new System.EventHandler(this.btnBatDau_Click);
            // 
            // btnThoat
            // 
            this.btnThoat.Location = new System.Drawing.Point(1212, 702);
            this.btnThoat.Name = "btnThoat";
            this.btnThoat.Size = new System.Drawing.Size(97, 47);
            this.btnThoat.TabIndex = 20;
            this.btnThoat.Text = "Thoát";
            this.btnThoat.UseVisualStyleBackColor = true;
            this.btnThoat.Click += new System.EventHandler(this.btnThoat_Click);
            // 
            // lblLanThi
            // 
            this.lblLanThi.AutoSize = true;
            this.lblLanThi.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLanThi.Location = new System.Drawing.Point(25, 147);
            this.lblLanThi.Name = "lblLanThi";
            this.lblLanThi.Size = new System.Drawing.Size(81, 25);
            this.lblLanThi.TabIndex = 22;
            this.lblLanThi.Text = "Lần thi :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(1293, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(210, 29);
            this.label1.TabIndex = 23;
            this.label1.Text = "Thòi gian làm bài :";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // cmbLop
            // 
            this.cmbLop.FormattingEnabled = true;
            this.cmbLop.Location = new System.Drawing.Point(107, 50);
            this.cmbLop.Name = "cmbLop";
            this.cmbLop.Size = new System.Drawing.Size(154, 28);
            this.cmbLop.TabIndex = 24;
            // 
            // cmbMonThi
            // 
            this.cmbMonThi.FormattingEnabled = true;
            this.cmbMonThi.Location = new System.Drawing.Point(107, 99);
            this.cmbMonThi.Name = "cmbMonThi";
            this.cmbMonThi.Size = new System.Drawing.Size(154, 28);
            this.cmbMonThi.TabIndex = 25;
            // 
            // cmbLanThi
            // 
            this.cmbLanThi.FormattingEnabled = true;
            this.cmbLanThi.Location = new System.Drawing.Point(107, 148);
            this.cmbLanThi.Name = "cmbLanThi";
            this.cmbLanThi.Size = new System.Drawing.Size(154, 28);
            this.cmbLanThi.TabIndex = 26;
            // 
            // formThi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1742, 807);
            this.Controls.Add(this.cmbLanThi);
            this.Controls.Add(this.cmbMonThi);
            this.Controls.Add(this.cmbLop);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblLanThi);
            this.Controls.Add(this.btnThoat);
            this.Controls.Add(this.btnBatDau);
            this.Controls.Add(this.lblNoiDungCauHoi);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblThoiGian);
            this.Controls.Add(this.btnNopBai);
            this.Controls.Add(this.btnCauSau);
            this.Controls.Add(this.btnCauTruoc);
            this.Controls.Add(this.lblNgayThi);
            this.Controls.Add(this.lblMonThi);
            this.Controls.Add(this.lblHoTen);
            this.Controls.Add(this.lblLop);
            this.Name = "formThi";
            this.Text = "formThi";
            this.Load += new System.EventHandler(this.formThi_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblLop;
        private System.Windows.Forms.Label lblHoTen;
        private System.Windows.Forms.Label lblMonThi;
        private System.Windows.Forms.Label lblNgayThi;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RadioButton rdoA;
        private System.Windows.Forms.RadioButton rdoB;
        private System.Windows.Forms.RadioButton rdoC;
        private System.Windows.Forms.RadioButton rdoD;
        private System.Windows.Forms.Button btnCauTruoc;
        private System.Windows.Forms.Button btnCauSau;
        private System.Windows.Forms.Button btnNopBai;
        private System.Windows.Forms.Label lblThoiGian;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblNoiDungCauHoi;
        private System.Windows.Forms.Button btnBatDau;
        private System.Windows.Forms.Button btnThoat;
        private System.Windows.Forms.Label lblLanThi;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbLop;
        private System.Windows.Forms.ComboBox cmbMonThi;
        private System.Windows.Forms.ComboBox cmbLanThi;
    }
}