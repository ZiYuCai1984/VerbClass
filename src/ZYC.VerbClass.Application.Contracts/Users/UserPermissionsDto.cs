namespace ZYC.VerbClass.Application.Contracts.Users;

public class UserPermissionsDto
{
    public bool CanCreate { get; set; }

    public bool CanUpdate { get; set; }

    public bool CanDelete { get; set; }

    public bool CanAssignRoles { get; set; }
}
