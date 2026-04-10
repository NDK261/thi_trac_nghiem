using System;
using System.Data;
using System.Data.SqlClient;

namespace QLThiTracNghiem
{
    internal class DBHelper
    {
        /// <summary>
        /// Lấy connection từ Program.connStr
        /// </summary>
        public static SqlConnection GetConnection()
        {
            if (string.IsNullOrWhiteSpace(Program.connStr))
                throw new Exception("Chưa có chuỗi kết nối đăng nhập.");

            return new SqlConnection(Program.connStr);
        }

        /// <summary>
        /// Kiểm tra kết nối database
        /// </summary>
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

        /// <summary>
        /// Lấy DataTable từ câu SQL (SELECT)
        /// </summary>
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

        /// <summary>
        /// Thực thi Stored Procedure không có kết quả (INSERT, UPDATE, DELETE)
        /// Trả về số dòng bị ảnh hưởng
        /// </summary>
        public static int ExecuteNonQuery(string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi thực thi: " + procedureName + "\nChi tiết: " + ex.Message);
            }
        }

        /// <summary>
        /// Thực thi Stored Procedure và lấy Return Value (mã lỗi)
        /// </summary>
        public static int ExecuteNonQueryWithReturn(string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        // Thêm Return Value parameter
                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        // Trả về mã lỗi từ Stored Procedure
                        if (returnValue.Value != null && returnValue.Value != DBNull.Value)
                        {
                            return Convert.ToInt32(returnValue.Value);
                        }
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi thực thi: " + procedureName + "\nChi tiết: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy 1 giá trị đơn từ Stored Procedure (mã, tên, v.v.)
        /// </summary>
        public static object ExecuteScalar(string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi truy vấn: " + procedureName + "\nChi tiết: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy DataTable từ Stored Procedure
        /// </summary>
        public static DataTable ExecuteDataTable(string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy dữ liệu: " + procedureName + "\nChi tiết: " + ex.Message);
            }
        }

        /// <summary>
        /// Thực thi câu SQL trực tiếp (INSERT, UPDATE, DELETE)
        /// Sử dụng khi không dùng Stored Procedure
        /// </summary>
        public static int ExecuteNonQueryDirect(string sql)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi thực thi SQL:\n" + sql + "\nChi tiết: " + ex.Message);
            }
        }
    }
}