namespace ZYC.VerbClass.Application.Contracts.Roles;

public class RolePermissionItemDto
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsGranted { get; set; }
}
