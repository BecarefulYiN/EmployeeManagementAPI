namespace EmployeeManagementAPI.Models.RequestModels.Employee
{
    public class EmployeeUpdateRequestModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int? DepartmentId { get; set; }
        public int? RoleId { get; set; }
    }
}
