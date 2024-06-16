using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Models.RequestModels.Project;
using EmployeeManagementAPI.Models.ResponseModesl.Employee;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace EmployeeManagementAPI.Controllers;

public class ProjectController : ControllerBase
{
    private readonly AdoDotNetServices _adoDotNetServices;

    public ProjectController(AdoDotNetServices adoDotNetServices)
    {
        _adoDotNetServices = adoDotNetServices;
    }

    [HttpGet]
    [Route("/api/project-list")]
    public IActionResult GetProjectList()
    {
        try
        {
            string query = @"
            SELECT [ProjectId]
                  ,[ProjectName]
                  ,[StartDate]
                  ,CASE 
                       WHEN [Status] IS NULL THEN 'Ongoing' 
                       ELSE CONVERT(varchar, [Status], 23) 
                   END AS [Status]
                  ,[IsActive]
            FROM [dbo].[ProjectTable] 
            WHERE IsActive = @IsActive";

            List<SqlParameter> parameters = new()
        {
            new SqlParameter("@IsActive", true)
        };

            List<ProjectModel> projects = _adoDotNetServices.Query<ProjectModel>(query, parameters.ToArray());
            return Ok(projects);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet]
    [Route("/api/project/{id}")]
    public IActionResult GetProjectById(int id)
    {
        try
        {
            string query = @"
        SELECT [ProjectId]
              ,[ProjectName]
              ,[StartDate]
              ,CASE 
                   WHEN [Status] IS NULL THEN 'Ongoing' 
                   ELSE CONVERT(varchar, [Status], 23) 
               END AS [Status]
              ,[IsActive]
        FROM [dbo].[ProjectTable] 
        WHERE ProjectId = @ProjectId";

            List<SqlParameter> parameters = new()
        {
            new SqlParameter("@ProjectId", id)
        };

            List<ProjectModel> projects = _adoDotNetServices.Query<ProjectModel>(query, parameters.ToArray());
            return Ok(projects);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet]
    [Route("/api/ended-projects")]
    public IActionResult GetEndedProjects()
    {
        try
        {
            string query = @"
            SELECT [ProjectId]
                  ,[ProjectName]
                  ,[StartDate]
                  ,CASE 
                       WHEN [Status] IS NULL THEN 'Ongoing' 
                       ELSE CONVERT(varchar, [Status], 23) 
                   END AS [Status]
                  ,[IsActive]
            FROM [dbo].[ProjectTable] 
            WHERE IsActive = @IsActive AND [Status] IS NOT NULL";

            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@IsActive", true)
            };

            List<ProjectModel> projects = _adoDotNetServices.Query<ProjectModel>(query, parameters.ToArray());
            return Ok(projects);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }


    [HttpPost]
    [Route("/api/create-project")]

    public IActionResult CreateProject([FromBody] ProjectRequestModel requestModel)
    {
        try
        {
            if (string.IsNullOrEmpty(requestModel.ProjectName)) return BadRequest("Department need to be fill");

            string duplicateQuery = @"SELECT [ProjectId]
      ,[ProjectName]
      ,[StartDate]
      ,[Status]
      ,[IsActive]
  FROM [dbo].[ProjectTable] WHERE ProjectName = @ProjectName AND IsActive = @IsActive";

            List<SqlParameter> duplicateParameters = new()
            {
                new SqlParameter("@ProjectName", requestModel.ProjectName.ToLower()),
                new SqlParameter("@IsActive",true)
            };

            DataTable Projects = _adoDotNetServices.QueryFirstOrDefault(duplicateQuery, duplicateParameters.ToArray());
            if (Projects.Rows.Count > 0) return BadRequest("Projects already exist");

            string query = @"INSERT INTO [dbo].[ProjectTable]
           ([ProjectName]
           ,[StartDate]
           ,[Status]
           ,[IsActive])
     VALUES (@ProjectName,@StartDate,@Status, @IsActive)";

            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@ProjectName", requestModel.ProjectName.ToLower()),
                new SqlParameter("@IsActive",true),
                new SqlParameter("@StartDate",DateTime.Now),
                new SqlParameter("@Status", DBNull.Value)
            };

            int result = _adoDotNetServices.Execute(query, parameters.ToArray());

            return result > 0 ? StatusCode(201, "Create Successfully") : BadRequest("Create Fail");
        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/update-project/{id}")]
    public IActionResult UpdateProject([FromBody] ProjectRequestModel requestModel, long id)
    {
        try
        {
            string checkExistQuery = @"SELECT [ProjectId]
      ,[ProjectName]
      ,[StartDate]
      ,[Status]
      ,[IsActive]
  FROM [dbo].[ProjectTable] WHERE ProjectId = @ProjectId AND IsActive = @IsActive";
            List<SqlParameter> existenceParameters = new()
            {
                new SqlParameter("@ProjectId", id),
                new SqlParameter("@IsActive", true)
            };

            DataTable existQuery = _adoDotNetServices.QueryFirstOrDefault(checkExistQuery, existenceParameters.ToArray());
            if (existQuery.Rows.Count == 0)
                return NotFound("Role not found");

            // Validate
            if (string.IsNullOrEmpty(requestModel.ProjectName))
                return BadRequest("Project Name does not allow empty");

            // Check for duplicate role name (excluding the current role)
            string duplicateRoleQuery = @"SELECT COUNT(*) FROM [dbo].[ProjectTable] WHERE ProjectName = @ProjectName AND ProjectId != @ProjectId";
            List<SqlParameter> duplicateRoleParameters = new()
            {
                new SqlParameter("@ProjectName", requestModel.ProjectName),
                new SqlParameter("@ProjectId", id)
            };
            int duplicateCount = Convert.ToInt32(_adoDotNetServices.QueryFirstOrDefault(duplicateRoleQuery, duplicateRoleParameters.ToArray()).Rows[0][0]);
            if (duplicateCount > 0)
            {
                return Conflict("This Project already exists");
            }

            // Update 
            string updateQuery = @"UPDATE [dbo].[ProjectTable] SET ProjectName = @ProjectName WHERE [ProjectId] = @ProjectId";
            List<SqlParameter> updateParameters = new()
            {
                new SqlParameter("@ProjectName", requestModel.ProjectName),
                new SqlParameter("@ProjectId", id)
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
        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/delete-project/{id}")]

    public IActionResult DeleteProject(long id)
    {
        try
        {
            string query = @"UPDATE [dbo].[ProjectTable]
   SET IsActive = @IsActive
 WHERE ProjectId = @ProjectId";

            List<SqlParameter> parameter = new()
            {

                new SqlParameter("@IsActive", false),
                new SqlParameter("@ProjectId", id)
            };

            int result = _adoDotNetServices.Execute(query, parameter.ToArray());
            return result > 0 ? StatusCode(201, "Delete Successful") : BadRequest("Delete Fail");
        } catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

}
