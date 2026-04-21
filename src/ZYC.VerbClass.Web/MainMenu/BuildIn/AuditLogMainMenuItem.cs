using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.MainMenu.BuildIn;

public class AuditLogMainMenuItem : MainMenuItem, ISingletonDependency
{
    private readonly ILifetimeScope _lifetimeScope;

    public AuditLogMainMenuItem(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public override string Href => Routes.Admin_AuditLog;

    public override string Title => "Audit Log";

    public override async Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        if (!(context.CurrentUser?.IsAuthenticated ?? false))
        {
            return false;
        }

        return await _lifetimeScope.Resolve<IPermissionChecker>()
            .IsGrantedAsync(VerbClassPermissions.AuditLogs.Access);
    }
}