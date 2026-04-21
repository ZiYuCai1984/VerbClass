namespace ZYC.VerbClass.Web.Pages.Admin;

public record UserListItem(
    Guid Id,
    string DisplayName,
    string? UserName,
    string? Email,
    bool IsActive,
    string? DepartmentSummary);