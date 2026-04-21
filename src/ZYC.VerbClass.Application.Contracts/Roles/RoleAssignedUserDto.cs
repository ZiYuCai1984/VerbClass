namespace ZYC.VerbClass.Application.Contracts.Roles;

public class RoleAssignedUserDto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
}
