using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Models.RequestModels.Department;
using EmployeeManagementAPI.Models.RequestModels.Employee_Role;
using EmployeeManagementAPI.Models.ResponseModesl.Department;
using EmployeeManagementAPI.Models.ResponseModesl.Employee;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace EmployeeManagementAPI.Controllers;

public class DepartmentController : ControllerBase
{
    private readonly AdoDotNetServices _adoDotNetServices;

    public DepartmentController(AdoDotNetServices adoDotNetServices)
    {
        _adoDotNetServices = adoDotNetServices;
    }

    [HttpGet]
    [Route("/api/department")]
    public IActionResult getDepartmentList()
    {
        try
        {
            string query = @"
            SELECT 
                d.[DepartmentId],
                d.[DepartmentName],
                d.[IsActive],
                COUNT(e.[EmployeeId]) AS EmployeeCount
            FROM 
                [dbo].[DepartmentTable] d
            LEFT JOIN 
                [dbo].[EmployeeTable] e ON d.[DepartmentId] = e.[DepartmentId]
            WHERE 
                d.[IsActive] = @IsActive
            GROUP BY 
                d.[DepartmentId], d.[DepartmentName], d.[IsActive]";

            List<SqlParameter> parameters = new()
        {
            new SqlParameter("@IsActive", true)
        };

            List<DepartmentModel> lst = _adoDotNetServices.Query<DepartmentModel>(query, parameters.ToArray());
            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet]
    [Route("/api/department/{id}")]
    public IActionResult GetDepartmentById(int id)
    {
        try
        {
            string query = @"
        SELECT 
            d.[DepartmentId],
            d.[DepartmentName],
            d.[IsActive],
            COUNT(e.[EmployeeId]) AS EmployeeCount
        FROM 
            [dbo].[DepartmentTable] d
        LEFT JOIN 
            [dbo].[EmployeeTable] e ON d.[DepartmentId] = e.[DepartmentId]
        WHERE 
            d.[DepartmentId] = @DepartmentId
        GROUP BY 
            d.[DepartmentId], d.[DepartmentName], d.[IsActive]";

            List<SqlParameter> parameters = new()
        {
            new SqlParameter("@DepartmentId", id)
        };

            List<GetDepartmentResponseModel> lst = _adoDotNetServices.Query<GetDepartmentResponseModel>(query, parameters.ToArray());

            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }




    [HttpPost]
    [Route("/api/create-department")]

    public IActionResult CreateDepartment([FromBody] DepartmentRequestModel requestModel)
    {
        try
        {
            if (string.IsNullOrEmpty(requestModel.DepartmentName)) return BadRequest("Department need to be fill");

            string duplicateQuery = @"SELECT [DepartmentId]
      ,[DepartmentName]
      ,[IsActive]
  FROM [dbo].[DepartmentTable] WHERE DepartmentName = @DepartmentName AND IsActive = @IsActive";

            List<SqlParameter> duplicateParameters = new()
            {
                new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                new SqlParameter("@IsActive",true)
            };

            DataTable departments = _adoDotNetServices.QueryFirstOrDefault(duplicateQuery, duplicateParameters.ToArray());
            if (departments.Rows.Count > 0) return BadRequest("Department with this name already exist");

            string query = @"INSERT INTO [dbo].[DepartmentTable]
           ([DepartmentName]
           ,[IsActive])
     VALUES (@DepartmentName, @IsActive)";

            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                new SqlParameter("@IsActive",true)
            };

            int result = _adoDotNetServices.Execute(query, parameters.ToArray());

            return result > 0 ? StatusCode(201, "Create Successfully") : BadRequest("Create Fail");


        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/update-department/{id}")]
    public IActionResult UpdateDepartment([FromBody] DepartmentRequestModel requestModel, long id)
    {
        try
        {
            string checkExistQuery = @"SELECT [DepartmentId]
      ,[DepartmentName]
      ,[IsActive]
  FROM [dbo].[DepartmentTable] WHERE DepartmentId = @DepartmentId";
            List<SqlParameter> existenceParameters = new()
            {
                new SqlParameter("@DepartmentId", id)
            };

            DataTable existQuery = _adoDotNetServices.QueryFirstOrDefault(checkExistQuery, existenceParameters.ToArray());
            if (existQuery.Rows.Count == 0)
                return NotFound("Role not found");

            // Validate
            if (string.IsNullOrEmpty(requestModel.DepartmentName))
                return BadRequest("Department name does not allow empty");

            // Check for duplicate role name (excluding the current role)
            string duplicateRoleQuery = @"SELECT COUNT(*) FROM [dbo].[DepartmentTable] WHERE DepartmentName = @DepartmentName AND DepartmentId != @DepartmentId";
            List<SqlParameter> duplicateRoleParameters = new()
            {
                new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                new SqlParameter("@DepartmentId", id)
            };
            int duplicateCount = Convert.ToInt32(_adoDotNetServices.QueryFirstOrDefault(duplicateRoleQuery, duplicateRoleParameters.ToArray()).Rows[0][0]);
            if (duplicateCount > 0)
            {
                return Conflict("This department already exists");
            }

            // Update role record
            string updateQuery = @"UPDATE [dbo].[DepartmentTable] SET DepartmentName = @DepartmentName WHERE [DepartmentId] = @DepartmentId";
            List<SqlParameter> updateParameters = new()
            {
                new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                new SqlParameter("@DepartmentId", id)
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
    [Route("/api/delete-department/{id}")]

    public IActionResult deleteDepartment(long id)
    {
        try
        {
            string query = @"UPDATE [dbo].[DepartmentTable]
   SET IsActive = @IsActive
 WHERE DepartmentId = @DepartmentId";

            List<SqlParameter> parameter = new()
            {

                new SqlParameter("@IsActive", false),
                new SqlParameter("@DepartmentId", id)
            };

            int result = _adoDotNetServices.Execute(query, parameter.ToArray());
            return result > 0 ? StatusCode(201, "Delete Successful") : BadRequest("Delete Fail");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
