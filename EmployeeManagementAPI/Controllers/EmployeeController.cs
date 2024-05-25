using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace EmployeeManagementAPI.Controllers;

public class EmployeeController : ControllerBase
{
    private readonly AdoDotNetServices _adoDotNetServices;

    public EmployeeController(AdoDotNetServices adoDotNetServices)
    {
        _adoDotNetServices = adoDotNetServices;
    }

    [HttpGet]
    [Route("/api/employee")]

    public IActionResult getEmployeeList()
    {
        try
        {
            string query = @"SELECT [EmployeeId]
      ,[FirstName]
      ,[LastName]
      ,[Email]
      ,[PhoneNumber]
      ,[HireDate]
      ,[DepartmentId]
      ,[RoleId]
      ,[IsActive]
  FROM [dbo].[EmployeeTable] WHERE IsActive = @IsActive";
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@IsActive", true)
            };
            List<EmployeeModel> lst = _adoDotNetServices.Query<EmployeeModel>(query, parameters.ToArray());
            return Ok(lst);
        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
