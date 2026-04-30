using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Abstractions.MainMenu;
using ZYC.VerbClass.Web.MainMenu.BuildIn;

namespace ZYC.VerbClass.Web.MainMenu;

public partial class MainMenuManager : IMainMenuManager, ISingletonDependency
{
    public MainMenuManager(
        ILifetimeScope lifetimeScope)
    {
        LifetimeScope = lifetimeScope;

        RegisterItem<RoleManagerMainMenuItem>();
        RegisterItem<UserManagerMainMenuItem>();
        RegisterItem<DepartmentManagerMainMenuItem>();
        RegisterItem<AuditLogMainMenuItem>();
        RegisterItem<SettingsMainMenuItem>();
        RegisterItem<SwitchAccountMainMenuItem>();
    }

    private ILifetimeScope LifetimeScope { get; }

    private List<IMainMenuItem> Items { get; } = new();

    public void RegisterItem(IMainMenuItem item)
    {
        Items.Add(item);
    }

    public void RegisterItem<T>() where T : IMainMenuItem
    {
        RegisterItem(LifetimeScope.Resolve<T>());
    }

    public async Task<IMainMenuItem[]> GetItemsAsync(UserTenantContext context)
    {
        var items = Items.ToArray();
        return await BuildVisibleItemsAsync(items, context);
    }

    private async Task<IMainMenuItem[]> BuildVisibleItemsAsync(
        IEnumerable<IMainMenuItem> items,
        UserTenantContext context)
    {
        var visibleItems = new List<IMainMenuItem>();

        foreach (var item in items.OrderBy(x => x.Priority))
        {
            var visibleItem = await BuildVisibleItemAsync(item, context);
            if (visibleItem is not null)
            {
                visibleItems.Add(visibleItem);
            }
        }

        return visibleItems.ToArray();
    }

    private async Task<IMainMenuItem?> BuildVisibleItemAsync(IMainMenuItem item, UserTenantContext context)
    {
        if (!await item.IsVisibleAsync(context))
        {
            return null;
        }

        var visibleSubItems = await BuildVisibleItemsAsync(item.SubItems, context);
        if (string.IsNullOrWhiteSpace(item.Href) && visibleSubItems.Length == 0)
        {
            return null;
        }

        return new VisibleMainMenuItem(item, visibleSubItems);
    }
}
