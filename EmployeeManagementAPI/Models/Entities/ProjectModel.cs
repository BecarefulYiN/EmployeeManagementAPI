namespace EmployeeManagementAPI.Models.Entities;

public class ProjectModel
{
    public long ProjectId { get; set; }
    public string ProjectName { get; set; }
    public DateTime StartDate {  get; set; }
    public string Status { get; set; } = null!;

    public bool IsActive {  get; set; }
}
