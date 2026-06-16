using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;

namespace QLThiTracNghiem
{
    public partial class formReportViewer : Form
    {
        private string reportPath;
        private string datasetName;
        private DataTable dataSource;
        private List<ReportParameter> parameters;

        public formReportViewer()
        {
            InitializeComponent();
        }

        public formReportViewer(string reportPath, string datasetName, DataTable dataSource, List<ReportParameter> parameters = null) : this()
        {
            this.reportPath = reportPath;
            this.datasetName = datasetName;
            this.dataSource = dataSource;
            this.parameters = parameters;
        }

        private void formReportViewer_Load(object sender, EventArgs e)
        {
            try
            {
                reportViewer1.LocalReport.ReportEmbeddedResource = reportPath;

                if (parameters != null && parameters.Count > 0)
                {
                    reportViewer1.LocalReport.SetParameters(parameters);
                }

                reportViewer1.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource(datasetName, dataSource);
                reportViewer1.LocalReport.DataSources.Add(rds);

                reportViewer1.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị báo cáo: " + ex.Message, "Báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
