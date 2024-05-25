using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Models.RequestModels.Employee;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
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

    [HttpPost]
    [Route("/api/create-employee")]
    public IActionResult CreateEmployee([FromBody] EmployeeRequestModel requestModel)
    {
        try
        {
            if (string.IsNullOrEmpty(requestModel.FirstName))
                return BadRequest("FirstName does't not allow empty");
            if (string.IsNullOrEmpty(requestModel.LastName))
                return BadRequest("LastName does't not allow empty");
            if (string.IsNullOrEmpty(requestModel.PhoneNumber))
                return BadRequest("PhoneNumber does't not allow empty");
            if (string.IsNullOrEmpty(requestModel.Email))
                return BadRequest("Email does't not allow empty");

            string duplicateQuery = @"SELECT [EmployeeId]
      ,[FirstName]
      ,[LastName]
      ,[Email]
      ,[PhoneNumber]
      ,[HireDate]
      ,[DepartmentId]
      ,[RoleId]
      ,[IsActive]
  FROM [dbo].[EmployeeTable] WHERE Email = @Email";

            List<SqlParameter> duplicateParameters = new()
            {
                new SqlParameter("@Email", requestModel.Email)
            };

            DataTable Employee = _adoDotNetServices.QueryFirstOrDefault(duplicateQuery, duplicateParameters.ToArray());
            if (Employee.Rows.Count > 0)
            {
                return Conflict("Employee with this phone number already exist!");
            }

            // Set DepartmentId and RoleId to NULL if they are 0
            object? departmentId = requestModel.DepartmentId.GetValueOrDefault() == 0 ? DBNull.Value : requestModel.DepartmentId;
            object? roleId = requestModel.RoleId.GetValueOrDefault() == 0 ? DBNull.Value : requestModel.RoleId;

           
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
            return result > 0 ? StatusCode(201, "Create successfully") : BadRequest("Create faill");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/employee-info-update/{id}")]

    public IActionResult UpdateEmployee([FromBody] EmployeeRequestModel requestModel,long id)
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
