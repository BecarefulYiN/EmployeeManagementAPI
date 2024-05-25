namespace EmployeeManagementAPI.Models.Entities;

public class AttendanceModel
{
    public long AttendanceId { get; set; }
    public long EmployeeId { get; set; }
    public DateTime Date {  get; set; }

    public string Status {  get; set; }
}
