using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using EmployeeManagementAPI.Models.RequestModels.LogIn;

namespace EmployeeManagementAPI.Controllers;

public class LogInController : ControllerBase
{
    private readonly AdoDotNetServices _adoDotNetServices;

    public LogInController(AdoDotNetServices adoDotNetServices)
    {
        _adoDotNetServices = adoDotNetServices;
    }


    [HttpPost]
    [Route("/api/check-login")]
    public IActionResult CheckLogin([FromBody] LogInRequestModel loginRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(loginRequest.Email))
                return BadRequest("Email cannot be empty");

            // Check if email exists and get role
            string query = @"
                    SELECT 
                        e.Email, 
                        r.RoleName 
                    FROM 
                        EmployeeTable e
                    JOIN 
                        EmployeeRoleTable r 
                    ON 
                        e.RoleId = r.RoleId 
                    WHERE 
                        e.Email = @Email";

            List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@Email", loginRequest.Email)
                };

            DataTable result = _adoDotNetServices.QueryFirstOrDefault(query, parameters.ToArray());

            if (result.Rows.Count == 0)
            {
                return NotFound("Email not found");
            }

            string roleName = result.Rows[0]["RoleName"].ToString()!;
            bool isAdmin = roleName.ToLower() == "admin";

            return Ok(isAdmin);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
