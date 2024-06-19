using System.Data;
using System.Data.SqlClient;
using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Models.RequestModels.Department;
using EmployeeManagementAPI.Models.ResponseModels.Department;
using EmployeeManagementAPI.Queries;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult GetDepartmentList()
    {
        try
        {
            string query = DepartmentQuery.GetDepartmentListQuery();
            List<SqlParameter> parameters = new() { new SqlParameter("@IsActive", true) };

            List<DepartmentModel> lst = _adoDotNetServices.Query<DepartmentModel>(
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

    [HttpGet]
    [Route("/api/department/{id}")]
    public IActionResult GetDepartmentById(int id)
    {
        try
        {
            string query = DepartmentQuery.GetDepartmentByIdQuery();
            List<SqlParameter> parameters = new() { new SqlParameter("@DepartmentId", id) };

            List<GetDepartmentResponseModel> lst =
                _adoDotNetServices.Query<GetDepartmentResponseModel>(query, parameters.ToArray());

            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [Route("/api/create-department")]
    public IActionResult CreateDepartment([FromBody] DepartmentRequestModel requestModel)
    {
        try
        {
            if (requestModel.DepartmentName.IsNullOrEmpty())
                return BadRequest("Department need to be fill");

            string duplicateQuery = DepartmentQuery.GetCheckDepartmentNameDuplicateQuery();
            List<SqlParameter> duplicateParameters =
                new()
                {
                    new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                    new SqlParameter("@IsActive", true)
                };

            DataTable departments = _adoDotNetServices.QueryFirstOrDefault(
                duplicateQuery,
                duplicateParameters.ToArray()
            );
            if (departments.Rows.Count > 0)
                return BadRequest("Department with this name already exist.");

            string query = DepartmentQuery.InsertDepartmentQuery();
            List<SqlParameter> parameters =
                new()
                {
                    new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                    new SqlParameter("@IsActive", true)
                };

            int result = _adoDotNetServices.Execute(query, parameters.ToArray());

            return result > 0
                ? StatusCode(201, "Creating Successfully")
                : BadRequest("Creating Fail");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/update-department/{id}")]
    public IActionResult UpdateDepartment([FromBody] DepartmentRequestModel requestModel, long id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Id is invalid.");

            string checkExistQuery = DepartmentQuery.GetCheckDepartmentExistsQuery();
            List<SqlParameter> existenceParameters =
                new() { new SqlParameter("@DepartmentId", id) };

            DataTable existQuery = _adoDotNetServices.QueryFirstOrDefault(
                checkExistQuery,
                existenceParameters.ToArray()
            );
            if (existQuery.Rows.Count == 0)
                return NotFound("Role not found");

            if (requestModel.DepartmentName.IsNullOrEmpty())
                return BadRequest("Department name does not allow empty");

            #region Check for duplicate role name (excluding the current role)

            string duplicateRoleQuery = DepartmentQuery.GetCheckDuplicateRoleQuery();
            List<SqlParameter> duplicateRoleParameters =
                new()
                {
                    new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                    new SqlParameter("@DepartmentId", id)
                };
            int duplicateCount = Convert.ToInt32(
                _adoDotNetServices
                    .QueryFirstOrDefault(duplicateRoleQuery, duplicateRoleParameters.ToArray())
                    .Rows[0][0]
            );
            if (duplicateCount > 0)
                return Conflict("This department already exists");

            #endregion

            #region Update role record

            string updateQuery = DepartmentQuery.GetUpdateDepartmentQuery();
            List<SqlParameter> updateParameters =
                new()
                {
                    new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                    new SqlParameter("@DepartmentId", id)
                };
            int rowsAffected = _adoDotNetServices.Execute(updateQuery, updateParameters.ToArray());

            #endregion

            return rowsAffected > 0 ? Ok("Updated successfully") : BadRequest("Failed to update.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/delete-department/{id}")]
    public IActionResult DeleteDepartment(long id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Id is invalid.");

            string query = DepartmentQuery.GetDeleteDepartmentQuery();
            List<SqlParameter> parameter =
                new()
                {
                    new SqlParameter("@IsActive", false),
                    new SqlParameter("@DepartmentId", id)
                };

            int result = _adoDotNetServices.Execute(query, parameter.ToArray());

            return result > 0
                ? StatusCode(201, "Deleting Successful")
                : BadRequest("Deleting Fail");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
