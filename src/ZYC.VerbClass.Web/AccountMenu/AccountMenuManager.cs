using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Abstractions.AccountMenu;
using ZYC.VerbClass.Web.AccountMenu.BuildIn;

namespace ZYC.VerbClass.Web.AccountMenu;

public class AccountMenuManager : IAccountMenuManager, ISingletonDependency
{
    public AccountMenuManager(
        ILifetimeScope lifetimeScope)
    {
        LifetimeScope = lifetimeScope;

        RegisterItem<UserSettingsAccountMenuItem>();
        RegisterItem<LogoutAccountMenuItem>();
    }

    private ILifetimeScope LifetimeScope { get; }

    private List<IAccountMenuItem> Items { get; } = new();

    public void RegisterItem(IAccountMenuItem item)
    {
        Items.Add(item);
    }

    public void RegisterItem<T>() where T : IAccountMenuItem
    {
        RegisterItem(LifetimeScope.Resolve<T>());
    }

    public async Task<IAccountMenuItem[]> GetItemsAsync(UserTenantContext context)
    {
        var visibleItems = new List<IAccountMenuItem>();

        foreach (var item in Items.OrderBy(x => x.Priority))
        {
            if (await item.IsVisibleAsync(context))
            {
                visibleItems.Add(item);
            }
        }

        return visibleItems.ToArray();
    }
}
