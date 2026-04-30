namespace ZYC.VerbClass.Web.Pages.Admin.UserManagerPartials;

public record UserListItem(
    Guid Id,
    string DisplayName,
    string? UserName,
    string? Email,
    bool IsActive,
    string? DepartmentSummary);