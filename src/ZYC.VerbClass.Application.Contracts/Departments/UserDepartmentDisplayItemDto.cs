namespace ZYC.VerbClass.Application.Contracts.Departments;

public class UserDepartmentDisplayItemDto
{
    public Guid DepartmentId { get; set; }

    public string Path { get; set; } = "/";

    public bool IsPrimary { get; set; }
}
