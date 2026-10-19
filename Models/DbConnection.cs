using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AromaDom.Models
{
    public static class DbConnection
    {
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["AromaHouse"].ConnectionString;

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static SqlDataReader ExecuteReader(SqlConnection connection, string sql,
            params SqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var cmd = new SqlCommand(sql, connection))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
