using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Web.Core.AccountMenu;

namespace ZYC.VerbClass.Web.AccountMenu.BuildIn;

public class UserSettingsAccountMenuItem : AccountMenuItem, ISingletonDependency
{
    private readonly ILifetimeScope _lifetimeScope;

    public UserSettingsAccountMenuItem(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public override string Href => Routes.Account_UserSettings;

    public override string Title => "User Settings";

    public override async Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        if (!(context.CurrentUser?.IsAuthenticated ?? false))
        {
            return false;
        }

        return await _lifetimeScope.Resolve<IPermissionChecker>()
            .IsGrantedAsync(VerbClassPermissions.UserSettings.Access);
    }
}
