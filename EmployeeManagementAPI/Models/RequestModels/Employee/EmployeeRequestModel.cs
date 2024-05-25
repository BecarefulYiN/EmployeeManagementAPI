namespace EmployeeManagementAPI.Models.RequestModels.Employee;

public class EmployeeRequestModel
{

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public long? DepartmentId { get; set; } = null!;

    public long? RoleId { get; set; } = null!;

}
