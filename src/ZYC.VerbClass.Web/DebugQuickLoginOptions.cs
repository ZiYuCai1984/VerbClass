using System.Collections.ObjectModel;
using ZYC.VerbClass.Domain;

namespace ZYC.VerbClass.Web;

public static class DebugQuickLoginOptions
{
    public const string LockedTenantName = VerbClassConsts.SakuradaUniversityDemoTenantName;

    public const string ReturnUrlPlaceholder = "__current_url__";

    private static readonly ReadOnlyCollection<QuickLoginOption> Options =
        new(
            [
                new QuickLoginOption(
                    "operations-admin",
                    "Operations admin",
                    "OperationsAdmin demo account",
                    "haruka.ichikawa",
                    VerbClassConsts.AdminPasswordDefaultValue
                ),
                new QuickLoginOption(
                    "teaching-manager",
                    "Teaching manager",
                    "TeachingManager demo account",
                    "rei.sakurada",
                    VerbClassConsts.AdminPasswordDefaultValue
                ),
                new QuickLoginOption(
                    "instructor",
                    "Instructor",
                    "Instructor demo account",
                    "faculty.takumi.takahashi.01",
                    VerbClassConsts.AdminPasswordDefaultValue
                ),
                new QuickLoginOption(
                    "assistant",
                    "Assistant",
                    "Assistant demo account",
                    "daiki.ishikawa",
                    VerbClassConsts.AdminPasswordDefaultValue
                ),
                new QuickLoginOption(
                    "student",
                    "Student",
                    "Student demo account",
                    "yuina.ito",
                    VerbClassConsts.AdminPasswordDefaultValue
                )
            ]
        );

    public static IReadOnlyList<QuickLoginOption> All => Options;

    public static QuickLoginOption? FindById(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return Options.FirstOrDefault(option => string.Equals(option.Id, id.Trim(), StringComparison.OrdinalIgnoreCase)
        );
    }

    public static string BuildSwitchAccountHref(string accountId)
    {
        return $"{Routes.Debug_Switch}?accountId={Uri.EscapeDataString(accountId)}&returnUrl={ReturnUrlPlaceholder}";
    }

    public record QuickLoginOption(
        string Id,
        string Title,
        string Description,
        string UserNameOrEmail,
        string Password
    );
}
