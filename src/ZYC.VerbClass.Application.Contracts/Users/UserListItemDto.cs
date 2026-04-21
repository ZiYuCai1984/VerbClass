namespace ZYC.VerbClass.Application.Contracts.Users;

public class UserListItemDto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public string? DepartmentSummary { get; set; }
}
