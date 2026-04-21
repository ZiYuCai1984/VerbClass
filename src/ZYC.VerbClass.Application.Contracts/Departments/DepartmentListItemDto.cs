namespace ZYC.VerbClass.Application.Contracts.Departments;

public class DepartmentListItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? ShortName { get; set; }

    public string PathDisplay { get; set; } = "/";

    public int Depth { get; set; }

    public bool IsActive { get; set; }

    public bool CanAssignUsers { get; set; }

    public int CurrentUserCount { get; set; }
}
