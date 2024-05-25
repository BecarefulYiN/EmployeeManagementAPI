using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;


namespace EmployeeManagementAPI.Services;

public class AdoDotNetServices
{
    public readonly IConfiguration _configuration;

    public AdoDotNetServices(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<T> Query<T>(string query, SqlParameter[]? parameters = null)
    {
        SqlConnection conn = new(_configuration.GetConnectionString("DbConnection"));
        conn.Open();
        SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddRange(parameters);
        DataTable dt = new();
        SqlDataAdapter adapter = new(cmd);
        adapter.Fill(dt);
        conn.Close();

        string jsonStr = JsonConvert.SerializeObject(dt);
        List<T> lst = JsonConvert.DeserializeObject<List<T>>(jsonStr)!;

        return lst;
    }

    public DataTable QueryFirstOrDefault(string query, SqlParameter[]? parameters = null)
    {
        SqlConnection conn = new(_configuration.GetConnectionString("DbConnection"));
        conn.Open();
        SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddRange(parameters);
        DataTable dt = new();
        SqlDataAdapter adapter = new(cmd);
        adapter.Fill(dt);
        conn.Close();
        return dt;
    }

    public int Execute(string query, SqlParameter[]? parameter = null)
    {
        SqlConnection conn = new(_configuration.GetConnectionString("DbConnection"));
        conn.Open();
        SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddRange(parameter);
        int result = cmd.ExecuteNonQuery();
        conn.Close();

        return result;
    }
}
