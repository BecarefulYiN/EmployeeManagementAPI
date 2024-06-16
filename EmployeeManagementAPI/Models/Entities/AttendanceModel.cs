namespace EmployeeManagementAPI.Models.Entities;

public class AttendanceModel
{
    public long AttendanceId { get; set; }
    public string EmployeeName { get; set; }
    public string Email { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; }
}
