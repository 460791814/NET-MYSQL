using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace DBUtility
{
    public class SqlHelper
    {
        private static readonly string constr = ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
        /// <summary>
        /// 增删改查nonquery
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pms"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] pms)
        {
            using (SqlConnection conn = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(pms);
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 获取单行单列的值 scalar
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pms"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql, params SqlParameter[] pms)
        {
            using (SqlConnection conn = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(pms);
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }
        /// <summary>
        /// ExecuteReader
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pms"></param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string sql, params SqlParameter[] pms)
        {
            SqlConnection conn = new SqlConnection(constr);
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.Parameters.AddRange(pms);
                    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
            catch (Exception)
            {
                if (conn!=null)
                {
                    conn.Close();
                    conn.Dispose();
                }
                throw;
            }
           
        }
        public static DataTable DataTable(string sql, params SqlParameter[] pms)
        {
            SqlDataAdapter sa = new SqlDataAdapter(sql, constr);
            sa.SelectCommand.Parameters.AddRange(pms);
            DataTable dt = new DataTable();
            sa.Fill(dt);
            return dt;
        }
    }
}
