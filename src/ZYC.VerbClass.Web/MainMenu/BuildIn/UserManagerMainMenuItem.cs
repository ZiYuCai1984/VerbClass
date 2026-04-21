using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Core.MainMenu;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Web.MainMenu.BuildIn;

public class UserManagerMainMenuItem : MainMenuItem, ISingletonDependency
{
    private readonly ILifetimeScope _lifetimeScope;

    public UserManagerMainMenuItem(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public override string Href => Routes.Admin_UserManager;

    public override string Title => "User Manager";

    public override async Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        if (!(context.CurrentUser?.IsAuthenticated ?? false))
        {
            return false;
        }

        return await _lifetimeScope.Resolve<IPermissionChecker>()
            .IsGrantedAsync(VerbClassPermissions.Users.Access);
    }
}
