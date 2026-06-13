using System;
using System.Data;
using System.Data.SqlClient;

namespace QLThiTracNghiem
{
    internal class DBHelper
    {
        /// <summary>
        /// Tạo kết nối SQL Server từ chuỗi kết nối đang lưu trong Program.connStr.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            if (string.IsNullOrWhiteSpace(Program.connStr))
                throw new Exception("Chưa có chuỗi kết nối đăng nhập.");

            return new SqlConnection(Program.connStr);
        }

        /// <summary>
        /// Mở thử kết nối để biết tài khoản hiện tại có vào được database hay không.
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
        /// Chạy một câu SELECT/EXEC đơn giản và trả kết quả về DataTable.
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
        /// Gọi stored procedure dạng INSERT/UPDATE/DELETE và trả về số dòng bị ảnh hưởng.
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
        /// Gọi stored procedure có RETURN code để form biết thành công hay lỗi nghiệp vụ.
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

                        // Thêm tham số đặc biệt để nhận RETURN code từ stored procedure.
                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        // Nếu SP có trả mã thì đổi về int, còn không thì xem như thành công.
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
        /// Gọi stored procedure có RETURN code trong một transaction đang mở.
        /// Dùng cho các nghiệp vụ phải lưu nhiều bảng cùng lúc, ví dụ nộp bài thi.
        /// </summary>
        public static int ExecuteNonQueryWithReturn(SqlConnection conn, SqlTransaction tran, string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(procedureName, conn, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    SqlParameter returnValue = new SqlParameter();
                    returnValue.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(returnValue);

                    cmd.ExecuteNonQuery();

                    if (returnValue.Value != null && returnValue.Value != DBNull.Value)
                    {
                        return Convert.ToInt32(returnValue.Value);
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi thực thi trong transaction: " + procedureName + "\nChi tiết: " + ex.Message);
            }
        }

        /// <summary>
        /// Gọi stored procedure và lấy một giá trị đơn, ví dụ COUNT hoặc một mã nào đó.
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
        /// Gọi stored procedure có trả bảng dữ liệu.
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
        /// Chạy trực tiếp một câu SQL dạng INSERT/UPDATE/DELETE khi phần đó không dùng SP.
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
