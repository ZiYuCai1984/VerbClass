namespace ZYC.VerbClass.Application.Contracts.Users;

public class UserEditorDto
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public Guid[] DepartmentIds { get; set; } = [];

    public Guid? PrimaryDepartmentId { get; set; }

    public string[] RoleNames { get; set; } = [];

    public bool IsActive { get; set; }

    public bool ForcePasswordChangeOnNextLogin { get; set; }
}
