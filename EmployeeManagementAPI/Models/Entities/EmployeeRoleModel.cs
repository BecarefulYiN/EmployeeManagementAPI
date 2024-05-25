namespace EmployeeManagementAPI.Models.Entities;

public class EmployeeRoleModel
{
    public long RoleId { get; set; }
    public string? RoleName { get; set; } = null!;
    public bool IsActive { get; set; } 
}
