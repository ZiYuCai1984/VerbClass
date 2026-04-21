namespace ZYC.VerbClass.Application.Contracts.Roles;

public class RolePermissionEditorDto
{
    public string Name { get; set; } = string.Empty;

    public RolePermissionItemDto[] Permissions { get; set; } = [];
}
