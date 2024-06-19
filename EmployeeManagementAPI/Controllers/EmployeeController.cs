using System.Data;
using System.Data.SqlClient;
using EmployeeManagementAPI.Models.RequestModels.Employee;
using EmployeeManagementAPI.Models.ResponseModesl.Employee;
using EmployeeManagementAPI.Queries;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult GetEmployeeList()
    {
        try
        {
            string query = EmployeeQuery.GetEmployeeListQuery();

            List<SqlParameter> parameters = new() { new SqlParameter("@IsActive", true) };
            List<GetEmployeeResponseModel> lst = _adoDotNetServices.Query<GetEmployeeResponseModel>(
                query,
                parameters.ToArray()
            );

            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet]
    [Route("/api/employee/{id}")]
    public IActionResult GetEmployeeById(int id)
    {
        try
        {
            string query = EmployeeQuery.GetEmployeeByIdQuery();
            List<SqlParameter> parameters = new() { new SqlParameter("@EmployeeId", id) };
            List<GetEmployeeResponseModel> lst = _adoDotNetServices.Query<GetEmployeeResponseModel>(
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
    [Route("/api/create-employee")]
    public IActionResult CreateEmployee([FromBody] EmployeeRequestModel requestModel)
    {
        try
        {
            if (requestModel.FirstName.IsNullOrEmpty())
                return BadRequest("FirstName does not allow empty");
            if (requestModel.LastName.IsNullOrEmpty())
                return BadRequest("LastName does not allow empty");
            if (requestModel.PhoneNumber.IsNullOrEmpty())
                return BadRequest("PhoneNumber does not allow empty");
            if (requestModel.Email.IsNullOrEmpty())
                return BadRequest("Email does not allow empty");

            string duplicateQuery = EmployeeQuery.GetCheckEmployeeEmailDuplicateQuery();

            List<SqlParameter> duplicateParameters =
                new() { new SqlParameter("@Email", requestModel.Email) };

            DataTable employee = _adoDotNetServices.QueryFirstOrDefault(
                duplicateQuery,
                duplicateParameters.ToArray()
            );
            if (employee.Rows.Count > 0)
                return Conflict("Employee with this email already exists!");

            // Check or insert Role
            object? roleId = DBNull.Value;
            if (!string.IsNullOrEmpty(requestModel.RoleName))
            {
                string roleQuery = EmployeeQuery.GetCheckEmployeeRoleQuery();
                List<SqlParameter> roleParameters =
                    new()
                    {
                        new SqlParameter("@RoleName", requestModel.RoleName),
                        new SqlParameter("@IsActive", true)
                    };

                DataTable role = _adoDotNetServices.QueryFirstOrDefault(
                    roleQuery,
                    roleParameters.ToArray()
                );
                if (role.Rows.Count == 0)
                {
                    // Insert the new role
                    string insertRoleQuery = EmployeeQuery.GetInsertRoleQuery();
                    roleId = _adoDotNetServices.Execute(insertRoleQuery, roleParameters.ToArray());
                }
                else
                {
                    roleId = role.Rows[0]["RoleId"];
                }
            }

            // Check or insert Department
            object? departmentId = DBNull.Value;
            if (!requestModel.DepartmentName!.IsNullOrEmpty())
            {
                string departmentQuery = EmployeeQuery.GetDepartmentQuery();
                List<SqlParameter> departmentParameters =
                    new()
                    {
                        new SqlParameter("@DepartmentName", requestModel.DepartmentName),
                        new SqlParameter("@IsActive", true)
                    };

                DataTable department = _adoDotNetServices.QueryFirstOrDefault(
                    departmentQuery,
                    departmentParameters.ToArray()
                );
                if (department.Rows.Count == 0)
                {
                    // Insert the new department
                    string insertDepartmentQuery = EmployeeQuery.InsertDepartmentQuery();
                    departmentId = _adoDotNetServices.Execute(
                        insertDepartmentQuery,
                        departmentParameters.ToArray()
                    );
                }
                else
                {
                    departmentId = department.Rows[0]["DepartmentId"];
                }
            }

            string query = EmployeeQuery.InsertEmployeeQuery();

            List<SqlParameter> parameters =
                new()
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
            return result > 0
                ? StatusCode(201, "Creating successfully")
                : BadRequest("Creating failed");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/employee-info-update/{id}")]
    public IActionResult UpdateEmployee([FromBody] EmployeeUpdateRequestModel requestModel, long id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Id is invalid.");

            string checkExistQuery = EmployeeQuery.GetCheckEmployeeExistsQuery();
            List<SqlParameter> existenceParameters = new() { new SqlParameter("@EmployeeId", id) };

            DataTable existQuery = _adoDotNetServices.QueryFirstOrDefault(
                checkExistQuery,
                existenceParameters.ToArray()
            );
            if (existQuery.Rows.Count == 0)
                return NotFound("Employee not found");

            #region Validation

            if (requestModel.FirstName.IsNullOrEmpty())
                return BadRequest("FirstName cannot be empty");
            if (requestModel.LastName.IsNullOrEmpty())
                return BadRequest("LastName cannot be empty");
            if (requestModel.PhoneNumber.IsNullOrEmpty())
                return BadRequest("PhoneNumber cannot be empty");
            if (requestModel.Email.IsNullOrEmpty())
                return BadRequest("Email cannot be empty");

            #endregion

            #region Check for duplicate email (excluding the current employee)

            string duplicateEmailQuery = EmployeeQuery.GetCheckDuplicateEmailQuery();
            List<SqlParameter> duplicateEmailParameters =
                new()
                {
                    new SqlParameter("@Email", requestModel.Email),
                    new SqlParameter("@EmployeeId", id)
                };
            int duplicateCount = Convert.ToInt32(
                _adoDotNetServices
                    .QueryFirstOrDefault(duplicateEmailQuery, duplicateEmailParameters.ToArray())
                    .Rows[0][0]
            );
            if (duplicateCount > 0)
                return Conflict("Employee with this email already exists");

            #endregion

            #region Fetch DepartmentId based on DepartmentName

            object? departmentId = DBNull.Value;
            if (!string.IsNullOrEmpty(requestModel.DepartmentName))
            {
                string departmentQuery = EmployeeQuery.GetDepartmentIdByDepartmentNameQuery();

                List<SqlParameter> departmentParameters =
                    new() { new SqlParameter("@DepartmentName", requestModel.DepartmentName) };
                DataTable departmentResult = _adoDotNetServices.QueryFirstOrDefault(
                    departmentQuery,
                    departmentParameters.ToArray()
                );
                if (departmentResult.Rows.Count == 0)
                    return BadRequest("Invalid Department");

                departmentId = departmentResult.Rows[0]["DepartmentId"];
            }

            #endregion

            #region Fetch RoleId based on RoleName

            object? roleId = DBNull.Value;
            if (!string.IsNullOrEmpty(requestModel.RoleName))
            {
                string roleQuery = EmployeeQuery.GetRoleIdByRoleName();
                List<SqlParameter> roleParameters =
                    new() { new SqlParameter("@RoleName", requestModel.RoleName) };
                DataTable roleResult = _adoDotNetServices.QueryFirstOrDefault(
                    roleQuery,
                    roleParameters.ToArray()
                );

                if (roleResult.Rows.Count == 0)
                    return BadRequest("Invalid Role");

                roleId = roleResult.Rows[0]["RoleId"];
            }

            #endregion

            #region Update employee record

            string updateQuery = EmployeeQuery.GetUpdateEmployeeQuery();
            List<SqlParameter> updateParameters =
                new()
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

            #endregion

            return rowsAffected > 0
                ? Ok("Employee updated successfully")
                : BadRequest("Failed to update employee");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    [Route("/api/delete-employee/{id}")]
    public IActionResult DeleteEmployee(long id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Id is invalid.");

            string query = EmployeeQuery.GetDeleteEmployeeQuery();

            List<SqlParameter> parameter =
                new() { new SqlParameter("@IsActive", false), new SqlParameter("@EmployeeId", id) };
            int result = _adoDotNetServices.Execute(query, parameter.ToArray());

            return result > 0
                ? StatusCode(201, "Deleting Successful")
                : BadRequest("Deleting Fail");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
