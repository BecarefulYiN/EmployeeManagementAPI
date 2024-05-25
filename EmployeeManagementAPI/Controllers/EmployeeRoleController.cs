using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Models.RequestModels.Employee_Role;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace EmployeeManagementAPI.Controllers;
public class EmployeeRoleController : ControllerBase
{
    private readonly AdoDotNetServices _adoDotNetServices;

    public EmployeeRoleController(AdoDotNetServices adoDotNetServices)
    {
        _adoDotNetServices = adoDotNetServices;
    }

    [HttpGet]
    [Route("/api/employee-role-list")]

    public IActionResult GetEmployeeRoleList()
    {
        try
        {
            string query = @"SELECT [RoleId]
      ,[RoleName]
      ,[IsActive]
  FROM [dbo].[EmployeeRoleTable] WHERE IsActive = @IsActive";
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@IsActive", true)
            };
            List<EmployeeRoleModel> lst = _adoDotNetServices.Query<EmployeeRoleModel>(query, parameters.ToArray());
            return Ok(lst);

        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPost]
    [Route("/api/create-employee-role")]

    public IActionResult CreateEmployeeRole([FromBody] EmployeeRoleRequestModel requestModel)
    {
        try
        {
            if (string.IsNullOrEmpty(requestModel.RoleName))
                return BadRequest("Role Name does't not allow empty");

            string duplicateQuery = @"SELECT [RoleId]
      ,[RoleName]
      ,[IsActive]
  FROM [dbo].[EmployeeRoleTable] WHERE RoleName = @RoleName";

            List<SqlParameter> duplicateParameters = new()
            {
                new SqlParameter("@RoleName", requestModel.RoleName.ToLower())
            };

            DataTable Roles = _adoDotNetServices.QueryFirstOrDefault(duplicateQuery, duplicateParameters.ToArray());
            if (Roles.Rows.Count > 0)
            {
                return Conflict(" This roles already exist!");
            }

            //insert 

            string query = @"INSERT INTO [dbo].[EmployeeRoleTable]
           ([RoleName]
           ,[IsActive])
     VALUES
            (@RoleName, @IsActive);
            SELECT SCOPE_IDENTITY();";

            List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@RoleName", requestModel.RoleName.ToLower()),
                    new SqlParameter("@IsActive", true)
                };

            int result = _adoDotNetServices.Execute(query, parameters.ToArray());
            return result > 0 ? StatusCode(201, "Create successfully") : BadRequest("Create faill");
        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/update-employee-role/{id}")]
    public IActionResult UpdateEmployeeRole([FromBody] EmployeeRoleRequestModel requestModel, long id)
    {
        try
        {
            string checkExistQuery = @"SELECT [RoleId], [RoleName], [IsActive] FROM [dbo].[EmployeeRoleTable] WHERE RoleId = @RoleId";
            List<SqlParameter> existenceParameters = new()
            {
                new SqlParameter("@RoleId", id)
            };

            DataTable existQuery = _adoDotNetServices.QueryFirstOrDefault(checkExistQuery, existenceParameters.ToArray());
            if (existQuery.Rows.Count == 0)
                return NotFound("Role not found");

            // Validate
            if (string.IsNullOrEmpty(requestModel.RoleName))
                return BadRequest("Role Name does not allow empty");

            // Check for duplicate role name (excluding the current role)
            string duplicateRoleQuery = @"SELECT COUNT(*) FROM [dbo].[EmployeeRoleTable] WHERE RoleName = @RoleName AND RoleId != @RoleId";
            List<SqlParameter> duplicateRoleParameters = new()
            {
                new SqlParameter("@RoleName", requestModel.RoleName),
                new SqlParameter("@RoleId", id)
            };
            int duplicateCount = Convert.ToInt32(_adoDotNetServices.QueryFirstOrDefault(duplicateRoleQuery, duplicateRoleParameters.ToArray()).Rows[0][0]);
            if (duplicateCount > 0)
            {
                return Conflict("This role already exists");
            }

            // Update role record
            string updateQuery = @"UPDATE [dbo].[EmployeeRoleTable] SET RoleName = @RoleName WHERE [RoleId] = @RoleId";
            List<SqlParameter> updateParameters = new()
            {
                new SqlParameter("@RoleName", requestModel.RoleName),
                new SqlParameter("@RoleId", id)
            };
            int rowsAffected = _adoDotNetServices.Execute(updateQuery, updateParameters.ToArray());
            if (rowsAffected > 0)
            {
                return Ok("Updated successfully");
            }
            else
            {
                return BadRequest("Failed to update");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/delete-employee-role/{id}")]

    public IActionResult DeleteEmployeeRole(long id)
    {
        try
        {
            string query = @"UPDATE [dbo].[EmployeeRoleTable]
   SET IsActive = @IsActive
 WHERE RoleId = @RoleId";

            List<SqlParameter> parameter = new()
            {

                new SqlParameter("@IsActive", false),
                new SqlParameter("@RoleId", id)
            };

            int result = _adoDotNetServices.Execute(query, parameter.ToArray());
            return result > 0 ? StatusCode(201, "Delete Successful") : BadRequest("Delete Fail");
        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
