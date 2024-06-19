namespace EmployeeManagementAPI.Models.RequestModels.Employee;

public class EmployeeRequestModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string? DepartmentName { get; set; }
    public string? RoleName { get; set; }
}
