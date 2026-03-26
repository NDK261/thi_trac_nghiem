namespace QLThiTracNghiem
{
    partial class formTestKetNoi
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
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.btnLoadMonHoc = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvData
            // 
            this.dgvData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Location = new System.Drawing.Point(26, 25);
            this.dgvData.Name = "dgvData";
            this.dgvData.Size = new System.Drawing.Size(461, 260);
            this.dgvData.TabIndex = 0;
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Location = new System.Drawing.Point(60, 336);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(100, 23);
            this.btnTestConnection.TabIndex = 1;
            this.btnTestConnection.Text = "Kiểm tra kết nối";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            // 
            // btnLoadMonHoc
            // 
            this.btnLoadMonHoc.Location = new System.Drawing.Point(344, 336);
            this.btnLoadMonHoc.Name = "btnLoadMonHoc";
            this.btnLoadMonHoc.Size = new System.Drawing.Size(110, 23);
            this.btnLoadMonHoc.TabIndex = 2;
            this.btnLoadMonHoc.Text = "Tải bảng MONHOC";
            this.btnLoadMonHoc.UseVisualStyleBackColor = true;
            this.btnLoadMonHoc.Click += new System.EventHandler(this.btnLoadMonHoc_Click);
            // 
            // formTestKetNoi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 433);
            this.Controls.Add(this.btnLoadMonHoc);
            this.Controls.Add(this.btnTestConnection);
            this.Controls.Add(this.dgvData);
            this.Name = "formTestKetNoi";
            this.Text = "formTestKetNoi";
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.Button btnLoadMonHoc;
    }
}