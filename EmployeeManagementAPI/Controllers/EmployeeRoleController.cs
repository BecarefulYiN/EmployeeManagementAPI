using System.Data;
using System.Data.SqlClient;
using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Models.RequestModels.Employee_Role;
using EmployeeManagementAPI.Queries;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

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
            string query = EmployeeRoleQuery.GetEmployeeRoleListQuery();
            List<SqlParameter> parameters = new() { new SqlParameter("@IsActive", true) };
            List<EmployeeRoleModel> lst = _adoDotNetServices.Query<EmployeeRoleModel>(
                query,
                parameters.ToArray()
            );

            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [Route("/api/create-employee-role")]
    public IActionResult CreateEmployeeRole([FromBody] EmployeeRoleRequestModel requestModel)
    {
        try
        {
            if (requestModel.RoleName!.IsNullOrEmpty())
                return BadRequest("Role Name doesn't not allow empty");

            string duplicateQuery = EmployeeRoleQuery.GetDuplicateEmployeeQuery();

            List<SqlParameter> duplicateParameters =
                new() { new SqlParameter("@RoleName", requestModel.RoleName!.ToLower()) };

            DataTable Roles = _adoDotNetServices.QueryFirstOrDefault(
                duplicateQuery,
                duplicateParameters.ToArray()
            );
            if (Roles.Rows.Count > 0)
                return Conflict("This roles already exist!");

            string query = EmployeeRoleQuery.GetInsertEmployeeQuery();

            List<SqlParameter> parameters =
                new()
                {
                    new SqlParameter("@RoleName", requestModel.RoleName.ToLower()),
                    new SqlParameter("@IsActive", true)
                };

            int result = _adoDotNetServices.Execute(query, parameters.ToArray());
            return result > 0 ? StatusCode(201, "Create successfully") : BadRequest("Create Fail.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/update-employee-role/{id}")]
    public IActionResult UpdateEmployeeRole(
        [FromBody] EmployeeRoleRequestModel requestModel,
        long id
    )
    {
        try
        {
            string checkExistQuery = EmployeeRoleQuery.GetCheckEmployeeExistQuery();

            List<SqlParameter> existenceParameters = new() { new SqlParameter("@RoleId", id) };

            DataTable existQuery = _adoDotNetServices.QueryFirstOrDefault(
                checkExistQuery,
                existenceParameters.ToArray()
            );
            if (existQuery.Rows.Count == 0)
                return NotFound("Role not found");

            if (string.IsNullOrEmpty(requestModel.RoleName))
                return BadRequest("Role Name does not allow empty");

            string duplicateRoleQuery = EmployeeRoleQuery.GetDuplicateRoleQuery();

            List<SqlParameter> duplicateRoleParameters =
                new()
                {
                    new SqlParameter("@RoleName", requestModel.RoleName),
                    new SqlParameter("@RoleId", id)
                };
            int duplicateCount = Convert.ToInt32(
                _adoDotNetServices
                    .QueryFirstOrDefault(duplicateRoleQuery, duplicateRoleParameters.ToArray())
                    .Rows[0][0]
            );
            if (duplicateCount > 0)
                return Conflict("This role already exists");

            string updateQuery = EmployeeRoleQuery.GetUpdateEmployeeQuery();
            List<SqlParameter> updateParameters =
                new()
                {
                    new SqlParameter("@RoleName", requestModel.RoleName),
                    new SqlParameter("@RoleId", id)
                };
            int rowsAffected = _adoDotNetServices.Execute(updateQuery, updateParameters.ToArray());

            if (rowsAffected > 0)
                return Ok("Updated successfully");
            else
                return BadRequest("Failed to update");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/delete-employee-role/{id}")]
    public IActionResult DeleteEmployeeRole(long id)
    {
        try
        {
            string query = EmployeeRoleQuery.GetDeleteEmployeeQuery();

            List<SqlParameter> parameter =
                new() { new SqlParameter("@IsActive", false), new SqlParameter("@RoleId", id) };

            int result = _adoDotNetServices.Execute(query, parameter.ToArray());

            return result > 0 ? StatusCode(201, "Delete Successful") : BadRequest("Delete Fail");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
