using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Models.RequestModels.Employee;
using EmployeeManagementAPI.Models.ResponseModesl.Employee;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace EmployeeManagementAPI.Controllers;

[EnableCors("AllowLocalhost3000")]
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
            string query = @"
            SELECT e.[EmployeeId]
                  ,e.[FirstName]
                  ,e.[LastName]
                  ,e.[Email]
                  ,e.[PhoneNumber]
                  ,e.[HireDate]
                  ,d.[DepartmentName] AS DepartmentName
                  ,r.[RoleName] AS RoleName
                  ,e.[IsActive]
            FROM [dbo].[EmployeeTable] e
            LEFT JOIN [dbo].[DepartmentTable] d ON e.[DepartmentId] = d.[DepartmentId]
            LEFT JOIN [dbo].[EmployeeRoleTable] r ON e.[RoleId] = r.[RoleId]
            WHERE e.[IsActive] = @IsActive";

            List<SqlParameter> parameters = new()
        {
            new SqlParameter("@IsActive", true)
        };

            List<GetEmployeeResponseModel> lst = _adoDotNetServices.Query<GetEmployeeResponseModel>(query, parameters.ToArray());
            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }


    [HttpPost]
    [Route("/api/create-employee")]
    public IActionResult CreateEmployee([FromBody] EmployeeRequestModel requestModel)
    {
        try
        {
            if (string.IsNullOrEmpty(requestModel.FirstName))
                return BadRequest("FirstName does not allow empty");
            if (string.IsNullOrEmpty(requestModel.LastName))
                return BadRequest("LastName does not allow empty");
            if (string.IsNullOrEmpty(requestModel.PhoneNumber))
                return BadRequest("PhoneNumber does not allow empty");
            if (string.IsNullOrEmpty(requestModel.Email))
                return BadRequest("Email does not allow empty");

            string duplicateQuery = @"SELECT [EmployeeId] FROM [dbo].[EmployeeTable] WHERE Email = @Email";

            List<SqlParameter> duplicateParameters = new()
        {
            new SqlParameter("@Email", requestModel.Email)
        };

            DataTable employee = _adoDotNetServices.QueryFirstOrDefault(duplicateQuery, duplicateParameters.ToArray());
            if (employee.Rows.Count > 0)
            {
                return Conflict("Employee with this email already exists!");
            }

            // Check or insert Role
            object? roleId = DBNull.Value;
            if (!string.IsNullOrEmpty(requestModel.RoleName))
            {
                string roleQuery = @"SELECT [RoleId] FROM [dbo].[EmployeeRoleTable] WHERE [RoleName] = @RoleName AND IsActive = @IsActive";
                List<SqlParameter> roleParameters = new()
            {
                new SqlParameter("@RoleName", requestModel.RoleName),
                new SqlParameter("@IsActive", true)
            };

                DataTable role = _adoDotNetServices.QueryFirstOrDefault(roleQuery, roleParameters.ToArray());
                if (role.Rows.Count == 0)
                {
                    // Insert the new role
                    string insertRoleQuery = @"INSERT INTO [dbo].[EmployeeRoleTable] ([RoleName], [IsActive]) VALUES (@RoleName, @IsActive);
                                           SELECT SCOPE_IDENTITY();";
                    roleId = _adoDotNetServices.Execute(insertRoleQuery, roleParameters.ToArray());
                }
                else
                {
                    roleId = role.Rows[0]["RoleId"];
                }
            }

            // Check or insert Department
            object? departmentId = DBNull.Value;
            if (!string.IsNullOrEmpty(requestModel.DepartmentName))
            {
                string departmentQuery = @"SELECT [DepartmentId] FROM [dbo].[DepartmentTable] WHERE [DepartmentName] = @DepartmentName AND IsActive = @IsActive";
                List<SqlParameter> departmentParameters = new()
            {
                new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                new SqlParameter("@IsActive", true)
            };

                DataTable department = _adoDotNetServices.QueryFirstOrDefault(departmentQuery, departmentParameters.ToArray());
                if (department.Rows.Count == 0)
                {
                    // Insert the new department
                    string insertDepartmentQuery = @"INSERT INTO [dbo].[DepartmentTable] ([DepartmentName], [IsActive]) VALUES (@DepartmentName, @IsActive);
                                                 SELECT SCOPE_IDENTITY();";
                    departmentId = _adoDotNetServices.Execute(insertDepartmentQuery, departmentParameters.ToArray());
                }
                else
                {
                    departmentId = department.Rows[0]["DepartmentId"];
                }
            }

            string query = @"INSERT INTO [dbo].[EmployeeTable]
                                ([FirstName], [LastName], [Email], [PhoneNumber], [HireDate], [DepartmentId], [RoleId], [IsActive])
                                VALUES
                                (@FirstName, @LastName, @Email, @PhoneNumber, @HireDate, @DepartmentId, @RoleId, @IsActive);
                                SELECT SCOPE_IDENTITY();";

            List<SqlParameter> parameters = new()
        {
            new SqlParameter("@FirstName", requestModel.FirstName),
            new SqlParameter("@LastName", requestModel.LastName),
            new SqlParameter("@Email", requestModel.Email),
            new SqlParameter("@PhoneNumber", requestModel.PhoneNumber),
            new SqlParameter("@HireDate", DateTime.Now),
            new SqlParameter("@DepartmentId", departmentId),
            new SqlParameter("@RoleId", roleId),
            new SqlParameter("@IsActive", true)
        };

            int result = _adoDotNetServices.Execute(query, parameters.ToArray());
            return result > 0 ? StatusCode(201, "Create successfully") : BadRequest("Create failed");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }



    [HttpPut]
    [Route("/api/employee-info-update/{id}")]

    public IActionResult UpdateEmployee([FromBody] EmployeeUpdateRequestModel requestModel,long id)
    {
        try
        {
            string CheckExistQuery = @"SELECT [EmployeeId]
      ,[FirstName]
      ,[LastName]
      ,[Email]
      ,[PhoneNumber]
      ,[HireDate]
      ,[DepartmentId]
      ,[RoleId]
      ,[IsActive]
  FROM [dbo].[EmployeeTable] WHERE EmployeeId = @EmployeeId";
            List<SqlParameter> existenceParameters = new()
            {
                 new SqlParameter("@EmployeeId", id)
            };

            DataTable existQuery = _adoDotNetServices.QueryFirstOrDefault(CheckExistQuery, existenceParameters.ToArray());
            if (existQuery.Rows.Count == 0) return NotFound("Employee not fount");

            // Validate request model
            if (string.IsNullOrEmpty(requestModel.FirstName))
                return BadRequest("FirstName cannot be empty");
            if (string.IsNullOrEmpty(requestModel.LastName))
                return BadRequest("LastName cannot be empty");
            if (string.IsNullOrEmpty(requestModel.PhoneNumber))
                return BadRequest("PhoneNumber cannot be empty");
            if (string.IsNullOrEmpty(requestModel.Email))
                return BadRequest("Email cannot be empty");

            // Check for duplicate email (excluding the current employee)
            string duplicateEmailQuery = @"SELECT COUNT(*) FROM [dbo].[EmployeeTable] WHERE [Email] = @Email AND [EmployeeId] != @EmployeeId";
            List<SqlParameter> duplicateEmailParameters = new()
            {
                    new SqlParameter("@Email", requestModel.Email),
                    new SqlParameter("@EmployeeId", id)
            };
            int duplicateCount = Convert.ToInt32(_adoDotNetServices.QueryFirstOrDefault(duplicateEmailQuery, duplicateEmailParameters.ToArray()).Rows[0][0]);
            if (duplicateCount > 0)
            {
                return Conflict("Employee with this email already exists");
            }

            // Set DepartmentId and RoleId to NULL if they are 0
            object? departmentId = requestModel.DepartmentId.GetValueOrDefault() == 0 ? DBNull.Value : requestModel.DepartmentId;
            object? roleId = requestModel.RoleId.GetValueOrDefault() == 0 ? DBNull.Value : requestModel.RoleId;

            // Update employee record
            string updateQuery = @"UPDATE [dbo].[EmployeeTable] SET
                                   [FirstName] = @FirstName,
                                   [LastName] = @LastName,
                                   [Email] = @Email,
                                   [PhoneNumber] = @PhoneNumber,
                                   [DepartmentId] = @DepartmentId,
                                   [RoleId] = @RoleId
                               WHERE [EmployeeId] = @EmployeeId";
            List<SqlParameter> updateParameters = new()
        {
            new SqlParameter("@FirstName", requestModel.FirstName),
            new SqlParameter("@LastName", requestModel.LastName),
            new SqlParameter("@Email", requestModel.Email),
            new SqlParameter("@PhoneNumber", requestModel.PhoneNumber),
            new SqlParameter("@DepartmentId", departmentId),
            new SqlParameter("@RoleId", roleId),
            new SqlParameter("@EmployeeId", id)
        };
            int rowsAffected = _adoDotNetServices.Execute(updateQuery, updateParameters.ToArray());
            if (rowsAffected > 0)
            {
                return Ok("Employee updated successfully");
            }
            else
            {
                return BadRequest("Failed to update employee");
            }

        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/delete-employee/{id}")]

    public IActionResult deleteEmployee(long id)
    {
        try
        {
            string query = @"UPDATE [dbo].[EmployeeTable]
   SET IsActive = @IsActive
 WHERE EmployeeId = @EmployeeId";

            List<SqlParameter> parameter = new()
            {

                new SqlParameter("@IsActive", false),
                new SqlParameter("@EmployeeId", id)
            };

            int result = _adoDotNetServices.Execute(query, parameter.ToArray());
            return result > 0 ? StatusCode(201, "Delete Successful") : BadRequest("Delete Fail");
        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
