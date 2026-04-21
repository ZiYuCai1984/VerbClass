namespace ZYC.VerbClass.Web.Abstractions.MainMenu;

public interface IMainMenuManager
{
    void RegisterItem(IMainMenuItem item);

    void RegisterItem<T>() where T : IMainMenuItem;

    Task<IMainMenuItem[]> GetItemsAsync(UserTenantContext context);
}