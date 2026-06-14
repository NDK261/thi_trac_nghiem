using System;
using System.Data;
using System.Data.SqlClient;

namespace QLThiTracNghiem
{
    internal class DBHelper
    {
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(Program.GetActiveConnectionString());
        }

        public static bool TestConnection(out string message)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    message = $"Ket noi thanh cong toi {Program.serverName} / {Program.dbName}.";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Ket noi that bai: " + ex.Message;
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
                throw new Exception("Loi thuc thi: " + procedureName + "\nChi tiet: " + ex.Message);
            }
        }

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

                        SqlParameter returnValue = new SqlParameter
                        {
                            Direction = ParameterDirection.ReturnValue
                        };
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

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
                throw new Exception("Loi thuc thi: " + procedureName + "\nChi tiet: " + ex.Message);
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
                throw new Exception("Loi truy van: " + procedureName + "\nChi tiet: " + ex.Message);
            }
        }

        public static DataTable ExecuteDataTable(string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
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
            catch (Exception ex)
            {
                throw new Exception("Loi lay du lieu: " + procedureName + "\nChi tiet: " + ex.Message);
            }
        }

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
                throw new Exception("Loi thuc thi SQL:\n" + sql + "\nChi tiet: " + ex.Message);
            }
        }
    }
}
