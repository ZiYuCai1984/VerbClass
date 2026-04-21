using Volo.Abp.Identity;

namespace ZYC.VerbClass.Domain;

public static class VerbClassConsts
{
    public const string DbTablePrefix = "App";

    public const string? DbSchema = null;
    
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    
    public const string AdminPasswordDefaultValue = "123456";

    public const string InitialTenantName = "桜田大学";

    public const string InitialTenantAdminEmail = "admin@sakurada-u.ac.jp";

    public const string InitialAdminPassword = AdminPasswordDefaultValue;
}
