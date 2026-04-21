using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.MainMenu.BuildIn;

public class DepartmentManagerMainMenuItem : MainMenuItem, ISingletonDependency
{
    private readonly ILifetimeScope _lifetimeScope;

    public DepartmentManagerMainMenuItem(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public override string Href => Routes.Admin_DepartmentManager;

    public override string Title => "Department Manager";

    public override async Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        if (!(context.CurrentUser?.IsAuthenticated ?? false))
        {
            return false;
        }

        return await _lifetimeScope.Resolve<IPermissionChecker>()
            .IsGrantedAsync(VerbClassPermissions.Departments.Access);
    }
}
