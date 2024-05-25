namespace EmployeeManagementAPI.Models.ResponseModesl.Employee;

public class GetEmployeeResponseModel
{
    public long EmployeeId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;
    public DateTime HireDate { get; set; }

    public string? DepartmentName { get; set; }

    public string? RoleName { get; set; }
    public bool IsActive { get; set; }
}
