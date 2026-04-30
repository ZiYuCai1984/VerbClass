namespace ZYC.VerbClass.Web.Pages.Admin.DepartmentManagerPartials;

public record DepartmentListItem(
    Guid Id,
    string Name,
    string Code,
    string? ShortName,
    string PathDisplay,
    int Depth,
    bool IsActive,
    bool CanAssignUsers,
    int CurrentUserCount
);