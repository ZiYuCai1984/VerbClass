namespace ZYC.VerbClass.Application.Contracts.Departments;

public class UserDepartmentSummaryDto
{
    public Guid UserId { get; set; }

    public string? Summary { get; set; }
}
