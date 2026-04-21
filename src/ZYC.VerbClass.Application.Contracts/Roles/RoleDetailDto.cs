namespace ZYC.VerbClass.Application.Contracts.Roles;

public class RoleDetailDto
{
    public string Name { get; set; } = string.Empty;

    public int UserCount { get; set; }

    public int GrantedPermissionCount { get; set; }

    public int AvailablePermissionCount { get; set; }

    public RolePermissionEntryDto[] GrantedPermissions { get; set; } = [];

    public RoleAssignedUserDto[] AssignedUsers { get; set; } = [];

    public int RemainingUserCount { get; set; }

    public bool CanManagePermissions { get; set; }
}
