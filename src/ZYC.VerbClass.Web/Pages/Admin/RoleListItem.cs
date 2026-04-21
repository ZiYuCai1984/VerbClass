namespace ZYC.VerbClass.Web.Pages.Admin;

public record RoleListItem(
    string Name,
    int UserCount,
    int GrantedPermissionCount,
    int AvailablePermissionCount
);
