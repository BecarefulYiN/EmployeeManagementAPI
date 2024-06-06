namespace EmployeeManagementAPI.Models.Entities;

public class DepartmentModel
{
    public long DepartmentId { get; set; }
    public string DepartmentName { get; set;}
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
}
