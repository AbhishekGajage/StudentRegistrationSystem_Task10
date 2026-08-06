using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Centralised ADO.NET helper for all database access in the
/// Student Registration System. Keeping connection logic in one
/// place avoids repeating boilerplate in every code-behind file.
/// </summary>
public static class DBHelper
{
    private static string ConnectionString =>
        ConfigurationManager.ConnectionStrings["StudentDB"].ConnectionString;

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConnectionString);
    }

    /// <summary>Executes a query and returns the results as a DataTable.</summary>
    public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
    {
        DataTable dt = new DataTable();
        using (SqlConnection conn = GetConnection())
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.CommandType = CommandType.Text;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
        }
        return dt;
    }

    /// <summary>Executes an INSERT/UPDATE/DELETE and returns rows affected.</summary>
    public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
    {
        using (SqlConnection conn = GetConnection())
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.CommandType = CommandType.Text;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Executes a query and returns a single scalar value.</summary>
    public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
    {
        using (SqlConnection conn = GetConnection())
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.CommandType = CommandType.Text;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteScalar();
        }
    }

    /// <summary>Calls the usp_GetNextStudentId stored procedure to get e.g. STU00001.</summary>
    public static string GetNextStudentId()
    {
        using (SqlConnection conn = GetConnection())
        using (SqlCommand cmd = new SqlCommand("usp_GetNextStudentId", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            SqlParameter outputParam = new SqlParameter("@NextId", SqlDbType.NVarChar, 20)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            conn.Open();
            cmd.ExecuteNonQuery();

            return outputParam.Value.ToString();
        }
    }
}
