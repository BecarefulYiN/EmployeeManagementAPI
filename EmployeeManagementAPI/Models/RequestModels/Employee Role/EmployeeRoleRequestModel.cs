namespace EmployeeManagementAPI.Models.RequestModels.Employee_Role;

public class EmployeeRoleRequestModel
{
    public string? RoleName { get; set; } = null!;
    public bool IsActive { get; set; }
}
