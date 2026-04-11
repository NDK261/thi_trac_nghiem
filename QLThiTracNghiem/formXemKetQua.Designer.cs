namespace QLThiTracNghiem
{
    partial class formXemKetQua
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblKyThi = new System.Windows.Forms.Label();
            this.cmbKyThi = new System.Windows.Forms.ComboBox();
            this.btnXem = new System.Windows.Forms.Button();
            this.lblMaSV = new System.Windows.Forms.Label();
            this.lblMaSVValue = new System.Windows.Forms.Label();
            this.lblHoTen = new System.Windows.Forms.Label();
            this.lblHoTenValue = new System.Windows.Forms.Label();
            this.lblLop = new System.Windows.Forms.Label();
            this.lblLopValue = new System.Windows.Forms.Label();
            this.lblMonThi = new System.Windows.Forms.Label();
            this.lblMonThiValue = new System.Windows.Forms.Label();
            this.lblNgayThi = new System.Windows.Forms.Label();
            this.lblNgayThiValue = new System.Windows.Forms.Label();
            this.lblLanThi = new System.Windows.Forms.Label();
            this.lblLanThiValue = new System.Windows.Forms.Label();
            this.lblTrinhDo = new System.Windows.Forms.Label();
            this.lblTrinhDoValue = new System.Windows.Forms.Label();
            this.dgvKetQua = new System.Windows.Forms.DataGridView();
            this.btnThoat = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKetQua)).BeginInit();
            this.SuspendLayout();
            // 
            // lblKyThi
            // 
            this.lblKyThi.AutoSize = true;
            this.lblKyThi.Location = new System.Drawing.Point(18, 20);
            this.lblKyThi.Name = "lblKyThi";
            this.lblKyThi.Size = new System.Drawing.Size(41, 16);
            this.lblKyThi.TabIndex = 0;
            this.lblKyThi.Text = "Kỳ thi:";
            // 
            // cmbKyThi
            // 
            this.cmbKyThi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKyThi.FormattingEnabled = true;
            this.cmbKyThi.Location = new System.Drawing.Point(107, 17);
            this.cmbKyThi.Name = "cmbKyThi";
            this.cmbKyThi.Size = new System.Drawing.Size(223, 24);
            this.cmbKyThi.TabIndex = 1;
            // 
            // btnXem
            // 
            this.btnXem.Location = new System.Drawing.Point(356, 17);
            this.btnXem.Name = "btnXem";
            this.btnXem.Size = new System.Drawing.Size(89, 30);
            this.btnXem.TabIndex = 2;
            this.btnXem.Text = "Xem";
            this.btnXem.UseVisualStyleBackColor = true;
            this.btnXem.Click += new System.EventHandler(this.btnXem_Click);
            // 
            // lblMaSV
            // 
            this.lblMaSV.AutoSize = true;
            this.lblMaSV.Location = new System.Drawing.Point(18, 60);
            this.lblMaSV.Name = "lblMaSV";
            this.lblMaSV.Size = new System.Drawing.Size(50, 16);
            this.lblMaSV.TabIndex = 3;
            this.lblMaSV.Text = "Mã SV:";
            // 
            // lblMaSVValue
            // 
            this.lblMaSVValue.AutoSize = true;
            this.lblMaSVValue.Location = new System.Drawing.Point(107, 60);
            this.lblMaSVValue.Name = "lblMaSVValue";
            this.lblMaSVValue.Size = new System.Drawing.Size(19, 16);
            this.lblMaSVValue.TabIndex = 4;
            this.lblMaSVValue.Text = "---";
            // 
            // lblHoTen
            // 
            this.lblHoTen.AutoSize = true;
            this.lblHoTen.Location = new System.Drawing.Point(302, 60);
            this.lblHoTen.Name = "lblHoTen";
            this.lblHoTen.Size = new System.Drawing.Size(55, 16);
            this.lblHoTen.TabIndex = 5;
            this.lblHoTen.Text = "Họ Tên:";
            // 
            // lblHoTenValue
            // 
            this.lblHoTenValue.AutoSize = true;
            this.lblHoTenValue.Location = new System.Drawing.Point(391, 60);
            this.lblHoTenValue.Name = "lblHoTenValue";
            this.lblHoTenValue.Size = new System.Drawing.Size(19, 16);
            this.lblHoTenValue.TabIndex = 6;
            this.lblHoTenValue.Text = "---";
            // 
            // lblLop
            // 
            this.lblLop.AutoSize = true;
            this.lblLop.Location = new System.Drawing.Point(18, 90);
            this.lblLop.Name = "lblLop";
            this.lblLop.Size = new System.Drawing.Size(33, 16);
            this.lblLop.TabIndex = 7;
            this.lblLop.Text = "Lớp:";
            // 
            // lblLopValue
            // 
            this.lblLopValue.AutoSize = true;
            this.lblLopValue.Location = new System.Drawing.Point(107, 90);
            this.lblLopValue.Name = "lblLopValue";
            this.lblLopValue.Size = new System.Drawing.Size(19, 16);
            this.lblLopValue.TabIndex = 8;
            this.lblLopValue.Text = "---";
            // 
            // lblMonThi
            // 
            this.lblMonThi.AutoSize = true;
            this.lblMonThi.Location = new System.Drawing.Point(302, 90);
            this.lblMonThi.Name = "lblMonThi";
            this.lblMonThi.Size = new System.Drawing.Size(52, 16);
            this.lblMonThi.TabIndex = 9;
            this.lblMonThi.Text = "Môn thi:";
            // 
            // lblMonThiValue
            // 
            this.lblMonThiValue.AutoSize = true;
            this.lblMonThiValue.Location = new System.Drawing.Point(391, 90);
            this.lblMonThiValue.Name = "lblMonThiValue";
            this.lblMonThiValue.Size = new System.Drawing.Size(19, 16);
            this.lblMonThiValue.TabIndex = 10;
            this.lblMonThiValue.Text = "---";
            // 
            // lblNgayThi
            // 
            this.lblNgayThi.AutoSize = true;
            this.lblNgayThi.Location = new System.Drawing.Point(18, 120);
            this.lblNgayThi.Name = "lblNgayThi";
            this.lblNgayThi.Size = new System.Drawing.Size(59, 16);
            this.lblNgayThi.TabIndex = 11;
            this.lblNgayThi.Text = "Ngày thi:";
            // 
            // lblNgayThiValue
            // 
            this.lblNgayThiValue.AutoSize = true;
            this.lblNgayThiValue.Location = new System.Drawing.Point(107, 120);
            this.lblNgayThiValue.Name = "lblNgayThiValue";
            this.lblNgayThiValue.Size = new System.Drawing.Size(19, 16);
            this.lblNgayThiValue.TabIndex = 12;
            this.lblNgayThiValue.Text = "---";
            // 
            // lblLanThi
            // 
            this.lblLanThi.AutoSize = true;
            this.lblLanThi.Location = new System.Drawing.Point(302, 120);
            this.lblLanThi.Name = "lblLanThi";
            this.lblLanThi.Size = new System.Drawing.Size(48, 16);
            this.lblLanThi.TabIndex = 13;
            this.lblLanThi.Text = "Lần thi:";
            // 
            // lblLanThiValue
            // 
            this.lblLanThiValue.AutoSize = true;
            this.lblLanThiValue.Location = new System.Drawing.Point(391, 136);
            this.lblLanThiValue.Name = "lblLanThiValue";
            this.lblLanThiValue.Size = new System.Drawing.Size(19, 16);
            this.lblLanThiValue.TabIndex = 14;
            this.lblLanThiValue.Text = "---";
            // 
            // lblTrinhDo
            // 
            this.lblTrinhDo.AutoSize = true;
            this.lblTrinhDo.Location = new System.Drawing.Point(18, 150);
            this.lblTrinhDo.Name = "lblTrinhDo";
            this.lblTrinhDo.Size = new System.Drawing.Size(59, 16);
            this.lblTrinhDo.TabIndex = 15;
            this.lblTrinhDo.Text = "Trình độ:";
            // 
            // lblTrinhDoValue
            // 
            this.lblTrinhDoValue.AutoSize = true;
            this.lblTrinhDoValue.Location = new System.Drawing.Point(107, 150);
            this.lblTrinhDoValue.Name = "lblTrinhDoValue";
            this.lblTrinhDoValue.Size = new System.Drawing.Size(19, 16);
            this.lblTrinhDoValue.TabIndex = 16;
            this.lblTrinhDoValue.Text = "---";
            // 
            // dgvKetQua
            // 
            this.dgvKetQua.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvKetQua.Location = new System.Drawing.Point(18, 180);
            this.dgvKetQua.Name = "dgvKetQua";
            this.dgvKetQua.ReadOnly = true;
            this.dgvKetQua.RowHeadersWidth = 62;
            this.dgvKetQua.Size = new System.Drawing.Size(480, 240);
            this.dgvKetQua.TabIndex = 17;
            // 
            // btnThoat
            // 
            this.btnThoat.Location = new System.Drawing.Point(431, 430);
            this.btnThoat.Name = "btnThoat";
            this.btnThoat.Size = new System.Drawing.Size(67, 30);
            this.btnThoat.TabIndex = 18;
            this.btnThoat.Text = "Thoát";
            this.btnThoat.UseVisualStyleBackColor = true;
            this.btnThoat.Click += new System.EventHandler(this.btnThoat_Click);
            // 
            // formXemKetQua
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 574);
            this.Controls.Add(this.btnThoat);
            this.Controls.Add(this.dgvKetQua);
            this.Controls.Add(this.lblTrinhDoValue);
            this.Controls.Add(this.lblTrinhDo);
            this.Controls.Add(this.lblLanThiValue);
            this.Controls.Add(this.lblLanThi);
            this.Controls.Add(this.lblNgayThiValue);
            this.Controls.Add(this.lblNgayThi);
            this.Controls.Add(this.lblMonThiValue);
            this.Controls.Add(this.lblMonThi);
            this.Controls.Add(this.lblLopValue);
            this.Controls.Add(this.lblLop);
            this.Controls.Add(this.lblHoTenValue);
            this.Controls.Add(this.lblHoTen);
            this.Controls.Add(this.lblMaSVValue);
            this.Controls.Add(this.lblMaSV);
            this.Controls.Add(this.btnXem);
            this.Controls.Add(this.cmbKyThi);
            this.Controls.Add(this.lblKyThi);
            this.Name = "formXemKetQua";
            this.Text = "Xem Kết Quả Thi";
            this.Load += new System.EventHandler(this.formXemKetQua_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKetQua)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        private System.Windows.Forms.Label lblKyThi;
        private System.Windows.Forms.ComboBox cmbKyThi;
        private System.Windows.Forms.Button btnXem;
        private System.Windows.Forms.Label lblMaSV;
        private System.Windows.Forms.Label lblMaSVValue;
        private System.Windows.Forms.Label lblHoTen;
        private System.Windows.Forms.Label lblHoTenValue;
        private System.Windows.Forms.Label lblLop;
        private System.Windows.Forms.Label lblLopValue;
        private System.Windows.Forms.Label lblMonThi;
        private System.Windows.Forms.Label lblMonThiValue;
        private System.Windows.Forms.Label lblNgayThi;
        private System.Windows.Forms.Label lblNgayThiValue;
        private System.Windows.Forms.Label lblLanThi;
        private System.Windows.Forms.Label lblLanThiValue;
        private System.Windows.Forms.Label lblTrinhDo;
        private System.Windows.Forms.Label lblTrinhDoValue;
        private System.Windows.Forms.DataGridView dgvKetQua;
        private System.Windows.Forms.Button btnThoat;
    }
}
