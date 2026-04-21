namespace ZYC.VerbClass.Application.Contracts.Roles;

public class RoleCommandResultDto
{
    public string Name { get; set; } = string.Empty;

    public int GrantedPermissionCount { get; set; }

    public int AvailablePermissionCount { get; set; }
}
