using ZYC.VerbClass.Application.Contracts.Departments;

namespace ZYC.VerbClass.Web.Pages.Admin;

public class UserInfoModel
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = "Unknown";

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    public string[] Roles { get; set; } = [];

    public UserDepartmentDisplayItemDto[] Departments { get; set; } = [];

    public bool HasAvatar { get; set; }

    public bool CanUpdate { get; set; }

    public bool CanDelete { get; set; }

    public string? AvatarImageSource { get; set; }

    public string AvatarFallbackText
    {
        get
        {
            var text = string.IsNullOrWhiteSpace(DisplayName) ? UserName : DisplayName;
            if (string.IsNullOrWhiteSpace(text))
            {
                return "?";
            }

            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 2)
            {
                return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
            }

            return text.Length == 1
                ? text.ToUpperInvariant()
                : text[..2].ToUpperInvariant();
        }
    }
}
