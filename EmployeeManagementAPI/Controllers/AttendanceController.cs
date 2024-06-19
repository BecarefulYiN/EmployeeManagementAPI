using System.Data;
using System.Data.SqlClient;
using EmployeeManagementAPI.Enums;
using EmployeeManagementAPI.Models.Entities;
using EmployeeManagementAPI.Models.RequestModels.Attendance;
using EmployeeManagementAPI.Queries;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementAPI.Controllers;

public class AttendanceController : ControllerBase
{
    private readonly AdoDotNetServices _adoDotNetServices;

    public AttendanceController(AdoDotNetServices adoDotNetServices)
    {
        _adoDotNetServices = adoDotNetServices;
    }

    [HttpPost]
    [Route("/api/attendance-record")]
    public IActionResult RecordAttendance([FromBody] AttendanceRequestModel requestModel)
    {
        try
        {
            if (requestModel.EmployeeId <= 0)
                return BadRequest("Invalid EmployeeId");

            //if (!IsValidStatus(requestModel.Status))
            //    return BadRequest("Status must be either 'Present' or 'Absent'");

            if (!Enum.IsDefined(typeof(EnumAttendanceStatus), requestModel.Status))
                return BadRequest("Status must be either 'Present' or 'Absent'");

            string employeeCheckQuery = AttendanceQuery.GetCheckEmployeeQuery();
            List<SqlParameter> employeeParameters =
                new() { new SqlParameter("@EmployeeId", requestModel.EmployeeId) };

            DataTable employee = _adoDotNetServices.QueryFirstOrDefault(
                employeeCheckQuery,
                employeeParameters.ToArray()
            );
            if (employee.Rows.Count == 0)
            {
                return BadRequest("Invalid EmployeeId");
            }

            string query = AttendanceQuery.InsertAttendanceQuery();
            List<SqlParameter> parameters =
                new()
                {
                    new SqlParameter("@EmployeeId", requestModel.EmployeeId),
                    new SqlParameter("@Date", requestModel.Date),
                    new SqlParameter("@Status", requestModel.Status.ToLower())
                };

            int result = _adoDotNetServices.Execute(query, parameters.ToArray());

            return result > 0
                ? StatusCode(201, "Attendance recorded successfully")
                : BadRequest("Failed to record attendance");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    [Route("/api/attendance-records")]
    public IActionResult GetAllAttendanceRecords()
    {
        try
        {
            string query = AttendanceQuery.GetAllAttendanceRecordsQuery();
            List<AttendanceModel> lst = _adoDotNetServices.Query<AttendanceModel>(query);

            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    [Route("/api/attendance-record/employee/{employeeId}")]
    public IActionResult GetAttendanceByEmployee(long employeeId)
    {
        try
        {
            if (employeeId <= 0)
                return BadRequest("Id is invalid.");

            #region Check if the EmployeeId exists

            string employeeCheckQuery = AttendanceQuery.GetCheckEmployeeExistQuery();
            List<SqlParameter> employeeParameters =
                new() { new SqlParameter("@EmployeeId", employeeId) };

            int employeeCount = Convert.ToInt32(
                _adoDotNetServices
                    .QueryFirstOrDefault(employeeCheckQuery, employeeParameters.ToArray())
                    .Rows[0][0]
            );
            if (employeeCount == 0)
                return NotFound("Employee not found");

            #endregion

            #region Fetch attendance records if the employee exists

            string query = AttendanceQuery.GetAttendanceRecordsByEmployee();
            List<SqlParameter> parameters = new() { new SqlParameter("@EmployeeId", employeeId) };

            List<AttendanceModel> lst = _adoDotNetServices.Query<AttendanceModel>(
                query,
                parameters.ToArray()
            );

            #endregion

            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
