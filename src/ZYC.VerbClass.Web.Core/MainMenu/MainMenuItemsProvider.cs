using ZYC.VerbClass.Web.Abstractions.MainMenu;

namespace ZYC.VerbClass.Web.Core.MainMenu;

public class MainMenuItemsProvider : MainMenuItem, IMainMenuItemsProvider
{
    private readonly List<IMainMenuItem> _mainMenuItems = new();

    public MainMenuItemsProvider(ILifetimeScope lifetimeScope)
    {
        LifetimeScope = lifetimeScope;
    }

    private ILifetimeScope LifetimeScope { get; }

    public override IMainMenuItem[] SubItems => _mainMenuItems.ToArray();

    public void RegisterItem(IMainMenuItem item)
    {
        _mainMenuItems.Add(item);
    }

    public void RegisterItem<T>() where T : IMainMenuItem
    {
        _mainMenuItems.Add(LifetimeScope.Resolve<T>());
    }
}