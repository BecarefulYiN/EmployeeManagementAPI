using EmployeeManagementAPI.Models.RequestModels.Attendance;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using EmployeeManagementAPI.Models.Entities;

namespace EmployeeManagementAPI.Controllers;

public class AttendanceController : ControllerBase
{
    private readonly AdoDotNetServices _adoDotNetServices;

    public AttendanceController(AdoDotNetServices adoDotNetServices)
    {
        _adoDotNetServices = adoDotNetServices;
    }


    // Record attendance
    [HttpPost]
    [Route("/api/attendance-record")]
    public IActionResult RecordAttendance([FromBody] AttendanceRequestModel requestModel)
    {
        try
        {
            if (requestModel.EmployeeId <= 0)
                return BadRequest("Invalid EmployeeId");

            if (!IsValidStatus(requestModel.Status))
                return BadRequest("Status must be either 'Present' or 'Absent'");

            string employeeCheckQuery = @"SELECT [EmployeeId] FROM [dbo].[EmployeeTable] WHERE EmployeeId = @EmployeeId";
            List<SqlParameter> employeeParameters = new()
                {
                    new SqlParameter("@EmployeeId", requestModel.EmployeeId)
                };

            DataTable employee = _adoDotNetServices.QueryFirstOrDefault(employeeCheckQuery, employeeParameters.ToArray());
            if (employee.Rows.Count == 0)
            {
                return BadRequest("Invalid EmployeeId");
            }

            string query = @"INSERT INTO [dbo].[AttendanceTable]
                                  ([EmployeeId], [Date], [Status])
                                  VALUES
                                  (@EmployeeId, @Date, @Status)";

            List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@EmployeeId", requestModel.EmployeeId),
                    new SqlParameter("@Date", requestModel.Date),
                    new SqlParameter("@Status", requestModel.Status.ToLower())
                };

            int result = _adoDotNetServices.Execute(query, parameters.ToArray());
            return result > 0 ? StatusCode(201, "Attendance recorded successfully") : BadRequest("Failed to record attendance");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    // Get all attendance records
    [HttpGet]
    [Route("/api/attendance-records")]
    public IActionResult GetAllAttendanceRecords()
    {
        try
        {
            string query = @"
            SELECT 
                A.AttendanceId, 
                E.FirstName + ' ' + E.LastName AS EmployeeName,
                E.Email,
                A.Date,
                A.Status
            FROM 
                [dbo].[AttendanceTable] A
            JOIN 
                [dbo].[EmployeeTable] E
            ON 
                A.EmployeeId = E.EmployeeId";

            List<SqlParameter> parameters = new();

            List<AttendanceModel> lst = _adoDotNetServices.Query<AttendanceModel>(query, parameters.ToArray());
            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    // Get attendance records by employee
    [HttpGet]
    [Route("/api/attendance-record/employee/{employeeId}")]
    public IActionResult GetAttendanceByEmployee(long employeeId)
    {
        try
        {
            // Check if the EmployeeId exists
            string employeeCheckQuery = @"SELECT COUNT(*) FROM [dbo].[EmployeeTable] WHERE [EmployeeId] = @EmployeeId";
            List<SqlParameter> employeeParameters = new()
        {
            new SqlParameter("@EmployeeId", employeeId)
        };

            int employeeCount = Convert.ToInt32(_adoDotNetServices.QueryFirstOrDefault(employeeCheckQuery, employeeParameters.ToArray()).Rows[0][0]);
            if (employeeCount == 0)
            {
                return NotFound("Employee not found");
            }

            // Fetch attendance records if the employee exists
            string query = @"SELECT [AttendanceId], [EmployeeId], [Date], [Status]
                          FROM [dbo].[AttendanceTable] WHERE [EmployeeId] = @EmployeeId";

            List<SqlParameter> parameters = new()
        {
            new SqlParameter("@EmployeeId", employeeId)
        };

            List<AttendanceModel> lst = _adoDotNetServices.Query<AttendanceModel>(query, parameters.ToArray());
            return Ok(lst);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    // Helper method to validate status
    private bool IsValidStatus(string status)
    {
        return status == "present" || status == "absent";
    }
}


