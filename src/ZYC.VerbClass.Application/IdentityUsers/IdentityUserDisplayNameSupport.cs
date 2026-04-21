using Volo.Abp.Identity;

namespace ZYC.VerbClass.Application.IdentityUsers;

internal static class IdentityUserDisplayNameSupport
{
    public static string BuildDisplayName(IdentityUser user, string fallbackDisplayName = "Unknown")
    {
        var parts = new[]
        {
            user.Surname?.Trim(),
            user.Name?.Trim()
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        var displayName = string.Join(' ', parts);
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        return string.IsNullOrWhiteSpace(user.UserName)
            ? fallbackDisplayName
            : user.UserName;
    }
}
