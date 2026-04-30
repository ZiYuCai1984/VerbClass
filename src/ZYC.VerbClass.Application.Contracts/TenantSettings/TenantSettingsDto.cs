namespace ZYC.VerbClass.Application.Contracts.TenantSettings;

public class TenantSettingsDto
{
    public TenantPasswordPolicyDto PasswordPolicy { get; set; } = new();

    public TenantSettingsPermissionsDto Permissions { get; set; } = new();
}
