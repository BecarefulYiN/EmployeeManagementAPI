﻿namespace EmployeeManagementAPI.Models.Entities;

public class EmployeeModel
{
    public long EmployeeId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;
    public DateTime HireDate { get; set; } 

    public long? DepartmentId { get; set; } = null!;

    public long? RoleId { get; set; } = null!;
    public bool IsActive { get; set; } 
}
