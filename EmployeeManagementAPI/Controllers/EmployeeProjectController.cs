using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Models.RequestModels.EMployeeProject;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace EmployeeManagementAPI.Controllers;

public class EmployeeProjectController : ControllerBase
{
    private readonly AdoDotNetServices _adoDotNetServices;

    public EmployeeProjectController(AdoDotNetServices adoDotNetServices)
    {
        _adoDotNetServices = adoDotNetServices;
    }

    [HttpGet]
    [Route("/api/employee-project-list")]

    public IActionResult GetEmployeeProjectList()
    {
        try { 
        string query = @"SELECT [EmployeeId]
      ,[ProjectId]
  FROM [dbo].[EmployeeProjectTable] 
            WHERE IsActive = @IsActive";

        List<SqlParameter> parameters = new()
        {
            new SqlParameter("@IsActive", true)
        };

        List<EmployeeProjectModel> projects = _adoDotNetServices.Query<EmployeeProjectModel>(query, parameters.ToArray());
        return Ok(projects);
    }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
}
    }

    [HttpPut]
    [Route("/api/create-employee-project")]
    public IActionResult CreateEmployeeProject([FromBody] EmployeeProjectRequestModel requestModel)
    {
        try
        {
            if (!requestModel.EmployeeId.HasValue || !requestModel.ProjectId.HasValue)
                return BadRequest("All need to be fill");

            // Check if the employee exists
            string checkEmployeeQuery = @"SELECT COUNT(*) FROM [dbo].[EmployeeTable] WHERE EmployeeId = @EmployeeId AND IsActive = @IsActive";
            List<SqlParameter> employeeParameters = new()
            {
                new SqlParameter("@EmployeeId", requestModel.EmployeeId),
                new SqlParameter("@IsActive", true)
            };
            int employeeCount = Convert.ToInt32(_adoDotNetServices.QueryFirstOrDefault(checkEmployeeQuery, employeeParameters.ToArray()).Rows[0][0]);
            if (employeeCount == 0)
                return BadRequest("Employee does not exist");

            // Check if the project exists
            string checkProjectQuery = @"SELECT COUNT(*) FROM [dbo].[ProjectTable] WHERE ProjectId = @ProjectId AND IsActive = @IsActive";
            List<SqlParameter> projectParameters = new()
            {
                new SqlParameter("@ProjectId", requestModel.ProjectId),
                new SqlParameter("@IsActive", true)
            };
            int projectCount = Convert.ToInt32(_adoDotNetServices.QueryFirstOrDefault(checkProjectQuery, projectParameters.ToArray()).Rows[0][0]);
            if (projectCount == 0)
                return BadRequest("Project does not exist");

            // Check for duplicate employee-project association
            string duplicateQuery = @"SELECT [EmployeeId], [ProjectId], [IsActive]
                                      FROM [dbo].[EmployeeProjectTable]
                                      WHERE EmployeeId = @EmployeeId AND ProjectId = @ProjectId AND IsActive = @IsActive";

            List<SqlParameter> duplicateParameters = new()
            {
                new SqlParameter("@EmployeeId", requestModel.EmployeeId),
                new SqlParameter("@ProjectId", requestModel.ProjectId),
                new SqlParameter("@IsActive", true)
            };

            DataTable employeeProjects = _adoDotNetServices.QueryFirstOrDefault(duplicateQuery, duplicateParameters.ToArray());
            if (employeeProjects.Rows.Count > 0)
                return BadRequest("For this employee, project association already exists");

            // Insert new employee-project association
            string query = @"INSERT INTO [dbo].[EmployeeProjectTable]
                             ([EmployeeId], [ProjectId], [IsActive])
                             VALUES (@EmployeeId, @ProjectId, @IsActive)";

            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@EmployeeId", requestModel.EmployeeId),
                new SqlParameter("@IsActive", true),
                new SqlParameter("@ProjectId", requestModel.ProjectId)
            };

            int result = _adoDotNetServices.Execute(query, parameters.ToArray());
            return result > 0 ? StatusCode(201, "Create Successfully") : BadRequest("Create Fail");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/delete-employee-project/{employeeId}/{projectId}")]

    public IActionResult DeleteProject(long projectId, long employeeId)
    {
        try
        {
            string query = @"UPDATE [dbo].[EmployeeProjectTable]
   SET IsActive = @IsActive
 WHERE ProjectId = @ProjectId AND EmployeeId = @EmployeeId";

            List<SqlParameter> parameter = new()
            {

                new SqlParameter("@IsActive", false),
                new SqlParameter("@ProjectId", projectId),
                new SqlParameter("@EmployeeId", employeeId)

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
