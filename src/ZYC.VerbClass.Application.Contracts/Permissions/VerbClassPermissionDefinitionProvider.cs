using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Domain.Shared.Localization;

namespace ZYC.VerbClass.Application.Contracts.Permissions;

public class VerbClassPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(VerbClassPermissions.GroupName);

        foreach (var permission in VerbClassPermissions.GetPermissions())
        {
            group.AddPermission(permission.Name, L(permission.DisplayName));
        }
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<VerbClassResource>(name);
    }
}
