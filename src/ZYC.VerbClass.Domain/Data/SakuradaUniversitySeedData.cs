using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.Data;

internal partial class SakuradaUniversitySeedData
{
    public const string DemoTenantName = VerbClassConsts.SakuradaUniversityDemoTenantName;

    public const string DemoTenantAdminEmail = VerbClassConsts.SakuradaUniversityDemoTenantAdminEmail;

    public const string DemoAdminPassword = VerbClassConsts.SakuradaUniversityDemoAdminPassword;

    private static readonly HashSet<string> OperationsAdminUserNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "rei.sakurada",
        "haruka.ichikawa",
        "naoki.fujimoto"
    };

    public static readonly UniversityUserSeedItem[] UserSeedItems = CreatePrimaryUserSeedItems()
        .Concat(CreateGeneratedFacultySeedItems())
        .Concat(CreateGeneratedAdministrativeSeedItems())
        .Concat(CreateGeneratedStudentSeedItems())
        .Select(ApplyDefaultRoles)
        .ToArray();

    private static UniversityUserSeedItem ApplyDefaultRoles(UniversityUserSeedItem item)
    {
        return item with
        {
            RoleNames = ResolveRoleNames(item)
        };
    }

    private static IReadOnlyList<string> ResolveRoleNames(UniversityUserSeedItem item)
    {
        if (item.EnrollmentYear.HasValue)
        {
            return [VerbClassRoles.Student];
        }

        if (item.UserName.StartsWith("faculty.", StringComparison.OrdinalIgnoreCase))
        {
            return [VerbClassRoles.Instructor];
        }

        if (item.UserName.StartsWith("staff.", StringComparison.OrdinalIgnoreCase))
        {
            return [VerbClassRoles.Assistant];
        }

        if (OperationsAdminUserNames.Contains(item.UserName))
        {
            return [VerbClassRoles.OperationsAdmin];
        }

        if (IsTeachingManagerDepartment(item.DepartmentCode))
        {
            return [VerbClassRoles.TeachingManager];
        }

        return [VerbClassRoles.Assistant];
    }

    private static bool IsTeachingManagerDepartment(string departmentCode)
    {
        return string.Equals(departmentCode, "academic-affairs", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(departmentCode, "affiliated-high-school", StringComparison.OrdinalIgnoreCase) ||
               departmentCode.StartsWith("department-", StringComparison.OrdinalIgnoreCase) ||
               departmentCode.StartsWith("graduate-school-", StringComparison.OrdinalIgnoreCase);
    }
}

internal record UniversityUserSeedItem(
    string UserName,
    string Email,
    string Password,
    string SurnameKanji,
    string NameKanji,
    string SurnameKana,
    string NameKana,
    string SurnameRomanized,
    string NameRomanized,
    DateTime BirthDate,
    Gender Gender,
    string BloodType,
    string Nationality,
    string Country,
    string Prefecture,
    string City,
    string Street,
    string PostalCode,
    int? EnrollmentYear,
    string DepartmentCode,
    DateTime DepartmentEffectiveFrom,
    IReadOnlyList<string> RoleNames
);
