using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.MainMenu.BuildIn;

public class SettingsMainMenuItem : MainMenuItem, ISingletonDependency
{
    private readonly ILifetimeScope _lifetimeScope;

    public SettingsMainMenuItem(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public override string Href => Routes.Admin_Settings;

    public override string Title => "Settings";

    public override async Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        if (!(context.CurrentUser?.IsAuthenticated ?? false) || context.CurrentTenant?.Id.HasValue != true)
        {
            return false;
        }

        return await _lifetimeScope.Resolve<IPermissionChecker>()
            .IsGrantedAsync(VerbClassPermissions.TenantSettings.Access);
    }
}
