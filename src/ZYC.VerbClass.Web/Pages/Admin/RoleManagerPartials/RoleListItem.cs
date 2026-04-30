namespace ZYC.VerbClass.Web.Pages.Admin.RoleManagerPartials;

public record RoleListItem(
    string Name,
    int UserCount,
    int GrantedPermissionCount,
    int AvailablePermissionCount
);
