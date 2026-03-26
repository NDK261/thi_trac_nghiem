using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace QLThiTracNghiem
{
    internal class DBHelper
    {
        private static readonly string connStr =
            ConfigurationManager.ConnectionStrings["ThiTracNghiemConn"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connStr);
        }

        public static bool TestConnection(out string message)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    message = "Kết nối thành công tới SQL Server!";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Kết nối thất bại: " + ex.Message;
                return false;
            }
        }

        public static DataTable GetDataTable(string sql)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}

