using Volo.Abp.Identity.Settings;
using Volo.Abp.Settings;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain;

public class ChangeIdentityPasswordPolicySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        var requireNonAlphanumeric = context.Get(IdentitySettingNames.Password.RequireNonAlphanumeric);
        requireNonAlphanumeric.DefaultValue = false.ToString();

        var requireLowercase = context.Get(IdentitySettingNames.Password.RequireLowercase);
        requireLowercase.DefaultValue = false.ToString();

        var requireUppercase = context.Get(IdentitySettingNames.Password.RequireUppercase);
        requireUppercase.DefaultValue = false.ToString();

        var requireDigit = context.Get(IdentitySettingNames.Password.RequireDigit);
        requireDigit.DefaultValue = false.ToString();

        var requiredLength = context.Get(IdentitySettingNames.Password.RequiredLength);
        requiredLength.DefaultValue = "1";

        //var requiredUniqueChars = context.Get(IdentitySettingNames.Password.RequiredUniqueChars);
        //!TODO Can not be "false" ??
        //requiredUniqueChars.DefaultValue = false.ToString();
    }
}