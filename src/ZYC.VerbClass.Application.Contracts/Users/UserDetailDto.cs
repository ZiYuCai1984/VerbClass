using ZYC.VerbClass.Application.Contracts.Departments;

namespace ZYC.VerbClass.Application.Contracts.Users;

public class UserDetailDto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = "Unknown";

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    public string[] Roles { get; set; } = [];

    public UserDepartmentDisplayItemDto[] Departments { get; set; } = [];

    public bool HasAvatar { get; set; }

    public bool CanUpdate { get; set; }

    public bool CanDelete { get; set; }
}
