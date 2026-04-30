using Volo.Abp.Identity;

namespace ZYC.VerbClass.Domain;

public static class VerbClassConsts
{
    public const string DbTablePrefix = "App";

    public const string? DbSchema = null;

    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;

    public const string AdminPasswordDefaultValue = "123456";

    public const string SakuradaUniversityDemoTenantName = "桜田大学";

    public const string SakuradaUniversityDemoTenantAdminEmail = "admin@sakurada-u.ac.jp";

    public const string SakuradaUniversityDemoAdminPassword = AdminPasswordDefaultValue;
}
