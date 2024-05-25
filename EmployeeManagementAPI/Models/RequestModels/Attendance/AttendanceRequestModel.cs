namespace EmployeeManagementAPI.Models.RequestModels.Attendance;

public class AttendanceRequestModel
{
    public long? EmployeeId { get; set; } = null!; 
    public DateTime Date { get; set; }
    public string Status { get; set; } = null!;
}
