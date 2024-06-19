using System.Data;
using System.Data.SqlClient;
using EmployeeManagementAPI.Enums;
using EmployeeManagementAPI.Models.RequestModels.LogIn;
using EmployeeManagementAPI.Queries;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementAPI.Controllers;

public class LogInController : ControllerBase
{
    private readonly AdoDotNetServices _adoDotNetServices;

    public LogInController(AdoDotNetServices adoDotNetServices)
    {
        _adoDotNetServices = adoDotNetServices;
    }

    [HttpPost]
    [Route("/api/account/login")]
    public IActionResult CheckLogin([FromBody] LogInRequestModel loginRequest)
    {
        try
        {
            if (loginRequest.Email.IsNullOrEmpty())
                return BadRequest("Email cannot be empty");

            string query = UserQuery.LoginQuery();

            List<SqlParameter> parameters =
                new() { new SqlParameter("@Email", loginRequest.Email) };

            DataTable result = _adoDotNetServices.QueryFirstOrDefault(query, parameters.ToArray());

            if (result.Rows.Count == 0)
                return NotFound("Email not found");

            string roleName = result.Rows[0]["RoleName"].ToString()!;
            bool isAdmin = roleName == EnumUserRole.Admin.ToString();

            return Ok(isAdmin);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
