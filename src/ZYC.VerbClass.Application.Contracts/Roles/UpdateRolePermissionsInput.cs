namespace ZYC.VerbClass.Application.Contracts.Roles;

public class UpdateRolePermissionsInput
{
    public string[] GrantedPermissionNames { get; set; } = [];
}
