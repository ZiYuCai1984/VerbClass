namespace ZYC.VerbClass.Application.Contracts.Roles;

public class RoleListItemDto
{
    public string Name { get; set; } = string.Empty;

    public int UserCount { get; set; }

    public int GrantedPermissionCount { get; set; }

    public int AvailablePermissionCount { get; set; }
}
